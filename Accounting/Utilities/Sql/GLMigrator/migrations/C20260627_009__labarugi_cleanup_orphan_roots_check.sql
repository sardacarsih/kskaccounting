SET SERVEROUTPUT ON
DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND account.IS_ACTIVE = 'Y'
       AND account.KODEACC_ROOT IN ('4', '5', '6', '7', '8', '80', '81');

    DBMS_OUTPUT.PUT_LINE('ACTIVE_ORPHAN_ROOTS=' || v_count);
    IF v_count <> 0 THEN
        RAISE_APPLICATION_ERROR(-20929, 'Active orphan LABARUGI root mappings still exist.');
    END IF;
END;
/