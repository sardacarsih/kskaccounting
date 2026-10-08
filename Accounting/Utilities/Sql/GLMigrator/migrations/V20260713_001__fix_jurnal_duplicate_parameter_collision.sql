SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    l_sql CLOB;
    l_signature_pattern VARCHAR2(4000);
    l_signature_replacement VARCHAR2(4000);
    l_predicate_old VARCHAR2(4000);
    l_predicate_old_aliased VARCHAR2(4000);
    l_predicate_new VARCHAR2(4000);
    l_cursor INTEGER := 0;
    l_error_count INTEGER;

    PROCEDURE execute_clob(p_sql IN CLOB) IS
    BEGIN
        l_cursor := DBMS_SQL.OPEN_CURSOR;
        DBMS_SQL.PARSE(l_cursor, p_sql, DBMS_SQL.NATIVE);
        DBMS_SQL.CLOSE_CURSOR(l_cursor);
        l_cursor := 0;
    EXCEPTION
        WHEN OTHERS THEN
            IF l_cursor <> 0 AND DBMS_SQL.IS_OPEN(l_cursor) THEN
                DBMS_SQL.CLOSE_CURSOR(l_cursor);
            END IF;
            l_cursor := 0;
            RAISE;
    END;
BEGIN
    SELECT XMLCAST(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE) AS CLOB)
      INTO l_sql
      FROM USER_SOURCE
     WHERE NAME = 'ACCT_JURNAL_V2'
       AND TYPE = 'PACKAGE BODY';

    IF l_sql IS NULL THEN
        RAISE_APPLICATION_ERROR(-20960, 'ACCT_JURNAL_V2 package body source not found.');
    END IF;

    IF UPPER(DBMS_LOB.SUBSTR(l_sql, 50, 1)) NOT LIKE 'CREATE OR REPLACE%' THEN
        l_sql := 'CREATE OR REPLACE ' || l_sql;
    END IF;

    l_signature_pattern :=
        'FUNCTION CekNoJurnalExist_input\(p_IDDATA[[:space:]]+IN[[:space:]]+VARCHAR2,[[:space:]]+nojurnal[[:space:]]+IN[[:space:]]+VARCHAR2,[[:space:]]+p_periode[[:space:]]+IN[[:space:]]+VARCHAR2\)[[:space:]]+RETURN[[:space:]]+INTEGER[[:space:]]+AS[[:space:]]+result[[:space:]]+INTEGER;';
    l_signature_replacement :=
        'FUNCTION CekNoJurnalExist_input(p_IDDATA IN VARCHAR2, nojurnal IN VARCHAR2, p_periode IN VARCHAR2) RETURN INTEGER AS'
        || CHR(10) || CHR(9) || 'result INTEGER;'
        || CHR(10) || CHR(9) || 'input_nojurnal VARCHAR2(30) := nojurnal;';
    l_predicate_old := 'UPPER(NOJURNAL) = UPPER(nojurnal);';
    l_predicate_old_aliased := 'UPPER(h.NOJURNAL) = UPPER(nojurnal);';
    l_predicate_new := 'UPPER(NOJURNAL) = UPPER(input_nojurnal);';

    IF DBMS_LOB.INSTR(l_sql, 'input_nojurnal VARCHAR2(30)') > 0 THEN
        DBMS_OUTPUT.PUT_LINE('ACCT_JURNAL_V2 duplicate-check parameter collision is already repaired.');
        RETURN;
    END IF;

    IF DBMS_LOB.INSTR(l_sql, 'FUNCTION CekNoJurnalExist_input') = 0 THEN
        RAISE_APPLICATION_ERROR(-20961, 'CekNoJurnalExist_input declaration marker not found.');
    END IF;

    IF DBMS_LOB.INSTR(l_sql, l_predicate_old) = 0
       AND DBMS_LOB.INSTR(l_sql, l_predicate_old_aliased) = 0 THEN
        RAISE_APPLICATION_ERROR(-20962, 'CekNoJurnalExist_input predicate marker not found.');
    END IF;

    l_sql := REGEXP_REPLACE(l_sql, l_signature_pattern, l_signature_replacement, 1, 0, 'c');
    IF DBMS_LOB.INSTR(l_sql, 'input_nojurnal VARCHAR2(30)') = 0 THEN
        RAISE_APPLICATION_ERROR(-20963, 'CekNoJurnalExist_input declaration could not be repaired.');
    END IF;
    l_sql := REPLACE(l_sql, l_predicate_old, l_predicate_new);
    l_sql := REPLACE(l_sql, l_predicate_old_aliased, l_predicate_new);
    execute_clob(l_sql);

    SELECT COUNT(1)
      INTO l_error_count
      FROM USER_ERRORS
     WHERE NAME = 'ACCT_JURNAL_V2'
       AND TYPE IN ('PACKAGE', 'PACKAGE BODY');

    IF l_error_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20964, 'ACCT_JURNAL_V2 has compilation errors after repair.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('Repaired ACCT_JURNAL_V2.CekNoJurnalExist_input parameter collision.');
END;
/
