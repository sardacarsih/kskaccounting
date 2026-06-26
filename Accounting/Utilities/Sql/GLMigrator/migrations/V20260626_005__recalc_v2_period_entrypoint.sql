-- Purpose: Add full-period recompute entrypoint to ACCT_RECALLCULATIONS_V2 for monthly closing.
--          For each impacted account (and all its ancestors), the monthly
--          mutation columns are SET to the absolute SUM over the account's
--          subtree for the period (not "+="), making recalc safe to re-run
--          (retry / stale-recovery / duplicate claim) and correct for
--          insert, update AND delete.
--          Adds ReCalcByJob which is driven by the durable per-job account
--          list (ACCT_RECALC_JOB_ACCOUNT) so deletes recompute correctly even
--          though the journal detail rows are already gone.
-- Date: 2026-06-26

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
    -- Recompute impacted accounts derived from a journal's current detail rows.
    -- Suitable for insert/update where the detail rows still exist.
    PROCEDURE ReCalcByJurnalID(
        p_IDDATA   IN VARCHAR2,
        p_BULAN    IN INTEGER,
        p_TAHUN    IN INTEGER,
        p_JURNALID IN NUMBER,
        p_PERIODE  IN VARCHAR2,
        p_USERID   IN VARCHAR2
    );

    -- Recompute impacted accounts from the durable per-job account list.
    -- Correct for insert/update/delete because it does not depend on the
    -- journal detail rows still being present.
    PROCEDURE ReCalcByJob(
        p_IDDATA  IN VARCHAR2,
        p_BULAN   IN INTEGER,
        p_TAHUN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_USERID  IN VARCHAR2,
        p_JOBID   IN NUMBER
    );

    -- Recompute the whole accounting period synchronously.
    -- Used by monthly closing because balance checks must see final values.
    PROCEDURE ReCalcPeriod(
        p_IDDATA  IN VARCHAR2,
        p_BULAN   IN INTEGER,
        p_TAHUN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_USERID  IN VARCHAR2
    );
