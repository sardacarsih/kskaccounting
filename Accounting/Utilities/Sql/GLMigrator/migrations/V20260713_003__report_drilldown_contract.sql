-- Purpose: provide one validated, report-aware drilldown contract for Laba Rugi and Neraca.
DECLARE
BEGIN
    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE ACCT_REPORT_DRILLDOWN_V1 AS
    PROCEDURE GET_DRILLDOWN(
        p_IDDATA      IN  VARCHAR2,
        p_BULAN       IN  INTEGER,
        p_TAHUN       IN  INTEGER,
        p_USERID      IN  VARCHAR2,
        p_REPORT_CODE IN  VARCHAR2,
        p_KODEACC     IN  VARCHAR2,
        p_CURSOR      OUT SYS_REFCURSOR
    );
END ACCT_REPORT_DRILLDOWN_V1;]';

    EXECUTE IMMEDIATE q'[
CREATE OR REPLACE PACKAGE BODY ACCT_REPORT_DRILLDOWN_V1 AS
    PROCEDURE GET_DRILLDOWN(
        p_IDDATA      IN  VARCHAR2,
        p_BULAN       IN  INTEGER,
        p_TAHUN       IN  INTEGER,
        p_USERID      IN  VARCHAR2,
        p_REPORT_CODE IN  VARCHAR2,
        p_KODEACC     IN  VARCHAR2,
        p_CURSOR      OUT SYS_REFCURSOR
    )
    IS
        v_mapping_count INTEGER := 0;
    BEGIN
        IF p_BULAN < 1 OR p_BULAN > 12 THEN
            RAISE_APPLICATION_ERROR(-20090, 'Bulan laporan tidak valid: ' || p_BULAN);
        END IF;

        IF p_REPORT_CODE IS NULL OR p_REPORT_CODE NOT IN ('LABARUGI', 'NERACA') THEN
            RAISE_APPLICATION_ERROR(-20091, 'Kode laporan drilldown tidak valid: ' || NVL(p_REPORT_CODE, '-'));
        END IF;

        IF TRIM(p_USERID) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20092, 'User drilldown wajib diisi.');
        END IF;

        SELECT COUNT(*)
          INTO v_mapping_count
          FROM ACCT_REPORT_SECTION section
          JOIN ACCT_REPORT_SECTION_ACCOUNT account
            ON account.SECTION_ID = section.SECTION_ID
         WHERE section.REPORT_CODE = p_REPORT_CODE
           AND section.IS_ACTIVE = 'Y'
           AND account.IS_ACTIVE = 'Y'
           AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)
           AND (
                (NVL(account.MATCH_MODE, 'TREE') = 'TREE' AND EXISTS (
                    SELECT 1
                      FROM ACCT_COA tree
                     WHERE tree.IDDATA = p_IDDATA
                       AND tree.TAHUN = p_TAHUN
                       AND tree.KODEACC = p_KODEACC
                     START WITH tree.KODEACC = account.KODEACC_ROOT
                   CONNECT BY NOCYCLE PRIOR tree.KODEACC = tree.PARENTACC
                          AND PRIOR tree.IDDATA = tree.IDDATA
                          AND PRIOR tree.TAHUN = tree.TAHUN
                ))
                OR
                (NVL(account.MATCH_MODE, 'TREE') = 'PARENT' AND EXISTS (
                    SELECT 1
                      FROM ACCT_COA child
                     WHERE child.IDDATA = p_IDDATA
                       AND child.TAHUN = p_TAHUN
                       AND (child.KODEACC = account.KODEACC_ROOT OR child.PARENTACC = account.KODEACC_ROOT)
                       AND child.KODEACC = p_KODEACC
                ))
                OR
                (NVL(account.MATCH_MODE, 'TREE') = 'GRP_LVL' AND EXISTS (
                    SELECT 1
                      FROM ACCT_COA grouped
                     WHERE grouped.IDDATA = p_IDDATA
                       AND grouped.TAHUN = p_TAHUN
                       AND grouped.GRP = account.GRP_CODE
                       AND grouped.KODEACC = p_KODEACC
                ))
           );

        IF v_mapping_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20093, 'Akun tidak termasuk konfigurasi laporan ' || p_REPORT_CODE || ': ' || p_KODEACC);
        END IF;

        OPEN p_CURSOR FOR
            SELECT coa.KODEACC,
                   coa.NAMAACC,
                   coa.PARENTACC,
                   NVL(coa.POSISI, 'D') POSISI,
                   CASE WHEN coa.ISHEADER = 'G' THEN 'G' ELSE 'D' END ISHEADER,
                   CASE p_REPORT_CODE
                       WHEN 'NERACA' THEN
                           CASE p_BULAN
                               WHEN 1 THEN coa."1S" WHEN 2 THEN coa."2S" WHEN 3 THEN coa."3S"
                               WHEN 4 THEN coa."4S" WHEN 5 THEN coa."5S" WHEN 6 THEN coa."6S"
                               WHEN 7 THEN coa."7S" WHEN 8 THEN coa."8S" WHEN 9 THEN coa."9S"
                               WHEN 10 THEN coa."10S" WHEN 11 THEN coa."11S" ELSE coa."12S"
                           END
                       ELSE
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
                               ELSE ACCT_REPORT_ENGINE_V1.NetAmount(coa."12D", coa."12K", NVL(coa.POSISI, 'D'))
                           END
                   END NILAI,
                   p_REPORT_CODE REPORT_CODE
              FROM ACCT_COA coa
             WHERE coa.IDDATA = p_IDDATA
               AND coa.TAHUN = p_TAHUN
             START WITH coa.KODEACC = p_KODEACC
           CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                  AND PRIOR coa.IDDATA = coa.IDDATA
                  AND PRIOR coa.TAHUN = coa.TAHUN
             ORDER BY coa.KODEACC;
    END GET_DRILLDOWN;
END ACCT_REPORT_DRILLDOWN_V1;]';
END;
/
