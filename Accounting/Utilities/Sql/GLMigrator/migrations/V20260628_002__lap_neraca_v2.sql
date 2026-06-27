-- Purpose: Migrate Neraca to metadata-driven ACCT_REPORT_ENGINE_V1 tables
--          and implement LAP_NERACA_V2 & LAP_NERACA_SUB_V2.
-- Date: 2026-06-28

SET SERVEROUTPUT ON;

DECLARE
    v_sec_id NUMBER;
BEGIN
    -- 1. Insert report definition for NERACA
    EXECUTE IMMEDIATE q'[
        MERGE INTO ACCT_REPORT_DEF target
            USING (SELECT 'NERACA' REPORT_CODE, 'Laporan Neraca' REPORT_NAME FROM DUAL) source
               ON (target.REPORT_CODE = source.REPORT_CODE)
             WHEN NOT MATCHED THEN
                INSERT (REPORT_CODE, REPORT_NAME) VALUES (source.REPORT_CODE, source.REPORT_NAME)
    ]';

    -- 2. Insert sections for NERACA (corresponding to ACCT_KATEGORI kelompok='N')
    EXECUTE IMMEDIATE q'[
        MERGE INTO ACCT_REPORT_SECTION target
            USING (
                SELECT 'NERACA' REPORT_CODE, '01' SECTION_CODE, 'AKTIVA LANCAR' SECTION_NAME, 10 DISPLAY_ORDER, 'D' NORMAL_POSISI FROM DUAL UNION ALL
                SELECT 'NERACA', '02', 'PENYERTAAN', 20, 'D' FROM DUAL UNION ALL
                SELECT 'NERACA', '03', 'AKTIVA TETAP', 30, 'D' FROM DUAL UNION ALL
                SELECT 'NERACA', '04', 'AKTIVA LAINNYA', 40, 'D' FROM DUAL UNION ALL
                SELECT 'NERACA', '05', 'HUTANG LANCAR', 50, 'K' FROM DUAL UNION ALL
                SELECT 'NERACA', '06', 'PERKIRAAN SEMENTARA', 60, 'D' FROM DUAL UNION ALL
                SELECT 'NERACA', '07', 'HUTANG JANGKA PANJANG', 70, 'K' FROM DUAL UNION ALL
                SELECT 'NERACA', '10', 'MODAL', 80, 'K' FROM DUAL
            ) source
               ON (target.REPORT_CODE = source.REPORT_CODE AND target.SECTION_CODE = source.SECTION_CODE)
             WHEN MATCHED THEN
                UPDATE SET target.SECTION_NAME = source.SECTION_NAME,
                           target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                           target.NORMAL_POSISI = source.NORMAL_POSISI,
                           target.IS_ACTIVE = 'Y'
             WHEN NOT MATCHED THEN
                INSERT (REPORT_CODE, SECTION_CODE, SECTION_NAME, DISPLAY_ORDER, NORMAL_POSISI)
                VALUES (source.REPORT_CODE, source.SECTION_CODE, source.SECTION_NAME, source.DISPLAY_ORDER, source.NORMAL_POSISI)
    ]';

    -- 3. Insert accounts for NERACA sections (Level 1 root accounts mapping)
    EXECUTE IMMEDIATE q'[
        MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
            USING (
                SELECT section.SECTION_ID, '*' JENIS_AKUNTING, CAST(NULL AS VARCHAR2(20)) IDDATA, CAST(NULL AS NUMBER) TAHUN, source_raw.KODEACC_ROOT, source_raw.DISPLAY_ORDER
                  FROM ACCT_REPORT_SECTION section
                  JOIN (
                      -- 01: AKTIVA LANCAR
                      SELECT '01' SECTION_CODE, '10.00000.000' KODEACC_ROOT, 10 DISPLAY_ORDER FROM DUAL UNION ALL
                      SELECT '01', '11.00000.000', 20 FROM DUAL UNION ALL
                      SELECT '01', '12.00000.000', 30 FROM DUAL UNION ALL
                      SELECT '01', '13.00000.000', 40 FROM DUAL UNION ALL
                      SELECT '01', '14.00000.000', 50 FROM DUAL UNION ALL
                      -- 02: PENYERTAAN
                      SELECT '02', '15.00000.000', 10 FROM DUAL UNION ALL
                      SELECT '02', '16.00000.000', 20 FROM DUAL UNION ALL
                      -- 03: AKTIVA TETAP
                      SELECT '03', '17.00000.000', 10 FROM DUAL UNION ALL
                      SELECT '03', '18.10000.000', 20 FROM DUAL UNION ALL
                      SELECT '03', '18.20000.000', 30 FROM DUAL UNION ALL
                      SELECT '03', '19.10000.000', 40 FROM DUAL UNION ALL
                      SELECT '03', '19.20000.000', 50 FROM DUAL UNION ALL
                      SELECT '03', '20.10000.000', 60 FROM DUAL UNION ALL
                      SELECT '03', '20.20000.000', 70 FROM DUAL UNION ALL
                      SELECT '03', '21.00000.000', 80 FROM DUAL UNION ALL
                      SELECT '03', '22.00000.000', 90 FROM DUAL UNION ALL
                      SELECT '03', '23.00000.000', 100 FROM DUAL UNION ALL
                      SELECT '03', '24.00000.000', 110 FROM DUAL UNION ALL
                      SELECT '03', '25.00000.000', 120 FROM DUAL UNION ALL
                      SELECT '03', '26.00000.000', 130 FROM DUAL UNION ALL
                      SELECT '03', '27.00000.000', 140 FROM DUAL UNION ALL
                      SELECT '03', '29.00000.000', 150 FROM DUAL UNION ALL
                      -- 04: AKTIVA LAINNYA
                      SELECT '04', '28.00000.000', 10 FROM DUAL UNION ALL
                      -- 05: HUTANG LANCAR
                      SELECT '05', '30.00000.000', 10 FROM DUAL UNION ALL
                      SELECT '05', '31.00000.000', 20 FROM DUAL UNION ALL
                      SELECT '05', '32.00000.000', 30 FROM DUAL UNION ALL
                      SELECT '05', '33.00000.000', 40 FROM DUAL UNION ALL
                      SELECT '05', '34.00000.000', 50 FROM DUAL UNION ALL
                      SELECT '05', '35.00000.000', 60 FROM DUAL UNION ALL
                      SELECT '05', '36.00000.000', 70 FROM DUAL UNION ALL
                      SELECT '05', '37.00000.000', 80 FROM DUAL UNION ALL
                      SELECT '05', '38.00000.000', 90 FROM DUAL UNION ALL
                      SELECT '05', '39.00000.000', 100 FROM DUAL UNION ALL
                      -- 06: PERKIRAAN SEMENTARA
                      SELECT '06', '40.00000.000', 10 FROM DUAL UNION ALL
                      -- 07: HUTANG JANGKA PANJANG
                      SELECT '07', '45.00000.000', 10 FROM DUAL UNION ALL
                      SELECT '07', '46.00000.000', 20 FROM DUAL UNION ALL
                      SELECT '07', '47.00000.000', 30 FROM DUAL UNION ALL
                      SELECT '07', '48.00000.000', 40 FROM DUAL UNION ALL
                      -- 10: MODAL
                      SELECT '10', '50.00000.000', 10 FROM DUAL UNION ALL
                      SELECT '10', '51.00000.000', 20 FROM DUAL UNION ALL
                      SELECT '10', '52.00000.000', 30 FROM DUAL UNION ALL
                      SELECT '10', '53.00000.000', 40 FROM DUAL UNION ALL
                      SELECT '10', '54.00000.000', 50 FROM DUAL UNION ALL
                      SELECT '10', '58.00000.000', 60 FROM DUAL UNION ALL
                      SELECT '10', '59.00000.000', 70 FROM DUAL
                  ) source_raw
                    ON section.SECTION_CODE = source_raw.SECTION_CODE
                 WHERE section.REPORT_CODE = 'NERACA'
            ) source
               ON (target.SECTION_ID = source.SECTION_ID
               AND target.JENIS_AKUNTING = source.JENIS_AKUNTING
               AND NVL(target.IDDATA, '*') = NVL(source.IDDATA, '*')
               AND NVL(target.TAHUN, 0) = NVL(source.TAHUN, 0)
               AND target.KODEACC_ROOT = source.KODEACC_ROOT)
             WHEN MATCHED THEN
                UPDATE SET target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                           target.IS_ACTIVE = 'Y'
             WHEN NOT MATCHED THEN
                INSERT (SECTION_ID, JENIS_AKUNTING, IDDATA, TAHUN, KODEACC_ROOT, DISPLAY_ORDER)
                VALUES (source.SECTION_ID, source.JENIS_AKUNTING, source.IDDATA, source.TAHUN, source.KODEACC_ROOT, source.DISPLAY_ORDER)
    ]';

    -- 4. Recreate package ACCT_LAPORAN_V2
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_LAPORAN_V2 AS
    PROCEDURE LAP_LABARUGI_V2(
        p_IDDATA        IN  VARCHAR2,
        p_BULAN         IN  INTEGER,
        p_TAHUN         IN  INTEGER,
        p_USERID        IN  VARCHAR2,
        p_JENISAKUNTING IN  VARCHAR2,
        p_CURSOR        OUT SYS_REFCURSOR
    );

    PROCEDURE LAP_LABARUGI_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_LAP     IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    );

    PROCEDURE LAP_NERACA_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    );

    PROCEDURE LAP_NERACA_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    );
