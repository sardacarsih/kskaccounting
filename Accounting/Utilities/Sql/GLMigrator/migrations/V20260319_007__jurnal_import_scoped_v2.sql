-- Purpose: Add scoped import package for jurnal import to avoid cross-user collision on ACCT_JURNAL_TMP.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_JURNAL_IMPORT_V2 AS
    FUNCTION CekAkunMaster(
        p_tahun   IN INTEGER,
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR;

    FUNCTION CekNoJurnalExist(
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR;

    FUNCTION CekJurnalKodeNull(
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR;

    FUNCTION ImportJurnalParsial(
        p_IDDATA  IN VARCHAR2,
        p_bulan   IN INTEGER,
        p_tahun   IN INTEGER,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN INTEGER;
END ACCT_JURNAL_IMPORT_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_JURNAL_IMPORT_V2 AS
    FUNCTION CekAkunMaster(
        p_tahun   IN INTEGER,
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DISTINCT a.kode AS asal, t.kodeacc AS tujuan
            FROM acct_jurnal_tmp a
            LEFT JOIN acct_coa t
                ON a.kode = t.kodeacc
               AND a.iddata = t.iddata
               AND t.tahun = p_tahun
               AND t.isheader = 'D'
            WHERE a.iddata = p_iddata
              AND a.periode = p_periode
              AND a.userid = p_userid
              AND t.kodeacc IS NULL
            ORDER BY a.kode ASC;

        RETURN cur;
    END CekAkunMaster;

    FUNCTION CekNoJurnalExist(
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT DISTINCT t.nojurnal, t.tanggal, j.nojurnal AS sudahada
            FROM acct_jurnal_tmp t
            LEFT JOIN acct_jurnal_hdr j
                ON t.iddata = j.iddata
               AND t.periode = j.periode
               AND t.nojurnal = j.nojurnal
               AND t.tanggal = j.tanggal
            WHERE t.iddata = p_iddata
              AND t.periode = p_periode
              AND t.userid = p_userid
              AND j.nojurnal IS NOT NULL
            ORDER BY t.nojurnal ASC;

        RETURN cur;
    END CekNoJurnalExist;

    FUNCTION CekJurnalKodeNull(
        p_iddata  IN VARCHAR2,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN SYS_REFCURSOR AS
        cur SYS_REFCURSOR;
    BEGIN
        OPEN cur FOR
            SELECT nojurnal, tanggal, kode
            FROM acct_jurnal_tmp
            WHERE iddata = p_iddata
              AND periode = p_periode
              AND userid = p_userid
              AND kode IS NULL
            ORDER BY nojurnal, tanggal;

        RETURN cur;
    END CekJurnalKodeNull;

    FUNCTION ImportJurnalParsial(
        p_IDDATA  IN VARCHAR2,
        p_bulan   IN INTEGER,
        p_tahun   IN INTEGER,
        p_periode IN VARCHAR2,
        p_userid  IN VARCHAR2
    ) RETURN INTEGER AS
        ISDUPLIKAT INTEGER;
        ISSUKSES   NUMBER;
        BALANCE    NUMBER(18, 4);

        sHID       VARCHAR2(65);
        sHIDREFF   VARCHAR2(65);
        sDID       VARCHAR2(80);
        sNOJURNAL  VARCHAR2(30);
        dTANGGAL   DATE;
        iBARIS     INT;
        inumber    INT;
        sKODE      VARCHAR2(30);
        sREKENING  VARCHAR2(100);
        nDEBET     NUMBER(18, 4);
        nKREDIT    NUMBER(18, 4);
        sKETERANGAN VARCHAR2(200);
        sPOSTED    VARCHAR2(30);
        sPERIODE   VARCHAR2(30);
        sIDDATA    VARCHAR2(30);
        sUSERID    VARCHAR2(30);
        sSUMBER    VARCHAR2(30);
        iGLYEAR    INT;
        iGLMONTH   INT;

        CURSOR JURNAL_DTL IS
            SELECT J.NOJURNAL,
                   J.TANGGAL,
                   J.BARIS,
                   J.KODE,
                   A.NAMAACC AS REKENING,
                   J.DEBET,
                   J.KREDIT,
                   J.KETERANGAN,
                   J.POSTED,
                   J.PERIODE,
                   J.IDDATA,
                   J.USERID,
                   'IMPORT',
                   J.GLYEAR,
                   J.GLMONTH
            FROM ACCT_JURNAL_TMP J
            JOIN ACCT_COA A
              ON A.KODEACC = J.KODE
             AND A.IDDATA = J.IDDATA
            WHERE J.IDDATA = p_IDDATA
              AND J.PERIODE = p_periode
              AND J.USERID = p_userid
              AND A.TAHUN = p_tahun
            ORDER BY J.NOJURNAL ASC;

        CURSOR JURNAL_HDR IS
            SELECT DISTINCT IDDATA, NOJURNAL, TANGGAL, PERIODE
            FROM ACCT_JURNAL_TMP
            WHERE IDDATA = p_IDDATA
              AND PERIODE = p_periode
              AND USERID = p_userid;
    BEGIN
        SELECT COUNT(DISTINCT periode)
          INTO ISDUPLIKAT
          FROM acct_jurnal_tmp
         WHERE iddata = p_IDDATA
           AND userid = p_userid
           AND periode = p_periode;

        SELECT NVL(SUM(DEBET) - SUM(KREDIT), 0)
          INTO BALANCE
          FROM ACCT_JURNAL_TMP
         WHERE iddata = p_IDDATA
           AND userid = p_userid
           AND periode = p_periode;

        IF ISDUPLIKAT > 1 THEN
            ISSUKSES := 0; -- duplikasi periode
        ELSIF BALANCE <> 0 THEN
            ISSUKSES := 1; -- jurnal tidak seimbang
        ELSE
            ISSUKSES := 99;

            OPEN JURNAL_HDR;
            LOOP
                FETCH JURNAL_HDR INTO sIDDATA, sNOJURNAL, dTANGGAL, sPERIODE;
                EXIT WHEN JURNAL_HDR%NOTFOUND;
                sHID := p_IDDATA || p_periode || sNOJURNAL || TO_CHAR(dTANGGAL, 'yyMMdd');

                INSERT INTO ACCT_JURNAL_HDR(HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, PC, IP_ADD)
                VALUES(sHID, sIDDATA, sNOJURNAL, dTANGGAL, sPERIODE, 'IMPORT', SYS_CONTEXT('USERENV', 'HOST'), SYS_CONTEXT('USERENV', 'IP_ADDRESS'));
            END LOOP;
            CLOSE JURNAL_HDR;

            inumber := 0;
            OPEN JURNAL_DTL;
            LOOP
                inumber := inumber + 1;
                FETCH JURNAL_DTL INTO sNOJURNAL, dTANGGAL, iBARIS, sKODE, sREKENING, nDEBET, nKREDIT, sKETERANGAN, sPOSTED, sPERIODE, sIDDATA, sUSERID, sSUMBER, iGLYEAR, iGLMONTH;
                EXIT WHEN JURNAL_DTL%NOTFOUND;

                sHIDREFF := p_IDDATA || p_periode || sNOJURNAL || TO_CHAR(dTANGGAL, 'yyMMdd');
                sDID := p_IDDATA || p_periode || TO_CHAR(dTANGGAL, 'yyMMdd') || sNOJURNAL || iBARIS || TRIM(TO_CHAR(inumber, '000000'));

                IF (nDEBET < 0 OR nKREDIT < 0) THEN
                    RAISE_APPLICATION_ERROR(-20201, 'NO JURNAL: ' || sNOJURNAL || ' TANGGAL: ' || dTANGGAL || ' NILAI TRANSAKSI JURNAL TIDAK BOLEH MINUS');
                END IF;

                INSERT INTO ACCT_JURNAL_DTL(
                    DID, HIDREFF, NOJURNAL, TANGGAL, BARIS, KODE, REKENING,
                    DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA,
                    USERID, SUMBER, GLYEAR, GLMONTH)
                VALUES(
                    sDID, sHIDREFF, sNOJURNAL, dTANGGAL, iBARIS, sKODE, sREKENING,
                    nDEBET, nKREDIT, sKETERANGAN, sPOSTED, sPERIODE, sIDDATA,
                    sUSERID, sSUMBER, iGLYEAR, iGLMONTH);
            END LOOP;
            CLOSE JURNAL_DTL;

            MERGE INTO ACCT_JURNAL_DTL detail
            USING (
                SELECT HID, jurnalid
                FROM ACCT_JURNAL_HDR
                WHERE IDDATA = p_IDDATA
                  AND PERIODE = p_periode
            ) header
               ON (detail.HIDREFF = header.HID)
            WHEN MATCHED THEN
                UPDATE SET detail.REFFID = header.JURNALID
                 WHERE detail.IDDATA = p_IDDATA
                   AND detail.PERIODE = p_periode
                   AND detail.USERID = p_userid;
        END IF;

        COMMIT;
        RETURN ISSUKSES;
    END ImportJurnalParsial;
END ACCT_JURNAL_IMPORT_V2;]';

    DBMS_OUTPUT.PUT_LINE('CREATED PACKAGE ACCT_JURNAL_IMPORT_V2');
END;
/
