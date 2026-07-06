-- Purpose: Seed default GL admin user 'gladmin' (password: gl2026) as ADMIN for GL and
--          ACCOUNTING modules with access to every configured location (IDDATA).
-- Date: 2026-07-06

SET SERVEROUTPUT ON;

DECLARE
    l_user_id       CONSTANT VARCHAR2(30)  := 'gladmin';
    l_password_hash CONSTANT VARCHAR2(200) := 'PBKDF2$SHA256$600000$1ucqFXywseu1RsE4UU+8dw==$CLGk6/TCBIMxjUB7K34K1I2orev7uJgDrsGfxJbNbLY=';
    l_admin_role_id NUMBER;
    l_loc_count     NUMBER := 0;

    PROCEDURE assign_admin(p_module_name IN VARCHAR2) IS
        l_module_id NUMBER;
    BEGIN
        SELECT MODULE_ID
          INTO l_module_id
          FROM MASTER_MODULES
         WHERE MODULE_NAME = p_module_name;

        MERGE INTO MASTER_USER_ROLES target
        USING (
            SELECT l_user_id AS USER_ID, l_admin_role_id AS ROLE_ID, l_module_id AS MODULE_ID
              FROM dual
        ) source
           ON (target.USER_ID = source.USER_ID AND target.MODULE_ID = source.MODULE_ID)
         WHEN MATCHED THEN
            UPDATE SET target.ROLE_ID = source.ROLE_ID
         WHEN NOT MATCHED THEN
            INSERT (USER_ID, ROLE_ID, MODULE_ID)
            VALUES (source.USER_ID, source.ROLE_ID, source.MODULE_ID);

        DBMS_OUTPUT.PUT_LINE('ASSIGNED ' || l_user_id || ' TO ADMIN FOR MODULE: ' || p_module_name);
    END;
BEGIN
    MERGE INTO MASTER_LOGIN target
    USING (
        SELECT l_user_id        AS USERID,
               'GL Default Admin' AS NAMA,
               'IT'               AS DEPT,
               l_password_hash    AS PASSWORD,
               'System Administrator' AS JABATAN,
               'Y'                AS AKTIF
          FROM dual
    ) source
       ON (target.USERID = source.USERID)
     WHEN NOT MATCHED THEN
        INSERT (USERID, NAMA, DEPT, PASSWORD, JABATAN, AKTIF)
        VALUES (source.USERID, source.NAMA, source.DEPT, source.PASSWORD, source.JABATAN, source.AKTIF);

    DBMS_OUTPUT.PUT_LINE('ENSURED MASTER_LOGIN: ' || l_user_id);

    SELECT ROLE_ID
      INTO l_admin_role_id
      FROM MASTER_ROLES
     WHERE UPPER(ROLE_NAME) = 'ADMIN';

    assign_admin('GL');
    assign_admin('ACCOUNTING');

    MERGE INTO MASTER_USER_ROLES_LOC target
    USING (
        SELECT DISTINCT l_user_id AS USER_ID, d.IDDATA AS IDDATA
          FROM MASTER_PT_DTL d
         WHERE d.IDDATA IS NOT NULL
    ) source
       ON (target.USER_ID = source.USER_ID AND target.IDDATA = source.IDDATA)
     WHEN NOT MATCHED THEN
        INSERT (USER_ID, IDDATA)
        VALUES (source.USER_ID, source.IDDATA);

    SELECT COUNT(1)
      INTO l_loc_count
      FROM MASTER_USER_ROLES_LOC
     WHERE USER_ID = l_user_id;

    DBMS_OUTPUT.PUT_LINE('GRANTED LOCATION ACCESS: ' || l_loc_count || ' IDDATA rows for ' || l_user_id);
END;
/
