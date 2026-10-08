-- Purpose: Seed legacy PKS Laba Rugi structure into the metadata report settings.
--          The structure follows ACCT_LAPX_PKS while keeping the current V2 facade.
-- Date: 2026-07-10

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND COLUMN_NAME = 'MATCH_MODE';

    IF v_count = 0 THEN
        EXECUTE IMMEDIATE q'[
            ALTER TABLE ACCT_REPORT_SECTION_ACCOUNT
              ADD MATCH_MODE VARCHAR2(20) DEFAULT 'TREE' NOT NULL]';
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND COLUMN_NAME = 'GRP_CODE';

    IF v_count = 0 THEN
        EXECUTE IMMEDIATE q'[
            ALTER TABLE ACCT_REPORT_SECTION_ACCOUNT
              ADD GRP_CODE VARCHAR2(20)]';
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_CONSTRAINTS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND CONSTRAINT_NAME = 'CK_ACCT_REPORT_SEC_ACC_MATCH';

    IF v_count = 0 THEN
        EXECUTE IMMEDIATE q'[
            ALTER TABLE ACCT_REPORT_SECTION_ACCOUNT
              ADD CONSTRAINT CK_ACCT_REPORT_SEC_ACC_MATCH
              CHECK (MATCH_MODE IN ('TREE', 'PARENT', 'GRP_LVL'))]';
    END IF;

    MERGE INTO ACCT_REPORT_SECTION target
    USING (
        SELECT 'LABARUGI' REPORT_CODE, 'PKS_P1' SECTION_CODE, 'PENJUALAN' SECTION_NAME, 10 DISPLAY_ORDER, 'K' NORMAL_POSISI, 3 DISPLAY_LVL FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_HPP', 'HARGA POKOK PENJUALAN', 20, 'D', 3 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B1', 'BIAYA PENJUALAN', 30, 'D', 5 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B5', 'BIAYA OPERASIONAL TANKI TIMBUN', 40, 'D', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_P2', 'PENDAPATAN DILUAR USAHA', 50, 'K', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B6', 'BIAYA DILUAR USAHA', 60, 'D', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B3', 'BIAYA BUNGA', 70, 'D', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_PPH', 'PPH BADAN', 80, 'D', 3 FROM DUAL
    ) source
       ON (target.REPORT_CODE = source.REPORT_CODE AND target.SECTION_CODE = source.SECTION_CODE)
     WHEN MATCHED THEN
        UPDATE SET target.SECTION_NAME = source.SECTION_NAME,
                   target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.NORMAL_POSISI = source.NORMAL_POSISI,
                   target.DISPLAY_LVL = source.DISPLAY_LVL,
                   target.IS_ACTIVE = 'Y'
     WHEN NOT MATCHED THEN
        INSERT (REPORT_CODE, SECTION_CODE, SECTION_NAME, DISPLAY_ORDER, NORMAL_POSISI, DISPLAY_LVL)
        VALUES (source.REPORT_CODE, source.SECTION_CODE, source.SECTION_NAME, source.DISPLAY_ORDER, source.NORMAL_POSISI, source.DISPLAY_LVL);

    EXECUTE IMMEDIATE q'[
MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
    USING (
        SELECT section.SECTION_ID, 'PKS' JENIS_AKUNTING, CAST(NULL AS VARCHAR2(20)) IDDATA, CAST(NULL AS NUMBER) TAHUN,
               'GRP:11' KODEACC_ROOT, 10 DISPLAY_ORDER, 'GRP_LVL' MATCH_MODE, '11' GRP_CODE
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_P1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, 'GRP:12', 10, 'GRP_LVL', '12'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_HPP'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.11007.000', 10, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.12007.000', 20, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.13000.000', 30, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.21007.000', 40, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.22007.000', 50, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '89.23007.000', 60, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B1'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, '85.01007.000', 10, 'PARENT', NULL
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B5'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, 'GRP:16', 10, 'GRP_LVL', '16'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_P2'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, 'GRP:17', 10, 'GRP_LVL', '17'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B6'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, 'GRP:15', 10, 'GRP_LVL', '15'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_B3'
        UNION ALL SELECT section.SECTION_ID, 'PKS', NULL, NULL, 'GRP:18', 10, 'GRP_LVL', '18'
          FROM ACCT_REPORT_SECTION section WHERE section.REPORT_CODE = 'LABARUGI' AND section.SECTION_CODE = 'PKS_PPH'
    ) source
       ON (target.SECTION_ID = source.SECTION_ID
       AND target.JENIS_AKUNTING = source.JENIS_AKUNTING
       AND NVL(target.IDDATA, '*') = NVL(source.IDDATA, '*')
       AND NVL(target.TAHUN, 0) = NVL(source.TAHUN, 0)
       AND target.KODEACC_ROOT = source.KODEACC_ROOT)
     WHEN MATCHED THEN
        UPDATE SET target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.MATCH_MODE = source.MATCH_MODE,
                   target.GRP_CODE = source.GRP_CODE,
                   target.INCLUDE_CHILDREN = 'Y',
                   target.IS_ACTIVE = 'Y'
     WHEN NOT MATCHED THEN
        INSERT (SECTION_ID, JENIS_AKUNTING, IDDATA, TAHUN, KODEACC_ROOT, DISPLAY_ORDER, INCLUDE_CHILDREN, IS_ACTIVE, MATCH_MODE, GRP_CODE)
        VALUES (source.SECTION_ID, source.JENIS_AKUNTING, source.IDDATA, source.TAHUN, source.KODEACC_ROOT, source.DISPLAY_ORDER, 'Y', 'Y', source.MATCH_MODE, source.GRP_CODE)]';

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('SEEDED LEGACY PKS LABARUGI SETTINGS');
END;
/

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
                       NVL(section.DISPLAY_LVL, 1) DISPLAY_LVL,
                       NVL(section.NORMAL_POSISI, 'D') NORMAL_POSISI,
                       section.SHOW_ZERO,
                       account.KODEACC_ROOT,
                       account.DISPLAY_ORDER ACCOUNT_ORDER,
                       NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
                       account.GRP_CODE
                  FROM ACCT_REPORT_SECTION section
                  JOIN ACCT_REPORT_SECTION_ACCOUNT account
                    ON account.SECTION_ID = section.SECTION_ID
                 WHERE section.REPORT_CODE = p_REPORT_CODE
                   AND section.IS_ACTIVE = 'Y'
                   AND account.IS_ACTIVE = 'Y'
                   AND (
                        (p_JENISAKUNTING = 'PKS' AND account.JENIS_AKUNTING = 'PKS')
                        OR
                        (p_JENISAKUNTING <> 'PKS' AND account.JENIS_AKUNTING IN ('*', p_JENISAKUNTING))
                   )
                   AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
                   AND (account.TAHUN IS NULL OR account.TAHUN = p_TAHUN)
            ),
            tree_rows AS (
                SELECT section_accounts.SECTION_ID,
                       section_accounts.SECTION_CODE,
                       section_accounts.SECTION_NAME,
                       section_accounts.SECTION_ORDER,
                       section_accounts.DISPLAY_LVL,
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
                   AND section_accounts.MATCH_MODE = 'TREE'
                 START WITH coa.KODEACC = section_accounts.KODEACC_ROOT
                CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                       AND PRIOR section_accounts.SECTION_ID = section_accounts.SECTION_ID
                       AND PRIOR section_accounts.KODEACC_ROOT = section_accounts.KODEACC_ROOT
            ),
            parent_rows AS (
                SELECT section_accounts.SECTION_ID,
                       section_accounts.SECTION_CODE,
                       section_accounts.SECTION_NAME,
                       section_accounts.SECTION_ORDER,
                       section_accounts.DISPLAY_LVL,
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
                   AND coa.PARENTACC = section_accounts.KODEACC_ROOT
                 WHERE section_accounts.MATCH_MODE = 'PARENT'
            ),
            grp_rows AS (
                SELECT section_accounts.SECTION_ID,
                       section_accounts.SECTION_CODE,
                       section_accounts.SECTION_NAME,
                       section_accounts.SECTION_ORDER,
                       section_accounts.DISPLAY_LVL,
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
                   AND coa.GRP = section_accounts.GRP_CODE
                 WHERE section_accounts.MATCH_MODE = 'GRP_LVL'
            ),
            coa_tree AS (
                SELECT * FROM tree_rows
                UNION ALL
                SELECT * FROM parent_rows
                UNION ALL
                SELECT * FROM grp_rows
            ),
            calculated AS (
                SELECT coa_tree.IDDATA,
                       coa_tree.KODEACC,
                       coa_tree.SECTION_ORDER * 1000 + coa_tree.ACCOUNT_ORDER + ROW_NUMBER() OVER (PARTITION BY coa_tree.SECTION_ID ORDER BY coa_tree.KODEACC) URUT,
                       coa_tree.SECTION_NAME TIPEACC,
                       coa_tree.NAMAACC SUB1,
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
                       coa_tree.NORMAL_POSISI POSISI,
                       coa_tree.SHOW_ZERO
                  FROM coa_tree
                 WHERE coa_tree.LVL = coa_tree.DISPLAY_LVL
            )
            SELECT IDDATA, KODEACC, URUT, TIPEACC,
                   SUB1, SUB2, SUB3, SUB4, SUB5, SUB6,
                   BULANINI, TAHUNINI, JENIS, SETSUB, USERGEN, ISHEADER, POSISI
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

    DBMS_OUTPUT.PUT_LINE('UPDATED ACCT_REPORT_ENGINE_V1 FOR PKS LEGACY SETTINGS');
