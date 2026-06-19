-- Purpose: Ensure MASTER_LOGIN has the AKTIF status column required by login and user management.
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
        EXECUTE IMMEDIATE 'ALTER TABLE MASTER_LOGIN ADD (AKTIF CHAR(1) DEFAULT ''Y'')';
        EXECUTE IMMEDIATE 'UPDATE MASTER_LOGIN SET AKTIF = ''Y'' WHERE AKTIF IS NULL';
        DBMS_OUTPUT.PUT_LINE('ADDED MASTER_LOGIN.AKTIF');
    ELSE
        DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): MASTER_LOGIN.AKTIF');
    END IF;
END;
/
