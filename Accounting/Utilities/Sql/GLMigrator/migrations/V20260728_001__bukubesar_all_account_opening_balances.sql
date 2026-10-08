-- Purpose: Show a General Ledger opening-balance row every month for every detail account.
-- Date: 2026-07-28

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_body                    CLOB;
    v_sql                     CLOB;
    v_procedure               CLOB;
    v_new_sql                 CLOB;
    v_start_position          PLS_INTEGER;
    v_end_position            PLS_INTEGER;
    v_after_position          PLS_INTEGER;
    v_procedure_length        PLS_INTEGER;
    v_remaining               PLS_INTEGER;
    v_cursor                  INTEGER := 0;
    v_error_count             INTEGER := 0;
    v_group_filter            VARCHAR2(500) :=
        '                   AND TRIM(coa.GRP) IN (''01'', ''02'', ''03'', ''04'', ''05'', ''06'', ''07'', ''08'', ''09'', ''10'')';
    v_non_zero_filter         VARCHAR2(200) :=
        '                 WHERE opening.SALDO <> 0';

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
        RAISE_APPLICATION_ERROR(-20860, 'ACCT_LAPORAN_V2 package body source was not found.');
    END IF;

    v_sql := TO_CLOB('CREATE OR REPLACE ') || v_body;
    v_start_position := DBMS_LOB.INSTR(v_sql, 'PROCEDURE LAP_BUKUBESAR_V2(', 1, 1);
    v_end_position := DBMS_LOB.INSTR(v_sql, 'END LAP_BUKUBESAR_V2;', v_start_position, 1);

    IF v_start_position = 0 OR v_end_position = 0 THEN
        RAISE_APPLICATION_ERROR(-20861, 'LAP_BUKUBESAR_V2 source boundary was not found.');
    END IF;

    v_after_position := v_end_position + LENGTH('END LAP_BUKUBESAR_V2;');
    v_procedure_length := v_after_position - v_start_position;
    DBMS_LOB.CREATETEMPORARY(v_procedure, TRUE);
    DBMS_LOB.COPY(v_procedure, v_sql, v_procedure_length, 1, v_start_position);

    IF DBMS_LOB.INSTR(v_procedure, v_group_filter, 1, 1) = 0 THEN
        RAISE_APPLICATION_ERROR(-20862, 'Buku Besar account-type filter was not found.');
    END IF;

    IF DBMS_LOB.INSTR(v_procedure, v_non_zero_filter, 1, 1) = 0 THEN
        RAISE_APPLICATION_ERROR(-20863, 'Buku Besar non-zero opening filter was not found.');
    END IF;

    v_procedure := REPLACE(v_procedure, v_group_filter, '');
    v_procedure := REPLACE(v_procedure, v_non_zero_filter, '');

    IF DBMS_LOB.INSTR(v_procedure, v_group_filter, 1, 1) <> 0
        OR DBMS_LOB.INSTR(v_procedure, v_non_zero_filter, 1, 1) <> 0 THEN
        RAISE_APPLICATION_ERROR(-20864, 'Buku Besar opening-balance filters could not be removed.');
    END IF;

    DBMS_LOB.CREATETEMPORARY(v_new_sql, TRUE);

    IF v_start_position > 1 THEN
        DBMS_LOB.COPY(v_new_sql, v_sql, v_start_position - 1, 1, 1);
    END IF;

    DBMS_LOB.APPEND(v_new_sql, v_procedure);
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
        RAISE_APPLICATION_ERROR(-20865, 'ACCT_LAPORAN_V2 has compilation errors after the all-account opening-balance repair.');
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_procedure) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_procedure);
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_new_sql);
    END IF;

    DBMS_OUTPUT.PUT_LINE('REPAIRED ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2 FOR ALL MONTHLY ACCOUNT OPENINGS');
EXCEPTION
    WHEN OTHERS THEN
        IF v_cursor <> 0 AND DBMS_SQL.IS_OPEN(v_cursor) THEN
            DBMS_SQL.CLOSE_CURSOR(v_cursor);
        END IF;

        IF DBMS_LOB.ISTEMPORARY(v_procedure) = 1 THEN
            DBMS_LOB.FREETEMPORARY(v_procedure);
        END IF;

        IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
            DBMS_LOB.FREETEMPORARY(v_new_sql);
        END IF;

        RAISE;
END;
/