END ACCT_LAPORAN_V2;]';

    -- 5. Recreate package body ACCT_LAPORAN_V2 with metadata-driven query
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_LAPORAN_V2 AS
    PROCEDURE LAP_LABARUGI_V2(
        p_IDDATA        IN  VARCHAR2,
        p_BULAN         IN  INTEGER,
        p_TAHUN         IN  INTEGER,
        p_USERID        IN  VARCHAR2,
        p_JENISAKUNTING IN  VARCHAR2,
        p_CURSOR        OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        ACCT_REPORT_ENGINE_V1.GET_REPORT(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_USERID,
            'LABARUGI',
            p_JENISAKUNTING,
            p_CURSOR
        );
    END LAP_LABARUGI_V2;

    PROCEDURE LAP_LABARUGI_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_LAP     IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        ACCT_REPORT_ENGINE_V1.GET_DRILLDOWN(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            'LABARUGI',
            NULL,
            p_KODEACC,
            p_CURSOR
        );
    END LAP_LABARUGI_SUB_V2;

    PROCEDURE LAP_NERACA_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        IF p_BULAN < 1 OR p_BULAN > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan laporan tidak valid: ' || p_BULAN);
        END IF;

        OPEN p_CURSOR FOR
            WITH section_accounts AS (
                SELECT section.SECTION_CODE KODE,
                       'NERACA' CAT1,
                       CASE WHEN section.NORMAL_POSISI='D' THEN 'AKTIVA' ELSE 'PASIVA' END KAT,
                       section.SECTION_NAME CAT2,
                       account.KODEACC_ROOT,
                       section.NORMAL_POSISI POSISI
                  FROM ACCT_REPORT_SECTION section
                  JOIN ACCT_REPORT_SECTION_ACCOUNT account
                    ON account.SECTION_ID = section.SECTION_ID
                 WHERE section.REPORT_CODE = 'NERACA'
                   AND section.IS_ACTIVE = 'Y'
                   AND account.IS_ACTIVE = 'Y'
                   AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
                   AND (account.TAHUN IS NULL OR account.TAHUN = p_TAHUN)
            ),
            coa_roots AS (
                SELECT section_accounts.KODE,
                       section_accounts.CAT1,
                       section_accounts.KAT,
                       section_accounts.CAT2,
                       coa.KODEACC AKUN,
                       coa.NAMAACC TIPE,
                       section_accounts.POSISI,
                       coa.SALDOAWAL,
                       coa."1S" AS "1S", coa."2S" AS "2S", coa."3S" AS "3S", coa."4S" AS "4S", coa."5S" AS "5S", coa."6S" AS "6S",
                       coa."7S" AS "7S", coa."8S" AS "8S", coa."9S" AS "9S", coa."10S" AS "10S", coa."11S" AS "11S", coa."12S" AS "12S"
                  FROM section_accounts
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = p_IDDATA
                   AND coa.TAHUN = p_TAHUN
                   AND coa.LVL = 1
                   AND coa.KODEACC = section_accounts.KODEACC_ROOT
            )
            SELECT KODE, CAT1, KAT, CAT2, AKUN, TIPE, POSISI,
                   CASE p_BULAN
                       WHEN 1 THEN "1S"
                       WHEN 2 THEN "2S"
                       WHEN 3 THEN "3S"
                       WHEN 4 THEN "4S"
                       WHEN 5 THEN "5S"
                       WHEN 6 THEN "6S"
                       WHEN 7 THEN "7S"
                       WHEN 8 THEN "8S"
                       WHEN 9 THEN "9S"
                       WHEN 10 THEN "10S"
                       WHEN 11 THEN "11S"
                       ELSE "12S"
                   END BULANINI,
                   CASE p_BULAN
                       WHEN 1 THEN SALDOAWAL
                       WHEN 2 THEN "1S"
                       WHEN 3 THEN "2S"
                       WHEN 4 THEN "3S"
                       WHEN 5 THEN "4S"
                       WHEN 6 THEN "5S"
                       WHEN 7 THEN "6S"
                       WHEN 8 THEN "7S"
                       WHEN 9 THEN "8S"
                       WHEN 10 THEN "9S"
                       WHEN 11 THEN "10S"
                       ELSE "11S"
                   END BULANLALU,
                   SALDOAWAL AWALTAHUN
              FROM coa_roots
             WHERE (SALDOAWAL <> 0 OR
                    (p_BULAN = 1 AND "1S" <> 0) OR
                    (p_BULAN = 2 AND ("1S" <> 0 OR "2S" <> 0)) OR
                    (p_BULAN = 3 AND ("2S" <> 0 OR "3S" <> 0)) OR
                    (p_BULAN = 4 AND ("3S" <> 0 OR "4S" <> 0)) OR
                    (p_BULAN = 5 AND ("4S" <> 0 OR "5S" <> 0)) OR
                    (p_BULAN = 6 AND ("5S" <> 0 OR "6S" <> 0)) OR
                    (p_BULAN = 7 AND ("6S" <> 0 OR "7S" <> 0)) OR
                    (p_BULAN = 8 AND ("7S" <> 0 OR "8S" <> 0)) OR
                    (p_BULAN = 9 AND ("8S" <> 0 OR "9S" <> 0)) OR
                    (p_BULAN = 10 AND ("9S" <> 0 OR "10S" <> 0)) OR
                    (p_BULAN = 11 AND ("10S" <> 0 OR "11S" <> 0)) OR
                    (p_BULAN = 12 AND ("11S" <> 0 OR "12S" <> 0)))
             ORDER BY KAT, KODE, AKUN;
    END LAP_NERACA_V2;

    PROCEDURE LAP_NERACA_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        ACCT_REPORT_ENGINE_V1.GET_DRILLDOWN(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            'NERACA',
            NULL,
            p_KODEACC,
            p_CURSOR
        );
    END LAP_NERACA_SUB_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('MIGRATED LAP_NERACA_V2 & LAP_NERACA_SUB_V2 TO METADATA-DRIVEN ENGINE');
END;
/
