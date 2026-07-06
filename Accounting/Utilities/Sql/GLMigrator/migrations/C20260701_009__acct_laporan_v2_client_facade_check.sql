SET SERVEROUTPUT ON;
DECLARE
    v_count NUMBER;

    PROCEDURE fail(p_message VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20999, p_message);
    END;
BEGIN
    SELECT COUNT(*)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF v_count <> 2 THEN
        fail('ACCT_LAPORAN_V2 package/body must be valid');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND PROCEDURE_NAME IN ('LAP_LABARUGI_V2', 'LAP_LABARUGI_SUB_V2', 'LAP_NERACA_V2', 'LAP_NERACA_SUB_V2', 'LAP_BUKUBESAR_V2');

    IF v_count <> 5 THEN
        fail('ACCT_LAPORAN_V2 facade procedures are incomplete');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_LAPORAN.%';

    IF v_count > 0 THEN
        fail('ACCT_LAPORAN_V2 must not delegate to legacy ACCT_LAPORAN');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: ACCT_LAPORAN_V2 client facade valid and legacy-free');
END;
/