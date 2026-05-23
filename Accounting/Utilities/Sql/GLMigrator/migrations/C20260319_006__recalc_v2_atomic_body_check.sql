-- Purpose: Verify ACCT_RECALLCULATIONS_V2 atomic body is active.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc V2 Atomic Body Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE APPLYMUTASIBYMONTH%';
    IF v_count = 0 THEN
        fail('ApplyMutasiByMonth helper not found in ACCT_RECALLCULATIONS_V2 body');
    ELSE
        ok('ApplyMutasiByMonth helper found');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%UPDSALDOAKHIR_SD_DES_BYKODE%';
    IF v_count = 0 THEN
        fail('Saldo propagation call not found');
    ELSE
        ok('Saldo propagation call found');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%COMMIT%';
    IF v_count > 0 THEN
        fail('Unexpected COMMIT statement found in V2 package body');
    ELSE
        ok('No COMMIT statement found in V2 package body');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
