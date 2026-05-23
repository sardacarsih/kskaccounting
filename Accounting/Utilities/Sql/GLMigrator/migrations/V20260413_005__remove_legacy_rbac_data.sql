-- Purpose: Remove legacy RBAC data that is no longer used after standard role catalog alignment.
-- Date: 2026-04-13

SET SERVEROUTPUT ON;

DECLARE
    v_legacy_user_assignments NUMBER := 0;
    v_removed_role_mappings NUMBER := 0;
    v_removed_roles NUMBER := 0;
    v_removed_permissions NUMBER := 0;
BEGIN
    SELECT COUNT(1)
      INTO v_legacy_user_assignments
      FROM MASTER_USER_ROLES mur
      JOIN MASTER_ROLES mr
        ON mr.ROLE_ID = mur.ROLE_ID
     WHERE UPPER(mr.ROLE_NAME) IN ('MANAGER', 'KABAG', 'ASISTEN', 'TAMU', 'AUDIT');

    IF v_legacy_user_assignments > 0 THEN
        RAISE_APPLICATION_ERROR(-20121, 'Legacy roles masih dipakai user. Cleanup dibatalkan.');
    END IF;

    DELETE FROM MASTER_ROLE_PERMISSIONS
     WHERE ROLE_ID IN (
         SELECT ROLE_ID
         FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) IN ('MANAGER', 'KABAG', 'ASISTEN', 'TAMU', 'AUDIT')
     );

    v_removed_role_mappings := SQL%ROWCOUNT;

    DELETE FROM MASTER_ROLES
     WHERE UPPER(ROLE_NAME) IN ('MANAGER', 'KABAG', 'ASISTEN', 'TAMU', 'AUDIT');

    v_removed_roles := SQL%ROWCOUNT;

    DELETE FROM MASTER_ROLE_PERMISSIONS
     WHERE PERMISSION_ID IN (
         SELECT mp.PERMISSION_ID
         FROM MASTER_PERMISSIONS mp
         JOIN MASTER_MODULES mm
           ON mm.MODULE_ID = mp.MODULE_ID
        WHERE mm.MODULE_NAME = 'ACCOUNTING'
          AND mp.PERMISSION_NAME = 'ALL'
     );

    DELETE FROM MASTER_PERMISSIONS
     WHERE MODULE_ID IN (
         SELECT MODULE_ID
         FROM MASTER_MODULES
         WHERE MODULE_NAME = 'ACCOUNTING'
     )
       AND PERMISSION_NAME = 'ALL';

    v_removed_permissions := SQL%ROWCOUNT;

    DBMS_OUTPUT.PUT_LINE('REMOVED LEGACY ROLE MAPPINGS: ' || v_removed_role_mappings);
    DBMS_OUTPUT.PUT_LINE('REMOVED LEGACY ROLES        : ' || v_removed_roles);
    DBMS_OUTPUT.PUT_LINE('REMOVED LEGACY PERMISSIONS  : ' || v_removed_permissions);
END;
/
