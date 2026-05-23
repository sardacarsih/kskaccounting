-- Check script for V20260413_002__coa_reports_settings_rbac_permissions.sql

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
            RAISE_APPLICATION_ERROR(-20071, 'TABLE MISSING: ' || UPPER(p_table));
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
            RAISE_APPLICATION_ERROR(-20072, 'PERMISSION MISSING: ' || p_permission_name);
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
            RAISE_APPLICATION_ERROR(-20073, 'ROLE MAPPING MISSING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ROLE MAPPING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
    END;
BEGIN
    require_table('MASTER_MODULES');
    require_table('MASTER_PERMISSIONS');
    require_table('MASTER_ROLES');
    require_table('MASTER_ROLE_PERMISSIONS');

    require_permission('COA_VIEW');
    require_permission('COA_ENTRY');
    require_permission('COA_DELETE');
    require_permission('COA_IMPORT');
    require_permission('COA_EXPORT');
    require_permission('REPORT_VIEW');
    require_permission('REPORT_EXPORT');
    require_permission('REPORT_ESTATE_VIEW');
    require_permission('SETTING_PERIOD');
    require_permission('SETTING_COMPANY');
    require_permission('SETTING_PROFILE');
    require_permission('SETTING_RL');
    require_permission('SETTING_DEVELOPER');
    require_permission('AUDIT_TRAIL_VIEW');

    require_mapping_when_role_exists('VIEWER', 'COA_VIEW');
    require_mapping_when_role_exists('VIEWER', 'REPORT_VIEW');
    require_mapping_when_role_exists('MAKER', 'COA_ENTRY');
    require_mapping_when_role_exists('SUPERVISOR', 'SETTING_PERIOD');
    require_mapping_when_role_exists('ADMIN', 'SETTING_COMPANY');
    require_mapping_when_role_exists('ADMINISTRATOR', 'SETTING_DEVELOPER');
    require_mapping_when_role_exists('SUPERADMIN', 'AUDIT_TRAIL_VIEW');
END;
/
