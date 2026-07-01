-- Purpose: Remove the last legacy-recalc dependency from the Tutup Tahun chain.
--   The orchestrator called ACCT_JURNAL.JurnalRE, whose body recalculates via the legacy
--   ACCT_RECALLCULATIONS.ReCalcByNoHID. This adds ACCT_JURNAL_RE_V2.JURNAL_RE -- a faithful copy of the
--   reversal-journal logic that recalculates through ACCT_RECALLCULATIONS_V2.ReCalcPeriod instead -- and
--   repoints ACCT_CLOSING_YEAR_V2.CLOSE_YEAR at it. The legacy ACCT_JURNAL.JurnalRE is left untouched so
--   monthly closing (FrmClosingMonth -> JurnalServices.JurnalRE) is unaffected.
-- Notes:
--   * ReCalcPeriod selects ACCT_JURNAL_DTL by PERIODE (not GLMONTH) and writes the month column chosen by
--     p_BULAN, so the recalc month is derived from the target period (sPERIODE), which is correct for the
--     December-to-January rollover.
--   * JURNAL_RE keeps the legacy row shape verbatim (incl. GLMONTH = source month) for report compatibility;
--     only the recalc call changed.
--   * p_commit (DEFAULT 'Y') lets the orchestrator defer the commit and stay atomic.
-- Date: 2026-07-01

CREATE OR REPLACE PACKAGE ACCT_JURNAL_RE_V2 AS
    PROCEDURE JURNAL_RE(
        p_IDDATA  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2,
        p_commit  IN CHAR DEFAULT 'Y'
    );
END ACCT_JURNAL_RE_V2;
/

CREATE OR REPLACE PACKAGE BODY ACCT_JURNAL_RE_V2 AS
    PROCEDURE JURNAL_RE(
        p_IDDATA  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2,
        p_commit  IN CHAR DEFAULT 'Y'
    )
    AS
        sHID VARCHAR2(40);
        sNOJURNAL VARCHAR2(30);
        sHIDNEW VARCHAR2(40);
        sDID VARCHAR2(40);
        sNOJURNALNEW VARCHAR2(30);
        dTANGGAL DATE;
        sKODE VARCHAR2(20);
        sREKENING VARCHAR2(100);
        nDEBET NUMBER(18,4);
        nKREDIT NUMBER(18,4);
        sKETERANGAN VARCHAR2(200);
        iGLYEAR INTEGER;
        iTAHUN INTEGER;
        iBARIS INTEGER;
        iBULAN INTEGER;
        sPERIODE CHAR(7);
        v_re_count INTEGER := 0;
        v_recalc_bulan INTEGER;

        CURSOR JurnalREHeader IS
            SELECT NOJURNAL, HID
              FROM acct_jurnal_hdr
             WHERE iddata = p_IDDATA AND periode = p_periode AND isre = 'Y';

        CURSOR JurnalREDetail IS
            SELECT KODE, REKENING, KREDIT AS DEBET, 0 AS KREDIT, KETERANGAN
              FROM acct_jurnal_dtl WHERE HIDREFF = sHID AND KREDIT > 0
            UNION ALL
            SELECT KODE, REKENING, 0 AS DEBET, DEBET AS KREDIT, KETERANGAN
              FROM acct_jurnal_dtl WHERE HIDREFF = sHID AND DEBET > 0;
    BEGIN
        iBULAN := TO_NUMBER(SUBSTR(p_periode, 1, 2), '99');
        iTAHUN := TO_NUMBER(SUBSTR(p_periode, 4, 4), '9999');

        IF (iBULAN = 12) THEN
            iGLYEAR := iTAHUN + 1;
            sPERIODE := '01/' || iGLYEAR;
            dTANGGAL := TO_DATE(iGLYEAR || '0101', 'yyyymmdd');
        ELSE
            iGLYEAR := iTAHUN;
            iBULAN := iBULAN + 1;
            sPERIODE := TRIM(TO_CHAR(iBULAN, '00') || '/' || iGLYEAR);
            dTANGGAL := TO_DATE(iGLYEAR || TO_CHAR(iBULAN, '00') || '01', 'yyyymmdd');
        END IF;

        DELETE FROM ACCT_JURNAL_HDR WHERE PERIODE = sPERIODE AND SUMBER = 'RE';

        OPEN JurnalREHeader;
        LOOP
            FETCH JurnalREHeader INTO sNOJURNAL, sHID;
            EXIT WHEN JurnalREHeader%NOTFOUND;

            sNOJURNALNEW := sNOJURNAL || '-RE';
            sHIDNEW := p_IDDATA || sPERIODE || sNOJURNALNEW || TO_CHAR(dTANGGAL, 'yyMMdd');
            INSERT INTO ACCT_JURNAL_HDR (HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, CREATED, PC, IP_ADD, ISRE)
                VALUES (sHIDNEW, p_IDDATA, sNOJURNALNEW, dTANGGAL, sPERIODE, 'RE', p_userid, SYSDATE, SYS_CONTEXT('USERENV', 'HOST'), SYS_CONTEXT('USERENV', 'IP_ADDRESS'), 'T');

            iBARIS := 0;
            OPEN JurnalREDetail;
            LOOP
                FETCH JurnalREDetail INTO sKODE, sREKENING, nDEBET, nKREDIT, sKETERANGAN;
                EXIT WHEN JurnalREDetail%NOTFOUND;
                iBARIS := iBARIS + 1;
                sDID := sHIDNEW || iBARIS;
                INSERT INTO ACCT_JURNAL_DTL (IDDATA, PERIODE, NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, USERID, GLYEAR, GLMONTH, DID, HIDREFF, SUMBER)
                    VALUES (p_IDDATA, sPERIODE, sNOJURNALNEW, dTANGGAL, iBARIS, sKODE, sREKENING, nDEBET, nKREDIT, sKETERANGAN, 'FALSE', p_userid, iGLYEAR, iBULAN, sDID, sHIDNEW, 'RE');
            END LOOP;
            CLOSE JurnalREDetail;
            v_re_count := v_re_count + 1;
        END LOOP;
        CLOSE JurnalREHeader;

        -- V2 recompute of the target period, replacing the legacy per-HID recalc (ReCalcByNoHID).
        -- Month is taken from the target period so the December -> January rollover recomputes month 1.
        IF v_re_count > 0 THEN
            v_recalc_bulan := TO_NUMBER(SUBSTR(sPERIODE, 1, 2), '99');
            ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, v_recalc_bulan, iGLYEAR, sPERIODE, p_userid);
        END IF;

        IF NVL(p_commit, 'Y') = 'Y' THEN
            COMMIT;
        END IF;
    END JURNAL_RE;
END ACCT_JURNAL_RE_V2;
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
        IF v_create_journal THEN
            p_LABA_RUGI := ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING(p_IDDATA, 12, p_TAHUN, p_USERID, p_JENISAKUNTING, 'N');

            v_selisih := ACCT_LAPORAN.BALANCED_CHECK(p_IDDATA, 12, p_TAHUN);
            IF v_selisih <> 0 THEN
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

        -- Reversal journal now runs through the V2 recompute (no legacy recalc dependency).
        ACCT_JURNAL_RE_V2.JURNAL_RE(p_IDDATA, v_periode, p_USERID, 'N');
        ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, 1, p_NEXT_YEAR, '01/' || TO_CHAR(p_NEXT_YEAR), p_USERID);

        COMMIT;
    END CLOSE_YEAR;
END ACCT_CLOSING_YEAR_V2;
/
