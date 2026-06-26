-- Purpose: Recreate the legacy COA mutation triggers on ACCT_JURNAL_DTL and leave
--          them DISABLED, restoring the pre-drop state (triggers present but
--          disabled, as left by V20260320_003).
-- Date: 2026-06-26

SET SERVEROUTPUT ON;

CREATE OR REPLACE EDITIONABLE TRIGGER UPDATE_COA_FROM_UPDATE
BEFORE UPDATE OF DEBET,KREDIT ON ACCT_JURNAL_DTL
FOR EACH ROW
WHEN (NEW.DEBET <> OLD.DEBET OR NEW.KREDIT <> OLD.KREDIT)
DECLARE
    DEBET_diff number;
    KREDIT_diff number;
    sIDDATA VARCHAR2(20);
    iTahun integer;
    iBulan integer;
    sKodeAcc VARCHAR2(20);
    sInduk VARCHAR2(20);
    sRekeningAcc VARCHAR2(100);
    nDebet number(18,4);
    nKredit number(18,4);
    iLevel INTEGER;
    sPosisi char(1);
    iRecord integer;
BEGIN
    IF SYS_CONTEXT('USERENV', 'CLIENT_IDENTIFIER') = 'JURNAL_ASYNC_RECALC' THEN
        RETURN;
    END IF;

    DEBET_diff := :NEW.DEBET - :OLD.DEBET;
    KREDIT_diff := :NEW.KREDIT - :OLD.KREDIT;
    sKodeAcc := :OLD.KODE;
    sIDDATA := :OLD.IDDATA;
    iTahun := :OLD.GLYEAR;
    iBulan := :OLD.GLMONTH;

    FOR recDataSaldo IN (
        WITH X AS (
            SELECT KODEACC, PARENTACC, NAMAACC, LVL, POSISI
            FROM ACCT_COA
            WHERE IDDATA = sIDDATA
              AND TAHUN = iTahun
        )
        SELECT KODEACC, PARENTACC, NAMAACC, LVL, POSISI
        FROM X
        START WITH KODEACC = sKodeAcc
        CONNECT BY KODEACC = PRIOR PARENTACC
    ) LOOP
        iRecord := iRecord + 1;
        sKodeAcc := recDataSaldo.KODEACC;
        sInduk := recDataSaldo.PARENTACC;
        sRekeningAcc := recDataSaldo.NAMAACC;
        iLevel := recDataSaldo.LVL;
        sPosisi := recDataSaldo.POSISI;
        nDebet := DEBET_diff;
        nKredit := KREDIT_diff;

        IF (iBulan = 1) THEN
            UPDATE ACCT_COA SET "1D" = "1D" + nDebet, "1K" = "1K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 2) THEN
            UPDATE ACCT_COA SET "2D" = "2D" + nDebet, "2K" = "2K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 3) THEN
            UPDATE ACCT_COA SET "3D" = "3D" + nDebet, "3K" = "3K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 4) THEN
            UPDATE ACCT_COA SET "4D" = "4D" + nDebet, "4K" = "4K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 5) THEN
            UPDATE ACCT_COA SET "5D" = "5D" + nDebet, "5K" = "5K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 6) THEN
            UPDATE ACCT_COA SET "6D" = "6D" + nDebet, "6K" = "6K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 7) THEN
            UPDATE ACCT_COA SET "7D" = "7D" + nDebet, "7K" = "7K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 8) THEN
            UPDATE ACCT_COA SET "8D" = "8D" + nDebet, "8K" = "8K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 9) THEN
            UPDATE ACCT_COA SET "9D" = "9D" + nDebet, "9K" = "9K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 10) THEN
            UPDATE ACCT_COA SET "10D" = "10D" + nDebet, "10K" = "10K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 11) THEN
            UPDATE ACCT_COA SET "11D" = "11D" + nDebet, "11K" = "11K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSE
            UPDATE ACCT_COA SET "12D" = "12D" + nDebet, "12K" = "12K" + nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        END IF;

        ACCT_RECALLCULATIONS.UpdSaldoAkhir_sd_Des_byKODE(sIDDATA, iBulan, iTahun, sKodeAcc);
    END LOOP;
END;
/

