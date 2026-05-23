-- Purpose: Verify telemetry columns on ACCT_RECALC_JOB.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc Job Telemetry Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1) INTO v_count FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'ACCT_RECALC_JOB' AND COLUMN_NAME = 'RECALC_SCOPE';
    IF v_count = 0 THEN
        fail('Missing column ACCT_RECALC_JOB.RECALC_SCOPE');
    ELSE
        ok('Column RECALC_SCOPE exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'ACCT_RECALC_JOB' AND COLUMN_NAME = 'IMPACTED_ACCOUNT_COUNT';
    IF v_count = 0 THEN
        fail('Missing column ACCT_RECALC_JOB.IMPACTED_ACCOUNT_COUNT');
    ELSE
        ok('Column IMPACTED_ACCOUNT_COUNT exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'ACCT_RECALC_JOB' AND COLUMN_NAME = 'IMPACTED_ROW_COUNT';
    IF v_count = 0 THEN
        fail('Missing column ACCT_RECALC_JOB.IMPACTED_ROW_COUNT');
    ELSE
        ok('Column IMPACTED_ROW_COUNT exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_INDEXES WHERE INDEX_NAME = 'IDX_ACCT_RECALC_JOB_SCOPE';
    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_RECALC_JOB_SCOPE');
    ELSE
        ok('Index IDX_ACCT_RECALC_JOB_SCOPE exists');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
