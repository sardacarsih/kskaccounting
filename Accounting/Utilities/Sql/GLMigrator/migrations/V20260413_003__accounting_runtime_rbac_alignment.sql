-- Purpose: Mirror GL RBAC permissions and default role mappings into ACCOUNTING so the runtime role-management UI reads the same RBAC model.
-- Date: 2026-04-13

SET SERVEROUTPUT ON;

DECLARE
    v_module_table NUMBER := 0;
    v_perm_table NUMBER := 0;
    v_role_table NUMBER := 0;
    v_role_perm_table NUMBER := 0;
    v_source_module_id NUMBER := 0;
    v_target_module_id NUMBER := 0;
    v_source_permission_count NUMBER := 0;

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
BEGIN
    SELECT COUNT(1) INTO v_module_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_MODULES';
    SELECT COUNT(1) INTO v_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_PERMISSIONS';
    SELECT COUNT(1) INTO v_role_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLES';
    SELECT COUNT(1) INTO v_role_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_ROLE_PERMISSIONS';

    IF v_module_table = 0 OR v_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20081, 'MASTER_MODULES / MASTER_PERMISSIONS wajib tersedia sebelum alignment RBAC ACCOUNTING.');
    END IF;

    IF v_role_table = 0 OR v_role_perm_table = 0 THEN
        RAISE_APPLICATION_ERROR(-20082, 'MASTER_ROLES / MASTER_ROLE_PERMISSIONS wajib tersedia sebelum alignment RBAC ACCOUNTING.');
    END IF;

    SELECT MODULE_ID
      INTO v_source_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'GL';

    ensure_module('ACCOUNTING', v_target_module_id);

    SELECT COUNT(1)
      INTO v_source_permission_count
      FROM MASTER_PERMISSIONS
     WHERE MODULE_ID = v_source_module_id
       AND PERMISSION_NAME IN (
           'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
           'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
           'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
           'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
           'AUDIT_TRAIL_VIEW'
       );

    IF v_source_permission_count <> 19 THEN
        RAISE_APPLICATION_ERROR(-20083, 'Source GL RBAC permission set tidak lengkap. Jalankan seed GL RBAC lebih dulu.');
    END IF;

    FOR source_permission IN (
        SELECT
            PERMISSION_NAME,
            MENU,
            DESCRIPTION,
            URUT1,
            URUT2
        FROM MASTER_PERMISSIONS
        WHERE MODULE_ID = v_source_module_id
          AND PERMISSION_NAME IN (
              'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
              'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
              'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
              'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
              'AUDIT_TRAIL_VIEW'
          )
        ORDER BY URUT1, URUT2, PERMISSION_NAME
    ) LOOP
        MERGE INTO MASTER_PERMISSIONS target
        USING (
            SELECT
                v_target_module_id AS MODULE_ID,
                source_permission.PERMISSION_NAME AS PERMISSION_NAME,
                source_permission.MENU AS MENU,
                source_permission.DESCRIPTION AS DESCRIPTION,
                source_permission.URUT1 AS URUT1,
                source_permission.URUT2 AS URUT2
            FROM DUAL
        ) source
           ON (target.MODULE_ID = source.MODULE_ID AND target.PERMISSION_NAME = source.PERMISSION_NAME)
        WHEN MATCHED THEN
            UPDATE SET
                target.MENU = source.MENU,
                target.DESCRIPTION = source.DESCRIPTION,
                target.URUT1 = source.URUT1,
                target.URUT2 = source.URUT2
        WHEN NOT MATCHED THEN
            INSERT (PERMISSION_ID, MODULE_ID, PERMISSION_NAME, MENU, DESCRIPTION, URUT1, URUT2)
            VALUES (
                (SELECT NVL(MAX(PERMISSION_ID), 0) + 1 FROM MASTER_PERMISSIONS),
                source.MODULE_ID,
                source.PERMISSION_NAME,
                source.MENU,
                source.DESCRIPTION,
                source.URUT1,
                source.URUT2
            );

        DBMS_OUTPUT.PUT_LINE('SYNCED ACCOUNTING PERMISSION: ' || source_permission.PERMISSION_NAME);
    END LOOP;

    FOR source_mapping IN (
        SELECT
            mrp.ROLE_ID,
            mp.PERMISSION_NAME,
            mrp.CAN_CREATE,
            mrp.CAN_READ,
            mrp.CAN_UPDATE,
            mrp.CAN_DELETE
        FROM MASTER_ROLE_PERMISSIONS mrp
        JOIN MASTER_PERMISSIONS mp
            ON mp.PERMISSION_ID = mrp.PERMISSION_ID
        WHERE mp.MODULE_ID = v_source_module_id
          AND mp.PERMISSION_NAME IN (
              'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
              'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
              'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
              'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
              'AUDIT_TRAIL_VIEW'
          )
    ) LOOP
        MERGE INTO MASTER_ROLE_PERMISSIONS target
        USING (
            SELECT
                source_mapping.ROLE_ID AS ROLE_ID,
                (
                    SELECT PERMISSION_ID
                    FROM MASTER_PERMISSIONS
                    WHERE MODULE_ID = v_target_module_id
                      AND PERMISSION_NAME = source_mapping.PERMISSION_NAME
                ) AS PERMISSION_ID,
                source_mapping.CAN_CREATE AS CAN_CREATE,
                source_mapping.CAN_READ AS CAN_READ,
                source_mapping.CAN_UPDATE AS CAN_UPDATE,
                source_mapping.CAN_DELETE AS CAN_DELETE
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

        DBMS_OUTPUT.PUT_LINE('SYNCED ACCOUNTING ROLE MAPPING: ROLE_ID=' || source_mapping.ROLE_ID || ' -> ' || source_mapping.PERMISSION_NAME);
    END LOOP;
END;
/
