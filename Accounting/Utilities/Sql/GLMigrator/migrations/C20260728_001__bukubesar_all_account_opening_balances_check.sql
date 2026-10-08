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
    v_bulan             INTEGER;
    v_kodeacc           ACCT_COA.KODEACC%TYPE;

    PROCEDURE verify_opening(
        p_iddata       IN ACCT_COA.IDDATA%TYPE,
        p_tahun        IN INTEGER,
        p_bulan        IN INTEGER,
        p_kodeacc      IN ACCT_COA.KODEACC%TYPE,
        p_scenario     IN VARCHAR2
    ) IS
        v_test_cursor      SYS_REFCURSOR;
        v_baris            NUMBER;
        v_param            NUMBER;
        v_periode          VARCHAR2(20);
        v_kode             VARCHAR2(100);
        v_rekening         VARCHAR2(500);
        v_nojurnal         VARCHAR2(100);
        v_tanggal          DATE;
        v_keterangan       VARCHAR2(4000);
        v_debet            NUMBER;
        v_kredit           NUMBER;
        v_expected_balance NUMBER;
        v_position         ACCT_COA.POSISI%TYPE;
        v_opening_count    INTEGER := 0;
        v_expected_param   NUMBER := (p_tahun * 100) + p_bulan;
    BEGIN
        SELECT NVL(UPPER(TRIM(coa.POSISI)), 'D'),
               CASE p_bulan
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
          INTO v_position, v_expected_balance
          FROM ACCT_COA coa
         WHERE coa.IDDATA = p_iddata
           AND coa.TAHUN = p_tahun
           AND coa.KODEACC = p_kodeacc
           AND coa.ISHEADER = 'D';

        ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2(
            p_iddata,
            p_tahun,
            p_tahun,
            p_bulan,
            p_bulan,
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

                IF v_param <> v_expected_param
                    OR v_periode <> LPAD(TO_CHAR(p_bulan), 2, '0') || '/' || TO_CHAR(p_tahun)
                    OR v_kode <> p_kodeacc
                    OR v_nojurnal <> '000'
                    OR v_tanggal <> TO_DATE(TO_CHAR(v_expected_param) || '01', 'YYYYMMDD') THEN
                    RAISE_APPLICATION_ERROR(-20866, p_scenario || ': opening-row identity is invalid.');
                END IF;

                IF v_position = 'D' THEN
                    IF NVL(v_debet, 0) <> v_expected_balance OR NVL(v_kredit, 0) <> 0 THEN
                        RAISE_APPLICATION_ERROR(-20867, p_scenario || ': debit opening balance is invalid.');
                    END IF;
                ELSIF NVL(v_kredit, 0) <> v_expected_balance OR NVL(v_debet, 0) <> 0 THEN
                    RAISE_APPLICATION_ERROR(-20868, p_scenario || ': credit opening balance is invalid.');
                END IF;
            END IF;
        END LOOP;

        CLOSE v_test_cursor;

        IF v_opening_count <> 1 THEN
            RAISE_APPLICATION_ERROR(
                -20869,
                p_scenario || ': expected exactly one opening row, received ' || v_opening_count || '.'
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
        RAISE_APPLICATION_ERROR(-20870, 'ACCT_LAPORAN_V2 package and body must both be VALID.');
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
        RAISE_APPLICATION_ERROR(-20871, 'LAP_BUKUBESAR_V2 signature is incomplete.');
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
        RAISE_APPLICATION_ERROR(-20872, 'Buku Besar cursor must expose exactly ten columns.');
    END IF;

    FOR i IN 1 .. v_column_count LOOP
        IF UPPER(v_columns(i).col_name) <> v_expected_columns(i) THEN
            RAISE_APPLICATION_ERROR(
                -20872,
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
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        FOR month_number IN 1 .. 12 LOOP
            verify_opening(v_iddata, v_tahun, month_number, v_kodeacc, 'Every-month opening');
        END LOOP;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no detail account is available for the every-month scenario.');
    END;

    BEGIN
        SELECT IDDATA, TAHUN, KODEACC
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
                SELECT coa.IDDATA, coa.TAHUN, coa.KODEACC
                  FROM ACCT_COA coa
                 WHERE coa.ISHEADER = 'D'
                   AND (
                       TRIM(coa.GRP) NOT IN ('01', '02', '03', '04', '05', '06', '07', '08', '09', '10')
                       OR coa.GRP IS NULL
                   )
                 ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;

        verify_opening(v_iddata, v_tahun, 1, v_kodeacc, 'Account-type-independent opening');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no formerly excluded account type is available.');
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

        verify_opening(v_iddata, v_tahun, v_bulan, v_kodeacc, 'Zero opening');
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no zero opening-balance fixture is available.');
    END;

    DBMS_OUTPUT.PUT_LINE('OK: Buku Besar emits one opening row every month for all detail account types.');
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor_number <> 0 AND DBMS_SQL.IS_OPEN(v_cursor_number) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
        END IF;

        RAISE;
END;
/
