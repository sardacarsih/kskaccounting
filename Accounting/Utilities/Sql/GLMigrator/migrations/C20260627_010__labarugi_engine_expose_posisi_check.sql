SET SERVEROUTPUT ON
DECLARE
    v_package_count NUMBER;
    v_invalid_count NUMBER;
    v_posisi_count NUMBER;
BEGIN
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

    SELECT COUNT(1)
      INTO v_posisi_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_REPORT_ENGINE_V1'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%NORMAL_POSISI POSISI%';

    DBMS_OUTPUT.PUT_LINE('PACKAGE_OBJECTS=' || v_package_count);
    DBMS_OUTPUT.PUT_LINE('INVALID_OBJECTS=' || v_invalid_count);
    DBMS_OUTPUT.PUT_LINE('POSISI_PROJECTION=' || v_posisi_count);

    IF v_package_count <> 2 OR v_invalid_count <> 0 OR v_posisi_count < 1 THEN
        RAISE_APPLICATION_ERROR(-20930, 'Laba Rugi engine POSISI exposure verification failed.');
    END IF;
END;
/
