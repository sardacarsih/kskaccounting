-- Purpose: Remove only the thirteen global PKS Laba Rugi mappings inserted by the repair.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DELETE FROM ACCT_REPORT_SECTION_ACCOUNT account
 WHERE account.JENIS_AKUNTING = 'PKS'
   AND account.IDDATA IS NULL
   AND EXISTS (
       SELECT 1
         FROM ACCT_REPORT_SECTION section
        WHERE section.SECTION_ID = account.SECTION_ID
          AND section.REPORT_CODE = 'LABARUGI'
          AND (
              (section.SECTION_CODE = 'PKS_P1' AND account.KODEACC_ROOT = 'GRP:11') OR
              (section.SECTION_CODE = 'PKS_HPP' AND account.KODEACC_ROOT = 'GRP:12') OR
              (section.SECTION_CODE = 'PKS_B1' AND account.KODEACC_ROOT IN (
                  '89.11007.000', '89.12007.000', '89.13000.000',
                  '89.21007.000', '89.22007.000', '89.23007.000')) OR
              (section.SECTION_CODE = 'PKS_B5' AND account.KODEACC_ROOT = '85.01007.000') OR
              (section.SECTION_CODE = 'PKS_P2' AND account.KODEACC_ROOT = 'GRP:16') OR
              (section.SECTION_CODE = 'PKS_B6' AND account.KODEACC_ROOT = 'GRP:17') OR
              (section.SECTION_CODE = 'PKS_B3' AND account.KODEACC_ROOT = 'GRP:15') OR
              (section.SECTION_CODE = 'PKS_PPH' AND account.KODEACC_ROOT = 'GRP:18')
          )
   );

COMMIT;

BEGIN
    DBMS_OUTPUT.PUT_LINE('REMOVED GLOBAL PKS LABA RUGI REPAIR MAPPINGS');
END;
/
