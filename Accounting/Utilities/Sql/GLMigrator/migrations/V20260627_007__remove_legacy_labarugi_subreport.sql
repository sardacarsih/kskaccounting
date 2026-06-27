-- Purpose: Remove legacy Laba Rugi sub-report dependency from ACCT_LAPORAN_V2.

DECLARE
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
    )
    IS
    BEGIN
        ACCT_REPORT_ENGINE_V1.GET_REPORT(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_USERID,
            'LABARUGI',
            p_JENISAKUNTING,
            p_CURSOR
        );
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
    )
    IS
    BEGIN
        ACCT_REPORT_ENGINE_V1.GET_DRILLDOWN(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            'LABARUGI',
            NULL,
            p_KODEACC,
            p_CURSOR
        );
    END LAP_LABARUGI_SUB_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('REMOVED LEGACY LABARUGI SUB-REPORT DEPENDENCY');
END;
/