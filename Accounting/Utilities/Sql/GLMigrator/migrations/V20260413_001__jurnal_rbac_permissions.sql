-- Purpose: Seed centralized RBAC permissions and default role mappings for Jurnal workspace/actions.
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
        RAISE_APPLICATION_ERROR(-20041, 'MASTER_MODULES / MASTER_PERMISSIONS wajib tersedia sebelum seed RBAC jurnal.');
    END IF;

    IF v_role_table = 0 OR v_role_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20042, 'MASTER_ROLES / MASTER_ROLE_PERMISSIONS wajib tersedia sebelum seed mapping RBAC jurnal.');
    END IF;

    ensure_module('GL', v_module_id);

    ensure_permission(v_module_id, 'JURNAL_VIEW',   'Jurnal Workspace', 'Open jurnal workspace and browse daftar jurnal', 100, 1);
    ensure_permission(v_module_id, 'JURNAL_ENTRY',  'Jurnal Entry',     'Create and update jurnal entries',                 100, 2);
    ensure_permission(v_module_id, 'JURNAL_DELETE', 'Jurnal Delete',    'Delete jurnal entries',                            100, 3);
    ensure_permission(v_module_id, 'JURNAL_IMPORT', 'Jurnal Import',    'Import jurnal from Excel or source modules',       100, 4);
    ensure_permission(v_module_id, 'JURNAL_EXPORT', 'Jurnal Export',    'Export jurnal to Excel',                           100, 5);

    ensure_role_permission('VIEWER',        'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('VIEWER',        'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('CHECKER',       'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('CHECKER',       'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('APPROVER',      'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('APPROVER',      'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('MAKER',         'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',         'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('MAKER',         'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('MAKER',         'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('USER',          'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('USER',          'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('USER',          'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('USER',          'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('OPERATOR',      'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR',      'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('OPERATOR',      'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('OPERATOR',      'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('STAFF',         'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',         'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('STAFF',         'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('STAFF',         'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('SUPERVISOR',    'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR',    'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERVISOR',    'JURNAL_DELETE', 'N', 'Y', 'N', 'Y');
    ensure_role_permission('SUPERVISOR',    'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('SUPERVISOR',    'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('ADMIN',         'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('ADMIN',         'JURNAL_DELETE', 'N', 'Y', 'N', 'Y');
    ensure_role_permission('ADMIN',         'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('ADMIN',         'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('ADMINISTRATOR', 'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('ADMINISTRATOR', 'JURNAL_DELETE', 'N', 'Y', 'N', 'Y');
    ensure_role_permission('ADMINISTRATOR', 'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('ADMINISTRATOR', 'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');

    ensure_role_permission('SUPERADMIN',    'JURNAL_VIEW',   'N', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'JURNAL_ENTRY',  'Y', 'Y', 'Y', 'N');
    ensure_role_permission('SUPERADMIN',    'JURNAL_DELETE', 'N', 'Y', 'N', 'Y');
    ensure_role_permission('SUPERADMIN',    'JURNAL_IMPORT', 'Y', 'Y', 'N', 'N');
    ensure_role_permission('SUPERADMIN',    'JURNAL_EXPORT', 'N', 'Y', 'N', 'N');
END;
/
