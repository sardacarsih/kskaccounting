-- Purpose: Verify every ACCT_DEFAULT AIS_KOMPONEN key is unique per company and points to an existing ACCT_COA account.
-- Date: 2026-11-21
SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER;
    v_errors NUMBER := 0;
BEGIN
    SELECT COUNT(1) INTO v_count FROM ACCT_DEFAULT WHERE NAMA LIKE 'AIS\_KOMPONEN\_%' ESCAPE '\';
    DBMS_OUTPUT.PUT_LINE('[OK]   ACCT_DEFAULT AIS_KOMPONEN keys present: ' || v_count);

    SELECT COUNT(1) INTO v_count
      FROM (SELECT IDDATA, NAMA FROM ACCT_DEFAULT
             WHERE NAMA LIKE 'AIS\_KOMPONEN\_%' ESCAPE '\'
             GROUP BY IDDATA, NAMA HAVING COUNT(*) > 1);
    IF v_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[FAIL] Duplicate AIS_KOMPONEN keys per IDDATA: ' || v_count);
        v_errors := v_errors + 1;
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   AIS_KOMPONEN keys are unique per IDDATA');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM ACCT_DEFAULT d
     WHERE d.NAMA LIKE 'AIS\_KOMPONEN\_%' ESCAPE '\'
       AND NOT EXISTS (SELECT 1 FROM ACCT_COA coa WHERE coa.IDDATA = d.IDDATA AND coa.KODEACC = d.KODEACC);
    IF v_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[FAIL] AIS_KOMPONEN keys pointing to an account missing from ACCT_COA: ' || v_count);
        v_errors := v_errors + 1;
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   All AIS_KOMPONEN accounts exist in ACCT_COA');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'AIS_KOMPONEN ACCT_DEFAULT check failed');
    END IF;
END;
/
