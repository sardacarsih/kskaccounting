-- Purpose: Diagnose whether the database RBAC data matches the runtime module used by the role-management UI.
-- Date: 2026-04-13
--
-- Default runtime assumptions in code:
--   - LoginInfo.MODULE defaults to ACCOUNTING
--   - April 13 RBAC seed migrations register new granular permissions under GL
--
-- This script is read-only. Run it in SQL*Plus or SQLcl.

SET SERVEROUTPUT ON;
SET PAGESIZE 200;
SET LINESIZE 220;
SET VERIFY OFF;

DEFINE P_RUNTIME_MODULE = 'ACCOUNTING';
DEFINE P_RBAC_MODULE = 'GL';
DEFINE P_ROLE_NAME = 'ADMIN';
DEFINE P_USER_1 = 'dharyadi';
DEFINE P_USER_2 = 'samsudin';
DEFINE P_USER_3 = 'stevanus';

PROMPT
PROMPT === RBAC Runtime Alignment Check ===
PROMPT Runtime module  : &P_RUNTIME_MODULE
PROMPT RBAC seed module: &P_RBAC_MODULE
PROMPT Focus role      : &P_ROLE_NAME
PROMPT

PROMPT === 1. Required Tables ===
SELECT TABLE_NAME
FROM USER_TABLES
WHERE TABLE_NAME IN (
    'MASTER_MODULES',
    'MASTER_PERMISSIONS',
    'MASTER_ROLES',
    'MASTER_ROLE_PERMISSIONS',
    'MASTER_USER_ROLES',
    'MASTER_LOGIN'
)
ORDER BY TABLE_NAME;

PROMPT
PROMPT === 2. Module Registry ===
COLUMN MODULE_NAME FORMAT A20
SELECT MODULE_ID, MODULE_NAME
FROM MASTER_MODULES
WHERE MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
ORDER BY MODULE_ID;

PROMPT
PROMPT === 3. Permission Count By Module ===
WITH target_modules AS (
    SELECT MODULE_ID, MODULE_NAME
    FROM MASTER_MODULES
    WHERE MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
)
SELECT
    tm.MODULE_NAME,
    COUNT(mp.PERMISSION_ID) AS TOTAL_PERMISSIONS
FROM target_modules tm
LEFT JOIN MASTER_PERMISSIONS mp
    ON mp.MODULE_ID = tm.MODULE_ID
GROUP BY tm.MODULE_NAME
ORDER BY tm.MODULE_NAME;

PROMPT
PROMPT === 4. Granular RBAC Permission Count By Module ===
WITH expected_permissions AS (
    SELECT 'JURNAL_VIEW' AS PERMISSION_NAME FROM DUAL UNION ALL
    SELECT 'JURNAL_ENTRY' FROM DUAL UNION ALL
    SELECT 'JURNAL_DELETE' FROM DUAL UNION ALL
    SELECT 'JURNAL_IMPORT' FROM DUAL UNION ALL
    SELECT 'JURNAL_EXPORT' FROM DUAL UNION ALL
    SELECT 'COA_VIEW' FROM DUAL UNION ALL
    SELECT 'COA_ENTRY' FROM DUAL UNION ALL
    SELECT 'COA_DELETE' FROM DUAL UNION ALL
    SELECT 'COA_IMPORT' FROM DUAL UNION ALL
    SELECT 'COA_EXPORT' FROM DUAL UNION ALL
    SELECT 'REPORT_VIEW' FROM DUAL UNION ALL
    SELECT 'REPORT_EXPORT' FROM DUAL UNION ALL
    SELECT 'REPORT_ESTATE_VIEW' FROM DUAL UNION ALL
    SELECT 'SETTING_PERIOD' FROM DUAL UNION ALL
    SELECT 'SETTING_COMPANY' FROM DUAL UNION ALL
    SELECT 'SETTING_PROFILE' FROM DUAL UNION ALL
    SELECT 'SETTING_RL' FROM DUAL UNION ALL
    SELECT 'SETTING_DEVELOPER' FROM DUAL UNION ALL
    SELECT 'AUDIT_TRAIL_VIEW' FROM DUAL
)
SELECT
    mm.MODULE_NAME,
    COUNT(*) AS GRANULAR_PERMISSION_COUNT
FROM MASTER_PERMISSIONS mp
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mp.MODULE_ID
JOIN expected_permissions ep
    ON ep.PERMISSION_NAME = mp.PERMISSION_NAME
WHERE mm.MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
GROUP BY mm.MODULE_NAME
ORDER BY mm.MODULE_NAME;

PROMPT
PROMPT === 5. Legacy Permission Check (ALL / USER_AKSES) ===
COLUMN PERMISSION_NAME FORMAT A24
COLUMN MENU FORMAT A24
SELECT
    mm.MODULE_NAME,
    mp.PERMISSION_ID,
    mp.PERMISSION_NAME,
    mp.MENU
FROM MASTER_PERMISSIONS mp
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mp.MODULE_ID
WHERE mm.MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
  AND mp.PERMISSION_NAME IN ('ALL', 'USER_AKSES')
