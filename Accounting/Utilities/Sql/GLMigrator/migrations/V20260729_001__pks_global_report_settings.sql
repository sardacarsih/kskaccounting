-- Purpose: Seed global PKS Laba Rugi and Neraca settings from the KSKPKS 2026 configuration.
--          IDDATA remains NULL so every PKS company, including MSLPKS, uses the same settings.
-- Date: 2026-07-29

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    c_migration_id       CONSTANT VARCHAR2(128) := '20260729_001_pks_global_report_settings';
    v_backup_table_count NUMBER := 0;
    v_backup_count       NUMBER := 0;
    v_live_count         NUMBER := 0;
    v_labarugi_count     NUMBER := 0;
    v_neraca_count       NUMBER := 0;
    v_active_count       NUMBER := 0;
    v_inactive_count     NUMBER := 0;
    v_known_iddata_count NUMBER := 0;
    v_distinct_iddata    NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_backup_table_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_RPT_PKS_SCOPE_BAK';

    IF v_backup_table_count = 1 THEN
        EXECUTE IMMEDIATE q'[
            SELECT COUNT(*)
              FROM ACCT_RPT_PKS_SCOPE_BAK
             WHERE MIGRATION_ID = :migration_id]'
          INTO v_backup_count
         USING c_migration_id;
    END IF;

    SELECT COUNT(*),
           COUNT(CASE WHEN section.REPORT_CODE = 'LABARUGI' THEN 1 END),
           COUNT(CASE WHEN section.REPORT_CODE = 'NERACA' THEN 1 END),
           COUNT(CASE WHEN account.IS_ACTIVE = 'Y' THEN 1 END),
           COUNT(CASE WHEN account.IS_ACTIVE = 'N' THEN 1 END),
           COUNT(CASE WHEN TRIM(account.IDDATA) IN ('FSKPKS', 'FSLPKS', 'KSKPKS', 'MSLPKS') THEN 1 END),
           COUNT(DISTINCT TRIM(account.IDDATA))
      INTO v_live_count,
           v_labarugi_count,
           v_neraca_count,
           v_active_count,
           v_inactive_count,
           v_known_iddata_count,
           v_distinct_iddata
      FROM ACCT_REPORT_SECTION_ACCOUNT account
      JOIN ACCT_REPORT_SECTION section
        ON section.SECTION_ID = account.SECTION_ID
     WHERE UPPER(TRIM(account.JENIS_AKUNTING)) = 'PKS'
       AND TRIM(account.IDDATA) IS NOT NULL;

    IF v_live_count = 0 AND v_backup_count = 74 THEN
        DBMS_OUTPUT.PUT_LINE('PRECHECK RESUME: scoped PKS mappings already backed up and removed');
    ELSIF v_live_count <> 74
        OR v_labarugi_count <> 36
        OR v_neraca_count <> 38
        OR v_active_count <> 34
        OR v_inactive_count <> 40
        OR v_known_iddata_count <> 74
        OR v_distinct_iddata <> 4 THEN
        RAISE_APPLICATION_ERROR(
            -20866,
            'PKS scoped baseline mismatch. Expected total/LR/Neraca/active/inactive=74/36/38/34/40; actual=' ||
            v_live_count || '/' || v_labarugi_count || '/' || v_neraca_count || '/' ||
            v_active_count || '/' || v_inactive_count
        );
    ELSE
        DBMS_OUTPUT.PUT_LINE('PRECHECK OK: 74 scoped PKS mappings across FSKPKS, FSLPKS, KSKPKS, and MSLPKS');
    END IF;
END;
/

