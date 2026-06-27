-- Rollback display-level behavior to the previous all-descendant engine package.
DECLARE
BEGIN
    UPDATE ACCT_REPORT_SECTION
       SET DISPLAY_LVL = 1
     WHERE REPORT_CODE = 'LABARUGI';
    COMMIT;
END;
/
-- Purpose: Fix ACCT_REPORT_ENGINE_V1 package compile by exposing NetAmount for SQL cursor calls.

DECLARE
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_REPORT_ENGINE_V1 AS
    FUNCTION NetAmount(
        p_debet IN NUMBER,
        p_kredit IN NUMBER,
        p_posisi IN VARCHAR2
    ) RETURN NUMBER;

    PROCEDURE GET_REPORT(
        p_IDDATA          IN  VARCHAR2,
        p_BULAN           IN  INTEGER,
        p_TAHUN           IN  INTEGER,
        p_USERID          IN  VARCHAR2,
        p_REPORT_CODE     IN  VARCHAR2,
        p_JENISAKUNTING   IN  VARCHAR2,
        p_CURSOR          OUT SYS_REFCURSOR
    );

    PROCEDURE GET_DRILLDOWN(
        p_IDDATA          IN  VARCHAR2,
        p_BULAN           IN  INTEGER,
        p_TAHUN           IN  INTEGER,
        p_REPORT_CODE     IN  VARCHAR2,
        p_SECTION_ID      IN  NUMBER,
        p_KODEACC         IN  VARCHAR2,
        p_CURSOR          OUT SYS_REFCURSOR
    );
