-- Purpose: Rollback staging GTT and recalc job table.
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

    PROCEDURE drop_constraint_if_exists(p_table IN VARCHAR2, p_constraint_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_CONSTRAINTS
         WHERE TABLE_NAME = UPPER(p_table)
           AND CONSTRAINT_NAME = UPPER(p_constraint_name);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' DROP CONSTRAINT ' || UPPER(p_constraint_name);
            DBMS_OUTPUT.PUT_LINE('DROPPED CONSTRAINT: ' || UPPER(p_constraint_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ' || UPPER(p_constraint_name));
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
    drop_index_if_exists('IDX_ACCT_RECALC_JOB_KEY');
    drop_index_if_exists('IDX_ACCT_RECALC_JOB_DUE');
    drop_constraint_if_exists('ACCT_RECALC_JOB', 'CK_ACCT_RECALC_JOB_STATUS');
    drop_table_if_exists('ACCT_RECALC_JOB');

    drop_index_if_exists('IDX_ACCT_JURNAL_DTL_STAGE_TMP');
    drop_table_if_exists('ACCT_JURNAL_DTL_STAGE_TMP');
END;
/
