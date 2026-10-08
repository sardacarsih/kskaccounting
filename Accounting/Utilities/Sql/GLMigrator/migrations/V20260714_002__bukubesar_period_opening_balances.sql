-- Purpose: Emit one General Ledger opening-balance row for every selected accounting period.
-- Date: 2026-07-14

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_body            CLOB;
    v_sql             CLOB;
    v_replacement     CLOB := q'~    PROCEDURE LAP_BUKUBESAR_V2(
        p_IDDATA       IN  VARCHAR2,
        p_TAHUNDARI    IN  INTEGER,
        p_TAHUNSAMPAI  IN  INTEGER,
        p_BULANDARI    IN  INTEGER,
        p_BULANSAMPAI  IN  INTEGER,
        p_DARIKODE     IN  VARCHAR2,
        p_SAMPAIKODE   IN  VARCHAR2,
        p_CURSOR       OUT SYS_REFCURSOR
    )
    IS
        v_start_param NUMBER := (p_TAHUNDARI * 100) + p_BULANDARI;
        v_end_param   NUMBER := (p_TAHUNSAMPAI * 100) + p_BULANSAMPAI;
    BEGIN
        IF p_BULANDARI < 1 OR p_BULANDARI > 12
            OR p_BULANSAMPAI < 1 OR p_BULANSAMPAI > 12 THEN
            RAISE_APPLICATION_ERROR(-20095, 'Rentang bulan Buku Besar tidak valid.');
        END IF;

        IF v_start_param > v_end_param THEN
            RAISE_APPLICATION_ERROR(-20096, 'Periode awal Buku Besar melebihi periode akhir.');
        END IF;

        IF TRIM(p_DARIKODE) IS NULL OR TRIM(p_SAMPAIKODE) IS NULL THEN
            RAISE_APPLICATION_ERROR(-20097, 'Rentang kode akun Buku Besar wajib diisi.');
        END IF;

        OPEN p_CURSOR FOR
            WITH report_periods AS (
                SELECT period_start,
                       EXTRACT(YEAR FROM period_start) AS PERIOD_YEAR,
                       EXTRACT(MONTH FROM period_start) AS PERIOD_MONTH,
                       TO_NUMBER(TO_CHAR(period_start, 'YYYYMM')) AS PARAM,
                       TO_CHAR(period_start, 'MM/YYYY') AS PERIODE
                  FROM (
                        SELECT ADD_MONTHS(
                                   TO_DATE(
                                       TO_CHAR(p_TAHUNDARI) || LPAD(TO_CHAR(p_BULANDARI), 2, '0') || '01',
                                       'YYYYMMDD'
                                   ),
                                   LEVEL - 1
                               ) AS period_start
                          FROM DUAL
                        CONNECT BY LEVEL <= MONTHS_BETWEEN(
                            TO_DATE(
                                TO_CHAR(p_TAHUNSAMPAI) || LPAD(TO_CHAR(p_BULANSAMPAI), 2, '0') || '01',
                                'YYYYMMDD'
                            ),
                            TO_DATE(
                                TO_CHAR(p_TAHUNDARI) || LPAD(TO_CHAR(p_BULANDARI), 2, '0') || '01',
                                'YYYYMMDD'
                            )
                        ) + 1
                  )
            ),
            selected_accounts AS (
                SELECT DISTINCT coa.TAHUN,
                                coa.KODEACC
                  FROM ACCT_COA coa
                 WHERE coa.IDDATA = p_IDDATA
                   AND coa.TAHUN BETWEEN p_TAHUNDARI AND p_TAHUNSAMPAI
                 START WITH coa.IDDATA = p_IDDATA
                    AND coa.TAHUN BETWEEN p_TAHUNDARI AND p_TAHUNSAMPAI
                    AND coa.KODEACC BETWEEN p_DARIKODE AND p_SAMPAIKODE
               CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                  AND PRIOR coa.IDDATA = coa.IDDATA
                  AND PRIOR coa.TAHUN = coa.TAHUN
            ),
            opening_balances AS (
                SELECT periods.PARAM,
                       periods.PERIODE,
                       periods.period_start AS TANGGAL,
                       coa.KODEACC AS KODE,
                       coa.NAMAACC AS REKENING,
                       NVL(UPPER(TRIM(coa.POSISI)), 'D') AS POSISI,
                       CASE periods.PERIOD_MONTH
                           WHEN 1 THEN NVL(coa.SALDOAWAL, 0)
                           WHEN 2 THEN NVL(coa."1S", 0)
                           WHEN 3 THEN NVL(coa."2S", 0)
                           WHEN 4 THEN NVL(coa."3S", 0)
                           WHEN 5 THEN NVL(coa."4S", 0)
                           WHEN 6 THEN NVL(coa."5S", 0)
                           WHEN 7 THEN NVL(coa."6S", 0)
                           WHEN 8 THEN NVL(coa."7S", 0)
                           WHEN 9 THEN NVL(coa."8S", 0)
                           WHEN 10 THEN NVL(coa."9S", 0)
                           WHEN 11 THEN NVL(coa."10S", 0)
                           ELSE NVL(coa."11S", 0)
                       END AS SALDO
                  FROM report_periods periods
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = p_IDDATA
                   AND coa.TAHUN = periods.PERIOD_YEAR
                  JOIN selected_accounts selected
                    ON selected.TAHUN = coa.TAHUN
                   AND selected.KODEACC = coa.KODEACC
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
            ),
            ledger_rows AS (
                SELECT 0 AS BARIS,
                       opening.PARAM,
                       opening.PERIODE,
                       opening.KODE,
                       opening.REKENING,
                       CAST('000' AS VARCHAR2(50)) AS NOJURNAL,
                       opening.TANGGAL,
                       CAST('SALDO AWAL' AS VARCHAR2(100)) AS KETERANGAN,
                       CASE WHEN opening.POSISI = 'D' THEN opening.SALDO ELSE 0 END AS DEBET,
                       CASE WHEN opening.POSISI = 'D' THEN 0 ELSE opening.SALDO END AS KREDIT
                  FROM opening_balances opening
                 WHERE opening.SALDO <> 0

                UNION ALL

                SELECT ROW_NUMBER() OVER (
                           ORDER BY dtl.GLYEAR, dtl.GLMONTH, dtl.KODE, dtl.TANGGAL, dtl.NOJURNAL
                       ) AS BARIS,
                       (dtl.GLYEAR * 100) + dtl.GLMONTH AS PARAM,
                       dtl.PERIODE,
                       dtl.KODE,
                       dtl.REKENING,
                       dtl.NOJURNAL,
                       dtl.TANGGAL,
                       dtl.KETERANGAN,
                       dtl.DEBET,
                       dtl.KREDIT
                  FROM ACCT_JURNAL_DTL dtl
                 WHERE dtl.IDDATA = p_IDDATA
                   AND ((dtl.GLYEAR * 100) + dtl.GLMONTH) BETWEEN v_start_param AND v_end_param
                   AND dtl.KODE IN (SELECT selected.KODEACC FROM selected_accounts selected)
            )
            SELECT ledger.BARIS,
                   ledger.PARAM,
                   ledger.PERIODE,
                   ledger.KODE,
                   ledger.REKENING,
                   ledger.NOJURNAL,
                   ledger.TANGGAL,
                   ledger.KETERANGAN,
                   ledger.DEBET,
                   ledger.KREDIT
              FROM ledger_rows ledger
             ORDER BY ledger.KODE,
                      ledger.PARAM,
                      ledger.BARIS,
                      ledger.TANGGAL,
                      ledger.NOJURNAL;
    END LAP_BUKUBESAR_V2;~';
    v_new_sql         CLOB;
    v_start_position  PLS_INTEGER;
    v_end_position    PLS_INTEGER;
    v_after_position  PLS_INTEGER;
    v_remaining       PLS_INTEGER;
    v_cursor          INTEGER := 0;
    v_error_count     INTEGER := 0;

    PROCEDURE execute_clob(p_sql IN CLOB) IS
    BEGIN
        v_cursor := DBMS_SQL.OPEN_CURSOR;
        DBMS_SQL.PARSE(v_cursor, p_sql, DBMS_SQL.NATIVE);
        DBMS_SQL.CLOSE_CURSOR(v_cursor);
        v_cursor := 0;
    EXCEPTION
        WHEN OTHERS THEN
            IF v_cursor <> 0 AND DBMS_SQL.IS_OPEN(v_cursor) THEN
                DBMS_SQL.CLOSE_CURSOR(v_cursor);
            END IF;

            v_cursor := 0;
            RAISE;
    END;
