DECLARE
    v_object_count  INTEGER := 0;
    v_invalid_count INTEGER := 0;
    v_argument_count INTEGER := 0;
    v_iddata        ACCT_COA.IDDATA%TYPE;
    v_tahun         ACCT_COA.TAHUN%TYPE;
    v_kodeacc       ACCT_COA.KODEACC%TYPE;
    v_cursor        SYS_REFCURSOR;
    v_cursor_number INTEGER;
    v_column_count  INTEGER;
    v_columns       DBMS_SQL.DESC_TAB3;
    v_has_kodeacc   BOOLEAN := FALSE;
    v_has_namaacc   BOOLEAN := FALSE;
    v_has_parentacc BOOLEAN := FALSE;
    v_has_posisi    BOOLEAN := FALSE;
    v_has_isheader  BOOLEAN := FALSE;
    v_has_debet     BOOLEAN := FALSE;
    v_has_kredit    BOOLEAN := FALSE;
    v_has_saldoakhir BOOLEAN := FALSE;
BEGIN
    SELECT COUNT(*),
           COUNT(CASE WHEN STATUS <> 'VALID' THEN 1 END)
      INTO v_object_count, v_invalid_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_COA_DRILLDOWN_V1'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY');

    IF v_object_count <> 2 OR v_invalid_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20967, 'ACCT_COA_DRILLDOWN_V1 package and body must both be VALID.');
    END IF;

    SELECT COUNT(*)
      INTO v_argument_count
      FROM USER_ARGUMENTS
     WHERE PACKAGE_NAME = 'ACCT_COA_DRILLDOWN_V1'
       AND OBJECT_NAME = 'GET_CHILDREN'
       AND DATA_LEVEL = 0
       AND ARGUMENT_NAME IN ('P_IDDATA', 'P_BULAN', 'P_TAHUN', 'P_USERID', 'P_KODEACC', 'P_CURSOR');

    IF v_argument_count <> 6 THEN
        RAISE_APPLICATION_ERROR(-20968, 'ACCT_COA_DRILLDOWN_V1.GET_CHILDREN signature is incomplete.');
    END IF;

    BEGIN
        SELECT iddata, tahun, kodeacc
          INTO v_iddata, v_tahun, v_kodeacc
          FROM (
              SELECT coa.IDDATA AS iddata, coa.TAHUN AS tahun, coa.KODEACC AS kodeacc
                FROM ACCT_COA coa
               WHERE coa.ISHEADER = 'G'
               ORDER BY coa.IDDATA, coa.TAHUN, coa.KODEACC
          )
         WHERE ROWNUM = 1;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            DBMS_OUTPUT.PUT_LINE('SKIP: no Gen G COA row is available to verify the cursor schema.');
            v_kodeacc := NULL;
    END;

    IF v_kodeacc IS NOT NULL THEN
        ACCT_COA_DRILLDOWN_V1.GET_CHILDREN(v_iddata, 1, v_tahun, 'VERIFY', v_kodeacc, v_cursor);
        v_cursor_number := DBMS_SQL.TO_CURSOR_NUMBER(v_cursor);
        DBMS_SQL.DESCRIBE_COLUMNS3(v_cursor_number, v_column_count, v_columns);

        FOR i IN 1 .. v_column_count LOOP
            CASE UPPER(v_columns(i).col_name)
                WHEN 'KODEACC' THEN v_has_kodeacc := TRUE;
                WHEN 'NAMAACC' THEN v_has_namaacc := TRUE;
                WHEN 'PARENTACC' THEN v_has_parentacc := TRUE;
                WHEN 'POSISI' THEN v_has_posisi := TRUE;
                WHEN 'ISHEADER' THEN v_has_isheader := TRUE;
                WHEN 'DEBET' THEN v_has_debet := TRUE;
                WHEN 'KREDIT' THEN v_has_kredit := TRUE;
                WHEN 'SALDOAKHIR' THEN v_has_saldoakhir := TRUE;
            END CASE;
        END LOOP;

        DBMS_SQL.CLOSE_CURSOR(v_cursor_number);

        IF NOT (v_has_kodeacc AND v_has_namaacc AND v_has_parentacc AND v_has_posisi
            AND v_has_isheader AND v_has_debet AND v_has_kredit AND v_has_saldoakhir) THEN
            RAISE_APPLICATION_ERROR(-20969, 'COA drilldown cursor contract must expose KODEACC, NAMAACC, PARENTACC, POSISI, ISHEADER, DEBET, KREDIT, and SALDOAKHIR.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('OK: COA drilldown package, signature, and Gen G cursor schema are valid.');
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor_number IS NOT NULL AND DBMS_SQL.IS_OPEN(v_cursor_number) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor_number);
        END IF;
        RAISE;
END;
/
