-- Purpose: Repair the global PKS Laba Rugi mappings when migration 20260729_001
--          completed without inserting the thirteen account selectors.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER := 0;
BEGIN
    MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
    USING (
        SELECT section.SECTION_ID, 'PKS' JENIS_AKUNTING, CAST(NULL AS VARCHAR2(20)) IDDATA,
               'GRP:11' KODEACC_ROOT, 10 DISPLAY_ORDER, 'GRP_LVL' MATCH_MODE, '11' GRP_CODE
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_P1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, 'GRP:12', 10, 'GRP_LVL', '12'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_HPP'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.11007.000', 10, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.12007.000', 20, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.13000.000', 30, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.21007.000', 40, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.22007.000', 50, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '89.23007.000', 60, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, '85.01007.000', 10, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B5'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, 'GRP:16', 10, 'GRP_LVL', '16'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_P2'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, 'GRP:17', 10, 'GRP_LVL', '17'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B6'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, 'GRP:15', 10, 'GRP_LVL', '15'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B3'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, 'GRP:18', 10, 'GRP_LVL', '18'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_PPH'
    ) source
       ON (target.SECTION_ID = source.SECTION_ID
       AND target.JENIS_AKUNTING = source.JENIS_AKUNTING
       AND NVL(target.IDDATA, '*') = NVL(source.IDDATA, '*')
       AND target.KODEACC_ROOT = source.KODEACC_ROOT)
     WHEN MATCHED THEN
        UPDATE SET target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.INCLUDE_CHILDREN = 'Y',
                   target.IS_ACTIVE = 'Y',
                   target.MATCH_MODE = source.MATCH_MODE,
                   target.GRP_CODE = source.GRP_CODE
     WHEN NOT MATCHED THEN
        INSERT (
            SECTION_ID, JENIS_AKUNTING, IDDATA, KODEACC_ROOT, DISPLAY_ORDER,
            INCLUDE_CHILDREN, IS_ACTIVE, MATCH_MODE, GRP_CODE)
        VALUES (
            source.SECTION_ID, source.JENIS_AKUNTING, source.IDDATA, source.KODEACC_ROOT,
            source.DISPLAY_ORDER, 'Y', 'Y', source.MATCH_MODE, source.GRP_CODE);

    SELECT COUNT(*)
      INTO v_count
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section ON section.SECTION_ID = account.SECTION_ID
     WHERE section.REPORT_CODE = 'LABARUGI'
       AND account.JENIS_AKUNTING = 'PKS'
       AND account.IDDATA IS NULL
       AND account.IS_ACTIVE = 'Y';

    IF v_count <> 13 THEN
        RAISE_APPLICATION_ERROR(-20880, 'Global PKS Laba Rugi repair is incomplete: ' || v_count || '/13 rows.');
    END IF;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('REPAIRED 13 GLOBAL PKS LABA RUGI MAPPINGS');
END;
/
