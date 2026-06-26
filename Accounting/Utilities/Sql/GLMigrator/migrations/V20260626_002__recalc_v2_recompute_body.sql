-- Tujuan: Menggantikan ACCT_RECALLCULATIONS_V2 bersifat aditif dengan implementasi
--         hitung ulang yang idempotent berdasarkan data sumber asli.
--         Untuk setiap akun yang terdampak (dan semua induknya), kolom mutasi bulanan
--         di-SET ke SUM absolut dari sub-tree akun tersebut pada periode berjalan (bukan "+="),
--         sehingga aman untuk dijalankan ulang (retry / stale-recovery / klaim ganda)
--         dan akurat untuk operasi insert, update, DAN delete.
--         Menambahkan ReCalcByJob yang digerakkan oleh daftar akun per-job yang persisten
--         (ACCT_RECALC_JOB_ACCOUNT) agar penghapusan dihitung dengan benar meskipun
--         baris detail jurnal sudah terhapus.
-- Tanggal: 2026-06-26

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_ACCT_COA_ID_TAHUN_PARENT ON ACCT_COA (IDDATA, TAHUN, PARENTACC, KODEACC)';
    DBMS_OUTPUT.PUT_LINE('CREATED IDX_ACCT_COA_ID_TAHUN_PARENT');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN
            DBMS_OUTPUT.PUT_LINE('IDX_ACCT_COA_ID_TAHUN_PARENT already exists');
        ELSE
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_RECALLCULATIONS_V2 AS
    -- Menghitung ulang akun terdampak dari baris detail jurnal saat ini.
    -- Cocok untuk insert/update di mana baris detail masih ada.
    PROCEDURE ReCalcByJurnalID(
        p_IDDATA   IN VARCHAR2,
        p_BULAN    IN INTEGER,
        p_TAHUN    IN INTEGER,
        p_JURNALID IN NUMBER,
        p_PERIODE  IN VARCHAR2,
        p_USERID   IN VARCHAR2
    );

    -- Menghitung ulang akun terdampak dari daftar akun per-job yang persisten.
    -- Akurat untuk insert/update/delete karena tidak bergantung pada apakah
    -- baris detail jurnal masih ada.
    PROCEDURE ReCalcByJob(
        p_IDDATA  IN VARCHAR2,
        p_BULAN   IN INTEGER,
        p_TAHUN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_USERID  IN VARCHAR2,
        p_JOBID   IN NUMBER
    );
