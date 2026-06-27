-- Rollback: remove the full Laba Rugi sections added in V20260627_011 and restore
-- BIAYA TM to its original DISPLAY_ORDER. Section-account mappings (if any were added via
-- the Setting RL form) are deleted first to satisfy the FK.

BEGIN
    DELETE FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE SECTION_ID IN (
        SELECT SECTION_ID
          FROM ACCT_REPORT_SECTION
         WHERE REPORT_CODE = 'LABARUGI'
           AND SECTION_CODE IN ('BIAYA_BUNGA', 'PPH_BADAN', 'LAIN2_ANAK')
     );

    DELETE FROM ACCT_REPORT_SECTION
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE IN ('BIAYA_BUNGA', 'PPH_BADAN', 'LAIN2_ANAK');

    UPDATE ACCT_REPORT_SECTION
       SET DISPLAY_ORDER = 60
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE = 'BIAYA_TM';

    COMMIT;

    DBMS_OUTPUT.PUT_LINE('REMOVED FULL LABARUGI SECTIONS');
END;
/
