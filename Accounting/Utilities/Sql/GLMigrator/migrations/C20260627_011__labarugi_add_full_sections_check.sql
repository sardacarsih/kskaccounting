SET SERVEROUTPUT ON
DECLARE
    v_new_count NUMBER;
    v_tm_order NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_new_count
      FROM ACCT_REPORT_SECTION
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE IN ('BIAYA_BUNGA', 'PPH_BADAN', 'LAIN2_ANAK')
       AND IS_ACTIVE = 'Y';

    SELECT MAX(DISPLAY_ORDER)
      INTO v_tm_order
      FROM ACCT_REPORT_SECTION
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE = 'BIAYA_TM';

    DBMS_OUTPUT.PUT_LINE('NEW_SECTIONS=' || v_new_count);
    DBMS_OUTPUT.PUT_LINE('BIAYA_TM_ORDER=' || NVL(v_tm_order, -1));

    IF v_new_count <> 3 OR NVL(v_tm_order, -1) <> 90 THEN
        RAISE_APPLICATION_ERROR(-20931, 'Laba Rugi full-sections verification failed.');
    END IF;
END;
/
