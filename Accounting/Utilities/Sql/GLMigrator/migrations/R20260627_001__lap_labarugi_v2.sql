-- Purpose: Rollback wrapper package ACCT_LAPORAN_V2 (Laba Rugi V2 entrypoint).
--          Legacy ACCT_LAPORAN.LAP_LABARUGI is untouched by this migration.
-- Date: 2026-06-27

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE = 'PACKAGE';

    IF v_count > 0 THEN
        EXECUTE IMMEDIATE 'DROP PACKAGE ACCT_LAPORAN_V2';
        DBMS_OUTPUT.PUT_LINE('DROPPED PACKAGE ACCT_LAPORAN_V2');
    ELSE
        DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): PACKAGE ACCT_LAPORAN_V2');
    END IF;
END;
/
