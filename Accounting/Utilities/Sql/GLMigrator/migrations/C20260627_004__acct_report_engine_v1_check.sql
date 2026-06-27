-- Purpose: Verify ACCT_REPORT_ENGINE_V1 metadata and package.

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;

    PROCEDURE ok(p_message VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('OK: ' || p_message);
    END;

    PROCEDURE fail(p_message VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20904, p_message);
    END;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_TABLES
     WHERE TABLE_NAME IN ('ACCT_REPORT_DEF', 'ACCT_REPORT_SECTION', 'ACCT_REPORT_SECTION_ACCOUNT');

    IF v_count <> 3 THEN
        fail('Missing accounting report metadata tables');
    ELSE
        ok('Accounting report metadata tables exist');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_DEF
     WHERE REPORT_CODE = 'LABARUGI'
       AND IS_ACTIVE = 'Y';

    IF v_count <> 1 THEN
        fail('Missing active LABARUGI report definition');
    ELSE
        ok('LABARUGI report definition exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION
     WHERE REPORT_CODE = 'LABARUGI'
       AND IS_ACTIVE = 'Y';

    IF v_count < 6 THEN
        fail('Expected LABARUGI default sections');
    ELSE
        ok('LABARUGI default sections exist');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('ACCT_REPORT_ENGINE_V1', 'ACCT_LAPORAN_V2')
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'INVALID';

    IF v_count > 0 THEN
        fail('Report package has INVALID object status');
    ELSE
        ok('Report packages are valid');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_REPORT_ENGINE_V1'
       AND PROCEDURE_NAME IN ('GET_REPORT', 'GET_DRILLDOWN');

    IF v_count < 2 THEN
        fail('Missing ACCT_REPORT_ENGINE_V1 public procedures');
    ELSE
        ok('ACCT_REPORT_ENGINE_V1 public procedures exist');
    END IF;
END;
/
