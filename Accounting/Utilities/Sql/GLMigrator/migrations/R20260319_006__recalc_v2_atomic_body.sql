-- Purpose: Rollback ACCT_RECALLCULATIONS_V2 body to wrapper/delegating mode.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_RECALLCULATIONS_V2 AS
    PROCEDURE ReCalcByJurnalID(
        p_IDDATA   IN VARCHAR2,
        p_BULAN    IN INTEGER,
        p_TAHUN    IN INTEGER,
        p_JURNALID IN NUMBER,
        p_PERIODE  IN VARCHAR2,
        p_USERID   IN VARCHAR2
    ) IS
    BEGIN
        ACCT_RECALLCULATIONS.ReCalcByNoJurnalID(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_JURNALID,
            p_PERIODE,
            p_USERID
        );
    END ReCalcByJurnalID;
END ACCT_RECALLCULATIONS_V2;]';

    DBMS_OUTPUT.PUT_LINE('ROLLED BACK PACKAGE BODY ACCT_RECALLCULATIONS_V2 to wrapper mode');
END;
/
