-- Purpose: Verify ACCT_JURNAL_CLOSING_V2 is valid and uses ACCT_RECALLCULATIONS_V2.
-- Date: 2026-07-01

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;

    PROCEDURE ok(p_message VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('OK: ' || p_message);
    END;

    PROCEDURE fail(p_message VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20914, p_message);
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
    ELSE
        ok('Package ACCT_JURNAL_CLOSING_V2 is valid');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_RECALLCULATIONS.RECALCBYNOJURNALID%';

    IF v_count > 0 THEN
        fail('ACCT_JURNAL_CLOSING_V2 still references legacy ACCT_RECALLCULATIONS.ReCalcByNoJurnalID');
    ELSE
        ok('No legacy ReCalcByNoJurnalID reference');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_RECALLCULATIONS_V2.RECALCPERIOD%';

    IF v_count = 0 THEN
        fail('ACCT_JURNAL_CLOSING_V2 does not call ACCT_RECALLCULATIONS_V2.ReCalcPeriod');
    ELSE
        ok('Uses ACCT_RECALLCULATIONS_V2.ReCalcPeriod');
    END IF;
END;
/
