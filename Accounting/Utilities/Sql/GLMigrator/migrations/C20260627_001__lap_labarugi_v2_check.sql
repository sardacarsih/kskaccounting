-- Purpose: Verify wrapper package ACCT_LAPORAN_V2 (Laba Rugi V2 entrypoint).
-- Date: 2026-06-27
--
-- Accuracy validation (run manually with real params after deploy):
--   The V2 cursor must return byte-for-byte the same rows the legacy path
--   produces. Because V2 delegates to ACCT_LAPORAN.LAP_LABARUGI, totals are
--   identical by construction. To double-check for a given IDDATA/period:
--
--     VAR rc REFCURSOR;
--     EXEC ACCT_LAPORAN_V2.LAP_LABARUGI_V2('<IDDATA>', <BULAN>, <TAHUN>, '<USERID>', '<JENIS>', :rc);
--     -- then inspect :rc, and compare SUM(BULANINI)/SUM(TAHUNINI) against the
--     -- legacy ACC_TMPLRNR snapshot for the same IDDATA + USERGEN.

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
    DBMS_OUTPUT.PUT_LINE('=== Laba Rugi V2 Wrapper Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE = 'PACKAGE';
    IF v_count = 0 THEN
        fail('Missing package ACCT_LAPORAN_V2');
    ELSE
        ok('Package ACCT_LAPORAN_V2 exists');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE = 'PACKAGE BODY';
    IF v_count = 0 THEN
        fail('Missing package body ACCT_LAPORAN_V2');
    ELSE
        ok('Package body ACCT_LAPORAN_V2 exists');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND PROCEDURE_NAME = 'LAP_LABARUGI_V2';
    IF v_count = 0 THEN
        fail('Missing procedure ACCT_LAPORAN_V2.LAP_LABARUGI_V2');
    ELSE
        ok('Procedure LAP_LABARUGI_V2 exists');
    END IF;

    -- Legacy object must still be present and untouched.
    SELECT COUNT(1) INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_LAPORAN'
       AND PROCEDURE_NAME = 'LAP_LABARUGI';
    IF v_count = 0 THEN
        fail('Legacy ACCT_LAPORAN.LAP_LABARUGI is missing (must remain intact)');
    ELSE
        ok('Legacy ACCT_LAPORAN.LAP_LABARUGI still present');
    END IF;

    -- Package + body must be VALID (a CREATE can succeed while the body is INVALID
    -- if a dependency is missing, e.g. legacy function not present on this schema).
    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';
    IF v_count > 0 THEN
        fail('ACCT_LAPORAN_V2 has INVALID package/body (compile errors) - check USER_ERRORS');
    ELSE
        ok('ACCT_LAPORAN_V2 package and body are VALID');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
