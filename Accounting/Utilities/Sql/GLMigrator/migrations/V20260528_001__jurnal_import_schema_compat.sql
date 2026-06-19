-- Purpose: Ensure jurnal import staging and audit columns required by client-side import exist.
-- Date: 2026-05-28

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE add_column_if_missing(p_table IN VARCHAR2, p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
        l_count INTEGER;
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tab_columns
         WHERE table_name = UPPER(p_table)
           AND column_name = UPPER(p_column);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' ADD (' || UPPER(p_column) || ' ' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED ' || UPPER(p_table) || '.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table) || '.' || UPPER(p_column));
        END IF;
    END;
BEGIN
    add_column_if_missing('ACCT_JURNAL_TMP', 'IDDATA', 'VARCHAR2(20)');
    add_column_if_missing('ACCT_JURNAL_TMP', 'USERID', 'VARCHAR2(20)');
    add_column_if_missing('ACCT_JURNAL_TMP', 'GLYEAR', 'NUMBER(4)');
    add_column_if_missing('ACCT_JURNAL_TMP', 'GLMONTH', 'NUMBER(2)');

    add_column_if_missing('ACCT_JURNAL_HDR', 'CREATED_DATE', 'TIMESTAMP DEFAULT SYSTIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_HDR', 'CREATED_BY', 'VARCHAR2(50)');
    add_column_if_missing('ACCT_JURNAL_DTL', 'CREATED_DATE', 'TIMESTAMP DEFAULT SYSTIMESTAMP');
END;
/
