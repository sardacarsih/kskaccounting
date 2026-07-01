-- Purpose: Verify the atomic Tutup Tahun changes are in place:
--   * ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING exposes the optional p_COMMIT parameter.
--   * ACCT_CLOSING_YEAR_V2.CLOSE_YEAR commits exactly once and calls the closing journal with a deferred commit.
--   * Both packages are valid.
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
    -- Package validity
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('ACCT_CLOSING_YEAR_V2', 'ACCT_JURNAL_CLOSING_V2')
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_count > 0 THEN
        fail('Closing packages have invalid package/body');
    ELSE
        ok('ACCT_CLOSING_YEAR_V2 and ACCT_JURNAL_CLOSING_V2 are valid');
    END IF;

    -- JURNAL_CLOSING now exposes the optional commit control
    SELECT COUNT(1)
      INTO v_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_JURNAL_CLOSING_V2'
       AND OBJECT_NAME = 'JURNAL_CLOSING'
       AND ARGUMENT_NAME = 'P_COMMIT';

    IF v_count < 1 THEN
        fail('JURNAL_CLOSING is missing the p_commit parameter');
    ELSE
        ok('JURNAL_CLOSING exposes p_commit');
    END IF;

    -- CLOSE_YEAR must commit exactly once (single explicit COMMIT statement in the body).
    -- Case-sensitive on purpose: only the executable COMMIT is uppercase; comments use lowercase "commit".
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND REGEXP_LIKE(TEXT, '(^|[^_[:alnum:]])COMMIT([^_[:alnum:]]|$)');

    IF v_count <> 1 THEN
        fail('CLOSE_YEAR must contain exactly one COMMIT (found ' || v_count || ')');
    ELSE
        ok('CLOSE_YEAR commits exactly once');
    END IF;

    -- CLOSE_YEAR must invoke JURNAL_CLOSING with a deferred commit (the 'N' argument)
    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%JURNAL_CLOSING(%''N''%';

    IF v_count < 1 THEN
        fail('CLOSE_YEAR does not defer the closing-journal commit (expected JURNAL_CLOSING(..., ''N''))');
    ELSE
        ok('CLOSE_YEAR defers the closing-journal commit');
    END IF;

    -- No lingering legacy lock-status dependency (carried over from the previous check)
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
        RAISE_APPLICATION_ERROR(-20320, 'Closing year V2 atomic verification failed');
    END IF;
END;
/
