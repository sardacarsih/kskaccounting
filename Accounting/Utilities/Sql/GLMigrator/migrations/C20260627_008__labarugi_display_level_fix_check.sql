SET SERVEROUTPUT ON
DECLARE
    v_column_count NUMBER;
    v_package_count NUMBER;
    v_invalid_count NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_column_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION'
       AND COLUMN_NAME = 'DISPLAY_LVL';

    SELECT COUNT(1)
      INTO v_package_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_REPORT_ENGINE_V1'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY');

    SELECT COUNT(1)
      INTO v_invalid_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_REPORT_ENGINE_V1'
       AND STATUS <> 'VALID';

    DBMS_OUTPUT.PUT_LINE('DISPLAY_LVL_COLUMNS=' || v_column_count);
    DBMS_OUTPUT.PUT_LINE('PACKAGE_OBJECTS=' || v_package_count);
    DBMS_OUTPUT.PUT_LINE('INVALID_OBJECTS=' || v_invalid_count);

    IF v_column_count <> 1 OR v_package_count <> 2 OR v_invalid_count <> 0 THEN
        RAISE_APPLICATION_ERROR(-20928, 'Laba Rugi display level fix verification failed.');
    END IF;
END;
/