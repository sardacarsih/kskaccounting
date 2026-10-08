-- Purpose: Verify the global PKS Laba Rugi repair and the complete PKS globalisation state.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section ON section.SECTION_ID = account.SECTION_ID
      JOIN (
          SELECT 'PKS_P1' SECTION_CODE, 'GRP:11' KODEACC_ROOT FROM DUAL UNION ALL
          SELECT 'PKS_HPP', 'GRP:12' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.11007.000' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.12007.000' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.13000.000' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.21007.000' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.22007.000' FROM DUAL UNION ALL
          SELECT 'PKS_B1', '89.23007.000' FROM DUAL UNION ALL
          SELECT 'PKS_B5', '85.01007.000' FROM DUAL UNION ALL
          SELECT 'PKS_P2', 'GRP:16' FROM DUAL UNION ALL
          SELECT 'PKS_B6', 'GRP:17' FROM DUAL UNION ALL
          SELECT 'PKS_B3', 'GRP:15' FROM DUAL UNION ALL
          SELECT 'PKS_PPH', 'GRP:18' FROM DUAL
      ) expected
        ON expected.SECTION_CODE = section.SECTION_CODE
       AND expected.KODEACC_ROOT = account.KODEACC_ROOT
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND account.JENIS_AKUNTING = 'PKS'
       AND account.IDDATA IS NULL
       AND account.IS_ACTIVE = 'Y';

    IF v_count <> 13 THEN
        RAISE_APPLICATION_ERROR(-20881, 'Global PKS Laba Rugi mappings are incomplete: ' || v_count || '/13 rows.');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
       AND TRIM(IDDATA) IS NOT NULL;

    IF v_count <> 0 THEN
        RAISE_APPLICATION_ERROR(-20882, 'Scoped PKS mappings returned after Laba Rugi repair: ' || v_count || ' rows.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: 13 global PKS Laba Rugi mappings are valid.');
END;
/
