-- Purpose: Roll back global PKS report settings and restore the previous Neraca mapping filter.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    c_migration_id CONSTANT VARCHAR2(128) := '20260729_001_pks_global_report_settings';
    v_backup_count NUMBER := 0;
    v_restored_count NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_backup_count
      FROM ACCT_RPT_PKS_SCOPE_BAK
     WHERE MIGRATION_ID = c_migration_id;

    IF v_backup_count <> 74 THEN
        RAISE_APPLICATION_ERROR(-20868, 'Cannot restore scoped PKS mappings: backup contains ' || v_backup_count || '/74 rows.');
    END IF;

    MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
    USING (
        SELECT SECTION_ACCOUNT_ID,
               SECTION_ID,
               JENIS_AKUNTING,
               IDDATA,
               KODEACC_ROOT,
               DISPLAY_ORDER,
               INCLUDE_CHILDREN,
               IS_ACTIVE,
               CREATED_AT,
               MATCH_MODE,
               GRP_CODE
          FROM ACCT_RPT_PKS_SCOPE_BAK
         WHERE MIGRATION_ID = c_migration_id
    ) source
       ON (target.SECTION_ACCOUNT_ID = source.SECTION_ACCOUNT_ID)
     WHEN MATCHED THEN
        UPDATE SET target.SECTION_ID = source.SECTION_ID,
                   target.JENIS_AKUNTING = source.JENIS_AKUNTING,
                   target.IDDATA = source.IDDATA,
                   target.KODEACC_ROOT = source.KODEACC_ROOT,
                   target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.INCLUDE_CHILDREN = source.INCLUDE_CHILDREN,
                   target.IS_ACTIVE = source.IS_ACTIVE,
                   target.CREATED_AT = source.CREATED_AT,
                   target.MATCH_MODE = source.MATCH_MODE,
                   target.GRP_CODE = source.GRP_CODE
     WHEN NOT MATCHED THEN
        INSERT (
            SECTION_ACCOUNT_ID,
            SECTION_ID,
            JENIS_AKUNTING,
            IDDATA,
            KODEACC_ROOT,
            DISPLAY_ORDER,
            INCLUDE_CHILDREN,
            IS_ACTIVE,
            CREATED_AT,
            MATCH_MODE,
            GRP_CODE)
        VALUES (
            source.SECTION_ACCOUNT_ID,
            source.SECTION_ID,
            source.JENIS_AKUNTING,
            source.IDDATA,
            source.KODEACC_ROOT,
            source.DISPLAY_ORDER,
            source.INCLUDE_CHILDREN,
            source.IS_ACTIVE,
            source.CREATED_AT,
            source.MATCH_MODE,
            source.GRP_CODE);

    SELECT COUNT(*)
      INTO v_restored_count
      FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
       AND TRIM(IDDATA) IS NOT NULL;

    IF v_restored_count <> v_backup_count THEN
        RAISE_APPLICATION_ERROR(-20869, 'Scoped PKS restore is incomplete: ' || v_restored_count || '/' || v_backup_count || ' rows.');
    END IF;

    DELETE FROM ACCT_REPORT_SECTION_ACCOUNT account
     WHERE account.JENIS_AKUNTING = 'PKS'
       AND account.IDDATA IS NULL
       AND EXISTS (
            SELECT 1
              FROM ACCT_REPORT_SECTION section
             WHERE section.SECTION_ID = account.SECTION_ID
               AND section.REPORT_CODE = 'NERACA'
       )
       AND account.KODEACC_ROOT IN (
            '10.00000.000', '11.00000.000', '12.00000.000', '13.00000.000', '14.00000.000',
            '22.00000.000', '23.00000.000', '24.00000.000', '25.00000.000', '26.00000.000',
            '27.00000.000', '29.00000.000', '30.00000.000', '31.00000.000', '32.00000.000',
            '33.00000.000', '39.00000.000', '40.00000.000', '58.00000.000', '59.00000.000'
       );

    UPDATE ACCT_REPORT_SECTION
       SET DISPLAY_LVL = 3
     WHERE REPORT_CODE = 'LABARUGI'
       AND SECTION_CODE = 'PKS_P1';

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('RESTORED 74 SCOPED PKS MAPPINGS AND REMOVED GLOBAL PKS NERACA SETTINGS');
END;
/

DECLARE
    v_body               CLOB;
    v_sql                CLOB;
    v_procedure          CLOB;
    v_new_sql            CLOB;
    v_start_position     PLS_INTEGER;
    v_end_position       PLS_INTEGER;
    v_after_position     PLS_INTEGER;
    v_procedure_length   PLS_INTEGER;
    v_remaining          PLS_INTEGER;
    v_cursor             INTEGER := 0;
    v_error_count        INTEGER := 0;
    v_type_filter        VARCHAR2(4000) := q'~                   AND (
                        (
                            account.JENIS_AKUNTING = 'PKS'
                            AND EXISTS (
                                SELECT 1
                                  FROM MASTER_PT_DTL pks_detail
                                 WHERE pks_detail.IDDATA = p_IDDATA
                                   AND TRIM(pks_detail.JENIS_AKUNTANSI) = 'PKS'
                            )
                        )
                        OR
                        (
                            account.JENIS_AKUNTING IN (
                                '*',
                                NVL((
                                    SELECT MAX(TRIM(detail.JENIS_AKUNTANSI))
                                      FROM MASTER_PT_DTL detail
                                     WHERE detail.IDDATA = p_IDDATA
                                ), '*')
                            )
                            AND NOT EXISTS (
                                SELECT 1
                                  FROM MASTER_PT_DTL pks_detail
                                 WHERE pks_detail.IDDATA = p_IDDATA
                                   AND TRIM(pks_detail.JENIS_AKUNTANSI) = 'PKS'
                            )
                        )
                   )~';

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
        RAISE_APPLICATION_ERROR(-20876, 'ACCT_LAPORAN_V2 package body source was not found.');
    END IF;

    v_sql := TO_CLOB('CREATE OR REPLACE ') || v_body;
    v_start_position := DBMS_LOB.INSTR(v_sql, 'PROCEDURE LAP_NERACA_V2(', 1, 1);
    v_end_position := DBMS_LOB.INSTR(v_sql, 'END LAP_NERACA_V2;', v_start_position, 1);

    IF v_start_position = 0 OR v_end_position = 0 THEN
        RAISE_APPLICATION_ERROR(-20877, 'LAP_NERACA_V2 source boundary was not found.');
    END IF;

    v_after_position := v_end_position + LENGTH('END LAP_NERACA_V2;');
    v_procedure_length := v_after_position - v_start_position;
    DBMS_LOB.CREATETEMPORARY(v_procedure, TRUE);
    DBMS_LOB.COPY(v_procedure, v_sql, v_procedure_length, 1, v_start_position);

    IF DBMS_LOB.INSTR(v_procedure, v_type_filter, 1, 1) > 0 THEN
        v_procedure := REPLACE(v_procedure, v_type_filter || CHR(10), '');
    END IF;

    IF DBMS_LOB.INSTR(v_procedure, 'FROM MASTER_PT_DTL pks_detail', 1, 1) > 0 THEN
        RAISE_APPLICATION_ERROR(-20878, 'LAP_NERACA_V2 PKS mapping filter could not be removed.');
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
        RAISE_APPLICATION_ERROR(-20879, 'ACCT_LAPORAN_V2 has compilation errors after rollback.');
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_procedure) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_procedure);
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_new_sql);
    END IF;

    DBMS_OUTPUT.PUT_LINE('RESTORED PREVIOUS LAP_NERACA_V2 MAPPING FILTER');
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
