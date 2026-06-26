-- Purpose: Server-side drainer for ACCT_RECALC_JOB so queued saldo recalculations
--          complete even when no client application is running. Runs the same
--          idempotent ACCT_RECALLCULATIONS_V2.ReCalcByJob entrypoint used by the
--          in-process workers; claims are safe under concurrency (FOR UPDATE SKIP
--          LOCKED + status re-check), and recompute idempotency makes any overlap
--          with a client worker harmless.
-- Date: 2026-06-26

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_RECALC_DRAINER AS
    -- Process up to p_max_jobs due jobs in a single invocation.
    PROCEDURE RunOnce(p_max_jobs IN INTEGER DEFAULT 25);
END ACCT_RECALC_DRAINER;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_RECALC_DRAINER AS

    c_max_attempts   CONSTANT INTEGER := 4;   -- 1 initial + 3 retries
    c_stale_seconds  CONSTANT INTEGER := 180; -- recover RUNNING jobs stuck this long

    FUNCTION RetryDelaySeconds(p_attempt IN INTEGER) RETURN INTEGER IS
    BEGIN
        -- Mirror the in-process worker backoff schedule: 1, 5, 15 minutes.
        IF p_attempt <= 1 THEN
            RETURN 60;
        ELSIF p_attempt = 2 THEN
            RETURN 300;
        ELSE
            RETURN 900;
        END IF;
    END RetryDelaySeconds;

    PROCEDURE RecoverStale IS
    BEGIN
        UPDATE ACCT_RECALC_JOB
           SET STATUS = 'RETRY',
               LAST_ERROR = CASE WHEN LAST_ERROR IS NULL
                                 THEN 'Recovered stale RUNNING job (server drainer)'
                                 ELSE LAST_ERROR END,
               NEXT_RETRY_AT = SYSTIMESTAMP,
               UPDATED_DATE = SYSTIMESTAMP
         WHERE STATUS = 'RUNNING'
           AND NVL(STARTED_DATE, UPDATED_DATE) < SYSTIMESTAMP - NUMTODSINTERVAL(c_stale_seconds, 'SECOND');
        COMMIT;
    END RecoverStale;

    PROCEDURE RunOnce(p_max_jobs IN INTEGER DEFAULT 25) IS
        v_job_id    NUMBER;
        v_iddata    VARCHAR2(20);
        v_periode   VARCHAR2(7);
        v_jurnalid  NUMBER;
        v_glmonth   NUMBER;
        v_glyear    NUMBER;
        v_userid    VARCHAR2(20);
        v_attempt   NUMBER;
        v_error     VARCHAR2(1800);
        v_retry_seconds NUMBER;
        v_processed INTEGER := 0;
    BEGIN
        RecoverStale;

        WHILE v_processed < p_max_jobs LOOP
            v_job_id := NULL;

            -- Claim one due job atomically. Oracle does not allow FOR UPDATE on
            -- an ordered FETCH FIRST query in all supported versions, so use an
            -- ordered candidate list plus a status-checked UPDATE claim.
            FOR candidate IN (
                SELECT JOB_ID
                  FROM (
                        SELECT JOB_ID
                          FROM ACCT_RECALC_JOB
                         WHERE STATUS IN ('PENDING', 'RETRY')
                           AND (NEXT_RETRY_AT IS NULL OR NEXT_RETRY_AT <= SYSTIMESTAMP)
                         ORDER BY CREATED_DATE, JOB_ID
                  )
                 WHERE ROWNUM <= 8
            ) LOOP
                UPDATE ACCT_RECALC_JOB
                   SET STATUS = 'RUNNING',
                       ATTEMPT_COUNT = ATTEMPT_COUNT + 1,
                       LAST_ERROR = NULL,
                       STARTED_DATE = SYSTIMESTAMP,
                       UPDATED_DATE = SYSTIMESTAMP
                 WHERE JOB_ID = candidate.JOB_ID
                   AND STATUS IN ('PENDING', 'RETRY')
                   AND (NEXT_RETRY_AT IS NULL OR NEXT_RETRY_AT <= SYSTIMESTAMP);

                IF SQL%ROWCOUNT = 1 THEN
                    v_job_id := candidate.JOB_ID;
                    EXIT;
                END IF;
            END LOOP;

            IF v_job_id IS NULL THEN
                EXIT; -- nothing due
            END IF;

            SELECT IDDATA, PERIODE, JURNALID, GLMONTH, GLYEAR, USERID, ATTEMPT_COUNT
              INTO v_iddata, v_periode, v_jurnalid, v_glmonth, v_glyear, v_userid, v_attempt
              FROM ACCT_RECALC_JOB
             WHERE JOB_ID = v_job_id;
            COMMIT; -- release claim promptly; claim is now durable

            BEGIN
                ACCT_RECALLCULATIONS_V2.ReCalcByJob(
                    p_IDDATA  => v_iddata,
                    p_BULAN   => v_glmonth,
                    p_TAHUN   => v_glyear,
                    p_PERIODE => v_periode,
                    p_USERID  => v_userid,
                    p_JOBID   => v_job_id
                );

                UPDATE ACCT_RECALC_JOB
                   SET STATUS = 'DONE',
                       FINISHED_DATE = SYSTIMESTAMP,
                       UPDATED_DATE = SYSTIMESTAMP
                 WHERE JOB_ID = v_job_id;
                COMMIT;
            EXCEPTION
                WHEN OTHERS THEN
                    v_error := SUBSTR(SQLERRM, 1, 1800);
                    IF v_attempt >= c_max_attempts THEN
                        UPDATE ACCT_RECALC_JOB
                           SET STATUS = 'FAILED',
                               LAST_ERROR = v_error,
                               FINISHED_DATE = SYSTIMESTAMP,
                               UPDATED_DATE = SYSTIMESTAMP
                         WHERE JOB_ID = v_job_id;
                    ELSE
                        v_retry_seconds := RetryDelaySeconds(v_attempt);
                        UPDATE ACCT_RECALC_JOB
                           SET STATUS = 'RETRY',
                               LAST_ERROR = v_error,
                               NEXT_RETRY_AT = SYSTIMESTAMP + NUMTODSINTERVAL(v_retry_seconds, 'SECOND'),
                               UPDATED_DATE = SYSTIMESTAMP
                         WHERE JOB_ID = v_job_id;
                    END IF;
                    COMMIT;
            END;

            v_processed := v_processed + 1;
        END LOOP;
    END RunOnce;
