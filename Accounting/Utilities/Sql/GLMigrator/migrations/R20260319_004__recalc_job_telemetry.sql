-- Purpose: Rollback telemetry columns on ACCT_RECALC_JOB.
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

    PROCEDURE drop_column_if_exists(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' DROP COLUMN ' || UPPER(p_column);
            DBMS_OUTPUT.PUT_LINE('DROPPED COLUMN: ' || UPPER(p_table) || '.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_table) || '.' || UPPER(p_column));
        END IF;
    END;
BEGIN
    drop_index_if_exists('IDX_ACCT_RECALC_JOB_SCOPE');
    drop_column_if_exists('ACCT_RECALC_JOB', 'IMPACTED_ROW_COUNT');
    drop_column_if_exists('ACCT_RECALC_JOB', 'IMPACTED_ACCOUNT_COUNT');
    drop_column_if_exists('ACCT_RECALC_JOB', 'RECALC_SCOPE');
END;
/
