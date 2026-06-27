-- Purpose: Verify ACCT_LAPORAN_V2 no longer references legacy Laba Rugi generators.

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'INVALID';

    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20907, 'ACCT_LAPORAN_V2 is INVALID - check USER_ERRORS');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY'
       AND (UPPER(TEXT) LIKE '%ACCT_LAPORAN.ACC_GENREP_LRNR_SUB%'
        OR UPPER(TEXT) LIKE '%ACCT_LAPORAN.LAP_LABARUGI%'
        OR UPPER(TEXT) LIKE '%ACC_TMPLRNR%');

    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20907, 'ACCT_LAPORAN_V2 still references legacy Laba Rugi objects');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: ACCT_LAPORAN_V2 has no legacy Laba Rugi references');
END;
/