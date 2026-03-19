-- Purpose: Rollback audit trail — drop audit tables and remove audit columns.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_table_if_exists(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = UPPER(p_table);
        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP TABLE ' || UPPER(p_table) || ' CASCADE CONSTRAINTS';
            DBMS_OUTPUT.PUT_LINE('DROPPED: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE drop_column_if_exists(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_count
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
    -- Drop detail table first (FK dependency)
    drop_table_if_exists('ACCT_JURNAL_AUDIT_DTL');
    drop_table_if_exists('ACCT_JURNAL_AUDIT');

    -- Remove audit columns from HDR
    drop_column_if_exists('ACCT_JURNAL_HDR', 'CREATED_DATE');
    drop_column_if_exists('ACCT_JURNAL_HDR', 'CREATED_BY');
    drop_column_if_exists('ACCT_JURNAL_HDR', 'MODIFIED_DATE');
    drop_column_if_exists('ACCT_JURNAL_HDR', 'MODIFIED_BY');
    drop_column_if_exists('ACCT_JURNAL_HDR', 'MODIFIED_PC');
    drop_column_if_exists('ACCT_JURNAL_HDR', 'MODIFIED_IP');

    -- Remove audit columns from DTL
    drop_column_if_exists('ACCT_JURNAL_DTL', 'CREATED_DATE');
    drop_column_if_exists('ACCT_JURNAL_DTL', 'MODIFIED_DATE');
END;
/
