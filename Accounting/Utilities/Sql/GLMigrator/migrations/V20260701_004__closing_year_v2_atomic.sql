-- Purpose: Make Tutup Tahun (year-end closing) atomic and align its balance check with monthly closing.
--   1) ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING gains p_commit (DEFAULT 'Y') so an orchestrator can defer the
--      commit and keep the whole close in a single transaction. Existing 5-arg callers (monthly closing via
--      LaporanRepository) keep committing because the new parameter defaults to 'Y'.
--   2) ACCT_CLOSING_YEAR_V2.CLOSE_YEAR now (a) only runs BALANCED_CHECK when a closing journal is created
--      (mirroring FrmClosingMonth, so the "no closing journal" path no longer aborts on an expected imbalance),
--      and (b) commits exactly once at the very end. A balance failure therefore rolls back the closing journal
--      instead of leaving an orphaned/duplicate '001/CLOSE' journal that CekDoubleClosing would later flag.
-- NOTE: Full atomicity still assumes the legacy sub-procedures called after the balance check
--       (ACCOUNTING.ClosingEndYear/ClosingEndYearUpdateOnly, ReClassLabaRugi, ACCT_JURNAL.JurnalRE) and the
--       account-setup helpers inside JURNAL_CLOSING (ACCOUNTING.CreateClosingAcct/CreateNextPeriode) do not
--       COMMIT internally. Verify this against ALL_SOURCE before relying on rollback of those steps.
-- Date: 2026-07-01

DECLARE
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_JURNAL_CLOSING_V2 AS
    FUNCTION JURNAL_CLOSING(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2,
        p_commit        IN CHAR DEFAULT 'Y'
    ) RETURN NUMBER;
