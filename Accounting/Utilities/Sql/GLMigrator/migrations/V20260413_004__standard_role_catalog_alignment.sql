-- Purpose: Normalize standard RBAC role names, create missing standard roles, and sync their GL/ACCOUNTING mappings.
-- Date: 2026-04-13

SET SERVEROUTPUT ON;

DECLARE
    v_role_table NUMBER := 0;
    v_module_table NUMBER := 0;
    v_perm_table NUMBER := 0;
    v_role_perm_table NUMBER := 0;
    v_gl_module_id NUMBER := 0;
    v_accounting_module_id NUMBER := 0;

    PROCEDURE ensure_role(p_role_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) = UPPER(p_role_name);

        IF v_count = 0 THEN
            INSERT INTO MASTER_ROLES (ROLE_ID, ROLE_NAME)
            VALUES ((SELECT NVL(MAX(ROLE_ID), 0) + 1 FROM MASTER_ROLES), p_role_name);
            DBMS_OUTPUT.PUT_LINE('REGISTERED ROLE: ' || p_role_name);
        ELSE
            UPDATE MASTER_ROLES
               SET ROLE_NAME = p_role_name
             WHERE UPPER(ROLE_NAME) = UPPER(p_role_name)
               AND ROLE_NAME <> p_role_name;

            IF SQL%ROWCOUNT > 0 THEN
                DBMS_OUTPUT.PUT_LINE('RENAMED ROLE TO CANONICAL: ' || p_role_name);
            END IF;
        END IF;
    END;

    PROCEDURE ensure_role_permission(
        p_module_name IN VARCHAR2,
        p_role_name IN VARCHAR2,
        p_permission_name IN VARCHAR2,
        p_can_create IN CHAR,
        p_can_read IN CHAR,
        p_can_update IN CHAR,
        p_can_delete IN CHAR
    ) IS
        v_role_id NUMBER := 0;
        v_permission_id NUMBER := 0;
    BEGIN
        SELECT ROLE_ID
          INTO v_role_id
          FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) = UPPER(p_role_name);

        SELECT mp.PERMISSION_ID
          INTO v_permission_id
          FROM MASTER_PERMISSIONS mp
          JOIN MASTER_MODULES mm
            ON mm.MODULE_ID = mp.MODULE_ID
         WHERE mm.MODULE_NAME = p_module_name
           AND mp.PERMISSION_NAME = p_permission_name;

        MERGE INTO MASTER_ROLE_PERMISSIONS target
        USING (
            SELECT
                v_role_id AS ROLE_ID,
                v_permission_id AS PERMISSION_ID,
                p_can_create AS CAN_CREATE,
                p_can_read AS CAN_READ,
                p_can_update AS CAN_UPDATE,
                p_can_delete AS CAN_DELETE
            FROM DUAL
        ) source
           ON (target.ROLE_ID = source.ROLE_ID AND target.PERMISSION_ID = source.PERMISSION_ID)
        WHEN MATCHED THEN
            UPDATE SET
                target.CAN_CREATE = source.CAN_CREATE,
                target.CAN_READ = source.CAN_READ,
                target.CAN_UPDATE = source.CAN_UPDATE,
                target.CAN_DELETE = source.CAN_DELETE
        WHEN NOT MATCHED THEN
            INSERT (ROLE_ID, PERMISSION_ID, CAN_CREATE, CAN_READ, CAN_UPDATE, CAN_DELETE)
            VALUES (source.ROLE_ID, source.PERMISSION_ID, source.CAN_CREATE, source.CAN_READ, source.CAN_UPDATE, source.CAN_DELETE);

        DBMS_OUTPUT.PUT_LINE('SYNCED ROLE MAPPING: ' || p_module_name || ' | ' || p_role_name || ' -> ' || p_permission_name);
    END;

    PROCEDURE sync_module_permissions(p_module_name IN VARCHAR2) IS
    BEGIN
        ensure_role_permission(p_module_name, 'VIEWER',        'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'VIEWER',        'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'CHECKER',       'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'CHECKER',       'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'APPROVER',      'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'APPROVER',      'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'MAKER',         'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'MAKER',         'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'USER',          'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'USER',          'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'USER',          'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'USER',          'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'OPERATOR',      'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'OPERATOR',      'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'STAFF',         'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'STAFF',         'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'SUPERVISOR',    'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'JURNAL_DELETE',      'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'COA_DELETE',         'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'SETTING_RL',         'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERVISOR',    'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'ADMIN',         'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'JURNAL_DELETE',      'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'ADMIN',         'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'COA_DELETE',         'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'ADMIN',         'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'ADMIN',         'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'ADMIN',         'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'SETTING_RL',         'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMIN',         'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'JURNAL_DELETE',      'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'COA_DELETE',         'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'SETTING_RL',         'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'ADMINISTRATOR', 'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');

        ensure_role_permission(p_module_name, 'SUPERADMIN',    'JURNAL_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'JURNAL_ENTRY',       'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'JURNAL_DELETE',      'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'JURNAL_IMPORT',      'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'JURNAL_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'COA_VIEW',           'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'COA_ENTRY',          'Y', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'COA_DELETE',         'N', 'Y', 'N', 'Y');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'COA_IMPORT',         'Y', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'COA_EXPORT',         'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'REPORT_VIEW',        'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'REPORT_EXPORT',      'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'REPORT_ESTATE_VIEW', 'N', 'Y', 'N', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'SETTING_PERIOD',     'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'SETTING_COMPANY',    'Y', 'Y', 'Y', 'Y');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'SETTING_PROFILE',    'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'SETTING_RL',         'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'SETTING_DEVELOPER',  'N', 'Y', 'Y', 'N');
        ensure_role_permission(p_module_name, 'SUPERADMIN',    'AUDIT_TRAIL_VIEW',   'N', 'Y', 'N', 'N');
    END;
BEGIN
    SELECT COUNT(1) INTO v_role_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLES';
    SELECT COUNT(1) INTO v_module_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_MODULES';
    SELECT COUNT(1) INTO v_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_PERMISSIONS';
    SELECT COUNT(1) INTO v_role_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLE_PERMISSIONS';

    IF v_role_table = 0 OR v_module_table = 0 OR v_perm_table = 0 OR v_role_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20101, 'MASTER_ROLES / MASTER_MODULES / MASTER_PERMISSIONS / MASTER_ROLE_PERMISSIONS wajib tersedia.');
    END IF;

    SELECT MODULE_ID INTO v_gl_module_id FROM MASTER_MODULES WHERE MODULE_NAME = 'GL';
    SELECT MODULE_ID INTO v_accounting_module_id FROM MASTER_MODULES WHERE MODULE_NAME = 'ACCOUNTING';

    ensure_role('ADMIN');
    ensure_role('STAFF');
    ensure_role('USER');
    ensure_role('VIEWER');
    ensure_role('CHECKER');
    ensure_role('APPROVER');
    ensure_role('MAKER');
    ensure_role('OPERATOR');
    ensure_role('SUPERVISOR');
    ensure_role('ADMINISTRATOR');
    ensure_role('SUPERADMIN');

    sync_module_permissions('GL');
    sync_module_permissions('ACCOUNTING');
END;
/