CREATE OR REPLACE EDITIONABLE TRIGGER UPDATE_COA_FROM_DELETE
AFTER DELETE ON ACCT_JURNAL_DTL
FOR EACH ROW
DECLARE
    DEBET_DELETED number;
    KREDIT_DELETED number;
    sIDDATA VARCHAR2(20);
    iTahun integer;
    iBulan integer;
    sKodeAcc VARCHAR2(20);
    sInduk VARCHAR2(20);
    sRekeningAcc VARCHAR2(100);
    nDebet number(18,4);
    nKredit number(18,4);
    iLevel INTEGER;
    sPosisi char(1);
    iRecord integer;
BEGIN
    IF SYS_CONTEXT('USERENV', 'CLIENT_IDENTIFIER') = 'JURNAL_ASYNC_RECALC' THEN
        RETURN;
    END IF;

    DEBET_DELETED := :OLD.DEBET;
    KREDIT_DELETED := :OLD.KREDIT;
    sKodeAcc := :OLD.KODE;
    sIDDATA := :OLD.IDDATA;
    iTahun := :OLD.GLYEAR;
    iBulan := :OLD.GLMONTH;

    FOR recDataSaldo IN (
        WITH X AS (
            SELECT KODEACC, PARENTACC, NAMAACC, LVL, POSISI
            FROM ACCT_COA
            WHERE IDDATA = sIDDATA
              AND TAHUN = iTahun
        )
        SELECT KODEACC, PARENTACC, NAMAACC, LVL, POSISI
        FROM X
        START WITH KODEACC = sKodeAcc
        CONNECT BY KODEACC = PRIOR PARENTACC
    ) LOOP
        iRecord := iRecord + 1;
        sKodeAcc := recDataSaldo.KODEACC;
        sInduk := recDataSaldo.PARENTACC;
        sRekeningAcc := recDataSaldo.NAMAACC;
        iLevel := recDataSaldo.LVL;
        sPosisi := recDataSaldo.POSISI;
        nDebet := DEBET_DELETED;
        nKredit := KREDIT_DELETED;

        INSERT INTO ACC_UPDATE_COA_FROMJURNAL (IDDATA, TAHUN, KODEACC, PARENTACC, NAMAACC, LVL, POSISI, DEBET, KREDIT)
        VALUES (sIDDATA, iTahun, sKodeAcc, sInduk, sRekeningAcc, iLevel, sPosisi, nDebet, nKredit);

        IF (iBulan = 1) THEN
            UPDATE ACCT_COA SET "1D" = "1D" - nDebet, "1K" = "1K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 2) THEN
            UPDATE ACCT_COA SET "2D" = "2D" - nDebet, "2K" = "2K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 3) THEN
            UPDATE ACCT_COA SET "3D" = "3D" - nDebet, "3K" = "3K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 4) THEN
            UPDATE ACCT_COA SET "4D" = "4D" - nDebet, "4K" = "4K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 5) THEN
            UPDATE ACCT_COA SET "5D" = "5D" - nDebet, "5K" = "5K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 6) THEN
            UPDATE ACCT_COA SET "6D" = "6D" - nDebet, "6K" = "6K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 7) THEN
            UPDATE ACCT_COA SET "7D" = "7D" - nDebet, "7K" = "7K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 8) THEN
            UPDATE ACCT_COA SET "8D" = "8D" - nDebet, "8K" = "8K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 9) THEN
            UPDATE ACCT_COA SET "9D" = "9D" - nDebet, "9K" = "9K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 10) THEN
            UPDATE ACCT_COA SET "10D" = "10D" - nDebet, "10K" = "10K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSIF (iBulan = 11) THEN
            UPDATE ACCT_COA SET "11D" = "11D" - nDebet, "11K" = "11K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        ELSE
            UPDATE ACCT_COA SET "12D" = "12D" - nDebet, "12K" = "12K" - nKredit WHERE IDDATA = sIDDATA AND TAHUN = iTahun AND KODEACC = sKodeAcc;
        END IF;

        ACCT_RECALLCULATIONS.UpdSaldoAkhir_sd_Des_byKODE(sIDDATA, iBulan, iTahun, sKodeAcc);
    END LOOP;
END;
/

-- Restore the disabled state left by V20260320_003.
BEGIN
    EXECUTE IMMEDIATE 'ALTER TRIGGER UPDATE_COA_FROM_UPDATE DISABLE';
    EXECUTE IMMEDIATE 'ALTER TRIGGER UPDATE_COA_FROM_DELETE DISABLE';
    DBMS_OUTPUT.PUT_LINE('RECREATED AND DISABLED legacy COA triggers');
END;
/
