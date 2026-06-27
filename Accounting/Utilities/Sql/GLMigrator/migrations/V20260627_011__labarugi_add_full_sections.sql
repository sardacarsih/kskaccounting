-- Purpose: Add full Laba Rugi sections (BIAYA BUNGA, PPH BADAN, PENDAPATAN/PENGELUARAN
-- LAIN-LAIN ANAK PERUSAHAAN) and move BIAYA TM to the end. COA root account mappings are
-- assigned afterward via the Setting RL form (FrmSettingRL); this migration seeds section
-- definitions only.

BEGIN
    MERGE INTO ACCT_REPORT_SECTION target
    USING (
        SELECT 'LABARUGI' REPORT_CODE, 'BIAYA_BUNGA' SECTION_CODE, 'BIAYA BUNGA' SECTION_NAME, 60 DISPLAY_ORDER, 'D' NORMAL_POSISI FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PPH_BADAN', 'PPH BADAN', 70, 'D' FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'LAIN2_ANAK', 'PENDAPATAN/PENGELUARAN LAIN-LAIN ANAK PERUSAHAAN', 80, 'K' FROM DUAL
    ) source
       ON (target.REPORT_CODE = source.REPORT_CODE AND target.SECTION_CODE = source.SECTION_CODE)
     WHEN MATCHED THEN
        UPDATE SET target.SECTION_NAME = source.SECTION_NAME,
                   target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.NORMAL_POSISI = source.NORMAL_POSISI,
                   target.IS_ACTIVE = 'Y'
     WHEN NOT MATCHED THEN
        INSERT (REPORT_CODE, SECTION_CODE, SECTION_NAME, DISPLAY_ORDER, NORMAL_POSISI)
        VALUES (source.REPORT_CODE, source.SECTION_CODE, source.SECTION_NAME, source.DISPLAY_ORDER, source.NORMAL_POSISI);

    -- Move the existing BIAYA TM section to the end so the new sections precede it.
    UPDATE ACCT_REPORT_SECTION
       SET DISPLAY_ORDER = 90
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE = 'BIAYA_TM';

    -- Sensible default display level for the new sections (tunable per-section in Setting RL form).
    UPDATE ACCT_REPORT_SECTION
       SET DISPLAY_LVL = 2
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE IN ('BIAYA_BUNGA', 'PPH_BADAN', 'LAIN2_ANAK');

    COMMIT;

    DBMS_OUTPUT.PUT_LINE('ADDED FULL LABARUGI SECTIONS (BIAYA_BUNGA, PPH_BADAN, LAIN2_ANAK)');
END;
/
