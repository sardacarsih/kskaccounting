-- Purpose: Seed centralized RBAC permissions and default role mappings for COA, reports, and settings.
-- Date: 2026-04-13

SET SERVEROUTPUT ON;

DECLARE
    v_module_table      NUMBER := 0;
    v_perm_table        NUMBER := 0;
    v_role_table        NUMBER := 0;
    v_role_perm_table   NUMBER := 0;
    v_module_id         NUMBER := 0;

    PROCEDURE ensure_module(
        p_module_name IN VARCHAR2,
        p_module_id OUT NUMBER
    ) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_MODULES
         WHERE MODULE_NAME = p_module_name;

        IF v_count = 0 THEN
            INSERT INTO MASTER_MODULES (MODULE_ID, MODULE_NAME)
            VALUES ((SELECT NVL(MAX(MODULE_ID), 0) + 1 FROM MASTER_MODULES), p_module_name);
            DBMS_OUTPUT.PUT_LINE('REGISTERED MODULE: ' || p_module_name);
        END IF;

        SELECT MODULE_ID
          INTO p_module_id
          FROM MASTER_MODULES
         WHERE MODULE_NAME = p_module_name;
    END;

    PROCEDURE ensure_permission(
        p_module_id       IN NUMBER,
        p_permission_name IN VARCHAR2,
        p_menu            IN VARCHAR2,
        p_description     IN VARCHAR2,
        p_urut1           IN NUMBER,
        p_urut2           IN NUMBER
    ) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_PERMISSIONS
         WHERE MODULE_ID = p_module_id
           AND PERMISSION_NAME = p_permission_name;

        IF v_count = 0 THEN
            INSERT INTO MASTER_PERMISSIONS
                (PERMISSION_ID, MODULE_ID, PERMISSION_NAME, MENU, DESCRIPTION, URUT1, URUT2)
            VALUES
                ((SELECT NVL(MAX(PERMISSION_ID), 0) + 1 FROM MASTER_PERMISSIONS),
                 p_module_id,
                 p_permission_name,
                 p_menu,
                 p_description,
                 p_urut1,
                 p_urut2);
            DBMS_OUTPUT.PUT_LINE('REGISTERED PERMISSION: ' || p_permission_name);
        ELSE
            UPDATE MASTER_PERMISSIONS
               SET MENU = p_menu,
                   DESCRIPTION = p_description,
                   URUT1 = p_urut1,
                   URUT2 = p_urut2
             WHERE MODULE_ID = p_module_id
               AND PERMISSION_NAME = p_permission_name;
            DBMS_OUTPUT.PUT_LINE('SYNCED PERMISSION: ' || p_permission_name);
        END IF;
    END;

    PROCEDURE ensure_role_permission(
        p_role_name       IN VARCHAR2,
        p_permission_name IN VARCHAR2,
        p_can_create      IN CHAR,
        p_can_read        IN CHAR,
        p_can_update      IN CHAR,
        p_can_delete      IN CHAR
    ) IS
        v_role_id       NUMBER := 0;
        v_permission_id NUMBER := 0;
    BEGIN
        SELECT ROLE_ID
          INTO v_role_id
          FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) = UPPER(p_role_name);

        SELECT MP.PERMISSION_ID
          INTO v_permission_id
          FROM MASTER_PERMISSIONS MP
          JOIN MASTER_MODULES MM
            ON MM.MODULE_ID = MP.MODULE_ID
         WHERE MM.MODULE_NAME = 'GL'
           AND MP.PERMISSION_NAME = p_permission_name;

        MERGE INTO MASTER_ROLE_PERMISSIONS target
        USING (
            SELECT v_role_id AS ROLE_ID,
                   v_permission_id AS PERMISSION_ID,
                   p_can_create AS CAN_CREATE,
                   p_can_read AS CAN_READ,
                   p_can_update AS CAN_UPDATE,
                   p_can_delete AS CAN_DELETE
              FROM DUAL
        ) source
           ON (target.ROLE_ID = source.ROLE_ID AND target.PERMISSION_ID = source.PERMISSION_ID)
        WHEN MATCHED THEN
            UPDATE SET target.CAN_CREATE = source.CAN_CREATE,
                       target.CAN_READ = source.CAN_READ,
                       target.CAN_UPDATE = source.CAN_UPDATE,
                       target.CAN_DELETE = source.CAN_DELETE
        WHEN NOT MATCHED THEN
            INSERT (ROLE_ID, PERMISSION_ID, CAN_CREATE, CAN_READ, CAN_UPDATE, CAN_DELETE)
            VALUES (source.ROLE_ID, source.PERMISSION_ID, source.CAN_CREATE, source.CAN_READ, source.CAN_UPDATE, source.CAN_DELETE);

        DBMS_OUTPUT.PUT_LINE('SYNCED ROLE PERMISSION: ' || UPPER(p_role_name) || ' -> ' || p_permission_name);
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED ROLE PERMISSION (missing role or permission): '
                || UPPER(p_role_name) || ' -> ' || p_permission_name);
    END;
