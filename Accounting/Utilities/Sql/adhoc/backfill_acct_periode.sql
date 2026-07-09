-- Manual one-off backfill: create missing ACCT_PERIODE rows for periods that are
-- already referenced by existing ACCT_JURNAL_HDR transactions.
--
-- This is NOT a GLMigrator migration and is NOT tracked in migrations.manifest.json.
-- Run manually with sqlplus, e.g.:
--   sqlplus <user>@<tns> @backfill_acct_periode.sql
-- Enter the password interactively (or use an existing TNS/wallet) -- do not pass
-- credentials on the command line.
--
-- Column values below match exactly what the live ACCOUNTING.CreateNextPeriode
-- procedure does (extracted from ALL_SOURCE, owner FKM, PACKAGE BODY ACCOUNTING,
-- lines 682-712): it only ever sets IDPERIODE, IDDATA, TAHUN, BULAN, PERIODE,
-- NAMABULAN and leaves ISLOCKED/AKHIR/ISAKTIF NULL. IDPERIODE is MAX(IDPERIODE)+1
-- computed GLOBALLY across the whole table (no per-company scoping, no trigger).
-- Unique key is (IDDATA, TAHUN, BULAN) via ACCT_PERIODE_UK1.

-- 0. Confirm the actual live structure before trusting the assumptions above.
DESC ACCT_PERIODE;

SET SERVEROUTPUT ON

-- 1. Diagnostic (read-only): every (IDDATA, PERIODE) referenced by a transaction
--    but missing from ACCT_PERIODE.
SELECT DISTINCT h.IDDATA, h.PERIODE
FROM ACCT_JURNAL_HDR h
WHERE NOT EXISTS (
    SELECT 1 FROM ACCT_PERIODE p
    WHERE p.IDDATA = h.IDDATA AND p.PERIODE = h.PERIODE
)
ORDER BY h.IDDATA, h.PERIODE;

-- 2. Backfill: insert the missing periods, derived straight from the transactions.
INSERT INTO ACCT_PERIODE (IDPERIODE, IDDATA, TAHUN, BULAN, PERIODE, NAMABULAN)
SELECT
    (SELECT NVL(MAX(IDPERIODE),0) FROM ACCT_PERIODE)
        + ROW_NUMBER() OVER (ORDER BY x.TAHUN, x.BULAN, x.IDDATA) AS IDPERIODE,
    x.IDDATA,
    x.TAHUN,
    TRIM(TO_CHAR(x.BULAN,'00')) AS BULAN,
    x.PERIODE,
    CASE x.BULAN
        WHEN 1 THEN 'Januari'  WHEN 2 THEN 'Februari' WHEN 3 THEN 'Maret'
        WHEN 4 THEN 'April'    WHEN 5 THEN 'Mei'       WHEN 6 THEN 'Juni'
        WHEN 7 THEN 'Juli'     WHEN 8 THEN 'Agustus'   WHEN 9 THEN 'September'
        WHEN 10 THEN 'Oktober' WHEN 11 THEN 'Nopember' WHEN 12 THEN 'Desember'
    END AS NAMABULAN
FROM (
    SELECT DISTINCT h.IDDATA, h.PERIODE,
           TO_NUMBER(SUBSTR(h.PERIODE,1,2)) AS BULAN,
           TO_NUMBER(SUBSTR(h.PERIODE,4,4)) AS TAHUN
    FROM ACCT_JURNAL_HDR h
    WHERE NOT EXISTS (
        SELECT 1 FROM ACCT_PERIODE p
        WHERE p.IDDATA = h.IDDATA AND p.PERIODE = h.PERIODE
    )
) x;

-- 3. Verify: re-run the diagnostic -- expect 0 rows.
SELECT DISTINCT h.IDDATA, h.PERIODE
FROM ACCT_JURNAL_HDR h
WHERE NOT EXISTS (
    SELECT 1 FROM ACCT_PERIODE p
    WHERE p.IDDATA = h.IDDATA AND p.PERIODE = h.PERIODE
);

COMMIT;
