-- Purpose:
--   Create new Laba Rugi (Income Statement) report entrypoint ACCT_LAPORAN_V2
--   for the current app, keeping the legacy ACCT_LAPORAN.LAP_LABARUGI intact.
-- Note:
--   Official deployment should use GLMigrator migration V20260627_001.
--   This file is a dev convenience for running directly in SQL*Plus / SQL Developer.

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_LAPORAN_V2 AS
    PROCEDURE LAP_LABARUGI_V2(
        p_IDDATA        IN  VARCHAR2,
        p_BULAN         IN  INTEGER,
        p_TAHUN         IN  INTEGER,
        p_USERID        IN  VARCHAR2,
        p_JENISAKUNTING IN  VARCHAR2,
        p_CURSOR        OUT SYS_REFCURSOR
    );
END ACCT_LAPORAN_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_LAPORAN_V2 AS
    PROCEDURE LAP_LABARUGI_V2(
        p_IDDATA        IN  VARCHAR2,
        p_BULAN         IN  INTEGER,
        p_TAHUN         IN  INTEGER,
        p_USERID        IN  VARCHAR2,
        p_JENISAKUNTING IN  VARCHAR2,
        p_CURSOR        OUT SYS_REFCURSOR
    ) IS
        v_ignore NUMBER;
    BEGIN
        v_ignore := ACCT_LAPORAN.LAP_LABARUGI(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_USERID,
            p_JENISAKUNTING
        );

        OPEN p_CURSOR FOR
            SELECT IDDATA, KODEACC, URUT, TIPEACC,
                   SUB1, SUB2, SUB3, SUB4, SUB5, SUB6,
                   BULANINI, TAHUNINI, JENIS, SETSUB, USERGEN, ISHEADER
              FROM ACC_TMPLRNR
             WHERE IDDATA  = p_IDDATA
               AND USERGEN = p_USERID
             ORDER BY URUT;
    END LAP_LABARUGI_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('CREATED/REPLACED ACCT_LAPORAN_V2');
END;
/
