-- Purpose: Check jurnal import staging and audit columns required by client-side import.
-- Date: 2026-05-28

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE require_column(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tab_columns
         WHERE table_name = UPPER(p_table)
           AND column_name = UPPER(p_column);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20061, UPPER(p_table) || '.' || UPPER(p_column) || ' is missing');
        END IF;
    END;
BEGIN
    require_column('ACCT_JURNAL_TMP', 'IDDATA');
    require_column('ACCT_JURNAL_TMP', 'USERID');
    require_column('ACCT_JURNAL_TMP', 'GLYEAR');
    require_column('ACCT_JURNAL_TMP', 'GLMONTH');

    require_column('ACCT_JURNAL_HDR', 'CREATED_DATE');
    require_column('ACCT_JURNAL_HDR', 'CREATED_BY');
    require_column('ACCT_JURNAL_DTL', 'CREATED_DATE');

    DBMS_OUTPUT.PUT_LINE('OK: jurnal import schema compatibility columns are installed');
END;
/
