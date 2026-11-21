-- Purpose: Verify trigger bypass for async recalc path.
--          The triggers themselves are retired: disabled by 20260320_003 and dropped by
--          20260626_003 (COA balances come from the async ACCT_RECALLCULATIONS_V2 recompute).
--          A missing trigger is therefore OK; the bypass is only checked where it still exists.
-- Date: 2026-03-20 (updated 2026-11-21)

SET SERVEROUTPUT ON;

DECLARE
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

    PROCEDURE check_trigger(p_trigger IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_count FROM USER_TRIGGERS WHERE TRIGGER_NAME = p_trigger;
        IF v_count = 0 THEN
            ok(p_trigger || ' removed by later migration (20260626_003); bypass not needed');
            RETURN;
        END IF;

        SELECT COUNT(1)
          INTO v_count
          FROM USER_SOURCE
         WHERE TYPE = 'TRIGGER'
           AND NAME = p_trigger
           AND UPPER(TEXT) LIKE '%JURNAL_ASYNC_RECALC%';
        IF v_count = 0 THEN
            fail('Trigger ' || p_trigger || ' missing JURNAL_ASYNC_RECALC bypass');
        ELSE
            ok('Trigger ' || p_trigger || ' contains bypass');
        END IF;
    END;
BEGIN
    check_trigger('UPDATE_COA_FROM_UPDATE');
    check_trigger('UPDATE_COA_FROM_DELETE');

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
