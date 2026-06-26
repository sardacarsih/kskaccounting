-- Reconcile ACCT_COA monthly mutation columns with source journal detail sums.
-- Usage in SQL*Plus/SQLcl:
--   DEFINE IDDATA = 'EST1'
--   DEFINE PERIODE = '05/2026'
--   DEFINE TAHUN = 2026
--   DEFINE BULAN = 5
--   @20260626_recalc_reconciliation_check.sql

SET VERIFY OFF;
SET SERVEROUTPUT ON;

COLUMN KODEACC FORMAT A30;
COLUMN NAMAACC FORMAT A60;
COLUMN COA_DEBET FORMAT 999999999999990.99;
COLUMN SRC_DEBET FORMAT 999999999999990.99;
COLUMN DIFF_DEBET FORMAT 999999999999990.99;
COLUMN COA_KREDIT FORMAT 999999999999990.99;
COLUMN SRC_KREDIT FORMAT 999999999999990.99;
COLUMN DIFF_KREDIT FORMAT 999999999999990.99;

WITH coa_values AS (
    SELECT c.IDDATA,
           c.TAHUN,
           c.KODEACC,
           c.NAMAACC,
           CASE &BULAN
                WHEN 1 THEN NVL(c."1D", 0)
                WHEN 2 THEN NVL(c."2D", 0)
                WHEN 3 THEN NVL(c."3D", 0)
                WHEN 4 THEN NVL(c."4D", 0)
                WHEN 5 THEN NVL(c."5D", 0)
                WHEN 6 THEN NVL(c."6D", 0)
                WHEN 7 THEN NVL(c."7D", 0)
                WHEN 8 THEN NVL(c."8D", 0)
                WHEN 9 THEN NVL(c."9D", 0)
                WHEN 10 THEN NVL(c."10D", 0)
                WHEN 11 THEN NVL(c."11D", 0)
                ELSE NVL(c."12D", 0)
           END AS COA_DEBET,
           CASE &BULAN
                WHEN 1 THEN NVL(c."1K", 0)
                WHEN 2 THEN NVL(c."2K", 0)
                WHEN 3 THEN NVL(c."3K", 0)
                WHEN 4 THEN NVL(c."4K", 0)
                WHEN 5 THEN NVL(c."5K", 0)
                WHEN 6 THEN NVL(c."6K", 0)
                WHEN 7 THEN NVL(c."7K", 0)
                WHEN 8 THEN NVL(c."8K", 0)
                WHEN 9 THEN NVL(c."9K", 0)
                WHEN 10 THEN NVL(c."10K", 0)
                WHEN 11 THEN NVL(c."11K", 0)
                ELSE NVL(c."12K", 0)
           END AS COA_KREDIT
      FROM ACCT_COA c
     WHERE c.IDDATA = '&IDDATA'
       AND c.TAHUN = &TAHUN
), coa_scope AS (
    SELECT c.KODEACC,
           c.PARENTACC
      FROM ACCT_COA c
     WHERE c.IDDATA = '&IDDATA'
       AND c.TAHUN = &TAHUN
), posted_accounts AS (
    SELECT d.KODE AS KODEACC,
           SUM(NVL(d.DEBET, 0)) AS DEBET,
           SUM(NVL(d.KREDIT, 0)) AS KREDIT
      FROM ACCT_JURNAL_DTL d
     WHERE d.IDDATA = '&IDDATA'
       AND d.PERIODE = '&PERIODE'
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
SELECT c.KODEACC,
       c.NAMAACC,
       c.COA_DEBET,
       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
       c.COA_DEBET - NVL(s.SRC_DEBET, 0) AS DIFF_DEBET,
       c.COA_KREDIT,
       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT,
       c.COA_KREDIT - NVL(s.SRC_KREDIT, 0) AS DIFF_KREDIT
  FROM coa_values c
  LEFT JOIN source_values s ON s.KODEACC = c.KODEACC
 WHERE ABS(c.COA_DEBET - NVL(s.SRC_DEBET, 0)) > 0.005
    OR ABS(c.COA_KREDIT - NVL(s.SRC_KREDIT, 0)) > 0.005
 ORDER BY c.KODEACC;