-- Purpose: Roll back ACCT_LAPORAN_V2 table locking while keeping the V2 main/sub cursor API.
-- Date: 2026-06-27

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

    PROCEDURE LAP_LABARUGI_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_LAP     IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
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

    PROCEDURE LAP_LABARUGI_SUB_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_KODEACC IN  VARCHAR2,
        p_USERID  IN  VARCHAR2,
        p_LAP     IN  VARCHAR2,
        p_POSISI  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    ) IS
        v_ignore NUMBER;
    BEGIN
        v_ignore := ACCT_LAPORAN.ACC_GENREP_LRNR_SUB(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_KODEACC,
            p_USERID,
            p_LAP,
            p_POSISI
        );

        OPEN p_CURSOR FOR
            SELECT *
              FROM ACC_SUB_REPORT
             WHERE IDDATA  = p_IDDATA
               AND GENUSER = p_USERID;
    END LAP_LABARUGI_SUB_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('ROLLED BACK ACCT_LAPORAN_V2 TABLE LOCKING');
END;
/