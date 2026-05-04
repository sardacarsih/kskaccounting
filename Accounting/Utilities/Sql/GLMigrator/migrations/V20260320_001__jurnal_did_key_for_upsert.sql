-- Purpose: Use DID as detail key for jurnal upsert by extending stage table and DID indexes.
-- Date: 2026-03-20

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

    FUNCTION has_equivalent_single_column_index(
        p_table_name IN VARCHAR2,
        p_column_name IN VARCHAR2
    ) RETURN BOOLEAN IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES ui
         WHERE ui.TABLE_NAME = UPPER(p_table_name)
           AND (
                SELECT COUNT(1)
                  FROM USER_IND_COLUMNS uic
                 WHERE uic.INDEX_NAME = ui.INDEX_NAME
                   AND uic.TABLE_NAME = ui.TABLE_NAME
               ) = 1
           AND EXISTS (
                SELECT 1
                  FROM USER_IND_COLUMNS uic
                 WHERE uic.INDEX_NAME = ui.INDEX_NAME
                   AND uic.TABLE_NAME = ui.TABLE_NAME
                   AND uic.COLUMN_POSITION = 1
                   AND uic.COLUMN_NAME = UPPER(p_column_name)
               );

        RETURN v_count > 0;
    END;

    PROCEDURE create_index_if_missing_or_equivalent(
        p_index_name IN VARCHAR2,
        p_table_name IN VARCHAR2,
        p_column_name IN VARCHAR2,
        p_sql IN CLOB
    ) IS
    BEGIN
        IF has_equivalent_single_column_index(p_table_name, p_column_name) THEN
            DBMS_OUTPUT.PUT_LINE(
                'SKIPPED (equivalent index exists): ' ||
                UPPER(p_table_name) || '(' || UPPER(p_column_name) || ')'
            );
            RETURN;
        END IF;

        create_index_if_missing(p_index_name, p_sql);
    END;

    PROCEDURE add_column_if_missing(p_table_name IN VARCHAR2, p_column_name IN VARCHAR2, p_column_def IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table_name)
           AND COLUMN_NAME = UPPER(p_column_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table_name) || ' ADD (' || UPPER(p_column_name) || ' ' || p_column_def || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED COLUMN: ' || UPPER(p_table_name) || '.' || UPPER(p_column_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED COLUMN (exists): ' || UPPER(p_table_name) || '.' || UPPER(p_column_name));
        END IF;
    END;
BEGIN
    add_column_if_missing('ACCT_JURNAL_DTL_STAGE_TMP', 'DID', 'VARCHAR2(80)');

    create_index_if_missing(
        'IDX_ACCT_JD_STAGE_TMP_DID',
        'CREATE INDEX IDX_ACCT_JD_STAGE_TMP_DID ON ACCT_JURNAL_DTL_STAGE_TMP (SESSION_TOKEN, DID)'
    );

    create_index_if_missing_or_equivalent(
        'IDX_ACCT_JD_DID',
        'ACCT_JURNAL_DTL',
        'DID',
        'CREATE INDEX IDX_ACCT_JD_DID ON ACCT_JURNAL_DTL (DID)'
    );

    BEGIN
        DBMS_STATS.GATHER_TABLE_STATS(OWNNAME => USER, TABNAME => 'ACCT_JURNAL_DTL_STAGE_TMP', CASCADE => TRUE);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('STATS refresh skipped for ACCT_JURNAL_DTL_STAGE_TMP: ' || SQLERRM);
    END;

    BEGIN
        DBMS_STATS.GATHER_TABLE_STATS(OWNNAME => USER, TABNAME => 'ACCT_JURNAL_DTL', CASCADE => TRUE);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('STATS refresh skipped for ACCT_JURNAL_DTL: ' || SQLERRM);
    END;
END;
/
