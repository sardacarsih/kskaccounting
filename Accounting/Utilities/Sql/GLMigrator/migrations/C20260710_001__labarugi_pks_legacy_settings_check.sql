-- Purpose: Verify legacy PKS Laba Rugi metadata and package compilation.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER;

    PROCEDURE fail(p_message IN VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20955, p_message);
    END;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND COLUMN_NAME IN ('MATCH_MODE', 'GRP_CODE');

    IF v_count <> 2 THEN
        fail('Missing PKS Laba Rugi selector columns.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE IN ('PKS_P1', 'PKS_HPP', 'PKS_B1', 'PKS_B5', 'PKS_P2', 'PKS_B6', 'PKS_B3', 'PKS_PPH')
       AND IS_ACTIVE = 'Y';

    IF v_count <> 8 THEN
        fail('PKS Laba Rugi sections are incomplete.');
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
        fail('PKS Laba Rugi account mappings are incomplete.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND section.SECTION_CODE LIKE 'PKS\_%' ESCAPE '\'
       AND account.JENIS_AKUNTING = '*'
       AND account.IS_ACTIVE = 'Y';

    IF v_count <> 0 THEN
        fail('PKS Laba Rugi sections must not use global fallback mappings.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME IN ('ACCT_REPORT_ENGINE_V1', 'ACCT_JURNAL_CLOSING_V2')
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF v_count <> 4 THEN
        fail('PKS Laba Rugi engine or closing package is invalid.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF v_count <> 2 THEN
        fail('ACCT_LAPORAN_V2 is invalid.');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_REPORT_ENGINE_V1'
       AND TYPE = 'PACKAGE BODY'
       AND TEXT LIKE '%p_JENISAKUNTING = ''PKS'' AND account.JENIS_AKUNTING = ''PKS''%';

    IF v_count = 0 THEN
        fail('ACCT_REPORT_ENGINE_V1 does not enforce the PKS mapping override.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: Legacy PKS Laba Rugi settings are valid.');
END;
/
