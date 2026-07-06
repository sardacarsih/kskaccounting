SET SERVEROUTPUT ON;
DECLARE
    PROCEDURE add_column_if_missing(p_table IN VARCHAR2, p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
        l_count INTEGER;
    BEGIN
        SELECT COUNT(1)
          INTO l_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' ADD (' || UPPER(p_column) || ' ' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED ' || UPPER(p_table) || '.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table) || '.' || UPPER(p_column));
        END IF;
    END;
BEGIN
    add_column_if_missing('ACCT_JURNAL_TMP', 'SUMBER', 'VARCHAR2(30)');
    add_column_if_missing('ACCT_JURNAL_TMP', 'DID', 'VARCHAR2(80)');
    add_column_if_missing('ACCT_JURNAL_HDR', 'CREATED_DATE', 'TIMESTAMP DEFAULT SYSTIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_HDR', 'MODIFIED_DATE', 'TIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_DTL', 'CREATED_DATE', 'TIMESTAMP DEFAULT SYSTIMESTAMP');
END;
/

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_JURNAL_V2 AS
    FUNCTION GetJurnalList(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION GetJurnalDetails(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION GetJurnalDetailsV2(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR;
    PROCEDURE HapusJurnal(p_nomorHID IN VARCHAR2);
    FUNCTION ImportJurnalGlobal(p_IDDATA IN VARCHAR2, p_bulan IN INTEGER, p_tahun IN INTEGER, p_periode IN VARCHAR2) RETURN INTEGER;
    FUNCTION ImportJurnalParsial(p_IDDATA IN VARCHAR2, p_bulan IN INTEGER, p_tahun IN INTEGER, p_periode IN VARCHAR2) RETURN INTEGER;
    FUNCTION CekAkunMaster(p_tahun IN INTEGER) RETURN SYS_REFCURSOR;
    FUNCTION CekDuplikasiJurnal RETURN SYS_REFCURSOR;
    FUNCTION CekNoJurnalExist RETURN SYS_REFCURSOR;
    FUNCTION CekRecordJurnalExist(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER;
    FUNCTION CekNoJurnalExist_input(p_IDDATA IN VARCHAR2, nojurnal IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER;
    FUNCTION GetJurnalListEdit(p_nomorHID IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION CekPeriodeExist(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER;
    FUNCTION NOTADEBET(piddata IN VARCHAR2, pperiode IN VARCHAR2, pkodeacc IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION NOTAKREDIT(piddata IN VARCHAR2, pperiode IN VARCHAR2, pkodeacc IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION ExportJurnal(p_iddata IN VARCHAR2, fromDate IN DATE, toDate IN DATE) RETURN SYS_REFCURSOR;
    FUNCTION CekDoubleClosing(piddata IN VARCHAR2, pperiode IN VARCHAR2) RETURN SYS_REFCURSOR;
    FUNCTION CekGlobalJurnalNotBalanced(piddata IN VARCHAR2, pperiode IN VARCHAR2) RETURN SYS_REFCURSOR;
    PROCEDURE UpdateStatusND(p_did IN VARCHAR2);
END ACCT_JURNAL_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_JURNAL_V2 AS
    FUNCTION GetJurnalList(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT h.JURNALID,
                   h.HID,
                   h.NOJURNAL AS "NoJurnal",
                   h.TANGGAL AS "Tanggal",
                   h.PERIODE AS "Periode",
                   h.SUMBER AS "Sumber",
                   h.IDDATA,
                   h.USERID,
                   h.ISRE,
                   h.CREATED_DATE AS "CreatedDate",
                   h.MODIFIED_DATE AS "HeaderVersionUtc"
            FROM ACCT_JURNAL_HDR h
            WHERE h.IDDATA = p_IDDATA
              AND h.PERIODE = p_periode
            ORDER BY h.TANGGAL ASC, h.NOJURNAL ASC, h.JURNALID ASC;
        RETURN cur;
    END GetJurnalList;

    FUNCTION GetJurnalDetails(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT d.REFFID,
                   d.HIDREFF,
                   d.DID,
                   d.NOJURNAL AS "NoJurnal",
                   d.TANGGAL AS "Tanggal",
                   d.BARIS,
                   d.KODE AS "Kode",
                   d.REKENING AS "Rekening",
                   d.DEBET AS "Debet",
                   d.KREDIT AS "Kredit",
                   d.KETERANGAN AS "Keterangan",
                   d.POSTED AS "Posted",
                   d.PERIODE AS "Periode",
                   d.IDDATA,
                   d.USERID,
                   d.SUMBER
            FROM ACCT_JURNAL_DTL d
            WHERE d.IDDATA = p_IDDATA
              AND d.PERIODE = p_periode
            ORDER BY d.NOJURNAL ASC, d.BARIS ASC;
        RETURN cur;
    END GetJurnalDetails;

    FUNCTION GetJurnalDetailsV2(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN SYS_REFCURSOR AS
    BEGIN
        RETURN GetJurnalDetails(p_IDDATA, p_periode);
    END GetJurnalDetailsV2;

    PROCEDURE HapusJurnal(p_nomorHID IN VARCHAR2) AS
    BEGIN
        DELETE FROM ACCT_JURNAL_HDR
        WHERE HID = p_nomorHID;
    END HapusJurnal;

    FUNCTION ImportJurnalGlobal(p_IDDATA IN VARCHAR2, p_bulan IN INTEGER, p_tahun IN INTEGER, p_periode IN VARCHAR2) RETURN INTEGER AS
    BEGIN
        RETURN ImportJurnalParsial(p_IDDATA, p_bulan, p_tahun, p_periode);
    END ImportJurnalGlobal;

    FUNCTION ImportJurnalParsial(p_IDDATA IN VARCHAR2, p_bulan IN INTEGER, p_tahun IN INTEGER, p_periode IN VARCHAR2) RETURN INTEGER AS
        row_count INTEGER;
    BEGIN
        SELECT COUNT(1)
        INTO row_count
        FROM ACCT_JURNAL_TMP
        WHERE IDDATA = p_IDDATA
          AND PERIODE = p_periode;

        IF row_count = 0 THEN
            RETURN 0;
        END IF;

        INSERT INTO ACCT_JURNAL_HDR (HID, NOJURNAL, TANGGAL, PERIODE, IDDATA, USERID, SUMBER, CREATED_DATE)
        SELECT DISTINCT t.IDDATA || t.PERIODE || t.NOJURNAL || TO_CHAR(t.TANGGAL, 'YYMMDD'),
               t.NOJURNAL,
               t.TANGGAL,
               t.PERIODE,
               t.IDDATA,
               t.USERID,
               NVL(t.SUMBER, 'GL'),
               SYSTIMESTAMP
        FROM ACCT_JURNAL_TMP t
        WHERE t.IDDATA = p_IDDATA
          AND t.PERIODE = p_periode
          AND NOT EXISTS (
              SELECT 1
              FROM ACCT_JURNAL_HDR h
              WHERE h.IDDATA = t.IDDATA
                AND h.PERIODE = t.PERIODE
                AND h.NOJURNAL = t.NOJURNAL
                AND h.TANGGAL = t.TANGGAL
          );

        INSERT INTO ACCT_JURNAL_DTL (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID, CREATED_DATE)
        SELECT t.NOJURNAL,
               t.TANGGAL,
               t.BARIS,
               t.KODE,
               t.REKENING,
               NVL(t.DEBET, 0),
               NVL(t.KREDIT, 0),
               t.KETERANGAN,
               NVL(t.POSTED, 'False'),
               t.PERIODE,
               t.IDDATA,
               t.USERID,
               NVL(t.SUMBER, 'GL'),
               NVL(t.DID, h.JURNALID || '-' || TO_CHAR(t.BARIS)),
               p_tahun,
               p_bulan,
               h.HID,
               h.JURNALID,
               SYSTIMESTAMP
        FROM ACCT_JURNAL_TMP t
        JOIN ACCT_JURNAL_HDR h
          ON h.IDDATA = t.IDDATA
         AND h.PERIODE = t.PERIODE
         AND h.NOJURNAL = t.NOJURNAL
         AND h.TANGGAL = t.TANGGAL
        WHERE t.IDDATA = p_IDDATA
          AND t.PERIODE = p_periode
          AND NOT EXISTS (
              SELECT 1
              FROM ACCT_JURNAL_DTL d
              WHERE d.DID = NVL(t.DID, h.JURNALID || '-' || TO_CHAR(t.BARIS))
          );

        RETURN 1;
    END ImportJurnalParsial;

    FUNCTION CekAkunMaster(p_tahun IN INTEGER) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DISTINCT t.KODE AS asal, c.KODEACC AS tujuan
            FROM ACCT_JURNAL_TMP t
            LEFT JOIN ACCT_COA c
              ON c.KODEACC = t.KODE
             AND c.IDDATA = t.IDDATA
             AND c.TAHUN = p_tahun
             AND c.ISHEADER = 'D'
            WHERE c.KODEACC IS NULL
            ORDER BY t.KODE;
        RETURN cur;
    END CekAkunMaster;

    FUNCTION CekDuplikasiJurnal RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT IDDATA, PERIODE, NOJURNAL, TANGGAL, COUNT(1) AS JUMLAH
            FROM ACCT_JURNAL_TMP
            GROUP BY IDDATA, PERIODE, NOJURNAL, TANGGAL
            HAVING COUNT(1) > 1
            ORDER BY IDDATA, PERIODE, NOJURNAL, TANGGAL;
        RETURN cur;
    END CekDuplikasiJurnal;

    FUNCTION CekNoJurnalExist RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DISTINCT t.NOJURNAL, t.TANGGAL, h.NOJURNAL AS sudahada
            FROM ACCT_JURNAL_TMP t
            JOIN ACCT_JURNAL_HDR h
              ON h.IDDATA = t.IDDATA
             AND h.PERIODE = t.PERIODE
             AND h.NOJURNAL = t.NOJURNAL
             AND h.TANGGAL = t.TANGGAL
            ORDER BY t.NOJURNAL, t.TANGGAL;
        RETURN cur;
    END CekNoJurnalExist;

    FUNCTION CekRecordJurnalExist(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER AS
        result INTEGER;
    BEGIN
        SELECT COUNT(1)
        INTO result
        FROM ACCT_JURNAL_HDR
        WHERE IDDATA = p_IDDATA
          AND PERIODE = p_periode;
        RETURN result;
    END CekRecordJurnalExist;

    FUNCTION CekNoJurnalExist_input(p_IDDATA IN VARCHAR2, nojurnal IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER AS
        result INTEGER;
    BEGIN
        SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
        INTO result
        FROM ACCT_JURNAL_HDR
        WHERE IDDATA = p_IDDATA
          AND PERIODE = p_periode
          AND UPPER(NOJURNAL) = UPPER(nojurnal);
        RETURN result;
    END CekNoJurnalExist_input;

    FUNCTION GetJurnalListEdit(p_nomorHID IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT h.*
            FROM ACCT_JURNAL_HDR h
            WHERE h.HID = p_nomorHID;
        RETURN cur;
    END GetJurnalListEdit;

    FUNCTION CekPeriodeExist(p_IDDATA IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER AS
        result INTEGER;
    BEGIN
        SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
        INTO result
        FROM ACCT_PERIODE
        WHERE IDDATA = p_IDDATA
          AND PERIODE = p_periode;
        RETURN result;
    END CekPeriodeExist;

    FUNCTION NOTADEBET(piddata IN VARCHAR2, pperiode IN VARCHAR2, pkodeacc IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DID,
                   POSTED,
                   NOJURNAL AS ND,
                   TANGGAL,
                   NOJURNAL,
                   KETERANGAN,
                   DEBET,
                   CASE WHEN POSTED = 'True' THEN 'Terkirim' ELSE 'Belum Terkirim' END AS STATUS
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = piddata
              AND PERIODE = pperiode
              AND KODE = pkodeacc
              AND NVL(DEBET, 0) <> 0
            ORDER BY TANGGAL, NOJURNAL, BARIS;
        RETURN cur;
    END NOTADEBET;

    FUNCTION NOTAKREDIT(piddata IN VARCHAR2, pperiode IN VARCHAR2, pkodeacc IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DID,
                   POSTED,
                   NOJURNAL AS NK,
                   TANGGAL,
                   NOJURNAL,
                   KETERANGAN,
                   KREDIT,
                   CASE WHEN POSTED = 'True' THEN 'Terkirim' ELSE 'Belum Terkirim' END AS STATUS
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = piddata
              AND PERIODE = pperiode
              AND KODE = pkodeacc
              AND NVL(KREDIT, 0) <> 0
            ORDER BY TANGGAL, NOJURNAL, BARIS;
        RETURN cur;
    END NOTAKREDIT;

    FUNCTION ExportJurnal(p_iddata IN VARCHAR2, fromDate IN DATE, toDate IN DATE) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, DID
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = p_iddata
              AND TANGGAL >= TRUNC(fromDate)
              AND TANGGAL < TRUNC(toDate) + 1
            ORDER BY TANGGAL, NOJURNAL, BARIS;
        RETURN cur;
    END ExportJurnal;

    FUNCTION CekDoubleClosing(piddata IN VARCHAR2, pperiode IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT NOJURNAL, TANGGAL, SUM(NVL(DEBET, 0)) AS DEBET, SUM(NVL(KREDIT, 0)) AS KREDIT,
                   SUM(NVL(DEBET, 0)) - SUM(NVL(KREDIT, 0)) AS selisih
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = piddata
              AND PERIODE = pperiode
              AND (UPPER(NOJURNAL) LIKE '%CLOSE%' OR UPPER(NOJURNAL) LIKE '%CLOSING%')
            GROUP BY NOJURNAL, TANGGAL
            ORDER BY TANGGAL, NOJURNAL;
        RETURN cur;
    END CekDoubleClosing;

    FUNCTION CekGlobalJurnalNotBalanced(piddata IN VARCHAR2, pperiode IN VARCHAR2) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT NOJURNAL, TANGGAL, SUM(NVL(DEBET, 0)) AS DEBET, SUM(NVL(KREDIT, 0)) AS KREDIT,
                   SUM(NVL(DEBET, 0)) - SUM(NVL(KREDIT, 0)) AS selisih
            FROM ACCT_JURNAL_DTL
            WHERE IDDATA = piddata
              AND PERIODE = pperiode
            GROUP BY NOJURNAL, TANGGAL
            HAVING ROUND(SUM(NVL(DEBET, 0)) - SUM(NVL(KREDIT, 0)), 2) <> 0
            ORDER BY TANGGAL, NOJURNAL;
        RETURN cur;
    END CekGlobalJurnalNotBalanced;

    PROCEDURE UpdateStatusND(p_did IN VARCHAR2) AS
    BEGIN
        UPDATE ACCT_JURNAL_DTL
        SET POSTED = 'True'
        WHERE DID = p_did
          AND NVL(POSTED, 'False') <> 'True';
    END UpdateStatusND;
END ACCT_JURNAL_V2;]';
END;
/

SHOW ERRORS PACKAGE ACCT_JURNAL_V2;
SHOW ERRORS PACKAGE BODY ACCT_JURNAL_V2;

DECLARE
    l_error_count INTEGER;
    l_messages VARCHAR2(4000);
BEGIN
    SELECT COUNT(1)
      INTO l_error_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF l_error_count > 0 THEN
        FOR err IN (
            SELECT NAME, TYPE, LINE, POSITION, TEXT
              FROM USER_ERRORS
             WHERE NAME = 'ACCT_JURNAL_V2'
             ORDER BY SEQUENCE
        ) LOOP
            DBMS_OUTPUT.PUT_LINE(err.NAME || ' ' || err.TYPE || ' line ' || err.LINE || ':' || err.POSITION || ' - ' || err.TEXT);
            l_messages := SUBSTR(l_messages || err.TYPE || ' line ' || err.LINE || ':' || err.POSITION || ' - ' || err.TEXT || '; ', 1, 3500);
        END LOOP;

        RAISE_APPLICATION_ERROR(-20001, 'ACCT_JURNAL_V2 package is invalid after repair: ' || l_messages);
    END IF;
END;
/
