-- Purpose: Accelerate jurnal detail update MERGE/DELETE paths by REFFID + BARIS.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
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
    create_index_if_missing(
        'IDX_ACCT_JD_REFFID_BARIS',
        'CREATE INDEX IDX_ACCT_JD_REFFID_BARIS ON ACCT_JURNAL_DTL (REFFID, BARIS)'
    );

    BEGIN
        DBMS_STATS.GATHER_TABLE_STATS(OWNNAME => USER, TABNAME => 'ACCT_JURNAL_DTL', CASCADE => TRUE);
        DBMS_OUTPUT.PUT_LINE('STATS refreshed for ACCT_JURNAL_DTL');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('STATS refresh skipped: ' || SQLERRM);
    END;
END;
/