BEGIN
    SELECT COUNT(1) INTO v_module_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_MODULES';
    SELECT COUNT(1) INTO v_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_PERMISSIONS';
    SELECT COUNT(1) INTO v_role_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLES';
    SELECT COUNT(1) INTO v_role_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLE_PERMISSIONS';

    IF v_module_table = 0 OR v_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20061, 'MASTER_MODULES / MASTER_PERMISSIONS wajib tersedia sebelum seed RBAC COA/reports/settings.');
    END IF;

    IF v_role_table = 0 OR v_role_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20062, 'MASTER_ROLES / MASTER_ROLE_PERMISSIONS wajib tersedia sebelum seed mapping RBAC COA/reports/settings.');
    END IF;

    ensure_module('GL', v_module_id);

    ensure_permission(v_module_id, 'COA_VIEW',           'COA Workspace',    'Open chart of account workspace and browse COA data',           110, 1);
    ensure_permission(v_module_id, 'COA_ENTRY',          'COA Entry',        'Create and update chart of account records',                   110, 2);
    ensure_permission(v_module_id, 'COA_DELETE',         'COA Delete',       'Delete chart of account records',                              110, 3);
    ensure_permission(v_module_id, 'COA_IMPORT',         'COA Import',       'Import chart of account from Excel',                           110, 4);
    ensure_permission(v_module_id, 'COA_EXPORT',         'COA Export',       'Export chart of account data',                                 110, 5);
    ensure_permission(v_module_id, 'REPORT_VIEW',        'Reports View',     'Open financial report workspace and preview reports',          120, 1);
    ensure_permission(v_module_id, 'REPORT_EXPORT',      'Reports Export',   'Export financial reports to Excel',                            120, 2);
    ensure_permission(v_module_id, 'REPORT_ESTATE_VIEW', 'Estate Reports',   'Open estate-specific reporting workspace',                     120, 3);
    ensure_permission(v_module_id, 'SETTING_PERIOD',     'Period Setting',   'Manage accounting periods',                                    130, 1);
    ensure_permission(v_module_id, 'SETTING_COMPANY',    'Company Setting',  'Manage company master, locations, and GL access bootstrap',    130, 2);
    ensure_permission(v_module_id, 'SETTING_PROFILE',    'Profile Setting',  'Manage active company profile and accounting type metadata',   130, 3);
    ensure_permission(v_module_id, 'SETTING_RL',         'RL Setting',       'Manage laba rugi setup and mapping defaults',                  130, 4);
    ensure_permission(v_module_id, 'SETTING_DEVELOPER',  'Developer Tools',  'Run privileged developer SQL tooling',                         130, 5);
    ensure_permission(v_module_id, 'AUDIT_TRAIL_VIEW',   'Audit Trail',      'Open jurnal audit trail workspace',                            130, 6);

    ensure_role_permission('VIEWER',   'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('VIEWER',   'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('VIEWER',   'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('VIEWER',   'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('VIEWER',   'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('CHECKER',  'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('CHECKER',  'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('CHECKER',  'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('CHECKER',  'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('CHECKER',  'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('APPROVER', 'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('APPROVER', 'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('APPROVER', 'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('APPROVER', 'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('APPROVER', 'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('MAKER',    'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',    'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('MAKER',    'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',    'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',    'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',    'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',    'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('USER',     'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('USER',     'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('USER',     'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('USER',     'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('USER',     'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('USER',     'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('USER',     'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('OPERATOR', 'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR', 'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('OPERATOR', 'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR', 'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR', 'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR', 'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR', 'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('STAFF',    'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',    'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('STAFF',    'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',    'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',    'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',    'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',    'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

    ensure_role_permission('SUPERVISOR', 'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERVISOR', 'COA_DELETE',         'N', 'Y', 'N', 'Y');
    ensure_role_permission('SUPERVISOR', 'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR', 'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('SUPERVISOR', 'SETTING_RL',         'N', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERVISOR', 'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

    ensure_role_permission('ADMIN',         'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('ADMIN',         'COA_DELETE',         'N', 'Y', 'N', 'Y');
    ensure_role_permission('ADMIN',         'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('ADMIN',         'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('ADMIN',         'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMIN',         'SETTING_RL',         'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMIN',         'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMIN',         'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

    ensure_role_permission('ADMINISTRATOR', 'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('ADMINISTRATOR', 'COA_DELETE',         'N', 'Y', 'N', 'Y');
    ensure_role_permission('ADMINISTRATOR', 'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('ADMINISTRATOR', 'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('ADMINISTRATOR', 'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMINISTRATOR', 'SETTING_RL',         'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMINISTRATOR', 'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
    ensure_role_permission('ADMINISTRATOR', 'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

    ensure_role_permission('SUPERADMIN',    'COA_VIEW',           'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERADMIN',    'COA_DELETE',         'N', 'Y', 'N', 'Y');
    ensure_role_permission('SUPERADMIN',    'COA_IMPORT',         'Y', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'COA_EXPORT',         'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'REPORT_VIEW',        'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('SUPERADMIN',    'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('SUPERADMIN',    'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERADMIN',    'SETTING_RL',         'N', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERADMIN',    'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERADMIN',    'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');
END;
/
