-- Purpose: Add scoped COA import package to avoid cross-user collision on ACC_COA_TMP.
-- Date: 2026-05-25

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tab_columns
     WHERE table_name = 'ACC_COA_TMP'
       AND column_name = 'BATCH_ID';

    IF l_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE ACC_COA_TMP ADD (BATCH_ID VARCHAR2(32))';
        DBMS_OUTPUT.PUT_LINE('ADDED ACC_COA_TMP.BATCH_ID');
    ELSE
        DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ACC_COA_TMP.BATCH_ID');
    END IF;
END;
/

CREATE OR REPLACE PACKAGE ACCOUNTING_COA_IMPORT_V2 AS
    FUNCTION ValidateStage(
        p_iddata   IN VARCHAR2,
        p_tahun    IN INTEGER,
        p_userid   IN VARCHAR2,
        p_batch_id IN VARCHAR2,
        p_mode     IN VARCHAR2
    ) RETURN SYS_REFCURSOR;

    FUNCTION ImportCOAbyMerge(
        p_iddata   IN VARCHAR2,
        p_tahun    IN INTEGER,
        p_userid   IN VARCHAR2,
        p_batch_id IN VARCHAR2,
        p_mode     IN VARCHAR2
    ) RETURN INTEGER;

    PROCEDURE EnsurePeriodExists(
        p_iddata IN VARCHAR2,
        p_tahun  IN INTEGER
    );
END ACCOUNTING_COA_IMPORT_V2;
/

CREATE OR REPLACE PACKAGE BODY ACCOUNTING_COA_IMPORT_V2 AS
    FUNCTION ValidateStage(
        p_iddata   IN VARCHAR2,
        p_tahun    IN INTEGER,
        p_userid   IN VARCHAR2,
        p_batch_id IN VARCHAR2,
        p_mode     IN VARCHAR2
    ) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT 'SELF_PARENT' AS code,
                   'Induk Akun tidak boleh sama dengan kode akun' AS message,
                   'INDUK' AS field,
                   account AS value
              FROM acc_coa_tmp
             WHERE iddata = p_iddata
               AND tahun = p_tahun
               AND userid = p_userid
               AND batch_id = p_batch_id
               AND account = induk
            UNION ALL
            SELECT 'PARENT_NOT_FOUND' AS code,
                   'Induk Perkiraan tidak terdaftar' AS message,
                   'INDUK' AS field,
                   t.induk AS value
              FROM acc_coa_tmp t
             WHERE t.iddata = p_iddata
               AND t.tahun = p_tahun
               AND t.userid = p_userid
               AND t.batch_id = p_batch_id
               AND t.induk IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1
                      FROM acc_coa_tmp p
                     WHERE p.iddata = t.iddata
                       AND p.tahun = t.tahun
                       AND p.userid = t.userid
                       AND p.batch_id = t.batch_id
                       AND p.account = t.induk)
               AND NOT EXISTS (
                    SELECT 1
                      FROM acct_coa p
                     WHERE p.iddata = t.iddata
                       AND p.tahun = t.tahun
                       AND p.kodeacc = t.induk)
            UNION ALL
            SELECT 'DETAIL_WITHOUT_PARENT' AS code,
                   'Kode Perkiraan Detail tidak memiliki induk' AS message,
                   'ACCOUNT' AS field,
                   account AS value
              FROM acc_coa_tmp
             WHERE iddata = p_iddata
               AND tahun = p_tahun
               AND userid = p_userid
               AND batch_id = p_batch_id
               AND isheader = 'D'
               AND induk IS NULL
            UNION ALL
            SELECT 'DUPLICATE_ACCOUNT' AS code,
                   'Kode Perkiraan duplikat pada file import' AS message,
                   'ACCOUNT' AS field,
                   account AS value
              FROM acc_coa_tmp
             WHERE iddata = p_iddata
               AND tahun = p_tahun
               AND userid = p_userid
               AND batch_id = p_batch_id
             GROUP BY account
            HAVING COUNT(*) > 1
             ORDER BY code, value;

        RETURN cur;
    END ValidateStage;

    FUNCTION ImportCOAbyMerge(
        p_iddata   IN VARCHAR2,
        p_tahun    IN INTEGER,
        p_userid   IN VARCHAR2,
        p_batch_id IN VARCHAR2,
        p_mode     IN VARCHAR2
    ) RETURN INTEGER AS
    BEGIN
        IF UPPER(p_mode) = 'PARTIAL' THEN
            INSERT INTO acct_coa (
                acctcoaid, iddata, tahun, grp, parentacc, isheader, kodeacc,
                lvl, posisi, namaacc, saldoawal, isaktif, divisi, blok, tahuntanam
            )
            SELECT RAWTOHEX(SYS_GUID()), t.iddata, t.tahun, t.jenis, t.induk, t.isheader, t.account,
                   t.lvl, t.posisi, t.perkiraan, NVL(t.saldoawal, 0), 'Y',
                   t.divisi, t.blok, t.tahuntanam
              FROM acc_coa_tmp t
             WHERE t.iddata = p_iddata
               AND t.tahun = p_tahun
               AND t.userid = p_userid
               AND t.batch_id = p_batch_id
               AND NOT EXISTS (
                    SELECT 1
                      FROM acct_coa c
                     WHERE c.iddata = t.iddata
                       AND c.tahun = t.tahun
                       AND c.kodeacc = t.account);
        ELSE
            MERGE INTO acct_coa c
            USING (
                SELECT iddata, tahun, jenis, induk, isheader, account,
                       lvl, posisi, perkiraan, NVL(saldoawal, 0) saldoawal,
                       divisi, blok, tahuntanam
                  FROM acc_coa_tmp
                 WHERE iddata = p_iddata
                   AND tahun = p_tahun
                   AND userid = p_userid
                   AND batch_id = p_batch_id
            ) t
               ON (c.iddata = t.iddata AND c.tahun = t.tahun AND c.kodeacc = t.account)
             WHEN MATCHED THEN UPDATE SET
                  c.grp = t.jenis,
                  c.parentacc = t.induk,
                  c.isheader = t.isheader,
                  c.lvl = t.lvl,
                  c.posisi = t.posisi,
                  c.namaacc = t.perkiraan,
                  c.saldoawal = t.saldoawal,
                  c.divisi = t.divisi,
                  c.blok = t.blok,
                  c.tahuntanam = t.tahuntanam
             WHEN NOT MATCHED THEN INSERT (
                  acctcoaid, iddata, tahun, grp, parentacc, isheader, kodeacc,
                  lvl, posisi, namaacc, saldoawal, isaktif, divisi, blok, tahuntanam
             ) VALUES (
                  RAWTOHEX(SYS_GUID()), t.iddata, t.tahun, t.jenis, t.induk, t.isheader, t.account,
                  t.lvl, t.posisi, t.perkiraan, t.saldoawal, 'Y', t.divisi, t.blok, t.tahuntanam
             );
        END IF;

        RETURN 1;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE;
    END ImportCOAbyMerge;

    PROCEDURE EnsurePeriodExists(
        p_iddata IN VARCHAR2,
        p_tahun  IN INTEGER
    ) AS
        l_exists INTEGER;
    BEGIN
        l_exists := ACCOUNTING.CekPeriodeExist(p_iddata, 1, p_tahun);
        IF l_exists = 0 THEN
            ACCOUNTING.CreateNextPeriode(p_iddata, 0, p_tahun);
        END IF;
    END EnsurePeriodExists;
END ACCOUNTING_COA_IMPORT_V2;
/
