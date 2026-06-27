-- Purpose: Verify ACCT_JURNAL_CLOSING_V2 package is compiled and valid.
-- Date: 2026-06-28

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
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'INVALID';

    IF v_count > 0 THEN
        fail('Package ACCT_JURNAL_CLOSING_V2 is INVALID');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND OBJECT_TYPE = 'PACKAGE';

    IF v_count = 0 THEN
        fail('Package ACCT_JURNAL_CLOSING_V2 does not exist');
    ELSE
        ok('Package ACCT_JURNAL_CLOSING_V2 exists and is valid');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND PROCEDURE_NAME = 'JURNAL_CLOSING';

    IF v_count = 0 THEN
        fail('Missing JURNAL_CLOSING public function in ACCT_JURNAL_CLOSING_V2');
    ELSE
        ok('JURNAL_CLOSING function exists in ACCT_JURNAL_CLOSING_V2');
    END IF;
END;
/
