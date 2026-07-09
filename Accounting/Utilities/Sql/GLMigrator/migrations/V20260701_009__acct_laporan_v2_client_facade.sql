-- Purpose: Extend ACCT_LAPORAN_V2 as the single client-facing financial-report facade.
--          Adds Buku Besar V2 without changing or delegating to legacy ACCT_LAPORAN.
-- Date: 2026-07-01

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
BEGIN
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

    PROCEDURE LAP_BUKUBESAR_V2(
        p_IDDATA       IN  VARCHAR2,
        p_TAHUNDARI    IN  INTEGER,
        p_TAHUNSAMPAI  IN  INTEGER,
        p_BULANDARI    IN  INTEGER,
        p_BULANSAMPAI  IN  INTEGER,
        p_DARIKODE     IN  VARCHAR2,
        p_SAMPAIKODE   IN  VARCHAR2,
        p_CURSOR       OUT SYS_REFCURSOR
    );
END ACCT_LAPORAN_V2;]';

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

    PROCEDURE LAP_BUKUBESAR_V2(
        p_IDDATA       IN  VARCHAR2,
        p_TAHUNDARI    IN  INTEGER,
        p_TAHUNSAMPAI  IN  INTEGER,
        p_BULANDARI    IN  INTEGER,
        p_BULANSAMPAI  IN  INTEGER,
        p_DARIKODE     IN  VARCHAR2,
        p_SAMPAIKODE   IN  VARCHAR2,
        p_CURSOR       OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_CURSOR FOR
            SELECT dtl.periode,
                   dtl.kode,
                   dtl.rekening,
                   dtl.nojurnal,
                   dtl.tanggal,
                   dtl.keterangan,
                   dtl.debet,
                   dtl.kredit
              FROM acct_jurnal_dtl dtl
             WHERE dtl.iddata = p_IDDATA
               AND dtl.glyear BETWEEN p_TAHUNDARI AND p_TAHUNSAMPAI
               AND ((dtl.glyear = p_TAHUNDARI AND dtl.glmonth >= p_BULANDARI)
                    OR dtl.glyear > p_TAHUNDARI)
               AND ((dtl.glyear = p_TAHUNSAMPAI AND dtl.glmonth <= p_BULANSAMPAI)
                    OR dtl.glyear < p_TAHUNSAMPAI)
               AND dtl.kode IN (
                   SELECT coa.kodeacc
                     FROM acct_coa coa
                    WHERE coa.iddata = p_IDDATA
                      AND coa.tahun BETWEEN p_TAHUNDARI AND p_TAHUNSAMPAI
                    START WITH coa.iddata = p_IDDATA
                       AND coa.tahun BETWEEN p_TAHUNDARI AND p_TAHUNSAMPAI
                       AND coa.kodeacc BETWEEN p_DARIKODE AND p_SAMPAIKODE
                   CONNECT BY NOCYCLE PRIOR coa.kodeacc = coa.parentacc
                      AND PRIOR coa.iddata = coa.iddata
                      AND PRIOR coa.tahun = coa.tahun)
             ORDER BY dtl.kode, dtl.tanggal, dtl.nojurnal;
    END LAP_BUKUBESAR_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('EXTENDED ACCT_LAPORAN_V2 CLIENT FACADE WITH LAP_BUKUBESAR_V2');
END;
/