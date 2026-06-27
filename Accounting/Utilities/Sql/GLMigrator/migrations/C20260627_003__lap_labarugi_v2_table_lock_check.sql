-- Purpose: Verify ACCT_LAPORAN_V2 table-lock implementation.
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
    DBMS_OUTPUT.PUT_LINE('=== Laba Rugi V2 Non-Legacy Check ===');

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

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';
    IF v_count > 0 THEN
        fail('ACCT_LAPORAN_V2 has INVALID package/body - check USER_ERRORS');
    ELSE
        ok('ACCT_LAPORAN_V2 package and body are VALID');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY'
       AND (UPPER(TEXT) LIKE '%ACC_TMPLRNR%'
        OR UPPER(TEXT) LIKE '%ACCT_LAPORAN.LAP_LABARUGI%'
        OR UPPER(TEXT) LIKE '%ACCT_LAPORAN.ACC_GENREP_LRNR_SUB%');
    IF v_count > 0 THEN
        fail('Package body still references legacy Laba Rugi staging/generators');
    ELSE
        ok('Package body is metadata-engine based and no legacy staging lock is required');
    END IF;

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
        RAISE_APPLICATION_ERROR(-20083, 'Laba Rugi V2 non-legacy check failed');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/