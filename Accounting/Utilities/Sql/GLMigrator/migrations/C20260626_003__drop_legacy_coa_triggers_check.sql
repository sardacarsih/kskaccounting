-- Purpose: Verify legacy COA mutation triggers are removed.
-- Date: 2026-06-26

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
    DBMS_OUTPUT.PUT_LINE('=== Drop Legacy COA Triggers Check ===');

    SELECT COUNT(1) INTO v_count FROM USER_TRIGGERS WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_UPDATE';
    IF v_count > 0 THEN
        fail('UPDATE_COA_FROM_UPDATE still present');
    ELSE
        ok('UPDATE_COA_FROM_UPDATE removed');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_TRIGGERS WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE';
    IF v_count > 0 THEN
        fail('UPDATE_COA_FROM_DELETE still present');
    ELSE
        ok('UPDATE_COA_FROM_DELETE removed');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
