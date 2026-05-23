-- Purpose: Verify wrapper package ACCT_RECALLCULATIONS_V2.
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
    DBMS_OUTPUT.PUT_LINE('=== Recalc V2 Wrapper Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_RECALLCULATIONS_V2'
       AND OBJECT_TYPE = 'PACKAGE';
    IF v_count = 0 THEN
        fail('Missing package ACCT_RECALLCULATIONS_V2');
    ELSE
        ok('Package ACCT_RECALLCULATIONS_V2 exists');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_RECALLCULATIONS_V2'
       AND OBJECT_TYPE = 'PACKAGE BODY';
    IF v_count = 0 THEN
        fail('Missing package body ACCT_RECALLCULATIONS_V2');
    ELSE
        ok('Package body ACCT_RECALLCULATIONS_V2 exists');
    END IF;

    SELECT COUNT(1) INTO v_count
      FROM USER_PROCEDURES
     WHERE OBJECT_NAME = 'ACCT_RECALLCULATIONS_V2'
       AND PROCEDURE_NAME = 'RECALCBYJURNALID';
    IF v_count = 0 THEN
        fail('Missing procedure ACCT_RECALLCULATIONS_V2.ReCalcByJurnalID');
    ELSE
        ok('Procedure ReCalcByJurnalID exists');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
