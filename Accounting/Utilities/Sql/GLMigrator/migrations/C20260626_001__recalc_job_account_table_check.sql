-- Purpose: Verify per-job impacted-account table exists with expected shape.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc Job Account Table Check ===');

    SELECT COUNT(1) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'ACCT_RECALC_JOB_ACCOUNT';
    IF v_count = 0 THEN
        fail('Table ACCT_RECALC_JOB_ACCOUNT missing');
    ELSE
        ok('Table ACCT_RECALC_JOB_ACCOUNT present');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_CONSTRAINTS
     WHERE TABLE_NAME = 'ACCT_RECALC_JOB_ACCOUNT'
       AND CONSTRAINT_NAME = 'FK_ACCT_RECALC_JOB_ACCOUNT_JOB';
    IF v_count = 0 THEN
        fail('FK to ACCT_RECALC_JOB missing');
    ELSE
        ok('FK to ACCT_RECALC_JOB present');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_RECALC_JOB_ACCOUNT'
       AND COLUMN_NAME IN ('JOB_ID', 'KODEACC');
    IF v_count < 2 THEN
        fail('Expected columns JOB_ID, KODEACC not both present');
    ELSE
        ok('Columns JOB_ID, KODEACC present');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