DECLARE
    v_table_count NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_table_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_RPT_PKS_SCOPE_BAK';

    IF v_table_count = 0 THEN
        EXECUTE IMMEDIATE q'[
            CREATE TABLE ACCT_RPT_PKS_SCOPE_BAK
            (
                MIGRATION_ID      VARCHAR2(128) NOT NULL,
                REPORT_CODE       VARCHAR2(30)  NOT NULL,
                SECTION_ACCOUNT_ID NUMBER        NOT NULL,
                SECTION_ID         NUMBER        NOT NULL,
                JENIS_AKUNTING     VARCHAR2(20)  NOT NULL,
                IDDATA             VARCHAR2(20),
                KODEACC_ROOT       VARCHAR2(30)  NOT NULL,
                DISPLAY_ORDER      NUMBER        NOT NULL,
                INCLUDE_CHILDREN   CHAR(1)       NOT NULL,
                IS_ACTIVE          CHAR(1)       NOT NULL,
                CREATED_AT         DATE          NOT NULL,
                MATCH_MODE         VARCHAR2(20)  NOT NULL,
                GRP_CODE           VARCHAR2(20),
                BACKED_UP_AT       TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
                CONSTRAINT PK_ACCT_RPT_PKS_SCOPE_BAK
                    PRIMARY KEY (MIGRATION_ID, SECTION_ACCOUNT_ID)
            )]';

        DBMS_OUTPUT.PUT_LINE('CREATED BACKUP TABLE: ACCT_RPT_PKS_SCOPE_BAK');
    ELSE
        DBMS_OUTPUT.PUT_LINE('BACKUP TABLE EXISTS: ACCT_RPT_PKS_SCOPE_BAK');
    END IF;
END;
/

DECLARE
    c_migration_id CONSTANT VARCHAR2(128) := '20260729_001_pks_global_report_settings';
    v_live_count   NUMBER := 0;
    v_backup_count NUMBER := 0;
    v_deleted_count NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_live_count
      FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
       AND TRIM(IDDATA) IS NOT NULL;

    IF v_live_count > 0 THEN
        MERGE INTO ACCT_RPT_PKS_SCOPE_BAK target
        USING (
            SELECT c_migration_id MIGRATION_ID,
                   section.REPORT_CODE,
                   account.SECTION_ACCOUNT_ID,
                   account.SECTION_ID,
                   account.JENIS_AKUNTING,
                   account.IDDATA,
                   account.KODEACC_ROOT,
                   account.DISPLAY_ORDER,
                   account.INCLUDE_CHILDREN,
                   account.IS_ACTIVE,
                   account.CREATED_AT,
                   account.MATCH_MODE,
                   account.GRP_CODE
              FROM ACCT_REPORT_SECTION_ACCOUNT account
              JOIN ACCT_REPORT_SECTION section
                ON section.SECTION_ID = account.SECTION_ID
             WHERE UPPER(TRIM(account.JENIS_AKUNTING)) = 'PKS'
               AND TRIM(account.IDDATA) IS NOT NULL
        ) source
           ON (target.MIGRATION_ID = source.MIGRATION_ID
           AND target.SECTION_ACCOUNT_ID = source.SECTION_ACCOUNT_ID)
         WHEN MATCHED THEN
            UPDATE SET target.REPORT_CODE = source.REPORT_CODE,
                       target.SECTION_ID = source.SECTION_ID,
                       target.JENIS_AKUNTING = source.JENIS_AKUNTING,
                       target.IDDATA = source.IDDATA,
                       target.KODEACC_ROOT = source.KODEACC_ROOT,
                       target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                       target.INCLUDE_CHILDREN = source.INCLUDE_CHILDREN,
                       target.IS_ACTIVE = source.IS_ACTIVE,
                       target.CREATED_AT = source.CREATED_AT,
                       target.MATCH_MODE = source.MATCH_MODE,
                       target.GRP_CODE = source.GRP_CODE,
                       target.BACKED_UP_AT = SYSTIMESTAMP
         WHEN NOT MATCHED THEN
            INSERT (
                MIGRATION_ID,
                REPORT_CODE,
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
                GRP_CODE,
                BACKED_UP_AT)
            VALUES (
                source.MIGRATION_ID,
                source.REPORT_CODE,
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
                source.GRP_CODE,
                SYSTIMESTAMP);
    END IF;

    SELECT COUNT(*)
      INTO v_backup_count
      FROM ACCT_RPT_PKS_SCOPE_BAK
     WHERE MIGRATION_ID = c_migration_id;

    IF v_backup_count <> 74 THEN
        RAISE_APPLICATION_ERROR(-20867, 'PKS scoped backup is incomplete: ' || v_backup_count || '/74 rows.');
    END IF;

    DELETE FROM ACCT_REPORT_SECTION_ACCOUNT
     WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
       AND TRIM(IDDATA) IS NOT NULL;

    v_deleted_count := SQL%ROWCOUNT;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('BACKED UP 74 AND DELETED ' || v_deleted_count || ' SCOPED PKS REPORT MAPPINGS');
