-- Purpose: Verify global PKS Laba Rugi and Neraca settings and the PKS-aware Neraca facade.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    c_migration_id   CONSTANT VARCHAR2(128) := '20260729_001_pks_global_report_settings';
    v_count          NUMBER;
    v_labarugi_count NUMBER;
    v_neraca_count   NUMBER;
    v_active_count   NUMBER;
    v_inactive_count NUMBER;

    PROCEDURE fail(p_message IN VARCHAR2) IS
    BEGIN
        RAISE_APPLICATION_ERROR(-20875, p_message);
    END;
BEGIN
    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
       AND TRIM(IDDATA) IS NOT NULL;

    IF v_count <> 0 THEN
        fail('Scoped PKS report mappings still exist after globalisation: ' || v_count || ' rows.');
    END IF;

    SELECT COUNT(*),
           COUNT(CASE WHEN REPORT_CODE = 'LABARUGI' THEN 1 END),
           COUNT(CASE WHEN REPORT_CODE = 'NERACA' THEN 1 END),
           COUNT(CASE WHEN IS_ACTIVE = 'Y' THEN 1 END),
           COUNT(CASE WHEN IS_ACTIVE = 'N' THEN 1 END)
      INTO v_count,
           v_labarugi_count,
           v_neraca_count,
           v_active_count,
           v_inactive_count
      FROM ACCT_RPT_PKS_SCOPE_BAK
     WHERE MIGRATION_ID = c_migration_id;

    IF v_count <> 74
        OR v_labarugi_count <> 36
        OR v_neraca_count <> 38
        OR v_active_count <> 34
        OR v_inactive_count <> 40 THEN
        fail(
            'Scoped PKS backup mismatch. Expected total/LR/Neraca/active/inactive=74/36/38/34/40; actual=' ||
            v_count || '/' || v_labarugi_count || '/' || v_neraca_count || '/' ||
            v_active_count || '/' || v_inactive_count
        );
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION section
      JOIN (
          SELECT 'PKS_P1' SECTION_CODE, 4 DISPLAY_LVL FROM DUAL UNION ALL
          SELECT 'PKS_HPP', 3 FROM DUAL UNION ALL
          SELECT 'PKS_B1', 5 FROM DUAL UNION ALL
          SELECT 'PKS_B5', 4 FROM DUAL UNION ALL
          SELECT 'PKS_P2', 4 FROM DUAL UNION ALL
          SELECT 'PKS_B6', 4 FROM DUAL
      ) expected
        ON expected.SECTION_CODE = section.SECTION_CODE
       AND expected.DISPLAY_LVL = section.DISPLAY_LVL
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND section.IS_ACTIVE = 'Y';

    IF v_count <> 6 THEN
        fail('Global PKS Laba Rugi display levels do not match the KSKPKS source.');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND account.JENIS_AKUNTING = 'PKS'
       AND account.IDDATA IS NULL
       AND account.IS_ACTIVE = 'Y';

    IF v_count <> 13 THEN
        fail('Global PKS Laba Rugi mappings are incomplete: ' || v_count || '/13 rows.');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
      JOIN (
          SELECT '01' SECTION_CODE, '10.00000.000' KODEACC_ROOT FROM DUAL UNION ALL
          SELECT '01', '11.00000.000' FROM DUAL UNION ALL
          SELECT '01', '12.00000.000' FROM DUAL UNION ALL
          SELECT '01', '13.00000.000' FROM DUAL UNION ALL
          SELECT '01', '14.00000.000' FROM DUAL UNION ALL
          SELECT '03', '22.00000.000' FROM DUAL UNION ALL
          SELECT '03', '23.00000.000' FROM DUAL UNION ALL
          SELECT '03', '24.00000.000' FROM DUAL UNION ALL
          SELECT '03', '25.00000.000' FROM DUAL UNION ALL
          SELECT '03', '26.00000.000' FROM DUAL UNION ALL
          SELECT '03', '27.00000.000' FROM DUAL UNION ALL
          SELECT '03', '29.00000.000' FROM DUAL UNION ALL
          SELECT '05', '30.00000.000' FROM DUAL UNION ALL
          SELECT '05', '31.00000.000' FROM DUAL UNION ALL
          SELECT '05', '32.00000.000' FROM DUAL UNION ALL
          SELECT '05', '33.00000.000' FROM DUAL UNION ALL
          SELECT '05', '39.00000.000' FROM DUAL UNION ALL
          SELECT '06', '40.00000.000' FROM DUAL UNION ALL
          SELECT '10', '58.00000.000' FROM DUAL UNION ALL
          SELECT '10', '59.00000.000' FROM DUAL
      ) expected
        ON expected.SECTION_CODE = section.SECTION_CODE
       AND expected.KODEACC_ROOT = account.KODEACC_ROOT
     WHERE section.REPORT_CODE = 'NERACA'
       AND account.JENIS_AKUNTING = 'PKS'
       AND account.IDDATA IS NULL
       AND account.IS_ACTIVE = 'Y'
       AND NVL(account.MATCH_MODE, 'TREE') = 'TREE';

    IF v_count <> 20 THEN
        fail('Global PKS Neraca mappings from KSKPKS are incomplete.');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS = 'VALID';

    IF v_count <> 2 THEN
        fail('ACCT_LAPORAN_V2 is invalid after the global PKS settings migration.');
    END IF;

    SELECT COUNT(*)
      INTO v_count
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY'
       AND TEXT LIKE '%TRIM(pks_detail.JENIS_AKUNTANSI) = ''PKS''%';

    IF v_count = 0 THEN
        fail('LAP_NERACA_V2 does not select the PKS-specific global mappings.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: Global PKS report settings from KSKPKS are valid.');
END;
/
