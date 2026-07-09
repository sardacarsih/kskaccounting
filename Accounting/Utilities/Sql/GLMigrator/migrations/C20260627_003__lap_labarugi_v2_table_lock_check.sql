-- Purpose: Verify ACCT_LAPORAN_V2 table-lock prerequisites.
-- Date: 2026-06-27

SET SERVEROUTPUT ON;

DECLARE
    v_count  NUMBER := 0;
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
    DBMS_OUTPUT.PUT_LINE('=== Laba Rugi V2 Table Lock Check ===');

    SELECT COUNT(1) INTO v_count FROM USER_TABLES WHERE TABLE_NAME = 'ACCT_REPORT_LOCK';
    IF v_count = 0 THEN
        fail('Missing ACCT_REPORT_LOCK table');
    ELSE
        ok('ACCT_REPORT_LOCK table exists');
    END IF;

    SELECT COUNT(1) INTO v_count FROM ACCT_REPORT_LOCK WHERE LOCK_NAME = 'LABARUGI';
    IF v_count = 0 THEN
        fail('Missing LABARUGI lock row');
    ELSE
        ok('LABARUGI lock row exists');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND PROCEDURE_NAME IN ('LAP_LABARUGI_V2', 'LAP_LABARUGI_SUB_V2');
    IF v_count <> 2 THEN
        fail('Expected LAP_LABARUGI_V2 and LAP_LABARUGI_SUB_V2 procedures');
    ELSE
        ok('V2 main and sub procedures exist');
    END IF;

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
        RAISE_APPLICATION_ERROR(-20083, 'Laba Rugi V2 table-lock check failed');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
