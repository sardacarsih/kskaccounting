-- Check script for V20260413_004__standard_role_catalog_alignment.sql

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE require_role(p_role_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) = UPPER(p_role_name);

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20111, 'ROLE MISSING: ' || p_role_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ROLE: ' || p_role_name);
    END;

    PROCEDURE require_mapping(p_module_name IN VARCHAR2, p_role_name IN VARCHAR2, p_permission_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_ROLE_PERMISSIONS mrp
          JOIN MASTER_ROLES mr
            ON mr.ROLE_ID = mrp.ROLE_ID
          JOIN MASTER_PERMISSIONS mp
            ON mp.PERMISSION_ID = mrp.PERMISSION_ID
          JOIN MASTER_MODULES mm
            ON mm.MODULE_ID = mp.MODULE_ID
         WHERE mm.MODULE_NAME = p_module_name
           AND UPPER(mr.ROLE_NAME) = UPPER(p_role_name)
           AND mp.PERMISSION_NAME = p_permission_name;

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20112, 'ROLE MAPPING MISSING: ' || p_module_name || ' | ' || p_role_name || ' -> ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ROLE MAPPING: ' || p_module_name || ' | ' || p_role_name || ' -> ' || p_permission_name);
    END;
BEGIN
    require_role('ADMIN');
    require_role('STAFF');
    require_role('USER');
    require_role('VIEWER');
    require_role('CHECKER');
    require_role('APPROVER');
    require_role('MAKER');
    require_role('OPERATOR');
    require_role('SUPERVISOR');
    require_role('ADMINISTRATOR');
    require_role('SUPERADMIN');

    require_mapping('GL', 'VIEWER', 'JURNAL_VIEW');
    require_mapping('GL', 'MAKER', 'COA_ENTRY');
    require_mapping('GL', 'SUPERVISOR', 'SETTING_PERIOD');
    require_mapping('GL', 'ADMINISTRATOR', 'SETTING_DEVELOPER');
    require_mapping('GL', 'SUPERADMIN', 'AUDIT_TRAIL_VIEW');

    require_mapping('ACCOUNTING', 'VIEWER', 'JURNAL_VIEW');
    require_mapping('ACCOUNTING', 'MAKER', 'COA_ENTRY');
    require_mapping('ACCOUNTING', 'SUPERVISOR', 'SETTING_PERIOD');
    require_mapping('ACCOUNTING', 'ADMINISTRATOR', 'SETTING_DEVELOPER');
    require_mapping('ACCOUNTING', 'SUPERADMIN', 'AUDIT_TRAIL_VIEW');
END;
/
