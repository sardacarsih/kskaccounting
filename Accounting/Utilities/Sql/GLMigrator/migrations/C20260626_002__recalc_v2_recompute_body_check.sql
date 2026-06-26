-- Purpose: Verify ACCT_RECALLCULATIONS_V2 recompute body is active and valid.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc V2 Recompute Body Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    -- New job-driven entrypoint must exist.
    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE RECALCBYJOB%';
    IF v_count = 0 THEN
        fail('ReCalcByJob entrypoint not found');
    ELSE
        ok('ReCalcByJob entrypoint found');
    END IF;

    -- Recompute helper must exist (absolute SET= semantics).
    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE RECOMPUTENODE%';
    IF v_count = 0 THEN
        fail('RecomputeNode helper not found');
    ELSE
        ok('RecomputeNode helper found');
    END IF;

    -- The additive helper must be gone.
    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE APPLYMUTASIBYMONTH%';
    IF v_count > 0 THEN
        fail('Legacy additive ApplyMutasiByMonth still present');
    ELSE
        ok('Additive ApplyMutasiByMonth removed');
    END IF;

    -- Saldo propagation must still be invoked.
    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%UPDSALDOAKHIR_SD_DES_BYKODE%';
    IF v_count = 0 THEN
        fail('Saldo propagation call not found');
    ELSE
        ok('Saldo propagation call found');
    END IF;

    -- No COMMIT inside the package body (caller owns the transaction).
    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%COMMIT%';
    IF v_count > 0 THEN
        fail('Unexpected COMMIT statement found in V2 package body');
    ELSE
        ok('No COMMIT statement found in V2 package body');
    END IF;

    -- Package must compile cleanly.
    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_RECALLCULATIONS_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';
    IF v_count > 0 THEN
        fail('ACCT_RECALLCULATIONS_V2 has INVALID package/body');
    ELSE
        ok('ACCT_RECALLCULATIONS_V2 package and body VALID');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
