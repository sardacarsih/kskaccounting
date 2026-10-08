-- Purpose: Audit report-section mappings whose configured DISPLAY_LVL cannot be produced by their COA selector.
-- This migration is diagnostic only: it never changes ACCT_COA or report configuration data.
-- INACTIVE REFERENCE ONLY: intentionally excluded from migrations.manifest.json after the unscoped
-- ACCT_COA hierarchy audit exceeded the migrator timeout. Use FrmSettingRL > Validate Section instead;
-- that validation is scoped to the active company (IDDATA) and fiscal year (TAHUN).
DECLARE
    v_gap_count INTEGER := 0;
    v_invalid_packages INTEGER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_invalid_packages
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_REPORT_ENGINE_V1'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_invalid_packages > 0 THEN
        RAISE_APPLICATION_ERROR(-20961, 'ACCT_REPORT_ENGINE_V1 must be VALID before report section level audit.');
    END IF;

    FOR rec IN (
        SELECT section.REPORT_CODE,
               section.SECTION_CODE,
               section.SECTION_NAME,
               NVL(section.DISPLAY_LVL, 1) DISPLAY_LVL,
               account.KODEACC_ROOT,
               NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
               account.GRP_CODE
          FROM ACCT_REPORT_SECTION section
          JOIN ACCT_REPORT_SECTION_ACCOUNT account
            ON account.SECTION_ID = section.SECTION_ID
         WHERE section.IS_ACTIVE = 'Y'
           AND account.IS_ACTIVE = 'Y'
           AND NOT EXISTS (
               SELECT 1
                 FROM ACCT_COA coa
                WHERE coa.LVL = NVL(section.DISPLAY_LVL, 1)
                  AND (account.IDDATA IS NULL OR coa.IDDATA = account.IDDATA)
                  AND (
                      (NVL(account.MATCH_MODE, 'TREE') = 'TREE'
                       AND coa.KODEACC IN (
                           SELECT tree.KODEACC
                             FROM ACCT_COA tree
                            WHERE tree.IDDATA = coa.IDDATA
                              AND tree.TAHUN = coa.TAHUN
                           START WITH tree.KODEACC = account.KODEACC_ROOT
                           CONNECT BY NOCYCLE PRIOR tree.KODEACC = tree.PARENTACC
                                  AND PRIOR tree.IDDATA = tree.IDDATA
                                  AND PRIOR tree.TAHUN = tree.TAHUN
                       ))
                      OR
                      (NVL(account.MATCH_MODE, 'TREE') = 'PARENT'
                       AND coa.PARENTACC = account.KODEACC_ROOT)
                      OR
                      (NVL(account.MATCH_MODE, 'TREE') = 'GRP_LVL'
                       AND coa.GRP = account.GRP_CODE)
                  )
           )
    ) LOOP
        v_gap_count := v_gap_count + 1;
        DBMS_OUTPUT.PUT_LINE(
            'GAP_LVL: report=' || rec.REPORT_CODE ||
            ', section=' || rec.SECTION_CODE ||
            ', target_lvl=' || rec.DISPLAY_LVL ||
            ', mode=' || rec.MATCH_MODE ||
            ', root=' || NVL(rec.KODEACC_ROOT, 'GRP:' || NVL(rec.GRP_CODE, '-')));
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('REPORT_SECTION_LVL_GAP_COUNT=' || v_gap_count);
    DBMS_OUTPUT.PUT_LINE('OK: report section level audit completed without data changes.');
END;
/
