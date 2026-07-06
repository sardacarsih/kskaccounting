-- Purpose: Repair ACCT_CLOSING_YEAR_V2 with a stable V2 boundary that does not require legacy package changes.
-- Date: 2026-07-01
--
-- Design boundary:
--   * The application calls only ACCT_CLOSING_YEAR_V2.CLOSE_YEAR for Tutup Tahun.
--   * This migration creates/replaces only V2 objects. It does not alter legacy packages such as ACCOUNTING,
--     ACCT_LAPORAN, ACCT_JURNAL, or ACCT_RECALLCULATIONS.
--   * The package body must not compile against fragile legacy members that differ between deployed schemas,
--     notably ACCT_LAPORAN.BALANCED_CHECK and ACCT_JURNAL.JurnalRE.

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
    FUNCTION BalanceDifference(
        p_IDDATA IN VARCHAR2,
        p_BULAN  IN INTEGER,
        p_TAHUN  IN INTEGER
    ) RETURN NUMBER
    IS
        v_debet NUMBER := 0;
        v_kredit NUMBER := 0;
    BEGIN
        SELECT NVL(SUM(CASE p_BULAN
                   WHEN 1 THEN "1D"
                   WHEN 2 THEN "2D"
                   WHEN 3 THEN "3D"
                   WHEN 4 THEN "4D"
                   WHEN 5 THEN "5D"
                   WHEN 6 THEN "6D"
                   WHEN 7 THEN "7D"
                   WHEN 8 THEN "8D"
                   WHEN 9 THEN "9D"
                   WHEN 10 THEN "10D"
                   WHEN 11 THEN "11D"
                   WHEN 12 THEN "12D"
                   ELSE 0
               END), 0),
               NVL(SUM(CASE p_BULAN
                   WHEN 1 THEN "1K"
                   WHEN 2 THEN "2K"
                   WHEN 3 THEN "3K"
                   WHEN 4 THEN "4K"
                   WHEN 5 THEN "5K"
                   WHEN 6 THEN "6K"
                   WHEN 7 THEN "7K"
                   WHEN 8 THEN "8K"
                   WHEN 9 THEN "9K"
                   WHEN 10 THEN "10K"
                   WHEN 11 THEN "11K"
                   WHEN 12 THEN "12K"
                   ELSE 0
               END), 0)
          INTO v_debet, v_kredit
          FROM ACCT_COA
         WHERE IDDATA = p_IDDATA
           AND TAHUN = p_TAHUN
           AND LVL = 1;

        RETURN ROUND(v_debet - v_kredit, 2);
    END BalanceDifference;

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

        IF v_create_journal THEN
            p_LABA_RUGI := ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING(p_IDDATA, 12, p_TAHUN, p_USERID, p_JENISAKUNTING, 'N');

            v_selisih := BalanceDifference(p_IDDATA, 12, p_TAHUN);
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

        ACCT_JURNAL_RE_V2.JURNAL_RE(p_IDDATA, v_periode, p_USERID, 'N');
        ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, 1, p_NEXT_YEAR, '01/' || TO_CHAR(p_NEXT_YEAR), p_USERID);

        COMMIT;
    END CLOSE_YEAR;
END ACCT_CLOSING_YEAR_V2;
/