-- Purpose: Check dharyadi ADMIN RBAC assignment for GL and ACCOUNTING modules.
-- Date: 2026-06-02

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
BEGIN
    SELECT COUNT(DISTINCT mm.MODULE_NAME)
      INTO l_count
      FROM MASTER_USER_ROLES ur
      JOIN MASTER_ROLES mr
        ON mr.ROLE_ID = ur.ROLE_ID
      JOIN MASTER_MODULES mm
        ON mm.MODULE_ID = ur.MODULE_ID
     WHERE ur.USER_ID = 'dharyadi'
       AND UPPER(mr.ROLE_NAME) = 'ADMIN'
       AND mm.MODULE_NAME IN ('GL', 'ACCOUNTING');

    IF l_count <> 2 THEN
        RAISE_APPLICATION_ERROR(-20091, 'dharyadi is not ADMIN for both GL and ACCOUNTING modules');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: dharyadi is ADMIN for GL and ACCOUNTING');
END;
/
