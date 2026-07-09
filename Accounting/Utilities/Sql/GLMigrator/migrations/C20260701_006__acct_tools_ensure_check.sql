-- Purpose: Verify ACCT_TOOLS is valid and exposes Analisa_kesalahan_COA (the closing COA pre-check dependency).
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
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_TOOLS'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_count > 0 THEN
        fail('ACCT_TOOLS has invalid package/body');
    ELSE
        ok('ACCT_TOOLS package and body are valid');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_TOOLS'
       AND TYPE = 'PACKAGE'
       AND UPPER(TEXT) LIKE '%ANALISA_KESALAHAN_COA%';

    IF v_count < 1 THEN
        fail('ACCT_TOOLS spec does not expose Analisa_kesalahan_COA');
    ELSE
        ok('ACCT_TOOLS exposes Analisa_kesalahan_COA');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20322, 'ACCT_TOOLS ensure verification failed');
    END IF;
END;
/
