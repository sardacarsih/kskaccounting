-- Purpose: Verify the reversal-journal V2 change:
--   * ACCT_JURNAL_RE_V2 exists, is valid, recalculates through ACCT_RECALLCULATIONS_V2, and does not use the
--     legacy ACCT_RECALLCULATIONS.
--   * ACCT_CLOSING_YEAR_V2 now calls ACCT_JURNAL_RE_V2 and no longer the legacy ACCT_JURNAL.JurnalRE.
-- Date: 2026-07-01

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
    -- Validity of both packages
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('ACCT_JURNAL_RE_V2', 'ACCT_CLOSING_YEAR_V2')
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_count > 0 THEN
        fail('ACCT_JURNAL_RE_V2 / ACCT_CLOSING_YEAR_V2 have invalid package/body');
    ELSE
        ok('ACCT_JURNAL_RE_V2 and ACCT_CLOSING_YEAR_V2 are valid');
    END IF;

    -- JURNAL_RE recalculates through the V2 engine
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_RE_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_RECALLCULATIONS_V2.RECALCPERIOD%';

    IF v_count < 1 THEN
        fail('ACCT_JURNAL_RE_V2 does not call ACCT_RECALLCULATIONS_V2.ReCalcPeriod');
    ELSE
        ok('ACCT_JURNAL_RE_V2 uses ACCT_RECALLCULATIONS_V2.ReCalcPeriod');
    END IF;

    -- JURNAL_RE has no legacy recalc dependency
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_RE_V2'
       AND TYPE = 'PACKAGE BODY'
       AND REGEXP_LIKE(TEXT, 'ACCT_RECALLCULATIONS[^_]', 'i');  -- legacy pkg, not ..._V2

    IF v_count > 0 THEN
        fail('ACCT_JURNAL_RE_V2 still references legacy ACCT_RECALLCULATIONS');
    ELSE
        ok('ACCT_JURNAL_RE_V2 has no legacy ACCT_RECALLCULATIONS dependency');
    END IF;

    -- Orchestrator calls the V2 reversal proc
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_JURNAL_RE_V2.JURNAL_RE%';

    IF v_count < 1 THEN
        fail('CLOSE_YEAR does not call ACCT_JURNAL_RE_V2.JURNAL_RE');
    ELSE
        ok('CLOSE_YEAR calls ACCT_JURNAL_RE_V2.JURNAL_RE');
    END IF;

    -- Orchestrator no longer calls legacy ACCT_JURNAL.JurnalRE
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCT_JURNAL.JURNALRE%';

    IF v_count > 0 THEN
        fail('CLOSE_YEAR still calls legacy ACCT_JURNAL.JurnalRE');
    ELSE
        ok('CLOSE_YEAR no longer calls legacy ACCT_JURNAL.JurnalRE');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20321, 'Reversal-journal V2 verification failed');
    END IF;
END;
/
