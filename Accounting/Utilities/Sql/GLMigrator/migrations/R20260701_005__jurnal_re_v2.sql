-- Purpose: Roll back the V2 reversal-journal change:
--   * Restore ACCT_CLOSING_YEAR_V2.CLOSE_YEAR to the V20260701_004 definition (calls legacy ACCT_JURNAL.JurnalRE).
--   * Drop ACCT_JURNAL_RE_V2.
-- Date: 2026-07-01

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

        ACCT_JURNAL.JurnalRE(p_IDDATA, v_periode, p_USERID);
        ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, 1, p_NEXT_YEAR, '01/' || TO_CHAR(p_NEXT_YEAR), p_USERID);

        COMMIT;
    END CLOSE_YEAR;
END ACCT_CLOSING_YEAR_V2;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP PACKAGE ACCT_JURNAL_RE_V2';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/