END ACCT_REPORT_ENGINE_V1;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_REPORT_ENGINE_V1 AS
    FUNCTION NetAmount(
        p_debet IN NUMBER,
        p_kredit IN NUMBER,
        p_posisi IN VARCHAR2
    ) RETURN NUMBER
    IS
    BEGIN
        IF p_posisi = 'K' THEN
            RETURN NVL(p_kredit, 0) - NVL(p_debet, 0);
        END IF;

        RETURN NVL(p_debet, 0) - NVL(p_kredit, 0);
    END NetAmount;

    PROCEDURE ValidateMonth(p_bulan IN INTEGER)
    IS
    BEGIN
        IF p_bulan < 1 OR p_bulan > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan laporan tidak valid: ' || p_bulan);
        END IF;
    END ValidateMonth;

    PROCEDURE GET_REPORT(
        p_IDDATA          IN  VARCHAR2,
        p_BULAN           IN  INTEGER,
        p_TAHUN           IN  INTEGER,
        p_USERID          IN  VARCHAR2,
        p_REPORT_CODE     IN  VARCHAR2,
        p_JENISAKUNTING   IN  VARCHAR2,
        p_CURSOR          OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        ValidateMonth(p_BULAN);

        OPEN p_CURSOR FOR
            WITH section_accounts AS (
                SELECT section.SECTION_ID,
                       section.SECTION_CODE,
                       section.SECTION_NAME,
                       section.DISPLAY_ORDER SECTION_ORDER,
                       NVL(section.NORMAL_POSISI, 'D') NORMAL_POSISI,
                       section.SHOW_ZERO,
                       account.KODEACC_ROOT,
                       account.DISPLAY_ORDER ACCOUNT_ORDER
                  FROM ACCT_REPORT_SECTION section
                  JOIN ACCT_REPORT_SECTION_ACCOUNT account
                    ON account.SECTION_ID = section.SECTION_ID
                 WHERE section.REPORT_CODE = p_REPORT_CODE
                   AND section.IS_ACTIVE = 'Y'
                   AND account.IS_ACTIVE = 'Y'
                   AND account.JENIS_AKUNTING IN ('*', p_JENISAKUNTING)
                   AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
                   AND (account.TAHUN IS NULL OR account.TAHUN = p_TAHUN)
            ),
            coa_tree AS (
                SELECT section_accounts.SECTION_ID,
                       section_accounts.SECTION_CODE,
                       section_accounts.SECTION_NAME,
                       section_accounts.SECTION_ORDER,
                       section_accounts.NORMAL_POSISI,
                       section_accounts.SHOW_ZERO,
                       section_accounts.KODEACC_ROOT,
                       section_accounts.ACCOUNT_ORDER,
                       coa.IDDATA,
                       coa.KODEACC,
                       coa.PARENTACC,
                       coa.NAMAACC,
                       coa.LVL,
                       coa.POSISI,
                       coa.ISHEADER,
                       coa."1D", coa."1K", coa."2D", coa."2K", coa."3D", coa."3K",
                       coa."4D", coa."4K", coa."5D", coa."5K", coa."6D", coa."6K",
                       coa."7D", coa."7K", coa."8D", coa."8K", coa."9D", coa."9K",
                       coa."10D", coa."10K", coa."11D", coa."11K", coa."12D", coa."12K"
                  FROM section_accounts
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = p_IDDATA
                   AND coa.TAHUN = p_TAHUN
                 START WITH coa.KODEACC = section_accounts.KODEACC_ROOT
                CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                       AND PRIOR section_accounts.SECTION_ID = section_accounts.SECTION_ID
                       AND PRIOR section_accounts.KODEACC_ROOT = section_accounts.KODEACC_ROOT
            ),
            calculated AS (
                SELECT coa_tree.IDDATA,
                       coa_tree.KODEACC,
                       coa_tree.SECTION_ORDER * 1000 + coa_tree.ACCOUNT_ORDER + ROW_NUMBER() OVER (PARTITION BY coa_tree.SECTION_ID ORDER BY coa_tree.KODEACC) URUT,
                       coa_tree.SECTION_NAME TIPEACC,
                       coa_tree.SECTION_NAME SUB1,
                       coa_tree.NAMAACC SUB2,
                       CAST(NULL AS VARCHAR2(150)) SUB3,
                       CAST(NULL AS VARCHAR2(150)) SUB4,
                       CAST(NULL AS VARCHAR2(150)) SUB5,
                       CAST(NULL AS VARCHAR2(150)) SUB6,
                       CASE p_BULAN
                           WHEN 1 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."1D", coa_tree."1K", coa_tree.NORMAL_POSISI)
                           WHEN 2 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."2D", coa_tree."2K", coa_tree.NORMAL_POSISI)
                           WHEN 3 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."3D", coa_tree."3K", coa_tree.NORMAL_POSISI)
                           WHEN 4 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."4D", coa_tree."4K", coa_tree.NORMAL_POSISI)
                           WHEN 5 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."5D", coa_tree."5K", coa_tree.NORMAL_POSISI)
                           WHEN 6 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."6D", coa_tree."6K", coa_tree.NORMAL_POSISI)
                           WHEN 7 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."7D", coa_tree."7K", coa_tree.NORMAL_POSISI)
                           WHEN 8 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."8D", coa_tree."8K", coa_tree.NORMAL_POSISI)
                           WHEN 9 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."9D", coa_tree."9K", coa_tree.NORMAL_POSISI)
                           WHEN 10 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."10D", coa_tree."10K", coa_tree.NORMAL_POSISI)
                           WHEN 11 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."11D", coa_tree."11K", coa_tree.NORMAL_POSISI)
                           WHEN 12 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_tree."12D", coa_tree."12K", coa_tree.NORMAL_POSISI)
                       END BULANINI,
                       ACCT_REPORT_ENGINE_V1.NetAmount(
                           NVL(coa_tree."1D", 0)
                           + CASE WHEN p_BULAN >= 2 THEN NVL(coa_tree."2D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 3 THEN NVL(coa_tree."3D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 4 THEN NVL(coa_tree."4D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 5 THEN NVL(coa_tree."5D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 6 THEN NVL(coa_tree."6D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 7 THEN NVL(coa_tree."7D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 8 THEN NVL(coa_tree."8D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 9 THEN NVL(coa_tree."9D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 10 THEN NVL(coa_tree."10D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 11 THEN NVL(coa_tree."11D", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 12 THEN NVL(coa_tree."12D", 0) ELSE 0 END,
                           NVL(coa_tree."1K", 0)
                           + CASE WHEN p_BULAN >= 2 THEN NVL(coa_tree."2K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 3 THEN NVL(coa_tree."3K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 4 THEN NVL(coa_tree."4K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 5 THEN NVL(coa_tree."5K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 6 THEN NVL(coa_tree."6K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 7 THEN NVL(coa_tree."7K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 8 THEN NVL(coa_tree."8K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 9 THEN NVL(coa_tree."9K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 10 THEN NVL(coa_tree."10K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 11 THEN NVL(coa_tree."11K", 0) ELSE 0 END
                           + CASE WHEN p_BULAN >= 12 THEN NVL(coa_tree."12K", 0) ELSE 0 END,
                           coa_tree.NORMAL_POSISI
                       ) TAHUNINI,
                       p_REPORT_CODE JENIS,
                       coa_tree.SECTION_CODE SETSUB,
                       p_USERID USERGEN,
                       coa_tree.ISHEADER,
                       coa_tree.SHOW_ZERO
                  FROM coa_tree
            )
            SELECT IDDATA, KODEACC, URUT, TIPEACC,
                   SUB1, SUB2, SUB3, SUB4, SUB5, SUB6,
                   BULANINI, TAHUNINI, JENIS, SETSUB, USERGEN, ISHEADER
              FROM calculated
             WHERE SHOW_ZERO = 'Y'
                OR BULANINI <> 0
                OR TAHUNINI <> 0
             ORDER BY URUT, KODEACC;
    END GET_REPORT;

    PROCEDURE GET_DRILLDOWN(
        p_IDDATA          IN  VARCHAR2,
        p_BULAN           IN  INTEGER,
        p_TAHUN           IN  INTEGER,
        p_REPORT_CODE     IN  VARCHAR2,
        p_SECTION_ID      IN  NUMBER,
        p_KODEACC         IN  VARCHAR2,
        p_CURSOR          OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        ValidateMonth(p_BULAN);

        OPEN p_CURSOR FOR
            SELECT coa.KODEACC,
                   coa.NAMAACC,
                   coa.PARENTACC,
                   coa.POSISI,
                   coa.ISHEADER,
                   coa.LVL,
                   CASE p_BULAN
                       WHEN 1 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."1D", coa."1K", NVL(coa.POSISI, 'D'))
                       WHEN 2 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."2D", coa."2K", NVL(coa.POSISI, 'D'))
                       WHEN 3 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."3D", coa."3K", NVL(coa.POSISI, 'D'))
                       WHEN 4 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."4D", coa."4K", NVL(coa.POSISI, 'D'))
                       WHEN 5 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."5D", coa."5K", NVL(coa.POSISI, 'D'))
                       WHEN 6 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."6D", coa."6K", NVL(coa.POSISI, 'D'))
                       WHEN 7 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."7D", coa."7K", NVL(coa.POSISI, 'D'))
                       WHEN 8 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."8D", coa."8K", NVL(coa.POSISI, 'D'))
                       WHEN 9 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."9D", coa."9K", NVL(coa.POSISI, 'D'))
                       WHEN 10 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."10D", coa."10K", NVL(coa.POSISI, 'D'))
                       WHEN 11 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."11D", coa."11K", NVL(coa.POSISI, 'D'))
                       WHEN 12 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa."12D", coa."12K", NVL(coa.POSISI, 'D'))
                   END BULANINI
              FROM ACCT_COA coa
             WHERE coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_TAHUN
             START WITH coa.KODEACC = p_KODEACC
            CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
             ORDER BY coa.KODEACC;
    END GET_DRILLDOWN;
END ACCT_REPORT_ENGINE_V1;]';

    DBMS_OUTPUT.PUT_LINE('ROLLED BACK LABARUGI DISPLAY LEVEL ENGINE');
END;
/