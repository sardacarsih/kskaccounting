-- Purpose: Verify scoped import package exists and valid.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
    v_errors NUMBER := 0;

    PROCEDURE fail(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[FAIL] ' || p_msg);
        v_errors := v_errors + 1;
    END;

    PROCEDURE ok(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[OK]   ' || p_msg);
    END;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_IMPORT_V2'
       AND OBJECT_TYPE = 'PACKAGE'
       AND STATUS = 'VALID';

    IF v_count = 0 THEN
        fail('PACKAGE ACCT_JURNAL_IMPORT_V2 not valid');
    ELSE
        ok('PACKAGE ACCT_JURNAL_IMPORT_V2 is VALID');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_IMPORT_V2'
       AND OBJECT_TYPE = 'PACKAGE BODY'
       AND STATUS = 'VALID';

    IF v_count = 0 THEN
        fail('PACKAGE BODY ACCT_JURNAL_IMPORT_V2 not valid');
    ELSE
        ok('PACKAGE BODY ACCT_JURNAL_IMPORT_V2 is VALID');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_JURNAL_IMPORT_V2'
       AND OBJECT_NAME = 'IMPORTJURNALPARSIAL'
       AND ARGUMENT_NAME = 'P_USERID';

    IF v_count = 0 THEN
        fail('IMPORTJURNALPARSIAL does not expose P_USERID parameter');
    ELSE
        ok('IMPORTJURNALPARSIAL exposes P_USERID');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20051, 'Scoped jurnal import V2 check failed');
    END IF;
END;
/
