-- Rollback: remove standard role mappings created by the standard role catalog alignment.
-- Role rows are intentionally preserved to avoid deleting roles that may already be referenced elsewhere.

SET SERVEROUTPUT ON;

BEGIN
    DELETE FROM MASTER_ROLE_PERMISSIONS
     WHERE ROLE_ID IN (
         SELECT ROLE_ID
         FROM MASTER_ROLES
         WHERE UPPER(ROLE_NAME) IN (
             'VIEWER','CHECKER','APPROVER','MAKER','OPERATOR',
             'SUPERVISOR','ADMINISTRATOR','SUPERADMIN'
         )
     )
       AND PERMISSION_ID IN (
           SELECT mp.PERMISSION_ID
           FROM MASTER_PERMISSIONS mp
           JOIN MASTER_MODULES mm
             ON mm.MODULE_ID = mp.MODULE_ID
          WHERE mm.MODULE_NAME IN ('GL', 'ACCOUNTING')
            AND mp.PERMISSION_NAME IN (
                'JURNAL_VIEW','JURNAL_ENTRY','JURNAL_DELETE','JURNAL_IMPORT','JURNAL_EXPORT',
                'COA_VIEW','COA_ENTRY','COA_DELETE','COA_IMPORT','COA_EXPORT',
                'REPORT_VIEW','REPORT_EXPORT','REPORT_ESTATE_VIEW',
                'SETTING_PERIOD','SETTING_COMPANY','SETTING_PROFILE','SETTING_RL','SETTING_DEVELOPER',
                'AUDIT_TRAIL_VIEW'
            )
       );

    DBMS_OUTPUT.PUT_LINE('REMOVED STANDARD ROLE MAPPINGS: ' || SQL%ROWCOUNT);
END;
/
