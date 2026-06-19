-- Purpose: Check RBAC user assignment compatibility tables.
-- Date: 2026-06-02

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE require_table(p_table IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tables
         WHERE table_name = UPPER(p_table);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20081, UPPER(p_table) || ' is missing');
        END IF;
    END;
BEGIN
    require_table('MASTER_USER_ROLES');
    require_table('MASTER_USER_ROLES_LOC');
    require_table('MASTER_USER_ROLES_EST');

    DBMS_OUTPUT.PUT_LINE('OK: RBAC user assignment compatibility tables are installed');
END;
/