BEGIN
    SELECT XMLCAST(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE) AS CLOB)
      INTO v_body
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY';

    IF v_body IS NULL THEN
        RAISE_APPLICATION_ERROR(-20980, 'ACCT_LAPORAN_V2 package body source was not found.');
    END IF;

    v_sql := TO_CLOB('CREATE OR REPLACE ') || v_body;
    v_start_position := DBMS_LOB.INSTR(v_sql, 'PROCEDURE LAP_BUKUBESAR_V2(', 1, 1);
    v_end_position := DBMS_LOB.INSTR(v_sql, 'END LAP_BUKUBESAR_V2;', v_start_position, 1);

    IF v_start_position = 0 OR v_end_position = 0 THEN
        RAISE_APPLICATION_ERROR(-20981, 'LAP_BUKUBESAR_V2 source boundary was not found.');
    END IF;

    v_after_position := v_end_position + LENGTH('END LAP_BUKUBESAR_V2;');
    DBMS_LOB.CREATETEMPORARY(v_new_sql, TRUE);

    IF v_start_position > 1 THEN
        DBMS_LOB.COPY(v_new_sql, v_sql, v_start_position - 1, 1, 1);
    END IF;

    DBMS_LOB.APPEND(v_new_sql, v_replacement);
    v_remaining := DBMS_LOB.GETLENGTH(v_sql) - v_after_position + 1;

    IF v_remaining > 0 THEN
        DBMS_LOB.COPY(
            v_new_sql,
            v_sql,
            v_remaining,
            DBMS_LOB.GETLENGTH(v_new_sql) + 1,
            v_after_position
        );
    END IF;

    execute_clob(v_new_sql);

    SELECT COUNT(*)
      INTO v_error_count
      FROM USER_ERRORS
     WHERE NAME = 'ACCT_LAPORAN_V2'
       AND TYPE = 'PACKAGE BODY';

    IF v_error_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20982, 'ACCT_LAPORAN_V2 has compilation errors after period opening-balance repair.');
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_new_sql);
    END IF;

    DBMS_OUTPUT.PUT_LINE('REPAIRED ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2 PERIOD OPENING BALANCES');
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor <> 0 AND DBMS_SQL.IS_OPEN(v_cursor) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor);
        END IF;

        IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
            DBMS_LOB.FREETEMPORARY(v_new_sql);
        END IF;

        RAISE;
END;
/
