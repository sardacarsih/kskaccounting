-- Purpose: Create GTT for keep-baris strategy to avoid ORA-01795 (>1000 IN-list expressions).
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE create_gtt_if_missing(p_table IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED GTT: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table));
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
    create_gtt_if_missing('ACCT_JURNAL_KEEP_BARIS_TMP', '
        CREATE GLOBAL TEMPORARY TABLE ACCT_JURNAL_KEEP_BARIS_TMP
        (
            SESSION_TOKEN VARCHAR2(64) NOT NULL,
            BARIS         NUMBER(10)   NOT NULL
        )
        ON COMMIT DELETE ROWS
    ');

    create_index_if_missing(
        'IDX_ACCT_JURNAL_KEEP_BARIS_TMP',
        'CREATE INDEX IDX_ACCT_JURNAL_KEEP_BARIS_TMP ON ACCT_JURNAL_KEEP_BARIS_TMP (SESSION_TOKEN, BARIS)'
    );
END;
/
