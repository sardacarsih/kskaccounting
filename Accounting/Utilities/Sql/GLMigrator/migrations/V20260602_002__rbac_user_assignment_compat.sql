-- Purpose: Ensure RBAC user assignment tables exist and seed ACCOUNTING access from legacy GL app detail.
-- Date: 2026-06-02

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
    l_user_role_id NUMBER;
    l_gl_module_id NUMBER;
    l_accounting_module_id NUMBER;

    PROCEDURE create_table_if_missing(p_table IN VARCHAR2, p_sql IN CLOB) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tables
         WHERE table_name = UPPER(p_table);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED TABLE: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE seed_user_roles_for_module(p_module_id IN NUMBER, p_module_name IN VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE '
            INSERT INTO MASTER_USER_ROLES (USER_ID, ROLE_ID, MODULE_ID)
            SELECT DISTINCT d.USERID, :role_id, :module_id
              FROM MASTER_APPS_DETAIL d
             WHERE d.APPID = ''GL''
               AND d.USERID IS NOT NULL
               AND NOT EXISTS (
                   SELECT 1
                     FROM MASTER_USER_ROLES ur
                    WHERE ur.USER_ID = d.USERID
                      AND ur.MODULE_ID = :module_id
               )'
            USING l_user_role_id, p_module_id, p_module_id;

        DBMS_OUTPUT.PUT_LINE('SEEDED MASTER_USER_ROLES for ' || p_module_name || ': ' || SQL%ROWCOUNT);
    END;
BEGIN
    create_table_if_missing('MASTER_USER_ROLES', '
        CREATE TABLE MASTER_USER_ROLES
        (
            USER_ID   VARCHAR2(30) NOT NULL,
            ROLE_ID   NUMBER NOT NULL,
            MODULE_ID NUMBER NOT NULL,
            CONSTRAINT PK_MASTER_USER_ROLES PRIMARY KEY (USER_ID, MODULE_ID)
        )
    ');

    create_table_if_missing('MASTER_USER_ROLES_LOC', '
        CREATE TABLE MASTER_USER_ROLES_LOC
        (
            USER_ID VARCHAR2(30) NOT NULL,
            IDDATA  VARCHAR2(20) NOT NULL,
            CONSTRAINT PK_MASTER_USER_ROLES_LOC PRIMARY KEY (USER_ID, IDDATA)
        )
    ');

    create_table_if_missing('MASTER_USER_ROLES_EST', '
        CREATE TABLE MASTER_USER_ROLES_EST
        (
            USER_ID   VARCHAR2(30) NOT NULL,
            ESTATE_ID NUMBER NOT NULL,
            CONSTRAINT PK_MASTER_USER_ROLES_EST PRIMARY KEY (USER_ID, ESTATE_ID)
        )
    ');

    SELECT ROLE_ID
      INTO l_user_role_id
      FROM MASTER_ROLES
     WHERE UPPER(ROLE_NAME) = 'USER';

    SELECT MODULE_ID
      INTO l_gl_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'GL';

    SELECT MODULE_ID
      INTO l_accounting_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'ACCOUNTING';

    seed_user_roles_for_module(l_gl_module_id, 'GL');
    seed_user_roles_for_module(l_accounting_module_id, 'ACCOUNTING');

    EXECUTE IMMEDIATE '
        INSERT INTO MASTER_USER_ROLES_LOC (USER_ID, IDDATA)
        SELECT DISTINCT d.USERID, d.IDDATA
          FROM MASTER_APPS_DETAIL d
         WHERE d.APPID = ''GL''
           AND d.USERID IS NOT NULL
           AND d.IDDATA IS NOT NULL
           AND TRIM(d.IDDATA) IS NOT NULL
           AND NOT EXISTS (
               SELECT 1
                 FROM MASTER_USER_ROLES_LOC loc
                WHERE loc.USER_ID = d.USERID
                  AND loc.IDDATA = d.IDDATA
           )';

    DBMS_OUTPUT.PUT_LINE('SEEDED MASTER_USER_ROLES_LOC from legacy GL: ' || SQL%ROWCOUNT);
END;
/
