-- Purpose: Promote dharyadi to ADMIN for GL and ACCOUNTING RBAC modules.
-- Date: 2026-06-02

SET SERVEROUTPUT ON;

DECLARE
    l_user_id CONSTANT VARCHAR2(30) := 'dharyadi';
    l_admin_role_id NUMBER;

    PROCEDURE assign_admin(p_module_name IN VARCHAR2) IS
        l_module_id NUMBER;
    BEGIN
        SELECT MODULE_ID
          INTO l_module_id
          FROM MASTER_MODULES
         WHERE MODULE_NAME = p_module_name;

        DELETE FROM MASTER_USER_ROLES
         WHERE LOWER(USER_ID) = l_user_id
           AND USER_ID <> l_user_id
           AND MODULE_ID = l_module_id;

        MERGE INTO MASTER_USER_ROLES target
        USING (
            SELECT l_user_id AS USER_ID,
                   l_admin_role_id AS ROLE_ID,
                   l_module_id AS MODULE_ID
              FROM dual
        ) source
           ON (target.USER_ID = source.USER_ID AND target.MODULE_ID = source.MODULE_ID)
         WHEN MATCHED THEN
            UPDATE SET target.ROLE_ID = source.ROLE_ID
         WHEN NOT MATCHED THEN
            INSERT (USER_ID, ROLE_ID, MODULE_ID)
            VALUES (source.USER_ID, source.ROLE_ID, source.MODULE_ID);

        DBMS_OUTPUT.PUT_LINE('PROMOTED dharyadi TO ADMIN FOR MODULE: ' || p_module_name);
    END;
BEGIN
    SELECT ROLE_ID
      INTO l_admin_role_id
      FROM MASTER_ROLES
     WHERE UPPER(ROLE_NAME) = 'ADMIN';

    assign_admin('GL');
    assign_admin('ACCOUNTING');
END;
/
