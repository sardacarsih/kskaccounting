-- Purpose: Replace DBMS_LOCK dependency in ACCT_LAPORAN_V2 with schema-owned table locking.
-- Date: 2026-06-27

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER := 0;
BEGIN
    SELECT COUNT(1) INTO v_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_REPORT_LOCK';

    IF v_count = 0 THEN
        EXECUTE IMMEDIATE 'CREATE TABLE ACCT_REPORT_LOCK (LOCK_NAME VARCHAR2(40) PRIMARY KEY, CREATED_AT DATE DEFAULT SYSDATE NOT NULL)';
    END IF;

    EXECUTE IMMEDIATE 'MERGE INTO ACCT_REPORT_LOCK target USING (SELECT ''LABARUGI'' LOCK_NAME FROM DUAL) source ON (target.LOCK_NAME = source.LOCK_NAME) WHEN NOT MATCHED THEN INSERT (LOCK_NAME) VALUES (source.LOCK_NAME)';
    COMMIT;

    DBMS_OUTPUT.PUT_LINE('ENSURED ACCT_REPORT_LOCK TABLE AND LABARUGI LOCK ROW');
END;
/

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
END ACCT_LAPORAN_V2;
/

CREATE OR REPLACE PACKAGE BODY ACCT_LAPORAN_V2 AS
    PROCEDURE AcquireReportLock IS
    BEGIN
        LOCK TABLE ACCT_REPORT_LOCK IN EXCLUSIVE MODE WAIT 120;
    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20081, 'Tidak dapat mengunci staging laporan Laba Rugi: ' || SQLERRM);
    END AcquireReportLock;

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
        AcquireReportLock;

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
        AcquireReportLock;

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
END ACCT_LAPORAN_V2;
/

BEGIN
    DBMS_OUTPUT.PUT_LINE('UPDATED PACKAGE ACCT_LAPORAN_V2 WITH TABLE LOCKING');
END;
/
