-- Rollback: Remove ACCOUNTING-side RBAC permissions and mappings mirrored from GL.

SET SERVEROUTPUT ON;

DECLARE
    v_target_module_id NUMBER := 0;
    v_removed_mappings NUMBER := 0;
    v_removed_permissions NUMBER := 0;
BEGIN
    SELECT COUNT(1)
      INTO v_target_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'ACCOUNTING';

    IF v_target_module_id = 0 THEN
        DBMS_OUTPUT.PUT_LINE('ACCOUNTING module not found. Nothing to rollback.');
        RETURN;
    END IF;

    SELECT MODULE_ID
      INTO v_target_module_id
      FROM MASTER_MODULES
     WHERE MODULE_NAME = 'ACCOUNTING';

    DELETE FROM MASTER_ROLE_PERMISSIONS
     WHERE PERMISSION_ID IN (
         SELECT PERMISSION_ID
         FROM MASTER_PERMISSIONS
         WHERE MODULE_ID = v_target_module_id
           AND PERMISSION_NAME IN (
               'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
               'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
               'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
               'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
               'AUDIT_TRAIL_VIEW'
           )
     );

    v_removed_mappings := SQL%ROWCOUNT;

    DELETE FROM MASTER_PERMISSIONS
     WHERE MODULE_ID = v_target_module_id
       AND PERMISSION_NAME IN (
           'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
           'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
           'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
           'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
           'AUDIT_TRAIL_VIEW'
       );

    v_removed_permissions := SQL%ROWCOUNT;

    DBMS_OUTPUT.PUT_LINE('REMOVED ACCOUNTING ROLE MAPPINGS: ' || v_removed_mappings);
    DBMS_OUTPUT.PUT_LINE('REMOVED ACCOUNTING PERMISSIONS : ' || v_removed_permissions);
END;
/
