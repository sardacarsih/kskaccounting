SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    TYPE number_by_param IS TABLE OF NUMBER INDEX BY PLS_INTEGER;
    TYPE position_by_param IS TABLE OF VARCHAR2(1) INDEX BY PLS_INTEGER;
    TYPE count_by_param IS TABLE OF PLS_INTEGER INDEX BY PLS_INTEGER;

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

    PROCEDURE verify_range(
        p_iddata         IN ACCT_COA.IDDATA%TYPE,
        p_tahun_dari     IN INTEGER,
        p_tahun_sampai   IN INTEGER,
        p_bulan_dari     IN INTEGER,
        p_bulan_sampai   IN INTEGER,
        p_kodeacc        IN ACCT_COA.KODEACC%TYPE,
        p_scenario       IN VARCHAR2
    ) IS
        v_test_cursor       SYS_REFCURSOR;
        v_baris             NUMBER;
        v_param             NUMBER;
        v_periode           VARCHAR2(20);
        v_kode              VARCHAR2(100);
        v_rekening          VARCHAR2(500);
        v_nojurnal          VARCHAR2(100);
        v_tanggal           DATE;
        v_keterangan        VARCHAR2(4000);
        v_debet             NUMBER;
        v_kredit            NUMBER;
        v_start_date        DATE := TO_DATE(
            TO_CHAR(p_tahun_dari) || LPAD(TO_CHAR(p_bulan_dari), 2, '0') || '01',
            'YYYYMMDD'
        );
        v_end_date          DATE := TO_DATE(
            TO_CHAR(p_tahun_sampai) || LPAD(TO_CHAR(p_bulan_sampai), 2, '0') || '01',
            'YYYYMMDD'
        );
        v_period_date       DATE;
        v_expected_balance  number_by_param;
        v_expected_position position_by_param;
        v_seen              count_by_param;
        v_param_key         PLS_INTEGER;
        v_expected_count    PLS_INTEGER := 0;
        v_opening_count     PLS_INTEGER := 0;
        v_is_header         ACCT_COA.ISHEADER%TYPE;
        v_group             ACCT_COA.GRP%TYPE;
        v_position          ACCT_COA.POSISI%TYPE;
        v_balance           NUMBER;
    BEGIN
        v_period_date := v_start_date;

        WHILE v_period_date <= v_end_date LOOP
            v_param_key := TO_NUMBER(TO_CHAR(v_period_date, 'YYYYMM'));

            BEGIN
                SELECT coa.ISHEADER,
                       coa.GRP,
                       NVL(UPPER(TRIM(coa.POSISI)), 'D'),
                       CASE TO_NUMBER(TO_CHAR(v_period_date, 'MM'))
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
                       END
                  INTO v_is_header,
                       v_group,
                       v_position,
                       v_balance
                  FROM ACCT_COA coa
                 WHERE coa.IDDATA = p_iddata
                   AND coa.TAHUN = TO_NUMBER(TO_CHAR(v_period_date, 'YYYY'))
                   AND coa.KODEACC = p_kodeacc;

                IF v_is_header = 'D' THEN
                    v_expected_balance(v_param_key) := v_balance;
                    v_expected_position(v_param_key) := v_position;
                    v_expected_count := v_expected_count + 1;
                END IF;
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    NULL;
            END;

            v_period_date := ADD_MONTHS(v_period_date, 1);
        END LOOP;

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

            IF v_baris = 0 AND UPPER(TRIM(v_keterangan)) = 'SALDO AWAL' THEN
                v_opening_count := v_opening_count + 1;
                v_param_key := TRUNC(v_param);

                IF v_kode <> p_kodeacc OR NOT v_expected_balance.EXISTS(v_param_key) THEN
                    RAISE_APPLICATION_ERROR(-20983, p_scenario || ': unexpected opening row.');
                END IF;

                IF v_seen.EXISTS(v_param_key) THEN
                    RAISE_APPLICATION_ERROR(
                        -20984,
                        p_scenario || ': duplicate opening row for period ' || TO_CHAR(v_param_key) || '.'
                    );
                END IF;

                v_seen(v_param_key) := 1;

                IF v_periode <> SUBSTR(TO_CHAR(v_param_key), 5, 2) || '/' || SUBSTR(TO_CHAR(v_param_key), 1, 4)
                    OR v_nojurnal <> '000'
                    OR v_tanggal IS NULL
                    OR v_tanggal <> TO_DATE(TO_CHAR(v_param_key) || '01', 'YYYYMMDD') THEN
                    RAISE_APPLICATION_ERROR(-20985, p_scenario || ': opening-row identity is invalid.');
                END IF;

                IF v_expected_position(v_param_key) = 'D' THEN
                    IF NVL(v_debet, 0) <> v_expected_balance(v_param_key) OR NVL(v_kredit, 0) <> 0 THEN
                        RAISE_APPLICATION_ERROR(-20986, p_scenario || ': debit opening balance is invalid.');
                    END IF;
                ELSIF NVL(v_kredit, 0) <> v_expected_balance(v_param_key) OR NVL(v_debet, 0) <> 0 THEN
                    RAISE_APPLICATION_ERROR(-20987, p_scenario || ': credit opening balance is invalid.');
                END IF;
            END IF;
        END LOOP;

        CLOSE v_test_cursor;

        v_param_key := v_expected_balance.FIRST;
        WHILE v_param_key IS NOT NULL LOOP
            IF NOT v_seen.EXISTS(v_param_key) THEN
                RAISE_APPLICATION_ERROR(
                    -20988,
                    p_scenario || ': opening row is missing for period ' || TO_CHAR(v_param_key) || '.'
                );
            END IF;

            v_param_key := v_expected_balance.NEXT(v_param_key);
        END LOOP;

        IF v_opening_count <> v_expected_count THEN
            RAISE_APPLICATION_ERROR(
                -20989,
                p_scenario || ': expected ' || v_expected_count ||
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
        RAISE_APPLICATION_ERROR(-20990, 'ACCT_LAPORAN_V2 package and body must both be VALID.');
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
        RAISE_APPLICATION_ERROR(-20991, 'LAP_BUKUBESAR_V2 signature is incomplete.');
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
        RAISE_APPLICATION_ERROR(-20992, 'Buku Besar cursor must expose exactly ten columns.');
    END IF;

    FOR i IN 1 .. v_column_count LOOP
        IF UPPER(v_columns(i).col_name) <> v_expected_columns(i) THEN
            RAISE_APPLICATION_ERROR(
                -20992,
                'Buku Besar cursor column ' || i || ' must be ' || v_expected_columns(i) ||
                ', received ' || v_columns(i).col_name || '.'
            );
        END IF;
    END LOOP;

    DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
    v_cursor_number := 0;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
                SELECT coa.IDDATA,
                       coa.TAHUN,
                       coa.KODEACC,
                       CASE WHEN NVL(coa.SALDOAWAL, 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."1S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."2S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."3S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."4S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."5S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."6S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."7S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."8S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."9S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."10S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."11S", 0) <> 0 THEN 1 ELSE 0 END AS NON_ZERO_COUNT
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(UPPER(TRIM(coa.POSISI)), 'D') = 'D'
                 ORDER BY NON_ZERO_COUNT DESC, coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE NON_ZERO_COUNT >= 2
           AND ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun, 1, 12, v_kodeacc, 'Debit full-year openings');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no debit Neraca account with multiple non-zero openings is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
                SELECT coa.IDDATA,
                       coa.TAHUN,
                       coa.KODEACC,
                       CASE WHEN NVL(coa.SALDOAWAL, 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."1S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."2S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."3S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."4S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."5S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."6S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."7S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."8S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."9S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."10S", 0) <> 0 THEN 1 ELSE 0 END
                       + CASE WHEN NVL(coa."11S", 0) <> 0 THEN 1 ELSE 0 END AS NON_ZERO_COUNT
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(UPPER(TRIM(coa.POSISI)), 'D') <> 'D'
                 ORDER BY NON_ZERO_COUNT DESC, coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE NON_ZERO_COUNT >= 2
           AND ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun, 1, 12, v_kodeacc, 'Credit full-year openings');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no credit Neraca account with multiple non-zero openings is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, BULAN
          INTO v_iddata, v_tahun, v_kodeacc, v_bulan
          FROM (
                SELECT IDDATA, TAHUN, KODEACC, BULAN
                  FROM (
                        SELECT coa.IDDATA,
                               coa.TAHUN,
                               coa.KODEACC,
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

        verify_range(
            v_iddata,
            v_tahun,
            v_tahun,
            v_bulan,
            LEAST(12, v_bulan + 2),
            v_kodeacc,
            'Mid-year range openings'
        );
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no non-zero mid-year Neraca opening is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC, BULAN
          INTO v_iddata, v_tahun, v_kodeacc, v_bulan
          FROM (
                SELECT IDDATA, TAHUN, KODEACC, BULAN
                  FROM (
                        SELECT coa.IDDATA,
                               coa.TAHUN,
                               coa.KODEACC,
                               coa.SALDOAWAL,
                               coa."1S", coa."2S", coa."3S", coa."4S", coa."5S", coa."6S",
                               coa."7S", coa."8S", coa."9S", coa."10S", coa."11S"
                          FROM ACCT_COA coa
                         WHERE coa.ISHEADER = 'D'
                           AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                  )
                  UNPIVOT INCLUDE NULLS (
                      SALDO FOR BULAN IN (
                          SALDOAWAL AS 1,
                          "1S" AS 2, "2S" AS 3, "3S" AS 4, "4S" AS 5, "5S" AS 6, "6S" AS 7,
                          "7S" AS 8, "8S" AS 9, "9S" AS 10, "10S" AS 11, "11S" AS 12
                      )
                  )
                 WHERE NVL(SALDO, 0) = 0
                 ORDER BY IDDATA, TAHUN, KODEACC, BULAN
          )
         WHERE ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun, v_bulan, v_bulan, v_kodeacc, 'Zero opening inclusion');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no zero Neraca opening fixture is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) NOT IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND (
                       NVL(coa.SALDOAWAL, 0) <> 0
                       OR NVL(coa."1S", 0) <> 0
                       OR NVL(coa."2S", 0) <> 0
                       OR NVL(coa."3S", 0) <> 0
                       OR NVL(coa."4S", 0) <> 0
                       OR NVL(coa."5S", 0) <> 0
                       OR NVL(coa."6S", 0) <> 0
                       OR NVL(coa."7S", 0) <> 0
                       OR NVL(coa."8S", 0) <> 0
                       OR NVL(coa."9S", 0) <> 0
                       OR NVL(coa."10S", 0) <> 0
                       OR NVL(coa."11S", 0) <> 0
                   )
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun, 1, 12, v_kodeacc, 'Profit and loss openings');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no non-zero Laba Rugi opening fixture is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND TRIM(coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND (
                       NVL(coa.SALDOAWAL, 0) <> 0
                       OR NVL(coa."1S", 0) <> 0
                       OR NVL(coa."2S", 0) <> 0
                       OR NVL(coa."3S", 0) <> 0
                       OR NVL(coa."4S", 0) <> 0
                       OR NVL(coa."5S", 0) <> 0
                       OR NVL(coa."6S", 0) <> 0
                       OR NVL(coa."7S", 0) <> 0
                       OR NVL(coa."8S", 0) <> 0
                       OR NVL(coa."9S", 0) <> 0
                       OR NVL(coa."10S", 0) <> 0
                       OR NVL(coa."11S", 0) <> 0
                   )
                   AND NOT EXISTS (
                       SELECT 1
                         FROM ACCT_JURNAL_DTL dtl
                        WHERE dtl.IDDATA = coa.IDDATA
                          AND dtl.GLYEAR = coa.TAHUN
                          AND dtl.KODE = coa.KODEACC
                   )
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun, 1, 12, v_kodeacc, 'Opening-only account');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no opening-only Neraca account is available.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, TAHUN_SAMPAI, KODEACC
          INTO v_iddata, v_tahun, v_tahun_sampai, v_kodeacc
          FROM (
                SELECT start_coa.IDDATA,
                       start_coa.TAHUN,
                       end_coa.TAHUN AS TAHUN_SAMPAI,
                       start_coa.KODEACC
                  FROM ACCT_COA start_coa
                  JOIN ACCT_COA end_coa
                    ON end_coa.IDDATA = start_coa.IDDATA
                   AND end_coa.KODEACC = start_coa.KODEACC
                   AND end_coa.TAHUN = start_coa.TAHUN + 1
                 WHERE start_coa.ISHEADER = 'D'
                   AND end_coa.ISHEADER = 'D'
                   AND TRIM(start_coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND TRIM(end_coa.GRP) IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                   AND NVL(start_coa."11S", 0) <> 0
                   AND NVL(end_coa.SALDOAWAL, 0) <> 0
                 ORDER BY start_coa.IDDATA, start_coa.TAHUN, start_coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_range(v_iddata, v_tahun, v_tahun_sampai, 12, 1, v_kodeacc, 'December-January rollover');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no consecutive-year December/January opening fixture is available.');
    END;

    DBMS_OUTPUT.PUT_LINE('OK: Buku Besar period opening balances and ten-column cursor contract are valid.');
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor_number <> 0 AND DBMS_SQL.IS_OPEN(v_cursor_number) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
        END IF;

        RAISE;
END;
/
