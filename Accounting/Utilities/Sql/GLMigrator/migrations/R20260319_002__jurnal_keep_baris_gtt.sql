-- Purpose: Rollback GTT for keep-baris strategy.
-- Date: 2026-03-19

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
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_index_name));
        END IF;
    END;

    PROCEDURE drop_table_if_exists(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP TABLE ' || UPPER(p_table);
            DBMS_OUTPUT.PUT_LINE('DROPPED TABLE: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_table));
        END IF;
    END;
BEGIN
    drop_index_if_exists('IDX_ACCT_JURNAL_KEEP_BARIS_TMP');
    drop_table_if_exists('ACCT_JURNAL_KEEP_BARIS_TMP');
END;
/
