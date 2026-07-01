-- Purpose: Verify ACCT_CLOSING_YEAR_V2 exists and avoids legacy lock-status dependency.
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
     WHERE OBJECT_NAME = 'ACCT_CLOSING_YEAR_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_count > 0 THEN
        fail('ACCT_CLOSING_YEAR_V2 has invalid package/body');
    ELSE
        ok('ACCT_CLOSING_YEAR_V2 package and body are valid');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_CLOSING_YEAR_V2'
       AND OBJECT_NAME = 'CLOSE_YEAR'
       AND ARGUMENT_NAME IN (
           'P_IDDATA',
           'P_TAHUN',
           'P_USERID',
           'P_JENISAKUNTING',
           'P_CREATE_CLOSING_JOURNAL',
           'P_NEXT_YEAR',
           'P_COA_ACTION',
           'P_LABA_RUGI'
       );

    IF v_count < 8 THEN
        fail('CLOSE_YEAR signature is incomplete');
    ELSE
        ok('CLOSE_YEAR signature found');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%GETSTATUSLOCK%';

    IF v_count > 0 THEN
        fail('ACCT_CLOSING_YEAR_V2 still references legacy GetStatusLock');
    ELSE
        ok('No legacy GetStatusLock dependency found');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20319, 'Closing year V2 orchestrator verification failed');
    END IF;
END;
/
