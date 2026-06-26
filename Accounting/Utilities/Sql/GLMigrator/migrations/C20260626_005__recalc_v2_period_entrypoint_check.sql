-- Purpose: Verify full-period recompute entrypoint for monthly closing.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc V2 Period Entrypoint Check ===');

    SELECT COUNT(1) INTO v_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_RECALLCULATIONS_V2'
       AND OBJECT_NAME = 'RECALCPERIOD'
       AND ARGUMENT_NAME IN ('P_IDDATA', 'P_BULAN', 'P_TAHUN', 'P_PERIODE', 'P_USERID');
    IF v_count < 5 THEN
        fail('ReCalcPeriod entrypoint/signature not found');
    ELSE
        ok('ReCalcPeriod entrypoint found');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE RECOMPUTEPERIODMUTASI%';
    IF v_count = 0 THEN
        fail('RecomputePeriodMutasi helper not found');
    ELSE
        ok('RecomputePeriodMutasi helper found');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_RECALLCULATIONS_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%PROCEDURE RECOMPUTEPERIODSALDO%';
    IF v_count = 0 THEN
        fail('RecomputePeriodSaldo helper not found');
    ELSE
        ok('RecomputePeriodSaldo helper found');
    END IF;

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

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Recalc V2 period entrypoint verification failed');
    END IF;
END;
/
