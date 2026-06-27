-- Purpose: Roll back corrected LABARUGI default COA roots.

BEGIN
    UPDATE ACCT_REPORT_SECTION_ACCOUNT account
       SET account.IS_ACTIVE = 'N'
     WHERE account.JENIS_AKUNTING = '*'
       AND account.IDDATA IS NULL
       AND account.TAHUN IS NULL
       AND account.KODEACC_ROOT IN ('60.00000.000', '61.00000.000', '70.00000.000', '91.00000.000', '92.00000.000', '80.10000.000', '80.20000.000', '81.10000.000', '81.20000.000')
       AND account.SECTION_ID IN (
           SELECT section.SECTION_ID
             FROM ACCT_REPORT_SECTION section
            WHERE section.REPORT_CODE = 'LABARUGI'
       );

    UPDATE ACCT_REPORT_SECTION_ACCOUNT account
       SET account.IS_ACTIVE = 'Y'
     WHERE account.JENIS_AKUNTING = '*'
       AND account.IDDATA IS NULL
       AND account.TAHUN IS NULL
       AND account.KODEACC_ROOT IN ('4', '5', '6', '7', '8')
       AND account.SECTION_ID IN (
           SELECT section.SECTION_ID
             FROM ACCT_REPORT_SECTION section
            WHERE section.REPORT_CODE = 'LABARUGI'
       );
END;
/