-- Purpose:
--   Upgrade ACCT_RECALLCULATIONS_V2 body to atomic mode (no per-row COMMIT).
-- Note:
--   Official deployment should use GLMigrator migration 20260319_006.

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_RECALLCULATIONS_V2 AS
    PROCEDURE ApplyMutasiByMonth(
        p_IDDATA IN VARCHAR2,
        p_TAHUN IN INTEGER,
        p_BULAN IN INTEGER,
        p_KODEACC IN VARCHAR2,
        p_DEBET IN NUMBER,
        p_KREDIT IN NUMBER
    ) IS
    BEGIN
        IF p_BULAN = 1 THEN
            UPDATE ACCT_COA SET "1D" = "1D" + p_DEBET, "1K" = "1K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 2 THEN
            UPDATE ACCT_COA SET "2D" = "2D" + p_DEBET, "2K" = "2K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 3 THEN
            UPDATE ACCT_COA SET "3D" = "3D" + p_DEBET, "3K" = "3K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 4 THEN
            UPDATE ACCT_COA SET "4D" = "4D" + p_DEBET, "4K" = "4K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 5 THEN
            UPDATE ACCT_COA SET "5D" = "5D" + p_DEBET, "5K" = "5K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 6 THEN
            UPDATE ACCT_COA SET "6D" = "6D" + p_DEBET, "6K" = "6K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 7 THEN
            UPDATE ACCT_COA SET "7D" = "7D" + p_DEBET, "7K" = "7K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 8 THEN
            UPDATE ACCT_COA SET "8D" = "8D" + p_DEBET, "8K" = "8K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 9 THEN
            UPDATE ACCT_COA SET "9D" = "9D" + p_DEBET, "9K" = "9K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 10 THEN
            UPDATE ACCT_COA SET "10D" = "10D" + p_DEBET, "10K" = "10K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 11 THEN
            UPDATE ACCT_COA SET "11D" = "11D" + p_DEBET, "11K" = "11K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSE
            UPDATE ACCT_COA SET "12D" = "12D" + p_DEBET, "12K" = "12K" + p_KREDIT
            WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        END IF;
    END ApplyMutasiByMonth;

    PROCEDURE ReCalcByJurnalID(
        p_IDDATA   IN VARCHAR2,
        p_BULAN    IN INTEGER,
        p_TAHUN    IN INTEGER,
        p_JURNALID IN NUMBER,
        p_PERIODE  IN VARCHAR2,
        p_USERID   IN VARCHAR2
    ) IS
    BEGIN
        DELETE FROM ACC_UPDATE_COA_FROMJURNAL WHERE USERID = p_USERID;

        FOR recData IN (
            SELECT KODE, SUM(NVL(DEBET, 0)) AS DEBET, SUM(NVL(KREDIT, 0)) AS KREDIT
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = p_IDDATA
              AND PERIODE = p_PERIODE
              AND REFFID = p_JURNALID
              AND KODE IS NOT NULL
            GROUP BY KODE
        ) LOOP
            FOR recDataSaldo IN (
                WITH X AS (
                    SELECT KODEACC, PARENTACC
                    FROM ACCT_COA
                    WHERE IDDATA = p_IDDATA
                      AND TAHUN = p_TAHUN
                )
                SELECT KODEACC
                FROM X
                START WITH KODEACC = recData.KODE
                CONNECT BY KODEACC = PRIOR PARENTACC
            ) LOOP
                ApplyMutasiByMonth(p_IDDATA, p_TAHUN, p_BULAN, recDataSaldo.KODEACC, recData.DEBET, recData.KREDIT);
                ACCT_RECALLCULATIONS.UpdSaldoAkhir_sd_Des_byKODE(p_IDDATA, p_BULAN, p_TAHUN, recDataSaldo.KODEACC);
            END LOOP;
        END LOOP;
    END ReCalcByJurnalID;
END ACCT_RECALLCULATIONS_V2;]';

    DBMS_OUTPUT.PUT_LINE('UPGRADED ACCT_RECALLCULATIONS_V2 BODY TO ATOMIC MODE');
END;
/
