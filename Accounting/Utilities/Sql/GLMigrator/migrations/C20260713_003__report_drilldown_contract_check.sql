DECLARE
    v_invalid_count INTEGER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_invalid_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_REPORT_DRILLDOWN_V1'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_invalid_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20962, 'ACCT_REPORT_DRILLDOWN_V1 must be VALID.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: report drilldown contract package is valid.');
END;
/
