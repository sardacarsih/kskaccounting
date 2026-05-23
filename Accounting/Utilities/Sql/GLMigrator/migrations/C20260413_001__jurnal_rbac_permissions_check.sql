-- Check script for V20260413_001__jurnal_rbac_permissions.sql

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE require_table(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20051, 'TABLE MISSING: ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE require_permission(p_permission_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_PERMISSIONS MP
          JOIN MASTER_MODULES MM
            ON MM.MODULE_ID = MP.MODULE_ID
         WHERE MM.MODULE_NAME = 'GL'
           AND MP.PERMISSION_NAME = p_permission_name;

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20052, 'PERMISSION MISSING: ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK PERMISSION: ' || p_permission_name);
    END;

    PROCEDURE require_mapping_when_role_exists(
        p_role_name       IN VARCHAR2,
        p_permission_name IN VARCHAR2
    ) IS
        v_role_count    NUMBER := 0;
        v_mapping_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_role_count
          FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) = UPPER(p_role_name);

        IF v_role_count = 0 THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED ROLE CHECK (role missing): ' || UPPER(p_role_name));
            RETURN;
        END IF;

        SELECT COUNT(1)
          INTO v_mapping_count
          FROM MASTER_ROLE_PERMISSIONS MRP
          JOIN MASTER_ROLES MR
            ON MR.ROLE_ID = MRP.ROLE_ID
          JOIN MASTER_PERMISSIONS MP
            ON MP.PERMISSION_ID = MRP.PERMISSION_ID
          JOIN MASTER_MODULES MM
            ON MM.MODULE_ID = MP.MODULE_ID
         WHERE UPPER(MR.ROLE_NAME) = UPPER(p_role_name)
           AND MM.MODULE_NAME = 'GL'
           AND MP.PERMISSION_NAME = p_permission_name;

        IF v_mapping_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20053, 'ROLE MAPPING MISSING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ROLE MAPPING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
    END;
BEGIN
    require_table('MASTER_MODULES');
    require_table('MASTER_PERMISSIONS');
    require_table('MASTER_ROLES');
    require_table('MASTER_ROLE_PERMISSIONS');

    require_permission('JURNAL_VIEW');
    require_permission('JURNAL_ENTRY');
    require_permission('JURNAL_DELETE');
    require_permission('JURNAL_IMPORT');
    require_permission('JURNAL_EXPORT');

    require_mapping_when_role_exists('VIEWER', 'JURNAL_VIEW');
    require_mapping_when_role_exists('VIEWER', 'JURNAL_EXPORT');
    require_mapping_when_role_exists('MAKER', 'JURNAL_ENTRY');
    require_mapping_when_role_exists('SUPERVISOR', 'JURNAL_DELETE');
    require_mapping_when_role_exists('ADMIN', 'JURNAL_DELETE');
    require_mapping_when_role_exists('ADMINISTRATOR', 'JURNAL_IMPORT');
    require_mapping_when_role_exists('SUPERADMIN', 'JURNAL_EXPORT');
END;
/
