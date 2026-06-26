-- Purpose: Verify the server-side recalc drainer package and scheduler job exist.
-- Date: 2026-06-26

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
    v_errors NUMBER := 0;
    v_state USER_SCHEDULER_JOBS.ENABLED%TYPE;

    PROCEDURE fail(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[FAIL] ' || p_msg);
        v_errors := v_errors + 1;
    END;

    PROCEDURE ok(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[OK]   ' || p_msg);
    END;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Recalc Server Drainer Check ===');

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_RECALC_DRAINER'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';
    IF v_count < 2 THEN
        fail('ACCT_RECALC_DRAINER package/body missing or INVALID');
    ELSE
        ok('ACCT_RECALC_DRAINER package and body VALID');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_SCHEDULER_JOBS WHERE JOB_NAME = 'ACCT_RECALC_DRAINER_JOB';
    IF v_count = 0 THEN
        fail('Scheduler job ACCT_RECALC_DRAINER_JOB missing');
    ELSE
        ok('Scheduler job ACCT_RECALC_DRAINER_JOB present');
        SELECT ENABLED INTO v_state FROM USER_SCHEDULER_JOBS WHERE JOB_NAME = 'ACCT_RECALC_DRAINER_JOB';
        IF UPPER(v_state) <> 'TRUE' THEN
            fail('Scheduler job ACCT_RECALC_DRAINER_JOB not enabled (enabled=' || v_state || ')');
        ELSE
            ok('Scheduler job enabled');
        END IF;
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
        RAISE_APPLICATION_ERROR(-20001, 'ACCT_RECALC_DRAINER verification failed');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
