-- Rollback: restore removed legacy roles and ACCOUNTING legacy ALL permission.

SET SERVEROUTPUT ON;

DECLARE
    v_accounting_module_id NUMBER := 0;
    v_finance_module_id NUMBER := 0;

    PROCEDURE ensure_role(p_role_id IN NUMBER, p_role_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_ROLES
         WHERE ROLE_ID = p_role_id
            OR UPPER(ROLE_NAME) = UPPER(p_role_name);

        IF v_count = 0 THEN
            INSERT INTO MASTER_ROLES (ROLE_ID, ROLE_NAME)
            VALUES (p_role_id, p_role_name);
            DBMS_OUTPUT.PUT_LINE('RESTORED ROLE: ' || p_role_name);
        END IF;
    END;

    PROCEDURE ensure_permission(
        p_permission_id IN NUMBER,
        p_module_id IN NUMBER,
        p_permission_name IN VARCHAR2,
        p_menu IN VARCHAR2,
        p_description IN VARCHAR2,
        p_urut1 IN NUMBER,
        p_urut2 IN NUMBER
    ) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM MASTER_PERMISSIONS
         WHERE MODULE_ID = p_module_id
           AND PERMISSION_NAME = p_permission_name;

        IF v_count = 0 THEN
            INSERT INTO MASTER_PERMISSIONS (PERMISSION_ID, MODULE_ID, PERMISSION_NAME, MENU, DESCRIPTION, URUT1, URUT2)
            VALUES (p_permission_id, p_module_id, p_permission_name, p_menu, p_description, p_urut1, p_urut2);
            DBMS_OUTPUT.PUT_LINE('RESTORED PERMISSION: ' || p_permission_name);
        END IF;
    END;

    PROCEDURE ensure_role_permission(
        p_role_name IN VARCHAR2,
        p_module_id IN NUMBER,
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

        SELECT PERMISSION_ID
          INTO v_permission_id
          FROM MASTER_PERMISSIONS
         WHERE MODULE_ID = p_module_id
           AND PERMISSION_NAME = p_permission_name;

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
            UPDATE SET
                target.CAN_CREATE = source.CAN_CREATE,
                target.CAN_READ = source.CAN_READ,
                target.CAN_UPDATE = source.CAN_UPDATE,
                target.CAN_DELETE = source.CAN_DELETE
        WHEN NOT MATCHED THEN
            INSERT (ROLE_ID, PERMISSION_ID, CAN_CREATE, CAN_READ, CAN_UPDATE, CAN_DELETE)
            VALUES (source.ROLE_ID, source.PERMISSION_ID, source.CAN_CREATE, source.CAN_READ, source.CAN_UPDATE, source.CAN_DELETE);
    END;
BEGIN
    SELECT MODULE_ID INTO v_accounting_module_id FROM MASTER_MODULES WHERE MODULE_NAME = 'ACCOUNTING';
    SELECT MODULE_ID INTO v_finance_module_id FROM MASTER_MODULES WHERE MODULE_NAME = 'FINANCE';

    ensure_role(2, 'Manager');
    ensure_role(3, 'Kabag');
    ensure_role(4, 'Asisten');
    ensure_role(7, 'Tamu');
    ensure_role(8, 'Audit');

    ensure_permission(30, v_accounting_module_id, 'ALL', 'Settings', 'ALL', 1, 1);

    ensure_role_permission('Asisten', v_finance_module_id, 'BKM',            'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'GAJI_STAFF',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'KODE_GL',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'KONVERSI_NIK',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'LAPBUL',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'LAPHAR',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'PERIODE',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'POT_KANTOR',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'POT_KOPERASI',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'PREMI',          'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'TUTUP_BULAN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'TUTUP_TAHUN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Asisten', v_finance_module_id, 'VERIFIKASI_BKM', 'Y', 'Y', 'Y', 'Y');

    ensure_role_permission('Kabag', v_finance_module_id, 'BKM',            'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'GAJI_STAFF',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'IMPORT_BKM',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'KODE_GL',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'KONVERSI_NIK',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'LAPBUL',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'LAPHAR',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'PERIODE',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'POT_KANTOR',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'POT_KOPERASI',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'PREMI',          'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'TUTUP_BULAN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'TUTUP_TAHUN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'USER_AKSES',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Kabag', v_finance_module_id, 'VERIFIKASI_BKM', 'Y', 'Y', 'Y', 'Y');

    ensure_role_permission('Manager', v_finance_module_id, 'BASIS',          'N', 'Y', 'N', 'N');
    ensure_role_permission('Manager', v_finance_module_id, 'BKM',            'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'GAJI_STAFF',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'IMPORT_BKM',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'IMPORT_MASTER',  'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'KODE_GL',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'KONVERSI_NIK',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'LAPBUL',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'LAPHAR',         'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'PERIODE',        'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'POT_KANTOR',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'POT_KOPERASI',   'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'PREMI',          'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'TUTUP_BULAN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'TUTUP_TAHUN',    'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'UNLOK_BKM',      'N', 'Y', 'N', 'N');
    ensure_role_permission('Manager', v_finance_module_id, 'USER_AKSES',     'Y', 'Y', 'Y', 'Y');
    ensure_role_permission('Manager', v_finance_module_id, 'VERIFIKASI_BKM', 'Y', 'Y', 'Y', 'Y');
END;
/
