-- Purpose: Verify trigger bypass for async recalc path.
-- Date: 2026-03-20

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
      FROM USER_TRIGGERS
     WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_UPDATE'
       AND STATUS = 'ENABLED';
    IF v_count = 0 THEN
        fail('Trigger UPDATE_COA_FROM_UPDATE not enabled');
    ELSE
        ok('Trigger UPDATE_COA_FROM_UPDATE enabled');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TRIGGERS
     WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE'
       AND STATUS = 'ENABLED';
    IF v_count = 0 THEN
        fail('Trigger UPDATE_COA_FROM_DELETE not enabled');
    ELSE
        ok('Trigger UPDATE_COA_FROM_DELETE enabled');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE TYPE = 'TRIGGER'
       AND NAME = 'UPDATE_COA_FROM_UPDATE'
       AND UPPER(TEXT) LIKE '%JURNAL_ASYNC_RECALC%';
    IF v_count = 0 THEN
        fail('Trigger UPDATE_COA_FROM_UPDATE missing JURNAL_ASYNC_RECALC bypass');
    ELSE
        ok('Trigger UPDATE_COA_FROM_UPDATE contains bypass');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE TYPE = 'TRIGGER'
       AND NAME = 'UPDATE_COA_FROM_DELETE'
       AND UPPER(TEXT) LIKE '%JURNAL_ASYNC_RECALC%';
    IF v_count = 0 THEN
        fail('Trigger UPDATE_COA_FROM_DELETE missing JURNAL_ASYNC_RECALC bypass');
    ELSE
        ok('Trigger UPDATE_COA_FROM_DELETE contains bypass');
    END IF;

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
