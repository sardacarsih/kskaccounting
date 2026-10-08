SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
    l_error_count INTEGER;
BEGIN
    SELECT COUNT(1)
      INTO l_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF l_count <> 2 THEN
        RAISE_APPLICATION_ERROR(-20964, 'ACCT_JURNAL_V2 package and body must be VALID.');
    END IF;

    SELECT COUNT(1)
      INTO l_error_count
      FROM USER_ERRORS
     WHERE NAME = 'ACCT_JURNAL_V2'
       AND TYPE IN ('PACKAGE', 'PACKAGE BODY');

    IF l_error_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20965, 'ACCT_JURNAL_V2 has compilation errors.');
    END IF;

    SELECT COUNT(1)
      INTO l_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%INPUT_NOJURNAL%';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20966, 'The duplicate-check parameter fix is missing from ACCT_JURNAL_V2.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('ACCT_JURNAL_V2 duplicate-check repair verified.');
END;
/
