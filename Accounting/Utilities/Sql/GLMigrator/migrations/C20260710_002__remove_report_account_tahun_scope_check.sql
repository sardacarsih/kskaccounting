-- Purpose: Verify ACCT_REPORT_SECTION_ACCOUNT no longer has year-scoped mappings.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER;

    PROCEDURE fail(p_message IN VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20958, p_message);
    END;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND COLUMN_NAME = 'TAHUN';

    IF v_count <> 0 THEN
        fail('ACCT_REPORT_SECTION_ACCOUNT.TAHUN still exists.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM (
            SELECT account.SECTION_ID,
                   account.JENIS_AKUNTING,
                   NVL(account.IDDATA, '*') IDDATA_SCOPE,
                   account.KODEACC_ROOT,
                   NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
                   NVL(account.GRP_CODE, CHR(0)) GRP_CODE
              FROM ACCT_REPORT_SECTION_ACCOUNT account
             GROUP BY account.SECTION_ID,
                      account.JENIS_AKUNTING,
                      NVL(account.IDDATA, '*'),
                      account.KODEACC_ROOT,
                      NVL(account.MATCH_MODE, 'TREE'),
                      NVL(account.GRP_CODE, CHR(0))
            HAVING COUNT(1) > 1
      );

    IF v_count <> 0 THEN
        fail('Duplicate report account mappings remain after removing TAHUN scope.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('ACCT_REPORT_ENGINE_V1', 'ACCT_LAPORAN_V2', 'ACCT_JURNAL_CLOSING_V2')
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF v_count <> 6 THEN
        fail('Report packages are invalid after removing TAHUN scope.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME IN ('ACCT_REPORT_ENGINE_V1', 'ACCT_LAPORAN_V2', 'ACCT_JURNAL_CLOSING_V2')
       AND TYPE = 'PACKAGE BODY'
       AND UPPER(TEXT) LIKE '%ACCOUNT.TAHUN%';

    IF v_count <> 0 THEN
        fail('Report package body still references account.TAHUN.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND section.SECTION_CODE LIKE 'PKS\_%' ESCAPE '\'
       AND account.JENIS_AKUNTING = 'PKS'
       AND account.MATCH_MODE IN ('PARENT', 'GRP_LVL')
       AND account.IS_ACTIVE = 'Y';

    IF v_count <> 13 THEN
        fail('PKS Laba Rugi account mappings are incomplete after removing TAHUN scope.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: Report account TAHUN scope removed.');
END;
/
