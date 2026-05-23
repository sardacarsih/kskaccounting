-- Check script for legacy RBAC coexistence.
-- The destructive cleanup migration is intentionally excluded from the active manifest.

SET SERVEROUTPUT ON;

DECLARE
    v_missing_role_count NUMBER := 0;
    v_accounting_all_count NUMBER := 0;
BEGIN
    SELECT COUNT(1)
      INTO v_missing_role_count
      FROM (
          SELECT 'MANAGER' AS ROLE_NAME FROM DUAL
          UNION ALL SELECT 'KABAG' FROM DUAL
          UNION ALL SELECT 'ASISTEN' FROM DUAL
          UNION ALL SELECT 'TAMU' FROM DUAL
          UNION ALL SELECT 'AUDIT' FROM DUAL
      ) expected
     WHERE NOT EXISTS (
          SELECT 1
            FROM MASTER_ROLES mr
           WHERE UPPER(mr.ROLE_NAME) = expected.ROLE_NAME
     );

    IF v_missing_role_count <> 0 THEN
        RAISE_APPLICATION_ERROR(-20131, 'LEGACY ROLE MISSING COUNT: ' || v_missing_role_count);
    END IF;

    SELECT COUNT(1)
      INTO v_accounting_all_count
      FROM MASTER_PERMISSIONS mp
      JOIN MASTER_MODULES mm
        ON mm.MODULE_ID = mp.MODULE_ID
     WHERE mm.MODULE_NAME = 'ACCOUNTING'
       AND mp.PERMISSION_NAME = 'ALL';

    IF v_accounting_all_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20134, 'LEGACY ACCOUNTING ALL PERMISSION MISSING.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK LEGACY ROLES PRESERVED          : MANAGER, KABAG, ASISTEN, TAMU, AUDIT');
    DBMS_OUTPUT.PUT_LINE('OK LEGACY ACCOUNTING ALL PERMISSION: ' || v_accounting_all_count);
END;
/
