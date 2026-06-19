-- Purpose: Check MASTER_LOGIN.AKTIF compatibility column.
-- Date: 2026-06-02

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tab_columns
     WHERE table_name = 'MASTER_LOGIN'
       AND column_name = 'AKTIF';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20071, 'MASTER_LOGIN.AKTIF is missing');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: MASTER_LOGIN.AKTIF is installed');
END;
/
