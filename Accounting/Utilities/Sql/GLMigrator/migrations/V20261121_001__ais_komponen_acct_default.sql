-- Purpose: Seed ACCT_DEFAULT keys AIS_KOMPONEN_<KETERANGAN>[_<DIVISIID>] used by AisJournalBuilder
--          to give FIN_JURNAL_KOMPONEN rows an account automatically.
-- Source:  the accounts users already picked by hand for komponen lines in saved AIS journals
--          (ACCT_JURNAL_HDR.SUMBER = 'AIS'), so every server is seeded from its own history.
-- Rules:   * company-wide key when a company always used ONE account for that komponen;
--          * otherwise a per-division key when that division always used ONE account (e.g. UANG MAKAN);
--          * ambiguous history is skipped (stays manual), existing keys are never overwritten,
--            and the account must exist in that company's ACCT_COA.
-- Key normalisation must match AisJournalBuilder.KomponenAccountKey: UPPER(TRIM(x)), non [A-Z0-9] -> '_'.
-- Date: 2026-11-21

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_inserted NUMBER := 0;
BEGIN
    INSERT INTO ACCT_DEFAULT (ID, IDDATA, NAMA, KODEACC, KETERANGAN)
    SELECT (SELECT NVL(MAX(ID), 0) FROM ACCT_DEFAULT) + ROWNUM,
           seed.IDDATA, seed.NAMA, seed.KODEACC,
           'Migrasi 20261121_001: dari riwayat jurnal AIS'
      FROM (
        WITH komponen AS (
            SELECT DISTINCT UPPER(TRIM(KETERANGAN)) AS KOMPONEN
              FROM FIN_JURNAL_KOMPONEN
             WHERE TRIM(KETERANGAN) IS NOT NULL
        ),
        history AS (
            SELECT dtl.IDDATA, k.KOMPONEN, md.DIVISIID, TRIM(dtl.KODE) AS KODE
              FROM ACCT_JURNAL_DTL dtl
              JOIN ACCT_JURNAL_HDR hdr ON hdr.JURNALID = dtl.REFFID
              JOIN komponen k ON UPPER(dtl.KETERANGAN) LIKE k.KOMPONEN || ' %'
              -- Builder writes "<KOMPONEN> <DIVISI> <ESTATE> R<n> <Bulan> <Tahun>".
              LEFT JOIN MASTER_DIVISI md
                ON UPPER(dtl.KETERANGAN) LIKE k.KOMPONEN || ' ' || UPPER(md.DIVISI) || ' ' || UPPER(md.ESTATEID) || ' R%'
             WHERE hdr.SUMBER = 'AIS'
               AND TRIM(dtl.KODE) IS NOT NULL
        ),
        company_level AS (
            SELECT IDDATA, KOMPONEN, MIN(KODE) AS KODE
              FROM history
             GROUP BY IDDATA, KOMPONEN
            HAVING COUNT(DISTINCT KODE) = 1
        ),
        division_level AS (
            SELECT h.IDDATA, h.KOMPONEN, h.DIVISIID, MIN(h.KODE) AS KODE
              FROM history h
             WHERE h.DIVISIID IS NOT NULL
               AND NOT EXISTS (SELECT 1 FROM company_level c WHERE c.IDDATA = h.IDDATA AND c.KOMPONEN = h.KOMPONEN)
             GROUP BY h.IDDATA, h.KOMPONEN, h.DIVISIID
            HAVING COUNT(DISTINCT h.KODE) = 1
        ),
        candidates AS (
            SELECT IDDATA, 'AIS_KOMPONEN_' || REGEXP_REPLACE(KOMPONEN, '[^A-Z0-9]', '_') AS NAMA, KODE AS KODEACC
              FROM company_level
            UNION ALL
            SELECT IDDATA, 'AIS_KOMPONEN_' || REGEXP_REPLACE(KOMPONEN, '[^A-Z0-9]', '_') || '_' || DIVISIID, KODE
              FROM division_level
        )
        SELECT c.IDDATA, c.NAMA, c.KODEACC
          FROM candidates c
         WHERE LENGTH(c.NAMA) <= 100
           AND NOT EXISTS (SELECT 1 FROM ACCT_DEFAULT d WHERE d.IDDATA = c.IDDATA AND d.NAMA = c.NAMA)
           AND EXISTS (SELECT 1 FROM ACCT_COA coa WHERE coa.IDDATA = c.IDDATA AND coa.KODEACC = c.KODEACC)
      ) seed;

    v_inserted := SQL%ROWCOUNT;
    COMMIT;

    DBMS_OUTPUT.PUT_LINE('AIS_KOMPONEN keys inserted into ACCT_DEFAULT: ' || v_inserted);
END;
/