END ACCT_JURNAL_CLOSING_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_JURNAL_CLOSING_V2 AS
    FUNCTION CALC_LABARUGI(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2
    ) RETURN NUMBER
    IS
        v_net_laba NUMBER := 0;
    BEGIN
        WITH section_accounts AS (
            SELECT section.SECTION_ID,
                   section.SECTION_CODE,
                   section.SECTION_NAME,
                   NVL(section.DISPLAY_LVL, 1) DISPLAY_LVL,
                   NVL(section.NORMAL_POSISI, 'D') NORMAL_POSISI,
                   account.KODEACC_ROOT
              FROM ACCT_REPORT_SECTION section
              JOIN ACCT_REPORT_SECTION_ACCOUNT account
                ON account.SECTION_ID = section.SECTION_ID
             WHERE section.REPORT_CODE = 'LABARUGI'
               AND section.IS_ACTIVE = 'Y'
               AND account.IS_ACTIVE = 'Y'
               AND account.JENIS_AKUNTING IN ('*', p_jenisakunting)
               AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
               AND (account.TAHUN IS NULL OR account.TAHUN = p_tahun)
        ),
        coa_tree AS (
            SELECT section_accounts.SECTION_ID,
                   section_accounts.NORMAL_POSISI,
                   coa.LVL,
                   coa.ISHEADER,
                   section_accounts.DISPLAY_LVL,
                   coa."1D", coa."1K", coa."2D", coa."2K", coa."3D", coa."3K",
                   coa."4D", coa."4K", coa."5D", coa."5K", coa."6D", coa."6K",
                   coa."7D", coa."7K", coa."8D", coa."8K", coa."9D", coa."9K",
                   coa."10D", coa."10K", coa."11D", coa."11K", coa."12D", coa."12K"
              FROM section_accounts
              JOIN ACCT_COA coa
                ON coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_tahun
             START WITH coa.KODEACC = section_accounts.KODEACC_ROOT
            CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                   AND PRIOR section_accounts.SECTION_ID = section_accounts.SECTION_ID
                   AND PRIOR section_accounts.KODEACC_ROOT = section_accounts.KODEACC_ROOT
        ),
        calculated AS (
            SELECT coa_tree.NORMAL_POSISI,
                   CASE p_bulan
                       WHEN 1 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."1D", coa_tree."1K", coa_tree.NORMAL_POSISI)
                       WHEN 2 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."2D", coa_tree."2K", coa_tree.NORMAL_POSISI)
                       WHEN 3 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."3D", coa_tree."3K", coa_tree.NORMAL_POSISI)
                       WHEN 4 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."4D", coa_tree."4K", coa_tree.NORMAL_POSISI)
                       WHEN 5 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."5D", coa_tree."5K", coa_tree.NORMAL_POSISI)
                       WHEN 6 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."6D", coa_tree."6K", coa_tree.NORMAL_POSISI)
                       WHEN 7 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."7D", coa_tree."7K", coa_tree.NORMAL_POSISI)
                       WHEN 8 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."8D", coa_tree."8K", coa_tree.NORMAL_POSISI)
                       WHEN 9 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."9D", coa_tree."9K", coa_tree.NORMAL_POSISI)
                       WHEN 10 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."10D", coa_tree."10K", coa_tree.NORMAL_POSISI)
                       WHEN 11 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."11D", coa_tree."11K", coa_tree.NORMAL_POSISI)
                       WHEN 12 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."12D", coa_tree."12K", coa_tree.NORMAL_POSISI)
                   END BULANINI
              FROM coa_tree
             WHERE coa_tree.LVL = coa_tree.DISPLAY_LVL
        )
        SELECT NVL(SUM(CASE WHEN NORMAL_POSISI = 'K' THEN BULANINI ELSE 0 END), 0) -
               NVL(SUM(CASE WHEN NORMAL_POSISI = 'D' THEN BULANINI ELSE 0 END), 0)
          INTO v_net_laba
          FROM calculated;

        RETURN v_net_laba;
    END CALC_LABARUGI;

    FUNCTION JURNAL_CLOSING(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2,
        p_commit        IN CHAR DEFAULT 'Y'
    ) RETURN NUMBER
    IS
        sPeriode_next VARCHAR2(7);
        PERIODEEXIST INTEGER;
        dLabaBersihBI NUMBER;
        sPeriode VARCHAR2(7);
        dLastdate DATE;
        AKUN_ALOKASI_LABADITAHAN VARCHAR2(30);
        AKUN_LRTAHUN_BERJALAN VARCHAR2(30);
        REK1 VARCHAR2(130);
        REK2 VARCHAR2(130);
        KET VARCHAR2(200);
        sHID VARCHAR2(40);
        p_jurnalid NUMBER := NULL;
        TSTATUS VARCHAR2(40);
        exist INT;
    BEGIN
        -- Month validation
        IF p_bulan < 1 OR p_bulan > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan closing tidak valid: ' || p_bulan);
        END IF;

        -- Create closing default accounts if they don't exist yet
        ACCOUNTING.CreateClosingAcct(p_IDDATA);

        sPeriode := TRIM(to_char(p_bulan,'00')||'/'||p_tahun);
        dLastdate := LAST_DAY(TO_DATE(TRIM('01'||to_char(p_bulan,'00')||p_tahun),'ddMMyyyy'));
        sHID := p_IDDATA||sPeriode||'001/CLOSE';
        KET := 'Alokasi ke Laba / Rugi tahun berjalan periode '||sPeriode;

        -- Calculate net profit/loss using the report engine metadata
        dLabaBersihBI := CALC_LABARUGI(p_IDDATA, p_bulan, p_tahun, p_userid, p_jenisakunting);

        -- Trigger status check and enable
        BEGIN
            SELECT STATUS INTO TSTATUS FROM USER_TRIGGERS WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE';
            IF TSTATUS = 'DISABLED' THEN
                EXECUTE IMMEDIATE 'ALTER TRIGGER UPDATE_COA_FROM_DELETE ENABLE';
            END IF;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                NULL;
        END;

        -- Idempotency: Clean up prior closing journals for this location/period
        DELETE FROM ACCT_JURNAL_DTL WHERE NOJURNAL = '001/CLOSE' AND IDDATA = p_IDDATA AND PERIODE = sPeriode;
        DELETE FROM ACCT_JURNAL_HDR WHERE NOJURNAL = '001/CLOSE' AND IDDATA = p_IDDATA AND PERIODE = sPeriode;

        -- Guarded lookup of accounts in ACCT_DEFAULT
        BEGIN
            SELECT KODEACC INTO AKUN_LRTAHUN_BERJALAN
              FROM ACCT_DEFAULT
             WHERE NAMA = 'RL_TAHUN_BERJALAN' AND IDDATA = p_IDDATA;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20201, 'Akun Jurnal Closing (Laba/Rugi Berjalan) belum di-setup untuk lokasi ' || p_IDDATA || ' tahun ' || p_tahun);
        END;

        BEGIN
            SELECT KODEACC INTO AKUN_ALOKASI_LABADITAHAN
              FROM ACCT_DEFAULT
             WHERE NAMA = 'ALOKASI_LABA_DITAHAN' AND IDDATA = p_IDDATA;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20201, 'Akun Alokasi Laba Ditahan belum di-setup untuk lokasi ' || p_IDDATA || ' tahun ' || p_tahun);
        END;

        -- Check if accounts exist in ACCT_COA for this year/location
        SELECT COUNT(1) INTO exist FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_LRTAHUN_BERJALAN;
        IF exist = 0 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_LRTAHUN_BERJALAN || ' ( KODE AKUN LABA/RUGI TAHUN BERJALAN TIDAK DITEMUKAN DI COA TAHUN ' || p_tahun || ' )');
        ELSIF exist > 1 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_LRTAHUN_BERJALAN || ' ( KODE AKUN LABA/RUGI TAHUN BERJALAN DOUBLE DI COA TAHUN ' || p_tahun || ' )');
        ELSE
            SELECT NAMAACC INTO REK1 FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_LRTAHUN_BERJALAN;
        END IF;

        SELECT COUNT(1) INTO exist FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_ALOKASI_LABADITAHAN;
        IF exist = 0 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_ALOKASI_LABADITAHAN || ' ( KODE AKUN ALOKASI LABA DITAHAN TIDAK DITEMUKAN DI COA TAHUN ' || p_tahun || ' )');
        ELSIF exist > 1 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_ALOKASI_LABADITAHAN || ' ( KODE AKUN ALOKASI LABA DITAHAN DOUBLE DI COA TAHUN ' || p_tahun || ' )');
        ELSE
            SELECT NAMAACC INTO REK2 FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_ALOKASI_LABADITAHAN;
        END IF;

        -- Post journal header and details if net laba is non-zero
        IF dLabaBersihBI < 0 THEN
            -- Header
            INSERT INTO ACCT_JURNAL_HDR (HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, UPDATEBY, POSTING)
            VALUES (sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 'Y')
            RETURNING JURNALID INTO p_jurnalid;

            -- Details
            -- DEBET
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'01', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 1, AKUN_LRTAHUN_BERJALAN, REK1, ABS(dLabaBersihBI), 0, KET, p_tahun, p_bulan);
            -- KREDIT
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'02', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 2, AKUN_ALOKASI_LABADITAHAN, REK2, 0, ABS(dLabaBersihBI), KET, p_tahun, p_bulan);

        ELSIF dLabaBersihBI > 0 THEN
            -- Header
            INSERT INTO ACCT_JURNAL_HDR (HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, UPDATEBY, POSTING)
            VALUES (sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 'Y')
            RETURNING JURNALID INTO p_jurnalid;

            -- Details
            -- DEBET
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'01', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 1, AKUN_ALOKASI_LABADITAHAN, REK2, ABS(dLabaBersihBI), 0, KET, p_tahun, p_bulan);
            -- KREDIT
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'02', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 2, AKUN_LRTAHUN_BERJALAN, REK1, 0, ABS(dLabaBersihBI), KET, p_tahun, p_bulan);
        END IF;

        -- Recalculate balances
        IF p_jurnalid IS NOT NULL THEN
            ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, p_bulan, p_tahun, sPeriode, p_userid);
        END IF;

        -- Commit is optional so an orchestrator (Tutup Tahun) can keep the whole close atomic.
        IF NVL(p_commit, 'Y') = 'Y' THEN
            COMMIT;
        END IF;

        -- Check next period ready
        IF p_bulan = 12 THEN
            sPeriode_next := '01/' || (p_tahun + 1);
        ELSE
            sPeriode_next := TRIM(to_char(p_bulan + 1, '00') || '/' || p_tahun);
        END IF;

        SELECT COUNT(1) INTO PERIODEEXIST FROM ACCT_PERIODE WHERE IDDATA = p_IDDATA AND PERIODE = sPeriode_next;
        IF PERIODEEXIST = 0 THEN
            ACCOUNTING.CreateNextPeriode(p_IDDATA, p_bulan, p_tahun);
        END IF;

        RETURN dLabaBersihBI;
    END JURNAL_CLOSING;
