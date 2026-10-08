SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_object_count      INTEGER := 0;
    v_invalid_count     INTEGER := 0;
    v_argument_count    INTEGER := 0;
    v_cursor            SYS_REFCURSOR;
    v_cursor_number     INTEGER := 0;
    v_column_count      INTEGER := 0;
    v_columns           DBMS_SQL.DESC_TAB3;
    v_expected_columns  SYS.ODCIVARCHAR2LIST := SYS.ODCIVARCHAR2LIST(
        'BARIS',
        'PARAM',
        'PERIODE',
        'KODE',
        'REKENING',
        'NOJURNAL',
        'TANGGAL',
        'KETERANGAN',
        'DEBET',
        'KREDIT'
    );
    v_iddata            ACCT_COA.IDDATA%TYPE;
    v_tahun             ACCT_COA.TAHUN%TYPE;
    v_tahun_sampai      ACCT_COA.TAHUN%TYPE;
    v_bulan             INTEGER;
    v_kodeacc           ACCT_COA.KODEACC%TYPE;
    v_posisi            ACCT_COA.POSISI%TYPE;
    v_saldo             NUMBER;

    PROCEDURE verify_opening(
        p_iddata         IN ACCT_COA.IDDATA%TYPE,
        p_tahun_dari     IN INTEGER,
        p_tahun_sampai   IN INTEGER,
        p_bulan_dari     IN INTEGER,
        p_bulan_sampai   IN INTEGER,
        p_kodeacc        IN ACCT_COA.KODEACC%TYPE,
        p_expected_saldo IN NUMBER,
        p_posisi         IN ACCT_COA.POSISI%TYPE,
        p_expected_count IN INTEGER,
        p_scenario       IN VARCHAR2
    ) IS
        v_test_cursor   SYS_REFCURSOR;
        v_baris         NUMBER;
        v_param         NUMBER;
        v_periode       VARCHAR2(20);
        v_kode          VARCHAR2(100);
        v_rekening      VARCHAR2(500);
        v_nojurnal      VARCHAR2(100);
        v_tanggal       DATE;
        v_keterangan    VARCHAR2(4000);
        v_debet         NUMBER;
        v_kredit        NUMBER;
        v_opening_count INTEGER := 0;
        v_expected_date DATE := TO_DATE(
            TO_CHAR(p_tahun_dari) || LPAD(TO_CHAR(p_bulan_dari), 2, '0') || '01',
            'YYYYMMDD'
        );
        v_expected_param NUMBER := (p_tahun_dari * 100) + p_bulan_dari;
    BEGIN
        ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2(
            p_iddata,
            p_tahun_dari,
            p_tahun_sampai,
            p_bulan_dari,
            p_bulan_sampai,
            p_kodeacc,
            p_kodeacc,
            v_test_cursor
        );

        LOOP
            FETCH v_test_cursor
             INTO v_baris,
                  v_param,
                  v_periode,
                  v_kode,
                  v_rekening,
                  v_nojurnal,
                  v_tanggal,
                  v_keterangan,
                  v_debet,
                  v_kredit;
            EXIT WHEN v_test_cursor%NOTFOUND;

            IF v_baris = 0
                AND v_keterangan = 'SALDO AWAL'
                AND v_param = v_expected_param THEN
                v_opening_count := v_opening_count + 1;

                IF v_kode <> p_kodeacc
                    OR v_tanggal <> v_expected_date
                    OR v_periode <> LPAD(TO_CHAR(p_bulan_dari), 2, '0') || '/' || TO_CHAR(p_tahun_dari) THEN
                    RAISE_APPLICATION_ERROR(-20976, p_scenario || ': opening-row identity is invalid.');
                END IF;

                IF NVL(TRIM(p_posisi), 'D') = 'D' THEN
                    IF NVL(v_debet, 0) <> p_expected_saldo OR NVL(v_kredit, 0) <> 0 THEN
                        RAISE_APPLICATION_ERROR(-20977, p_scenario || ': debit opening balance is invalid.');
                    END IF;
                ELSIF NVL(v_kredit, 0) <> p_expected_saldo OR NVL(v_debet, 0) <> 0 THEN
                    RAISE_APPLICATION_ERROR(-20978, p_scenario || ': credit opening balance is invalid.');
                END IF;
            END IF;
        END LOOP;

        CLOSE v_test_cursor;

        IF v_opening_count <> p_expected_count THEN
            RAISE_APPLICATION_ERROR(
                -20979,
                p_scenario || ': expected ' || p_expected_count ||
                ' opening row(s), received ' || v_opening_count || '.'
            );
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            IF v_test_cursor%ISOPEN THEN
                CLOSE v_test_cursor;
            END IF;

            RAISE;
    END;