END ACCT_RECALC_DRAINER;]';

    DBMS_OUTPUT.PUT_LINE('CREATED PACKAGE ACCT_RECALC_DRAINER');
END;
/

-- (Re)create the scheduler job that drains the queue once per minute.
DECLARE
    v_count NUMBER := 0;
BEGIN
    SELECT COUNT(1) INTO v_count FROM USER_SCHEDULER_JOBS WHERE JOB_NAME = 'ACCT_RECALC_DRAINER_JOB';
    IF v_count > 0 THEN
        DBMS_SCHEDULER.DROP_JOB(job_name => 'ACCT_RECALC_DRAINER_JOB', force => TRUE);
    END IF;

    DBMS_SCHEDULER.CREATE_JOB(
        job_name        => 'ACCT_RECALC_DRAINER_JOB',
        job_type        => 'PLSQL_BLOCK',
        job_action      => 'BEGIN ACCT_RECALC_DRAINER.RunOnce(); END;',
        start_date      => SYSTIMESTAMP,
        repeat_interval => 'FREQ=MINUTELY;INTERVAL=1',
        enabled         => TRUE,
        comments        => 'Drain ACCT_RECALC_JOB so saldo recalculations complete without a running client.'
    );
    DBMS_OUTPUT.PUT_LINE('CREATED SCHEDULER JOB: ACCT_RECALC_DRAINER_JOB');
END;
/
