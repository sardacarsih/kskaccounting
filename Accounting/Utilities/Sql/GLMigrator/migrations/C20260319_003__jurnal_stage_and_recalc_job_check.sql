-- Purpose: Verify staging GTT and recalc job migration.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
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
    DBMS_OUTPUT.PUT_LINE('=== Staging + Recalc Job Migration Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'ACCT_JURNAL_DTL_STAGE_TMP';
    IF v_count = 0 THEN
        fail('Missing table ACCT_JURNAL_DTL_STAGE_TMP');
    ELSE
        ok('Table ACCT_JURNAL_DTL_STAGE_TMP exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_INDEXES WHERE INDEX_NAME = 'IDX_ACCT_JURNAL_DTL_STAGE_TMP';
    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_JURNAL_DTL_STAGE_TMP');
    ELSE
        ok('Index IDX_ACCT_JURNAL_DTL_STAGE_TMP exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'ACCT_RECALC_JOB';
    IF v_count = 0 THEN
        fail('Missing table ACCT_RECALC_JOB');
    ELSE
        ok('Table ACCT_RECALC_JOB exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'ACCT_RECALC_JOB' AND COLUMN_NAME = 'STATUS';
    IF v_count = 0 THEN
        fail('Missing column ACCT_RECALC_JOB.STATUS');
    ELSE
        ok('Column STATUS exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_CONSTRAINTS WHERE TABLE_NAME = 'ACCT_RECALC_JOB' AND CONSTRAINT_NAME = 'CK_ACCT_RECALC_JOB_STATUS';
    IF v_count = 0 THEN
        fail('Missing constraint CK_ACCT_RECALC_JOB_STATUS');
    ELSE
        ok('Constraint CK_ACCT_RECALC_JOB_STATUS exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_INDEXES WHERE INDEX_NAME = 'IDX_ACCT_RECALC_JOB_DUE';
    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_RECALC_JOB_DUE');
    ELSE
        ok('Index IDX_ACCT_RECALC_JOB_DUE exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_INDEXES WHERE INDEX_NAME = 'IDX_ACCT_RECALC_JOB_KEY';
    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_RECALC_JOB_KEY');
    ELSE
        ok('Index IDX_ACCT_RECALC_JOB_KEY exists');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