BEGIN
    SELECT COUNT(*),
           COUNT(CASE WHEN STATUS <> 'VALID' THEN 1 END)
      INTO v_object_count, v_invalid_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY');

    IF v_object_count <> 2 OR v_invalid_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20973, 'ACCT_LAPORAN_V2 package and body must both be VALID.');
    END IF;

    SELECT COUNT(*)
      INTO v_argument_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_LAPORAN_V2'
       AND OBJECT_NAME = 'LAP_BUKUBESAR_V2'
       AND DATA_LEVEL = 0
       AND ARGUMENT_NAME IN (
           'P_IDDATA',
           'P_TAHUNDARI',
           'P_TAHUNSAMPAI',
           'P_BULANDARI',
           'P_BULANSAMPAI',
           'P_DARIKODE',
           'P_SAMPAIKODE',
           'P_CURSOR'
       );

    IF v_argument_count <> 8 THEN
        RAISE_APPLICATION_ERROR(-20974, 'LAP_BUKUBESAR_V2 signature is incomplete.');
    END IF;

    ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2(
        '__VERIFY__',
        2000,
        2000,
        1,
        1,
        '0',
        'ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ',
        v_cursor
    );
    v_cursor_number := DBMS_SQL.TO_CURSOR_NUMBER(v_cursor);
    DBMS_SQL.DESCRIBE_COLUMNS3(v_cursor_number, v_column_count, v_columns);

    IF v_column_count <> v_expected_columns.COUNT THEN
        RAISE_APPLICATION_ERROR(-20975, 'Buku Besar cursor must expose exactly ten columns.');
    END IF;

    FOR i IN 1 .. v_column_count LOOP
        IF UPPER(v_columns(i).col_name) <> v_expected_columns(i) THEN
            RAISE_APPLICATION_ERROR(
                -20975,
                'Buku Besar cursor column ' || i || ' must be ' || v_expected_columns(i) ||
                ', received ' || v_columns(i).col_name || '.'
            );
        END IF;
    END LOOP;

    DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
    v_cursor_number := 0;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, NVL(TRIM(POSISI), 'D'), NVL(SALDOAWAL, 0)
          INTO v_iddata, v_tahun, v_kodeacc, v_posisi, v_saldo
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC, coa.POSISI, coa.SALDOAWAL
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(coa.SALDOAWAL, 0) <> 0
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_opening(v_iddata, v_tahun, v_tahun, 1, 1, v_kodeacc, v_saldo, v_posisi, 1, 'January opening');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no non-zero January Neraca opening balance is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, NVL(TRIM(POSISI), 'D'), BULAN, SALDO
          INTO v_iddata, v_tahun, v_kodeacc, v_posisi, v_bulan, v_saldo
          FROM (
                SELECT IDDATA, TAHUN, KODEACC, POSISI, BULAN, SALDO
                  FROM (
                        SELECT coa.IDDATA,
                               coa.TAHUN,
                               coa.KODEACC,
                               coa.POSISI,
                               coa."1S", coa."2S", coa."3S", coa."4S", coa."5S", coa."6S",
                               coa."7S", coa."8S", coa."9S", coa."10S", coa."11S"
                          FROM ACCT_COA coa
                         WHERE coa.ISHEADER = 'D'
                           AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                  )
                  UNPIVOT (
                      SALDO FOR BULAN IN (
                          "1S" AS 2, "2S" AS 3, "3S" AS 4, "4S" AS 5, "5S" AS 6, "6S" AS 7,
                          "7S" AS 8, "8S" AS 9, "9S" AS 10, "10S" AS 11, "11S" AS 12
                      )
                  )
                 WHERE SALDO <> 0
                 ORDER BY IDDATA, TAHUN, KODEACC, BULAN
          )
         WHERE ROWNUM = 1;

        verify_opening(
            v_iddata,
            v_tahun,
            v_tahun,
            v_bulan,
            v_bulan,
            v_kodeacc,
            v_saldo,
            v_posisi,
            1,
            'Mid-year opening'
        );
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no non-zero mid-year Neraca opening balance is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, NVL(TRIM(POSISI), 'D'), NVL(SALDOAWAL, 0)
          INTO v_iddata, v_tahun, v_kodeacc, v_posisi, v_saldo
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC, coa.POSISI, coa.SALDOAWAL
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) NOT IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(coa.SALDOAWAL, 0) <> 0
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_opening(v_iddata, v_tahun, v_tahun, 1, 1, v_kodeacc, v_saldo, v_posisi, 1, 'Profit and loss opening');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no non-zero Laba Rugi opening fixture is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, TAHUN_SAMPAI, KODEACC, POSISI, SALDOAWAL
          INTO v_iddata, v_tahun, v_tahun_sampai, v_kodeacc, v_posisi, v_saldo
          FROM (
                SELECT start_coa.IDDATA,
                       start_coa.TAHUN,
                       end_coa.TAHUN AS TAHUN_SAMPAI,
                       start_coa.KODEACC,
                       NVL(TRIM(start_coa.POSISI), 'D') AS POSISI,
                       NVL(start_coa.SALDOAWAL, 0) AS SALDOAWAL
                  FROM ACCT_COA start_coa
                  JOIN ACCT_COA end_coa
                    ON end_coa.IDDATA = start_coa.IDDATA
                   AND end_coa.KODEACC = start_coa.KODEACC
                   AND end_coa.TAHUN = start_coa.TAHUN + 1
                 WHERE start_coa.ISHEADER = 'D'
                   AND TRIM(start_coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(start_coa.SALDOAWAL, 0) <> 0
                 ORDER BY start_coa.IDDATA, start_coa.TAHUN, start_coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_opening(
            v_iddata,
            v_tahun,
            v_tahun_sampai,
            1,
            12,
            v_kodeacc,
            v_saldo,
            v_posisi,
            1,
            'Multi-year opening'
        );
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no consecutive-year Neraca opening fixture is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, NVL(TRIM(POSISI), 'D'), NVL(SALDOAWAL, 0)
          INTO v_iddata, v_tahun, v_kodeacc, v_posisi, v_saldo
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC, coa.POSISI, coa.SALDOAWAL
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(coa.SALDOAWAL, 0) <> 0
                   AND NOT EXISTS (
                       SELECT 1
                         FROM ACCT_JURNAL_DTL dtl
                        WHERE dtl.IDDATA = coa.IDDATA
                          AND dtl.GLYEAR = coa.TAHUN
                          AND dtl.GLMONTH = 1
                          AND dtl.KODE = coa.KODEACC
                   )
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_opening(v_iddata, v_tahun, v_tahun, 1, 1, v_kodeacc, v_saldo, v_posisi, 1, 'Opening-only account');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no opening-only Neraca account is available.');
    END;

    DBMS_OUTPUT.PUT_LINE('OK: Buku Besar opening balance and ten-column cursor contract are valid.');
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor_number <> 0 AND DBMS_SQL.IS_OPEN(v_cursor_number) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
        END IF;

        RAISE;
END;
/
