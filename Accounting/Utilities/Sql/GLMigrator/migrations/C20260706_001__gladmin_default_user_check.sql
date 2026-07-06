-- Purpose: Verify gladmin default admin user, RBAC role assignment, and full location access.
-- Date: 2026-07-06

SET SERVEROUTPUT ON;

DECLARE
    l_count        INTEGER;
    l_role_count   INTEGER;
    l_loc_count    INTEGER;
    l_total_iddata INTEGER;
BEGIN
    SELECT COUNT(1)
      INTO l_count
      FROM MASTER_LOGIN
     WHERE USERID = 'gladmin'
       AND AKTIF = 'Y';

    IF l_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20092, 'gladmin MASTER_LOGIN row missing or inactive');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: gladmin MASTER_LOGIN is active');

    SELECT COUNT(DISTINCT mm.MODULE_NAME)
      INTO l_role_count
      FROM MASTER_USER_ROLES ur
      JOIN MASTER_ROLES mr ON mr.ROLE_ID = ur.ROLE_ID
      JOIN MASTER_MODULES mm ON mm.MODULE_ID = ur.MODULE_ID
     WHERE ur.USER_ID = 'gladmin'
       AND UPPER(mr.ROLE_NAME) = 'ADMIN'
       AND mm.MODULE_NAME IN ('GL', 'ACCOUNTING');

    IF l_role_count <> 2 THEN
        RAISE_APPLICATION_ERROR(-20093, 'gladmin is not ADMIN for both GL and ACCOUNTING modules');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: gladmin is ADMIN for GL and ACCOUNTING');

    SELECT COUNT(DISTINCT IDDATA)
      INTO l_total_iddata
      FROM MASTER_PT_DTL
     WHERE IDDATA IS NOT NULL;

    SELECT COUNT(1)
      INTO l_loc_count
      FROM MASTER_USER_ROLES_LOC
     WHERE USER_ID = 'gladmin';

    IF l_loc_count < l_total_iddata THEN
        RAISE_APPLICATION_ERROR(-20094, 'gladmin does not have access to all locations (' || l_loc_count || ' of ' || l_total_iddata || ')');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: gladmin has access to all ' || l_loc_count || ' locations');
END;
/
