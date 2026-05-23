-- Purpose: Rollback DID key support for jurnal upsert.
-- Date: 2026-03-20

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
            DBMS_OUTPUT.PUT_LINE('DROPPED INDEX: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED INDEX (not found): ' || UPPER(p_index_name));
        END IF;
    END;

    PROCEDURE drop_column_if_exists(p_table_name IN VARCHAR2, p_column_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table_name)
           AND COLUMN_NAME = UPPER(p_column_name);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table_name) || ' DROP COLUMN ' || UPPER(p_column_name);
            DBMS_OUTPUT.PUT_LINE('DROPPED COLUMN: ' || UPPER(p_table_name) || '.' || UPPER(p_column_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED COLUMN (not found): ' || UPPER(p_table_name) || '.' || UPPER(p_column_name));
        END IF;
    END;
BEGIN
    drop_index_if_exists('IDX_ACCT_JD_DID');
    drop_index_if_exists('IDX_ACCT_JD_STAGE_TMP_DID');
    drop_column_if_exists('ACCT_JURNAL_DTL_STAGE_TMP', 'DID');
END;
/