END;
/

DECLARE
BEGIN
    MERGE INTO ACCT_REPORT_SECTION target
    USING (
        SELECT 'LABARUGI' REPORT_CODE, 'PKS_P1' SECTION_CODE, 4 DISPLAY_LVL FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_HPP', 3 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B1', 5 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B5', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_P2', 4 FROM DUAL UNION ALL
        SELECT 'LABARUGI', 'PKS_B6', 4 FROM DUAL
    ) source
       ON (target.REPORT_CODE = source.REPORT_CODE
       AND target.SECTION_CODE = source.SECTION_CODE)
     WHEN MATCHED THEN
        UPDATE SET target.DISPLAY_LVL = source.DISPLAY_LVL,
                   target.IS_ACTIVE = 'Y';

    MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
    USING (
        SELECT section.SECTION_ID,
               'PKS' JENIS_AKUNTING,
               CAST(NULL AS VARCHAR2(20)) IDDATA,
               source_root.KODEACC_ROOT,
               source_root.DISPLAY_ORDER,
               'TREE' MATCH_MODE
          FROM ACCT_REPORT_SECTION section
          JOIN (
              SELECT '01' SECTION_CODE, '10.00000.000' KODEACC_ROOT, 10 DISPLAY_ORDER FROM DUAL UNION ALL
              SELECT '01', '11.00000.000', 20 FROM DUAL UNION ALL
              SELECT '01', '12.00000.000', 30 FROM DUAL UNION ALL
              SELECT '01', '13.00000.000', 40 FROM DUAL UNION ALL
              SELECT '01', '14.00000.000', 50 FROM DUAL UNION ALL
              SELECT '03', '22.00000.000', 10 FROM DUAL UNION ALL
              SELECT '03', '23.00000.000', 20 FROM DUAL UNION ALL
              SELECT '03', '24.00000.000', 30 FROM DUAL UNION ALL
              SELECT '03', '25.00000.000', 40 FROM DUAL UNION ALL
              SELECT '03', '26.00000.000', 50 FROM DUAL UNION ALL
              SELECT '03', '27.00000.000', 60 FROM DUAL UNION ALL
              SELECT '03', '29.00000.000', 70 FROM DUAL UNION ALL
              SELECT '05', '30.00000.000', 10 FROM DUAL UNION ALL
              SELECT '05', '31.00000.000', 20 FROM DUAL UNION ALL
              SELECT '05', '32.00000.000', 30 FROM DUAL UNION ALL
              SELECT '05', '33.00000.000', 40 FROM DUAL UNION ALL
              SELECT '05', '39.00000.000', 50 FROM DUAL UNION ALL
              SELECT '06', '40.00000.000', 10 FROM DUAL UNION ALL
              SELECT '10', '58.00000.000', 10 FROM DUAL UNION ALL
              SELECT '10', '59.00000.000', 20 FROM DUAL
          ) source_root
            ON source_root.SECTION_CODE = section.SECTION_CODE
         WHERE section.REPORT_CODE = 'NERACA'
    ) source
       ON (target.SECTION_ID = source.SECTION_ID
       AND target.JENIS_AKUNTING = source.JENIS_AKUNTING
       AND NVL(target.IDDATA, '*') = NVL(source.IDDATA, '*')
       AND target.KODEACC_ROOT = source.KODEACC_ROOT)
     WHEN MATCHED THEN
        UPDATE SET target.DISPLAY_ORDER = source.DISPLAY_ORDER,
                   target.INCLUDE_CHILDREN = 'Y',
                   target.IS_ACTIVE = 'Y',
                   target.MATCH_MODE = source.MATCH_MODE,
                   target.GRP_CODE = NULL
     WHEN NOT MATCHED THEN
        INSERT (
            SECTION_ID,
            JENIS_AKUNTING,
            IDDATA,
            KODEACC_ROOT,
            DISPLAY_ORDER,
            INCLUDE_CHILDREN,
            IS_ACTIVE,
            MATCH_MODE,
            GRP_CODE)
        VALUES (
            source.SECTION_ID,
            source.JENIS_AKUNTING,
            source.IDDATA,
            source.KODEACC_ROOT,
            source.DISPLAY_ORDER,
            'Y',
            'Y',
            source.MATCH_MODE,
            NULL);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('SEEDED GLOBAL PKS REPORT SETTINGS FROM KSKPKS');
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
    v_scope_anchor       VARCHAR2(200) :=
        '                   AND (account.IDDATA IS NULL OR account.IDDATA = p_IDDATA)';
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
        RAISE_APPLICATION_ERROR(-20870, 'ACCT_LAPORAN_V2 package body source was not found.');
    END IF;

    v_sql := TO_CLOB('CREATE OR REPLACE ') || v_body;
    v_start_position := DBMS_LOB.INSTR(v_sql, 'PROCEDURE LAP_NERACA_V2(', 1, 1);
    v_end_position := DBMS_LOB.INSTR(v_sql, 'END LAP_NERACA_V2;', v_start_position, 1);

    IF v_start_position = 0 OR v_end_position = 0 THEN
        RAISE_APPLICATION_ERROR(-20871, 'LAP_NERACA_V2 source boundary was not found.');
    END IF;

    v_after_position := v_end_position + LENGTH('END LAP_NERACA_V2;');
    v_procedure_length := v_after_position - v_start_position;
    DBMS_LOB.CREATETEMPORARY(v_procedure, TRUE);
    DBMS_LOB.COPY(v_procedure, v_sql, v_procedure_length, 1, v_start_position);

    IF DBMS_LOB.INSTR(v_procedure, 'FROM MASTER_PT_DTL pks_detail', 1, 1) = 0 THEN
        IF DBMS_LOB.INSTR(v_procedure, v_scope_anchor, 1, 1) = 0 THEN
            RAISE_APPLICATION_ERROR(-20872, 'LAP_NERACA_V2 mapping filter anchor was not found.');
        END IF;

        v_procedure := REPLACE(
            v_procedure,
            v_scope_anchor,
            v_type_filter || CHR(10) || v_scope_anchor
        );
    END IF;

    IF DBMS_LOB.INSTR(v_procedure, 'FROM MASTER_PT_DTL pks_detail', 1, 1) = 0 THEN
        RAISE_APPLICATION_ERROR(-20873, 'LAP_NERACA_V2 PKS mapping filter could not be installed.');
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
        RAISE_APPLICATION_ERROR(-20874, 'ACCT_LAPORAN_V2 has compilation errors after the PKS Neraca update.');
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_procedure) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_procedure);
    END IF;

    IF DBMS_LOB.ISTEMPORARY(v_new_sql) = 1 THEN
        DBMS_LOB.FREETEMPORARY(v_new_sql);
    END IF;

    DBMS_OUTPUT.PUT_LINE('UPDATED LAP_NERACA_V2 FOR GLOBAL PKS SETTINGS');
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
