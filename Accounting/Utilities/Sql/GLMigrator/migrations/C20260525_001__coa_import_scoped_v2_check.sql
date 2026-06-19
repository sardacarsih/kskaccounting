-- Purpose: Check scoped COA import package.
-- Date: 2026-05-25

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tab_columns
     WHERE table_name = 'ACC_COA_TMP'
       AND column_name = 'BATCH_ID';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'ACC_COA_TMP.BATCH_ID is missing');
    END IF;

    SELECT COUNT(*)
      INTO l_count
      FROM user_objects
     WHERE object_name = 'ACCOUNTING_COA_IMPORT_V2'
       AND object_type = 'PACKAGE'
       AND status = 'VALID';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'ACCOUNTING_COA_IMPORT_V2 package is missing or invalid');
    END IF;

    SELECT COUNT(*)
      INTO l_count
      FROM user_objects
     WHERE object_name = 'ACCOUNTING_COA_IMPORT_V2'
       AND object_type = 'PACKAGE BODY'
       AND status = 'VALID';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20003, 'ACCOUNTING_COA_IMPORT_V2 package body is missing or invalid');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: scoped COA import package is installed');
END;
/
