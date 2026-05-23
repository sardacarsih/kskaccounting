-- Check script for V20260413_003__accounting_runtime_rbac_alignment.sql

SET SERVEROUTPUT ON;

DECLARE
    v_accounting_module_id NUMBER := 0;
    v_permission_count NUMBER := 0;

    PROCEDURE require_table(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20091, 'TABLE MISSING: ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE require_permission(p_permission_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_PERMISSIONS mp
          JOIN MASTER_MODULES mm
            ON mm.MODULE_ID = mp.MODULE_ID
         WHERE mm.MODULE_NAME = 'ACCOUNTING'
           AND mp.PERMISSION_NAME = p_permission_name;

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20092, 'ACCOUNTING PERMISSION MISSING: ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ACCOUNTING PERMISSION: ' || p_permission_name);
    END;

    PROCEDURE require_mapping_when_role_exists(
        p_role_name IN VARCHAR2,
        p_permission_name IN VARCHAR2
    ) IS
        v_role_count NUMBER := 0;
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
          FROM MASTER_ROLE_PERMISSIONS mrp
          JOIN MASTER_ROLES mr
            ON mr.ROLE_ID = mrp.ROLE_ID
          JOIN MASTER_PERMISSIONS mp
            ON mp.PERMISSION_ID = mrp.PERMISSION_ID
          JOIN MASTER_MODULES mm
            ON mm.MODULE_ID = mp.MODULE_ID
         WHERE UPPER(mr.ROLE_NAME) = UPPER(p_role_name)
           AND mm.MODULE_NAME = 'ACCOUNTING'
           AND mp.PERMISSION_NAME = p_permission_name;

        IF v_mapping_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20093, 'ACCOUNTING ROLE MAPPING MISSING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK ACCOUNTING ROLE MAPPING: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
    END;
BEGIN
    require_table('MASTER_MODULES');
    require_table('MASTER_PERMISSIONS');
    require_table('MASTER_ROLES');
    require_table('MASTER_ROLE_PERMISSIONS');

    SELECT MODULE_ID
      INTO v_accounting_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'ACCOUNTING';

    SELECT COUNT(1)
      INTO v_permission_count
      FROM MASTER_PERMISSIONS
     WHERE MODULE_ID = v_accounting_module_id
       AND PERMISSION_NAME IN (
           'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
           'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
           'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
           'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
           'AUDIT_TRAIL_VIEW'
       );

    IF v_permission_count <> 19 THEN
        RAISE_APPLICATION_ERROR(-20094, 'ACCOUNTING granular RBAC permission count expected 19 but was ' || v_permission_count);
    END IF;

    require_permission('JURNAL_VIEW');
    require_permission('JURNAL_ENTRY');
    require_permission('JURNAL_DELETE');
    require_permission('JURNAL_IMPORT');
    require_permission('JURNAL_EXPORT');
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

    require_mapping_when_role_exists('VIEWER', 'JURNAL_VIEW');
    require_mapping_when_role_exists('MAKER', 'JURNAL_ENTRY');
    require_mapping_when_role_exists('SUPERVISOR', 'SETTING_PERIOD');
    require_mapping_when_role_exists('ADMIN', 'SETTING_COMPANY');
    require_mapping_when_role_exists('ADMIN', 'AUDIT_TRAIL_VIEW');
END;
/