END ACCT_RECALLCULATIONS_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_RECALLCULATIONS_V2 AS

    -- Recompute one COA node's monthly mutation columns as the absolute SUM
    -- of all journal detail postings under that node's subtree for the period.
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
        -- Recompute each impacted leaf account plus all of its ancestors. The
        -- impacted leaves are the accounts posted by this journal.
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
        -- Recompute each impacted leaf account plus all of its ancestors. The
        -- impacted leaves are read from the durable per-job account list so
        -- this is correct even for deletes (detail rows already gone).
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
    PROCEDURE RecomputePeriodMutasi(
        p_IDDATA  IN VARCHAR2,
        p_TAHUN   IN INTEGER,
        p_BULAN   IN INTEGER,
        p_PERIODE IN VARCHAR2
    ) IS
    BEGIN
        IF p_BULAN = 1 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."1D" = source.SRC_DEBET,
                           target."1K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 2 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."2D" = source.SRC_DEBET,
                           target."2K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 3 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."3D" = source.SRC_DEBET,
                           target."3K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 4 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."4D" = source.SRC_DEBET,
                           target."4K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 5 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."5D" = source.SRC_DEBET,
                           target."5K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 6 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."6D" = source.SRC_DEBET,
                           target."6K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 7 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."7D" = source.SRC_DEBET,
                           target."7K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 8 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."8D" = source.SRC_DEBET,
                           target."8K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 9 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."9D" = source.SRC_DEBET,
                           target."9K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 10 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."10D" = source.SRC_DEBET,
                           target."10K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 11 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."11D" = source.SRC_DEBET,
                           target."11K" = source.SRC_KREDIT;
        ELSIF p_BULAN = 12 THEN
            MERGE INTO ACCT_COA target
            USING (
                WITH coa_scope AS (
                    SELECT c.KODEACC, c.PARENTACC
                      FROM ACCT_COA c
                     WHERE c.IDDATA = p_IDDATA
                       AND c.TAHUN = p_TAHUN
                ), posted_accounts AS (
                    SELECT d.KODE AS KODEACC,
                           SUM(NVL(d.DEBET, 0)) AS DEBET,
                           SUM(NVL(d.KREDIT, 0)) AS KREDIT
                      FROM ACCT_JURNAL_DTL d
                     WHERE d.IDDATA = p_IDDATA
                       AND d.PERIODE = p_PERIODE
                       AND d.KODE IS NOT NULL
                     GROUP BY d.KODE
                ), account_ancestors AS (
                    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
                           cs.KODEACC AS KODEACC
                      FROM coa_scope cs
                     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
                     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
                ), source_values AS (
                    SELECT aa.KODEACC,
                           SUM(pa.DEBET) AS SRC_DEBET,
                           SUM(pa.KREDIT) AS SRC_KREDIT
                      FROM account_ancestors aa
                      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
                     GROUP BY aa.KODEACC
                )
                SELECT cs.KODEACC,
                       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
                       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT
                  FROM coa_scope cs
                  LEFT JOIN source_values s ON s.KODEACC = cs.KODEACC
            ) source
               ON (target.IDDATA = p_IDDATA AND target.TAHUN = p_TAHUN AND target.KODEACC = source.KODEACC)
             WHEN MATCHED THEN
                UPDATE SET target."12D" = source.SRC_DEBET,
                           target."12K" = source.SRC_KREDIT;
        ELSE
            RAISE_APPLICATION_ERROR(-20011, 'Bulan rekalkulasi tidak valid: ' || p_BULAN);
        END IF;
    END RecomputePeriodMutasi;

    PROCEDURE RecomputePeriodSaldo(
        p_IDDATA IN VARCHAR2,
        p_TAHUN  IN INTEGER,
        p_BULAN  IN INTEGER
    ) IS
        v_sql    VARCHAR2(32767);
        v_set    VARCHAR2(32767) := '';
        v_base   VARCHAR2(1000);
        v_expr_d VARCHAR2(12000);
        v_expr_k VARCHAR2(12000);
    BEGIN
        IF p_BULAN < 1 OR p_BULAN > 12 THEN
            RAISE_APPLICATION_ERROR(-20012, 'Bulan saldo tidak valid: ' || p_BULAN);
        END IF;

        IF p_BULAN = 1 THEN
            v_base := 'NVL(SALDOAWAL,0)';
        ELSE
            v_base := 'NVL("' || TO_CHAR(p_BULAN - 1) || 'S",0)';
        END IF;

        FOR target_month IN p_BULAN..12 LOOP
            v_expr_d := v_base;
            v_expr_k := v_base;

            FOR source_month IN p_BULAN..target_month LOOP
                v_expr_d := v_expr_d || '+(NVL("' || TO_CHAR(source_month) || 'D",0)-NVL("' || TO_CHAR(source_month) || 'K",0))';
                v_expr_k := v_expr_k || '+(NVL("' || TO_CHAR(source_month) || 'K",0)-NVL("' || TO_CHAR(source_month) || 'D",0))';
            END LOOP;

            IF LENGTH(v_set) > 0 THEN
                v_set := v_set || ',';
            END IF;

            v_set := v_set || '"' || TO_CHAR(target_month) || 'S" = CASE '
                || 'WHEN POSISI=''D'' THEN ' || v_expr_d || ' '
                || 'WHEN POSISI=''K'' THEN ' || v_expr_k || ' '
                || 'ELSE ' || v_expr_d || ' END';
        END LOOP;

        v_sql := 'UPDATE ACCT_COA SET ' || v_set || ' WHERE IDDATA = :p_iddata AND TAHUN = :p_tahun';
        EXECUTE IMMEDIATE v_sql USING p_IDDATA, p_TAHUN;
    END RecomputePeriodSaldo;

    PROCEDURE ReCalcPeriod(
        p_IDDATA  IN VARCHAR2,
        p_BULAN   IN INTEGER,
        p_TAHUN   IN INTEGER,
        p_PERIODE IN VARCHAR2,
        p_USERID  IN VARCHAR2
    ) IS
    BEGIN
        RecomputePeriodMutasi(p_IDDATA, p_TAHUN, p_BULAN, p_PERIODE);
        RecomputePeriodSaldo(p_IDDATA, p_TAHUN, p_BULAN);
    END ReCalcPeriod;
END ACCT_RECALLCULATIONS_V2;]';


    DBMS_OUTPUT.PUT_LINE('REPLACED ACCT_RECALLCULATIONS_V2 (period recompute entrypoint)');
END;
/
