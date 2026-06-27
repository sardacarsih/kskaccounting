-- Purpose: Add new Laba Rugi (Income Statement) report entrypoint for current app
--          while keeping the legacy ACCT_LAPORAN.LAP_LABARUGI function fully intact.
-- Date: 2026-06-27
--
-- Design notes:
--   * Mirrors the established V2 wrapper convention (see ACCT_RECALLCULATIONS_V2):
--     a brand-new package object delegates to the proven legacy logic so the
--     financial numbers are guaranteed identical, while exposing a cleaner,
--     single-round-trip SYS_REFCURSOR interface to the client.
--   * The legacy generator ACCT_LAPORAN.LAP_LABARUGI still populates the
--     user-scoped staging table ACC_TMPLRNR (keyed by IDDATA + USERGEN). V2 calls
--     it and immediately streams the result back as one cursor, so the client no
--     longer needs the separate Generate + View round-trip.
--   * Column list matches exactly what the Income_statement report binds to
--     (IDDATA, KODEACC, URUT, TIPEACC, SUB1..SUB6, BULANINI, TAHUNINI, JENIS,
--      SETSUB, USERGEN, ISHEADER).
--   * Future optimization (requires porting the legacy body from USER_SOURCE):
--     compute the result set directly from ACCT_COA to drop the staging write
--     entirely. Intentionally NOT done here to avoid altering proven numbers.

SET SERVEROUTPUT ON;

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
    ) IS
        v_ignore NUMBER;
    BEGIN
        -- Reuse the proven legacy generator (writes ACC_TMPLRNR, scoped by USERGEN).
        -- The legacy object is NOT modified here; it is only invoked.
        v_ignore := ACCT_LAPORAN.LAP_LABARUGI(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_USERID,
            p_JENISAKUNTING
        );

        OPEN p_CURSOR FOR
            SELECT IDDATA, KODEACC, URUT, TIPEACC,
                   SUB1, SUB2, SUB3, SUB4, SUB5, SUB6,
                   BULANINI, TAHUNINI, JENIS, SETSUB, USERGEN, ISHEADER
              FROM ACC_TMPLRNR
             WHERE IDDATA  = p_IDDATA
               AND USERGEN = p_USERID
             ORDER BY URUT;
    END LAP_LABARUGI_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('CREATED/REPLACED PACKAGE ACCT_LAPORAN_V2');
END;
/