END;
/

DECLARE
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_JURNAL_CLOSING_V2 AS
    FUNCTION JURNAL_CLOSING(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2,
        p_commit        IN CHAR DEFAULT 'Y'
    ) RETURN NUMBER;
END ACCT_JURNAL_CLOSING_V2;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_JURNAL_CLOSING_V2 AS
    FUNCTION CALC_LABARUGI(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2
    ) RETURN NUMBER
    IS
        v_net_laba NUMBER := 0;
    BEGIN
        WITH section_accounts AS (
            SELECT section.SECTION_ID,
                   NVL(section.DISPLAY_LVL, 1) DISPLAY_LVL,
                   NVL(section.NORMAL_POSISI, 'D') NORMAL_POSISI,
                   account.KODEACC_ROOT,
                   NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
                   account.GRP_CODE
              FROM ACCT_REPORT_SECTION section
              JOIN ACCT_REPORT_SECTION_ACCOUNT account
                ON account.SECTION_ID = section.SECTION_ID
             WHERE section.REPORT_CODE = 'LABARUGI'
               AND section.IS_ACTIVE = 'Y'
               AND account.IS_ACTIVE = 'Y'
               AND (
                    (p_jenisakunting = 'PKS' AND account.JENIS_AKUNTING = 'PKS')
                    OR
                    (p_jenisakunting <> 'PKS' AND account.JENIS_AKUNTING IN ('*', p_jenisakunting))
               )
               AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
               AND (account.TAHUN IS NULL OR account.TAHUN = p_tahun)
        ),
        tree_rows AS (
            SELECT section_accounts.SECTION_ID,
                   section_accounts.NORMAL_POSISI,
                   section_accounts.DISPLAY_LVL,
                   section_accounts.KODEACC_ROOT,
                   coa.LVL,
                   coa."1D", coa."1K", coa."2D", coa."2K", coa."3D", coa."3K",
                   coa."4D", coa."4K", coa."5D", coa."5K", coa."6D", coa."6K",
                   coa."7D", coa."7K", coa."8D", coa."8K", coa."9D", coa."9K",
                   coa."10D", coa."10K", coa."11D", coa."11K", coa."12D", coa."12K"
              FROM section_accounts
              JOIN ACCT_COA coa
                ON coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_tahun
               AND section_accounts.MATCH_MODE = 'TREE'
             START WITH coa.KODEACC = section_accounts.KODEACC_ROOT
            CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                   AND PRIOR section_accounts.SECTION_ID = section_accounts.SECTION_ID
                   AND PRIOR section_accounts.KODEACC_ROOT = section_accounts.KODEACC_ROOT
        ),
        parent_rows AS (
            SELECT section_accounts.SECTION_ID,
                   section_accounts.NORMAL_POSISI,
                   section_accounts.DISPLAY_LVL,
                   section_accounts.KODEACC_ROOT,
                   coa.LVL,
                   coa."1D", coa."1K", coa."2D", coa."2K", coa."3D", coa."3K",
                   coa."4D", coa."4K", coa."5D", coa."5K", coa."6D", coa."6K",
                   coa."7D", coa."7K", coa."8D", coa."8K", coa."9D", coa."9K",
                   coa."10D", coa."10K", coa."11D", coa."11K", coa."12D", coa."12K"
              FROM section_accounts
              JOIN ACCT_COA coa
                ON coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_tahun
               AND coa.PARENTACC = section_accounts.KODEACC_ROOT
             WHERE section_accounts.MATCH_MODE = 'PARENT'
        ),
        grp_rows AS (
            SELECT section_accounts.SECTION_ID,
                   section_accounts.NORMAL_POSISI,
                   section_accounts.DISPLAY_LVL,
                   section_accounts.KODEACC_ROOT,
                   coa.LVL,
                   coa."1D", coa."1K", coa."2D", coa."2K", coa."3D", coa."3K",
                   coa."4D", coa."4K", coa."5D", coa."5K", coa."6D", coa."6K",
                   coa."7D", coa."7K", coa."8D", coa."8K", coa."9D", coa."9K",
                   coa."10D", coa."10K", coa."11D", coa."11K", coa."12D", coa."12K"
              FROM section_accounts
              JOIN ACCT_COA coa
                ON coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_tahun
               AND coa.GRP = section_accounts.GRP_CODE
             WHERE section_accounts.MATCH_MODE = 'GRP_LVL'
        ),
        coa_rows AS (
            SELECT * FROM tree_rows
            UNION ALL
            SELECT * FROM parent_rows
            UNION ALL
            SELECT * FROM grp_rows
        ),
        calculated AS (
            SELECT coa_rows.NORMAL_POSISI,
                   CASE p_bulan
                       WHEN 1 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."1D", coa_rows."1K", coa_rows.NORMAL_POSISI)
                       WHEN 2 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."2D", coa_rows."2K", coa_rows.NORMAL_POSISI)
                       WHEN 3 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."3D", coa_rows."3K", coa_rows.NORMAL_POSISI)
                       WHEN 4 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."4D", coa_rows."4K", coa_rows.NORMAL_POSISI)
                       WHEN 5 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."5D", coa_rows."5K", coa_rows.NORMAL_POSISI)
                       WHEN 6 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."6D", coa_rows."6K", coa_rows.NORMAL_POSISI)
                       WHEN 7 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."7D", coa_rows."7K", coa_rows.NORMAL_POSISI)
                       WHEN 8 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."8D", coa_rows."8K", coa_rows.NORMAL_POSISI)
                       WHEN 9 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."9D", coa_rows."9K", coa_rows.NORMAL_POSISI)
                       WHEN 10 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."10D", coa_rows."10K", coa_rows.NORMAL_POSISI)
                       WHEN 11 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."11D", coa_rows."11K", coa_rows.NORMAL_POSISI)
                       WHEN 12 THEN ACCT_REPORT_ENGINE_V1.NetAmount(coa_rows."12D", coa_rows."12K", coa_rows.NORMAL_POSISI)
                   END BULANINI
              FROM coa_rows
             WHERE coa_rows.LVL = coa_rows.DISPLAY_LVL
        )
        SELECT NVL(SUM(CASE WHEN NORMAL_POSISI = 'K' THEN BULANINI ELSE 0 END), 0) -
               NVL(SUM(CASE WHEN NORMAL_POSISI = 'D' THEN BULANINI ELSE 0 END), 0)
          INTO v_net_laba
          FROM calculated;

        RETURN v_net_laba;
    END CALC_LABARUGI;

    FUNCTION JURNAL_CLOSING(
        p_IDDATA        IN VARCHAR2,
        p_bulan         IN INTEGER,
        p_tahun         IN INTEGER,
        p_userid        IN VARCHAR2,
        p_jenisakunting IN VARCHAR2,
        p_commit        IN CHAR DEFAULT 'Y'
    ) RETURN NUMBER
    IS
        sPeriode_next VARCHAR2(7);
        PERIODEEXIST INTEGER;
        dLabaBersihBI NUMBER;
        sPeriode VARCHAR2(7);
        dLastdate DATE;
        AKUN_ALOKASI_LABADITAHAN VARCHAR2(30);
        AKUN_LRTAHUN_BERJALAN VARCHAR2(30);
        REK1 VARCHAR2(130);
        REK2 VARCHAR2(130);
        KET VARCHAR2(200);
        sHID VARCHAR2(40);
        p_jurnalid NUMBER := NULL;
        TSTATUS VARCHAR2(40);
        exist INT;
    BEGIN
        IF p_bulan < 1 OR p_bulan > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan closing tidak valid: ' || p_bulan);
        END IF;

        ACCOUNTING.CreateClosingAcct(p_IDDATA);

        sPeriode := TRIM(to_char(p_bulan,'00')||'/'||p_tahun);
        dLastdate := LAST_DAY(TO_DATE(TRIM('01'||to_char(p_bulan,'00')||p_tahun),'ddMMyyyy'));
        sHID := p_IDDATA||sPeriode||'001/CLOSE';
        KET := 'Alokasi ke Laba / Rugi tahun berjalan periode '||sPeriode;

        dLabaBersihBI := CALC_LABARUGI(p_IDDATA, p_bulan, p_tahun, p_userid, p_jenisakunting);

        BEGIN
            SELECT STATUS INTO TSTATUS FROM USER_TRIGGERS WHERE TRIGGER_NAME = 'UPDATE_COA_FROM_DELETE';
            IF TSTATUS = 'DISABLED' THEN
                EXECUTE IMMEDIATE 'ALTER TRIGGER UPDATE_COA_FROM_DELETE ENABLE';
            END IF;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                NULL;
        END;

        DELETE FROM ACCT_JURNAL_DTL WHERE NOJURNAL = '001/CLOSE' AND IDDATA = p_IDDATA AND PERIODE = sPeriode;
        DELETE FROM ACCT_JURNAL_HDR WHERE NOJURNAL = '001/CLOSE' AND IDDATA = p_IDDATA AND PERIODE = sPeriode;

        BEGIN
            SELECT KODEACC INTO AKUN_LRTAHUN_BERJALAN
              FROM ACCT_DEFAULT
             WHERE NAMA = 'RL_TAHUN_BERJALAN' AND IDDATA = p_IDDATA;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20201, 'Akun Jurnal Closing (Laba/Rugi Berjalan) belum di-setup untuk lokasi ' || p_IDDATA || ' tahun ' || p_tahun);
        END;

        BEGIN
            SELECT KODEACC INTO AKUN_ALOKASI_LABADITAHAN
              FROM ACCT_DEFAULT
             WHERE NAMA = 'ALOKASI_LABA_DITAHAN' AND IDDATA = p_IDDATA;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20201, 'Akun Alokasi Laba Ditahan belum di-setup untuk lokasi ' || p_IDDATA || ' tahun ' || p_tahun);
        END;

        SELECT COUNT(1) INTO exist FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_LRTAHUN_BERJALAN;
        IF exist = 0 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_LRTAHUN_BERJALAN || ' ( KODE AKUN LABA/RUGI TAHUN BERJALAN TIDAK DITEMUKAN DI COA TAHUN ' || p_tahun || ' )');
        ELSIF exist > 1 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_LRTAHUN_BERJALAN || ' ( KODE AKUN LABA/RUGI TAHUN BERJALAN DOUBLE DI COA TAHUN ' || p_tahun || ' )');
        ELSE
            SELECT NAMAACC INTO REK1 FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_LRTAHUN_BERJALAN;
        END IF;

        SELECT COUNT(1) INTO exist FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_ALOKASI_LABADITAHAN;
        IF exist = 0 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_ALOKASI_LABADITAHAN || ' ( KODE AKUN ALOKASI LABA DITAHAN TIDAK DITEMUKAN DI COA TAHUN ' || p_tahun || ' )');
        ELSIF exist > 1 THEN
            RAISE_APPLICATION_ERROR(-20201, 'Kode: ' || AKUN_ALOKASI_LABADITAHAN || ' ( KODE AKUN ALOKASI LABA DITAHAN DOUBLE DI COA TAHUN ' || p_tahun || ' )');
        ELSE
            SELECT NAMAACC INTO REK2 FROM ACCT_COA WHERE TAHUN = p_tahun AND IDDATA = p_IDDATA AND KODEACC = AKUN_ALOKASI_LABADITAHAN;
        END IF;

        IF dLabaBersihBI < 0 THEN
            INSERT INTO ACCT_JURNAL_HDR (HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, UPDATEBY, POSTING)
            VALUES (sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 'Y')
            RETURNING JURNALID INTO p_jurnalid;

            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'01', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 1, AKUN_LRTAHUN_BERJALAN, REK1, ABS(dLabaBersihBI), 0, KET, p_tahun, p_bulan);
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'02', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 2, AKUN_ALOKASI_LABADITAHAN, REK2, 0, ABS(dLabaBersihBI), KET, p_tahun, p_bulan);
        ELSIF dLabaBersihBI > 0 THEN
            INSERT INTO ACCT_JURNAL_HDR (HID, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, UPDATEBY, POSTING)
            VALUES (sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 'Y')
            RETURNING JURNALID INTO p_jurnalid;

            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'01', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 1, AKUN_ALOKASI_LABADITAHAN, REK2, ABS(dLabaBersihBI), 0, KET, p_tahun, p_bulan);
            INSERT INTO ACCT_JURNAL_DTL (REFFID, DID, HIDREFF, IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, GLYEAR, GLMONTH)
            VALUES (p_jurnalid, sHID||'02', sHID, p_IDDATA, '001/CLOSE', dLastdate, sPeriode, 'CLOSING', p_userid, 2, AKUN_LRTAHUN_BERJALAN, REK1, 0, ABS(dLabaBersihBI), KET, p_tahun, p_bulan);
        END IF;

        IF p_jurnalid IS NOT NULL THEN
            ACCT_RECALLCULATIONS_V2.ReCalcPeriod(p_IDDATA, p_bulan, p_tahun, sPeriode, p_userid);
        END IF;

        IF NVL(p_commit, 'Y') = 'Y' THEN
            COMMIT;
        END IF;

        IF p_bulan = 12 THEN
            sPeriode_next := '01/' || (p_tahun + 1);
        ELSE
            sPeriode_next := TRIM(to_char(p_bulan + 1, '00') || '/' || p_tahun);
        END IF;

        SELECT COUNT(1) INTO PERIODEEXIST FROM ACCT_PERIODE WHERE IDDATA = p_IDDATA AND PERIODE = sPeriode_next;
        IF PERIODEEXIST = 0 THEN
            ACCOUNTING.CreateNextPeriode(p_IDDATA, p_bulan, p_tahun);
        END IF;

        RETURN dLabaBersihBI;
    END JURNAL_CLOSING;
END ACCT_JURNAL_CLOSING_V2;]';

    DBMS_OUTPUT.PUT_LINE('UPDATED ACCT_JURNAL_CLOSING_V2 FOR PKS LEGACY SETTINGS');
END;
/