ORDER BY mm.MODULE_NAME, mp.PERMISSION_NAME;

PROMPT
PROMPT === 6. Full Permission List For Runtime / RBAC Modules ===
COLUMN DESCRIPTION FORMAT A70
SELECT
    mm.MODULE_NAME,
    mp.PERMISSION_ID,
    mp.PERMISSION_NAME,
    mp.MENU,
    mp.DESCRIPTION,
    mp.URUT1,
    mp.URUT2
FROM MASTER_PERMISSIONS mp
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mp.MODULE_ID
WHERE mm.MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
ORDER BY mm.MODULE_NAME, mp.URUT1, mp.URUT2, mp.PERMISSION_NAME;

PROMPT
PROMPT === 7. Role Mapping For Focus Role Across Runtime / RBAC Modules ===
COLUMN ROLE_NAME FORMAT A20
SELECT
    mm.MODULE_NAME,
    mr.ROLE_NAME,
    mp.PERMISSION_NAME,
    mrp.CAN_CREATE,
    mrp.CAN_READ,
    mrp.CAN_UPDATE,
    mrp.CAN_DELETE
FROM MASTER_ROLE_PERMISSIONS mrp
JOIN MASTER_ROLES mr
    ON mr.ROLE_ID = mrp.ROLE_ID
JOIN MASTER_PERMISSIONS mp
    ON mp.PERMISSION_ID = mrp.PERMISSION_ID
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mp.MODULE_ID
WHERE UPPER(mr.ROLE_NAME) = UPPER('&P_ROLE_NAME')
  AND mm.MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
ORDER BY mm.MODULE_NAME, mp.URUT1, mp.URUT2, mp.PERMISSION_NAME;

PROMPT
PROMPT === 8. Runtime Role Summary Basis ===
SELECT
    mr.ROLE_ID,
    mr.ROLE_NAME,
    SUM(CASE WHEN mur.MODULE_ID = (
            SELECT MODULE_ID
            FROM MASTER_MODULES
            WHERE MODULE_NAME = '&P_RUNTIME_MODULE'
        ) THEN 1 ELSE 0 END) AS RUNTIME_USER_COUNT,
    CASE WHEN mr.ROLE_ID = 1 THEN 1 ELSE 0 END AS IS_PROTECTED,
    CASE
        WHEN UPPER(mr.ROLE_NAME) IN (
            'VIEWER','CHECKER','APPROVER','MAKER','USER',
            'OPERATOR','STAFF','SUPERVISOR','ADMIN',
            'ADMINISTRATOR','SUPERADMIN'
        ) THEN 1
        ELSE 0
    END AS IS_SYSTEM_ROLE
FROM MASTER_ROLES mr
LEFT JOIN MASTER_USER_ROLES mur
    ON mur.ROLE_ID = mr.ROLE_ID
GROUP BY mr.ROLE_ID, mr.ROLE_NAME
ORDER BY mr.ROLE_NAME;

PROMPT
PROMPT === 9. User Assignments Across Runtime / RBAC Modules ===
COLUMN USER_ID FORMAT A20
SELECT
    mm.MODULE_NAME,
    mur.USER_ID,
    mr.ROLE_ID,
    mr.ROLE_NAME
FROM MASTER_USER_ROLES mur
JOIN MASTER_ROLES mr
    ON mr.ROLE_ID = mur.ROLE_ID
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mur.MODULE_ID
WHERE mm.MODULE_NAME IN ('&P_RUNTIME_MODULE', '&P_RBAC_MODULE')
ORDER BY mm.MODULE_NAME, mr.ROLE_NAME, mur.USER_ID;

PROMPT
PROMPT === 10. Screenshot Users Across Runtime / RBAC Modules ===
SELECT
    mm.MODULE_NAME,
    mur.USER_ID,
    mr.ROLE_NAME
FROM MASTER_USER_ROLES mur
JOIN MASTER_ROLES mr
    ON mr.ROLE_ID = mur.ROLE_ID
JOIN MASTER_MODULES mm
    ON mm.MODULE_ID = mur.MODULE_ID
WHERE LOWER(mur.USER_ID) IN (
    LOWER('&P_USER_1'),
    LOWER('&P_USER_2'),
    LOWER('&P_USER_3')
)
ORDER BY mm.MODULE_NAME, mur.USER_ID;

PROMPT
PROMPT === 11. Diagnostic Summary ===
DECLARE
    v_runtime_module_id NUMBER;
    v_rbac_module_id NUMBER;
    v_runtime_granular_count NUMBER := 0;
    v_rbac_granular_count NUMBER := 0;
    v_runtime_legacy_all_count NUMBER := 0;
    v_runtime_user_akses_count NUMBER := 0;
    v_runtime_focus_role_mapping_count NUMBER := 0;
    v_rbac_focus_role_mapping_count NUMBER := 0;
