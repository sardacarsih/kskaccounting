-- Purpose: Deactivate orphan LABARUGI root mappings left from pre-formatted COA defaults.
BEGIN
    UPDATE ACCT_REPORT_SECTION_ACCOUNT account
       SET account.IS_ACTIVE = 'N'
     WHERE account.KODEACC_ROOT IN ('4', '5', '6', '7', '8', '80', '81')
       AND EXISTS (
           SELECT 1
             FROM ACCT_REPORT_SECTION section
            WHERE section.SECTION_ID = account.SECTION_ID
              AND section.REPORT_CODE = 'LABARUGI'
       );

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('DEACTIVATED ORPHAN LABARUGI ROOT MAPPINGS');
END;
/