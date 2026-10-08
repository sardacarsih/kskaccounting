-- Purpose: repair the deployed COA group drilldown cursor contract without altering prior migration history.
DECLARE
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_COA_DRILLDOWN_V1 AS
    PROCEDURE GET_CHILDREN(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_KODEACC IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    );
END ACCT_COA_DRILLDOWN_V1;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_COA_DRILLDOWN_V1 AS
    PROCEDURE GET_CHILDREN(
        p_IDDATA  IN  VARCHAR2,
        p_BULAN   IN  INTEGER,
        p_TAHUN   IN  INTEGER,
        p_USERID  IN  VARCHAR2,
        p_KODEACC IN  VARCHAR2,
        p_CURSOR  OUT SYS_REFCURSOR
    )
    IS
        v_account_count INTEGER := 0;
        v_kodeacc       VARCHAR2(30) := TRIM(p_KODEACC);
    BEGIN
        IF p_BULAN < 1 OR p_BULAN > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan COA drilldown tidak valid: ' || p_BULAN);
        END IF;

        IF TRIM(p_USERID) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20092, 'User COA drilldown wajib diisi.');
        END IF;

        IF v_kodeacc IS NULL THEN
            RAISE_APPLICATION_ERROR(-20093, 'Kode akun COA drilldown wajib diisi.');
        END IF;

        SELECT COUNT(*)
          INTO v_account_count
          FROM ACCT_COA coa
         WHERE coa.IDDATA = p_IDDATA
           AND coa.TAHUN = p_TAHUN
           AND coa.KODEACC = v_kodeacc;

        IF v_account_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20094, 'Akun COA tidak ditemukan: ' || v_kodeacc);
        END IF;

        OPEN p_CURSOR FOR
            SELECT coa.KODEACC    AS KODEACC,
                   coa.NAMAACC    AS NAMAACC,
                   coa.PARENTACC  AS PARENTACC,
                   NVL(coa.POSISI, 'D') AS POSISI,
                   CASE WHEN coa.ISHEADER = 'G' THEN 'G' ELSE 'D' END AS ISHEADER,
                   CASE p_BULAN
                       WHEN 1 THEN NVL(coa."1D", 0) WHEN 2 THEN NVL(coa."2D", 0)
                       WHEN 3 THEN NVL(coa."3D", 0) WHEN 4 THEN NVL(coa."4D", 0)
                       WHEN 5 THEN NVL(coa."5D", 0) WHEN 6 THEN NVL(coa."6D", 0)
                       WHEN 7 THEN NVL(coa."7D", 0) WHEN 8 THEN NVL(coa."8D", 0)
                       WHEN 9 THEN NVL(coa."9D", 0) WHEN 10 THEN NVL(coa."10D", 0)
                       WHEN 11 THEN NVL(coa."11D", 0) ELSE NVL(coa."12D", 0)
                   END AS DEBET,
                   CASE p_BULAN
                       WHEN 1 THEN NVL(coa."1K", 0) WHEN 2 THEN NVL(coa."2K", 0)
                       WHEN 3 THEN NVL(coa."3K", 0) WHEN 4 THEN NVL(coa."4K", 0)
                       WHEN 5 THEN NVL(coa."5K", 0) WHEN 6 THEN NVL(coa."6K", 0)
                       WHEN 7 THEN NVL(coa."7K", 0) WHEN 8 THEN NVL(coa."8K", 0)
                       WHEN 9 THEN NVL(coa."9K", 0) WHEN 10 THEN NVL(coa."10K", 0)
                       WHEN 11 THEN NVL(coa."11K", 0) ELSE NVL(coa."12K", 0)
                   END AS KREDIT,
                   CASE p_BULAN
                       WHEN 1 THEN NVL(coa."1S", 0) WHEN 2 THEN NVL(coa."2S", 0)
                       WHEN 3 THEN NVL(coa."3S", 0) WHEN 4 THEN NVL(coa."4S", 0)
                       WHEN 5 THEN NVL(coa."5S", 0) WHEN 6 THEN NVL(coa."6S", 0)
                       WHEN 7 THEN NVL(coa."7S", 0) WHEN 8 THEN NVL(coa."8S", 0)
                       WHEN 9 THEN NVL(coa."9S", 0) WHEN 10 THEN NVL(coa."10S", 0)
                       WHEN 11 THEN NVL(coa."11S", 0) ELSE NVL(coa."12S", 0)
                   END AS SALDOAKHIR
              FROM ACCT_COA coa
             WHERE coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_TAHUN
             START WITH coa.KODEACC = v_kodeacc
           CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                  AND PRIOR coa.IDDATA = coa.IDDATA
                  AND PRIOR coa.TAHUN = coa.TAHUN
             ORDER SIBLINGS BY coa.KODEACC;
    END GET_CHILDREN;
END ACCT_COA_DRILLDOWN_V1;]';
END;
/