BEGIN
    BEGIN
        SELECT MODULE_ID
        INTO v_runtime_module_id
        FROM MASTER_MODULES
        WHERE MODULE_NAME = '&P_RUNTIME_MODULE';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_runtime_module_id := NULL;
    END;

    BEGIN
        SELECT MODULE_ID
        INTO v_rbac_module_id
        FROM MASTER_MODULES
        WHERE MODULE_NAME = '&P_RBAC_MODULE';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            v_rbac_module_id := NULL;
    END;

    IF v_runtime_module_id IS NOT NULL THEN
        SELECT COUNT(*)
        INTO v_runtime_granular_count
        FROM MASTER_PERMISSIONS
        WHERE MODULE_ID = v_runtime_module_id
          AND PERMISSION_NAME IN (
              'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
              'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
              'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
              'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
              'AUDIT_TRAIL_VIEW'
          );

        SELECT COUNT(*)
        INTO v_runtime_legacy_all_count
        FROM MASTER_PERMISSIONS
        WHERE MODULE_ID = v_runtime_module_id
          AND PERMISSION_NAME = 'ALL';

        SELECT COUNT(*)
        INTO v_runtime_user_akses_count
        FROM MASTER_PERMISSIONS
        WHERE MODULE_ID = v_runtime_module_id
          AND PERMISSION_NAME = 'USER_AKSES';

        SELECT COUNT(*)
        INTO v_runtime_focus_role_mapping_count
        FROM MASTER_ROLE_PERMISSIONS mrp
        JOIN MASTER_ROLES mr
            ON mr.ROLE_ID = mrp.ROLE_ID
        JOIN MASTER_PERMISSIONS mp
            ON mp.PERMISSION_ID = mrp.PERMISSION_ID
        WHERE mp.MODULE_ID = v_runtime_module_id
          AND UPPER(mr.ROLE_NAME) = UPPER('&P_ROLE_NAME');
    END IF;

    IF v_rbac_module_id IS NOT NULL THEN
        SELECT COUNT(*)
        INTO v_rbac_granular_count
        FROM MASTER_PERMISSIONS
        WHERE MODULE_ID = v_rbac_module_id
          AND PERMISSION_NAME IN (
              'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
              'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
              'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
              'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
              'AUDIT_TRAIL_VIEW'
          );

        SELECT COUNT(*)
        INTO v_rbac_focus_role_mapping_count
        FROM MASTER_ROLE_PERMISSIONS mrp
        JOIN MASTER_ROLES mr
            ON mr.ROLE_ID = mrp.ROLE_ID
        JOIN MASTER_PERMISSIONS mp
            ON mp.PERMISSION_ID = mrp.PERMISSION_ID
        WHERE mp.MODULE_ID = v_rbac_module_id
          AND UPPER(mr.ROLE_NAME) = UPPER('&P_ROLE_NAME');
    END IF;

    DBMS_OUTPUT.PUT_LINE('Runtime module id         : ' || COALESCE(TO_CHAR(v_runtime_module_id), '<missing>'));
    DBMS_OUTPUT.PUT_LINE('RBAC seed module id       : ' || COALESCE(TO_CHAR(v_rbac_module_id), '<missing>'));
    DBMS_OUTPUT.PUT_LINE('Runtime granular count    : ' || v_runtime_granular_count);
    DBMS_OUTPUT.PUT_LINE('RBAC seed granular count  : ' || v_rbac_granular_count);
    DBMS_OUTPUT.PUT_LINE('Runtime ALL count         : ' || v_runtime_legacy_all_count);
    DBMS_OUTPUT.PUT_LINE('Runtime USER_AKSES count  : ' || v_runtime_user_akses_count);
    DBMS_OUTPUT.PUT_LINE('Runtime role mappings     : ' || v_runtime_focus_role_mapping_count);
    DBMS_OUTPUT.PUT_LINE('RBAC seed role mappings   : ' || v_rbac_focus_role_mapping_count);

    IF v_runtime_module_id IS NULL THEN
        DBMS_OUTPUT.PUT_LINE('RESULT: runtime module is missing from MASTER_MODULES.');
    ELSIF v_rbac_module_id IS NULL THEN
        DBMS_OUTPUT.PUT_LINE('RESULT: RBAC seed module is missing from MASTER_MODULES.');
    ELSIF v_runtime_granular_count = 0 AND v_rbac_granular_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('RESULT: mismatch detected. Granular RBAC exists in '
            || '&P_RBAC_MODULE' || ' but runtime UI reads ' || '&P_RUNTIME_MODULE' || '.');
    ELSIF v_runtime_granular_count > 0 AND v_runtime_focus_role_mapping_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('RESULT: runtime module has granular permissions, but focus role has no mappings there.');
    ELSIF v_runtime_legacy_all_count > 0 AND v_runtime_granular_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('RESULT: runtime module still looks legacy-only (ALL present, granular RBAC absent).');
    ELSE
        DBMS_OUTPUT.PUT_LINE('RESULT: runtime module appears aligned enough for deeper manual review.');
    END IF;
END;
/
