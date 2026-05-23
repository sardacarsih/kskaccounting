-- Purpose: Rollback persistent login hardening state and ACCT_LOGIN_AUDIT table.
-- Date: 2026-04-12

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_table_if_exists(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

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
    drop_table_if_exists('ACCT_LOGIN_AUDIT');

    drop_column_if_exists('MASTER_LOGIN', 'FAILED_LOGIN_COUNT');
    drop_column_if_exists('MASTER_LOGIN', 'LOCKOUT_UNTIL_UTC');
    drop_column_if_exists('MASTER_LOGIN', 'LAST_LOGIN_AT_UTC');
    drop_column_if_exists('MASTER_LOGIN', 'LAST_FAILED_LOGIN_AT_UTC');
    drop_column_if_exists('MASTER_LOGIN', 'PASSWORD_CHANGED_AT_UTC');
    drop_column_if_exists('MASTER_LOGIN', 'PASSWORD_RESET_REQUIRED');
END;
/
