-- Purpose: Add telemetry columns to ACCT_RECALC_JOB for delta recalc scope and impact metrics.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE add_column_if_missing(p_table IN VARCHAR2, p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' ADD (' || UPPER(p_column) || ' ' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED: ' || UPPER(p_table) || '.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table) || '.' || UPPER(p_COLUMN));
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
    add_column_if_missing('ACCT_RECALC_JOB', 'RECALC_SCOPE', 'VARCHAR2(20) DEFAULT ''NONE'' NOT NULL');
    add_column_if_missing('ACCT_RECALC_JOB', 'IMPACTED_ACCOUNT_COUNT', 'NUMBER(10) DEFAULT 0 NOT NULL');
    add_column_if_missing('ACCT_RECALC_JOB', 'IMPACTED_ROW_COUNT', 'NUMBER(10) DEFAULT 0 NOT NULL');

    create_index_if_missing(
        'IDX_ACCT_RECALC_JOB_SCOPE',
        'CREATE INDEX IDX_ACCT_RECALC_JOB_SCOPE ON ACCT_RECALC_JOB (STATUS, RECALC_SCOPE, CREATED_DATE)'
    );
END;
/
