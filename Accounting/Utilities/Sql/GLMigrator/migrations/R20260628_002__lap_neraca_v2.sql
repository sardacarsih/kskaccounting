-- Purpose: Rollback Neraca metadata and restore ACCT_LAPORAN_V2 to use legacy function delegation.
-- Date: 2026-06-28

SET SERVEROUTPUT ON;

BEGIN
    -- 1. Remove metadata
    DELETE FROM ACCT_REPORT_SECTION_ACCOUNT 
     WHERE SECTION_ID IN (SELECT SECTION_ID FROM ACCT_REPORT_SECTION WHERE REPORT_CODE = 'NERACA');
    
    DELETE FROM ACCT_REPORT_SECTION WHERE REPORT_CODE = 'NERACA';
    
    DELETE FROM ACCT_REPORT_DEF WHERE REPORT_CODE = 'NERACA';

    -- 2. Restore ACCT_LAPORAN_V2 package header without LAP_NERACA_SUB_V2
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

    PROCEDURE LAP_NERACA_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    );
END ACCT_LAPORAN_V2;]';

    -- 3. Restore ACCT_LAPORAN_V2 package body with legacy function delegation
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

    PROCEDURE LAP_NERACA_V2(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        p_CURSOR := ACCT_LAPORAN.LAP_NERACA(
            p_IDDATA,
            p_BULAN,
            p_TAHUN,
            p_USERID
        );
    END LAP_NERACA_V2;
END ACCT_LAPORAN_V2;]';

    DBMS_OUTPUT.PUT_LINE('ROLLBACK COMPLETE: Restored legacy LAP_NERACA delegation');
END;
/
