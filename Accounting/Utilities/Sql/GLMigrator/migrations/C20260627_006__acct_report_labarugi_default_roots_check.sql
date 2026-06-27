-- Purpose: Verify corrected LABARUGI default COA roots.

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND account.IS_ACTIVE = 'Y'
       AND account.KODEACC_ROOT IN ('60.00000.000', '61.00000.000', '70.00000.000', '91.00000.000', '92.00000.000', '80.10000.000', '80.20000.000', '81.10000.000', '81.20000.000');

    IF v_count < 9 THEN
        RAISE_APPLICATION_ERROR(-20906, 'Corrected LABARUGI root mappings are incomplete');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: Corrected LABARUGI root mappings exist');
END;
/