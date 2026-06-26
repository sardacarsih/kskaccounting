-- Purpose: Verify legacy COA triggers are disabled.
-- Date: 2026-03-20

SET SERVEROUTPUT ON;

DECLARE
    v_errors NUMBER := 0;
    v_status USER_TRIGGERS.STATUS%TYPE;
    v_count NUMBER := 0;

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
    SELECT COUNT(1) INTO v_count
    FROM USER_TRIGGERS
    WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_UPDATE';
    IF v_count = 0 THEN
        ok('UPDATE_COA_FROM_UPDATE removed by later migration');
    ELSE
        SELECT STATUS INTO v_status
        FROM USER_TRIGGERS
        WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_UPDATE';
        IF v_status <> 'DISABLED' THEN
            fail('UPDATE_COA_FROM_UPDATE status=' || v_status || ' (expected DISABLED or removed)');
        ELSE
            ok('UPDATE_COA_FROM_UPDATE disabled');
        END IF;
    END IF;

    SELECT COUNT(1) INTO v_count
    FROM USER_TRIGGERS
    WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE';
    IF v_count = 0 THEN
        ok('UPDATE_COA_FROM_DELETE removed by later migration');
    ELSE
        SELECT STATUS INTO v_status
        FROM USER_TRIGGERS
        WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE';
        IF v_status <> 'DISABLED' THEN
            fail('UPDATE_COA_FROM_DELETE status=' || v_status || ' (expected DISABLED or removed)');
        ELSE
            ok('UPDATE_COA_FROM_DELETE disabled');
        END IF;
    END IF;

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
