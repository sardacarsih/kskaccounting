-- Purpose: Verify the Closing Year V2 boundary does not require legacy package changes.
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

    PROCEDURE require_valid_package(p_name IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_OBJECTS
         WHERE OBJECT_NAME = p_name
           AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
           AND STATUS <> 'VALID';

        IF v_count > 0 THEN
            fail(p_name || ' has invalid package/body');
        ELSE
            ok(p_name || ' package/body are valid');
        END IF;
    END;

    PROCEDURE require_public_member(p_package IN VARCHAR2, p_member IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_PROCEDURES
         WHERE OBJECT_NAME = p_package
           AND PROCEDURE_NAME = p_member;

        IF v_count < 1 THEN
            fail(p_package || '.' || p_member || ' is missing from package spec');
        ELSE
            ok(p_package || '.' || p_member || ' is exposed');
        END IF;
    END;

    PROCEDURE forbid_source(p_pattern IN VARCHAR2, p_message IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_SOURCE
         WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
           AND TYPE = 'PACKAGE BODY'
           AND UPPER(TEXT) LIKE p_pattern;

        IF v_count > 0 THEN
            fail(p_message);
        ELSE
            ok(p_message || ' not found');
        END IF;
    END;
BEGIN
    require_valid_package('ACCT_CLOSING_YEAR_V2');
    require_valid_package('ACCT_JURNAL_CLOSING_V2');
    require_valid_package('ACCT_JURNAL_RE_V2');
    require_valid_package('ACCT_RECALLCULATIONS_V2');
    require_valid_package('ACCT_REPORT_ENGINE_V1');

    require_public_member('ACCT_CLOSING_YEAR_V2', 'CLOSE_YEAR');
    require_public_member('ACCT_JURNAL_CLOSING_V2', 'JURNAL_CLOSING');
    require_public_member('ACCT_JURNAL_RE_V2', 'JURNAL_RE');
    require_public_member('ACCT_RECALLCULATIONS_V2', 'RECALCPERIOD');

    forbid_source('%ACCT_LAPORAN.BALANCED_CHECK%', 'legacy ACCT_LAPORAN.BALANCED_CHECK dependency');
    forbid_source('%ACCT_JURNAL.JURNALRE%', 'legacy ACCT_JURNAL.JurnalRE dependency');
    forbid_source('%ACCT_RECALLCULATIONS.RECALC%', 'legacy ACCT_RECALLCULATIONS dependency');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%FUNCTION BALANCEDIFFERENCE%';

    IF v_count < 1 THEN
        fail('ACCT_CLOSING_YEAR_V2 is missing BalanceDifference helper');
    ELSE
        ok('BalanceDifference helper is present');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_CLOSING_YEAR_V2'
       AND TYPE = 'PACKAGE BODY'
       AND REGEXP_LIKE(TEXT, '(^|[^_[:alnum:]])COMMIT([^_[:alnum:]]|$)');

    IF v_count <> 1 THEN
        fail('ACCT_CLOSING_YEAR_V2 must contain exactly one COMMIT (found ' || v_count || ')');
    ELSE
        ok('ACCT_CLOSING_YEAR_V2 commits exactly once');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20322, 'Closing Year V2 boundary verification failed');
    END IF;
END;
/