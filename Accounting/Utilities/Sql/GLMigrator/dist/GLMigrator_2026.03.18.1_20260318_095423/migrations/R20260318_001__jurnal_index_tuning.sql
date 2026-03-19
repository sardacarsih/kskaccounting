-- Purpose: Rollback index tuning for jurnal query optimization.
-- Date: 2026-03-18

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_index_if_exists(p_index_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP INDEX ' || UPPER(p_index_name);
            DBMS_OUTPUT.PUT_LINE('DROPPED: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_index_name));
        END IF;
    END;
BEGIN
    drop_index_if_exists('IDX_ACCT_JD_ID_PRD_KODE');
    drop_index_if_exists('IDX_ACCT_JD_ID_PRD_TGL');
    drop_index_if_exists('IDX_ACCT_JD_ID_GY_GM_NJ_BR');
    drop_index_if_exists('IDX_ACCT_JD_ID_PRD_NJ_BR');
    drop_index_if_exists('IDX_ACCT_JH_ID_PRD_NJ');
END;
/