END ACCT_JURNAL_CLOSING_V2;]';

    DBMS_OUTPUT.PUT_LINE('REPAIRED PACKAGE ACCT_JURNAL_CLOSING_V2 WITH OPTIONAL COMMIT');
END;
/

CREATE OR REPLACE PACKAGE ACCT_CLOSING_YEAR_V2 AS
    PROCEDURE CLOSE_YEAR(
        p_IDDATA                 IN VARCHAR2,
        p_TAHUN                  IN INTEGER,
        p_USERID                 IN VARCHAR2,
        p_JENISAKUNTING          IN VARCHAR2,
        p_CREATE_CLOSING_JOURNAL IN CHAR,
        p_NEXT_YEAR              OUT INTEGER,
        p_COA_ACTION             OUT VARCHAR2,
        p_LABA_RUGI              OUT NUMBER
    );
END ACCT_CLOSING_YEAR_V2;
/

CREATE OR REPLACE PACKAGE BODY ACCT_CLOSING_YEAR_V2 AS
    PROCEDURE CLOSE_YEAR(
        p_IDDATA                 IN VARCHAR2,
        p_TAHUN                  IN INTEGER,
        p_USERID                 IN VARCHAR2,
        p_JENISAKUNTING          IN VARCHAR2,
        p_CREATE_CLOSING_JOURNAL IN CHAR,
        p_NEXT_YEAR              OUT INTEGER,
        p_COA_ACTION             OUT VARCHAR2,
        p_LABA_RUGI              OUT NUMBER
    )
    IS
        v_periode VARCHAR2(7);
        v_locked VARCHAR2(1);
        v_selisih NUMBER := 0;
        v_coa_next_year_exists INTEGER := 0;
        v_create_journal BOOLEAN;
    BEGIN
        v_periode := '12/' || TO_CHAR(p_TAHUN);
        p_NEXT_YEAR := p_TAHUN + 1;
        p_COA_ACTION := 'NONE';
        p_LABA_RUGI := 0;
        v_create_journal := (p_JENISAKUNTING <> 'LAIN' AND NVL(p_CREATE_CLOSING_JOURNAL, 'N') = 'Y');

        BEGIN
            SELECT NVL(ISLOCKED, 'N')
              INTO v_locked
              FROM ACCT_PERIODE
             WHERE IDDATA = p_IDDATA
               AND PERIODE = v_periode;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                v_locked := 'N';
        END;

        IF v_locked = 'Y' THEN
            RAISE_APPLICATION_ERROR(-20310, 'PERIOD_LOCKED: ' || v_periode);
        END IF;

        ACCOUNTING.UpdateLevel(p_IDDATA, p_TAHUN);
        ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, 12, p_TAHUN, v_periode, p_USERID);

        -- Create the closing journal (deferring its commit) and verify the neraca before anything is committed.
        -- The balance check only makes sense once laba-rugi has been reclassed to equity, so it is gated on the
        -- same condition as the journal (matching the monthly closing flow).
        IF v_create_journal THEN
            p_LABA_RUGI := ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING(p_IDDATA, 12, p_TAHUN, p_USERID, p_JENISAKUNTING, 'N');

            v_selisih := ACCT_LAPORAN.BALANCED_CHECK(p_IDDATA, 12, p_TAHUN);
            IF v_selisih <> 0 THEN
                -- Roll back the just-created closing journal so a retry does not produce a double closing.
                ROLLBACK;
                RAISE_APPLICATION_ERROR(-20311, 'NERACA_NOT_BALANCED: ' || TO_CHAR(v_selisih));
            END IF;
        END IF;

        SELECT ACCOUNTING.CekCOAExist(p_IDDATA, p_NEXT_YEAR)
          INTO v_coa_next_year_exists
          FROM DUAL;

        IF v_coa_next_year_exists = 1 THEN
            ACCOUNTING.ClosingEndYear(p_IDDATA, p_TAHUN, p_USERID);
            p_COA_ACTION := 'CREATE';
        ELSE
            ACCOUNTING.ClosingEndYearUpdateOnly(p_IDDATA, p_TAHUN, p_USERID);
            p_COA_ACTION := 'UPDATE_ONLY';
        END IF;

        IF v_create_journal THEN
            ACCOUNTING.ReClassLabaRugi(p_IDDATA, p_TAHUN, p_USERID);
        END IF;

        ACCT_JURNAL.JurnalRE(p_IDDATA, v_periode, p_USERID);
        ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, 1, p_NEXT_YEAR, '01/' || TO_CHAR(p_NEXT_YEAR), p_USERID);

        -- Single commit for the entire close.
        COMMIT;
    END CLOSE_YEAR;
END ACCT_CLOSING_YEAR_V2;
/
