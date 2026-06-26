-- Purpose: Durable per-job impacted-account list so async recompute can run by
--          account-set (correct for insert/update/delete, even after rows are gone).
-- Date: 2026-06-26

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE create_table_if_missing(p_table IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED TABLE: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE create_index_if_missing(p_index_name IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED INDEX: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_index_name));
        END IF;
    END;
BEGIN
    create_table_if_missing('ACCT_RECALC_JOB_ACCOUNT', '
        CREATE TABLE ACCT_RECALC_JOB_ACCOUNT
        (
            JOB_ID   NUMBER        NOT NULL,
            KODEACC  VARCHAR2(64)  NOT NULL,
            CONSTRAINT PK_ACCT_RECALC_JOB_ACCOUNT PRIMARY KEY (JOB_ID, KODEACC),
            CONSTRAINT FK_ACCT_RECALC_JOB_ACCOUNT_JOB
                FOREIGN KEY (JOB_ID) REFERENCES ACCT_RECALC_JOB (JOB_ID) ON DELETE CASCADE
        )
    ');

    create_index_if_missing(
        'IDX_ACCT_RECALC_JOB_ACCOUNT',
        'CREATE INDEX IDX_ACCT_RECALC_JOB_ACCOUNT ON ACCT_RECALC_JOB_ACCOUNT (JOB_ID)'
    );
END;
/