END ACCT_RECALLCULATIONS_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_RECALLCULATIONS_V2 AS

    -- Menghitung ulang kolom mutasi bulanan satu node COA sebagai SUM absolut
    -- dari semua posting detail jurnal di bawah sub-tree node tersebut untuk periode terkait.
    PROCEDURE RecomputeNode(
        p_IDDATA  IN VARCHAR2,
        p_TAHUN   IN INTEGER,
        p_BULAN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_KODEACC IN VARCHAR2
    ) IS
        v_debet  NUMBER := 0;
        v_kredit NUMBER := 0;
    BEGIN
        WITH coa_scope AS (
            SELECT c.KODEACC, c.PARENTACC
              FROM ACCT_COA c
             WHERE c.IDDATA = p_IDDATA
               AND c.TAHUN = p_TAHUN
        )
        SELECT NVL(SUM(NVL(d.DEBET, 0)), 0),
               NVL(SUM(NVL(d.KREDIT, 0)), 0)
          INTO v_debet, v_kredit
          FROM (
                SELECT KODEACC
                  FROM coa_scope
                 START WITH KODEACC = p_KODEACC
                 CONNECT BY NOCYCLE PRIOR KODEACC = PARENTACC
          ) subtree
          LEFT JOIN ACCT_JURNAL_DTL d
            ON d.IDDATA = p_IDDATA
           AND d.PERIODE = p_PERIODE
           AND d.KODE = subtree.KODEACC;

        IF p_BULAN = 1 THEN
            UPDATE ACCT_COA SET "1D" = v_debet, "1K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 2 THEN
            UPDATE ACCT_COA SET "2D" = v_debet, "2K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 3 THEN
            UPDATE ACCT_COA SET "3D" = v_debet, "3K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 4 THEN
            UPDATE ACCT_COA SET "4D" = v_debet, "4K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 5 THEN
            UPDATE ACCT_COA SET "5D" = v_debet, "5K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 6 THEN
            UPDATE ACCT_COA SET "6D" = v_debet, "6K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 7 THEN
            UPDATE ACCT_COA SET "7D" = v_debet, "7K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 8 THEN
            UPDATE ACCT_COA SET "8D" = v_debet, "8K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 9 THEN
            UPDATE ACCT_COA SET "9D" = v_debet, "9K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 10 THEN
            UPDATE ACCT_COA SET "10D" = v_debet, "10K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSIF p_BULAN = 11 THEN
            UPDATE ACCT_COA SET "11D" = v_debet, "11K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        ELSE
            UPDATE ACCT_COA SET "12D" = v_debet, "12K" = v_kredit
             WHERE IDDATA = p_IDDATA AND TAHUN = p_TAHUN AND KODEACC = p_KODEACC;
        END IF;

        ACCT_RECALLCULATIONS.UpdSaldoAkhir_sd_Des_byKODE(
            p_IDDATA => p_IDDATA,
            p_BULAN  => p_BULAN,
            p_TAHUN  => p_TAHUN,
            p_KODEACC => p_KODEACC
        );
    END RecomputeNode;

    PROCEDURE ReCalcByJurnalID(
        p_IDDATA   IN VARCHAR2,
        p_BULAN    IN INTEGER,
        p_TAHUN    IN INTEGER,
        p_JURNALID IN NUMBER,
        p_PERIODE  IN VARCHAR2,
        p_USERID   IN VARCHAR2
    ) IS
    BEGIN
        -- Menghitung ulang setiap akun daun (leaf) yang terdampak beserta semua induknya.
        -- Akun daun terdampak adalah akun yang diposting oleh jurnal ini.
        FOR node IN (
            WITH coa_scope AS (
                SELECT c.KODEACC, c.PARENTACC
                  FROM ACCT_COA c
                 WHERE c.IDDATA = p_IDDATA
                   AND c.TAHUN = p_TAHUN
            )
            SELECT DISTINCT KODEACC
              FROM coa_scope
             START WITH KODEACC IN (
                    SELECT DISTINCT d.KODE
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.REFFID = p_JURNALID
                       AND d.KODE IS NOT NULL
             )
             CONNECT BY NOCYCLE KODEACC = PRIOR PARENTACC
        ) LOOP
            RecomputeNode(p_IDDATA, p_TAHUN, p_BULAN, p_PERIODE, node.KODEACC);
        END LOOP;
    END ReCalcByJurnalID;

    PROCEDURE ReCalcByJob(
        p_IDDATA  IN VARCHAR2,
        p_BULAN   IN INTEGER,
        p_TAHUN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_USERID  IN VARCHAR2,
        p_JOBID   IN NUMBER
    ) IS
    BEGIN
        -- Menghitung ulang setiap akun daun (leaf) yang terdampak beserta semua induknya.
        -- Akun daun terdampak dibaca dari daftar akun per-job yang persisten
        -- sehingga ini akurat bahkan untuk proses delete (baris detail sudah hilang).
        FOR node IN (
            WITH coa_scope AS (
                SELECT c.KODEACC, c.PARENTACC
                  FROM ACCT_COA c
                 WHERE c.IDDATA = p_IDDATA
                   AND c.TAHUN = p_TAHUN
            )
            SELECT DISTINCT KODEACC
              FROM coa_scope
             START WITH KODEACC IN (
                    SELECT a.KODEACC
                      FROM ACCT_RECALC_JOB_ACCOUNT a
                     WHERE a.JOB_ID = p_JOBID
             )
             CONNECT BY NOCYCLE KODEACC = PRIOR PARENTACC
        ) LOOP
            RecomputeNode(p_IDDATA, p_TAHUN, p_BULAN, p_PERIODE, node.KODEACC);
        END LOOP;
    END ReCalcByJob;
END ACCT_RECALLCULATIONS_V2;]';

    DBMS_OUTPUT.PUT_LINE('REPLACED ACCT_RECALLCULATIONS_V2 (recompute-from-source mode)');
END;
/
