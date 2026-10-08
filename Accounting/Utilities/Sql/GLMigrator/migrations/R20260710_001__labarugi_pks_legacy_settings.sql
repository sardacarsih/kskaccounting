-- Rollback: deactivate legacy PKS Laba Rugi metadata seeded by V20260710_001.
-- Package bodies are intentionally left in place because the selector columns remain backward-compatible.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

BEGIN
    UPDATE ACCT_REPORT_SECTION_ACCOUNT account
       SET account.IS_ACTIVE = 'N'
     WHERE account.JENIS_AKUNTING = 'PKS'
       AND account.SECTION_ID IN (
           SELECT section.SECTION_ID
             FROM ACCT_REPORT_SECTION section
            WHERE section.REPORT_CODE = 'LABARUGI'
              AND section.SECTION_CODE LIKE 'PKS\_%' ESCAPE '\'
       );

    UPDATE ACCT_REPORT_SECTION
       SET IS_ACTIVE = 'N'
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE LIKE 'PKS\_%' ESCAPE '\';

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('DEACTIVATED LEGACY PKS LABARUGI SETTINGS');
END;
/
