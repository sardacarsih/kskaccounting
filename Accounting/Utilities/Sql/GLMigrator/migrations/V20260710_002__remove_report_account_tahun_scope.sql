-- Purpose: Remove TAHUN scope from ACCT_REPORT_SECTION_ACCOUNT.
--          Report year still comes from report parameters and ACCT_COA, not metadata settings.
-- Date: 2026-07-10

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_count NUMBER;
    v_cursor INTEGER := 0;

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

    PROCEDURE recreate_package_body(p_name IN VARCHAR2) IS
        v_sql CLOB;
    BEGIN
        SELECT 'CREATE OR REPLACE ' || XMLCAST(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE) AS CLOB)
          INTO v_sql
          FROM USER_SOURCE
         WHERE NAME = p_name
           AND TYPE = 'PACKAGE BODY';

        IF v_sql IS NULL THEN
            RAISE_APPLICATION_ERROR(-20956, 'Package body source not found: ' || p_name);
        END IF;

        v_sql := REPLACE(v_sql, '(account.TAHUN IS NULL OR account.TAHUN = p_TAHUN)', '1 = 1');
        v_sql := REPLACE(v_sql, '(account.TAHUN IS NULL OR account.TAHUN = p_tahun)', '1 = 1');

        execute_clob(v_sql);
    END;
BEGIN
    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT'
       AND COLUMN_NAME = 'TAHUN';

    IF v_count > 0 THEN
        MERGE INTO ACCT_REPORT_SECTION_ACCOUNT target
        USING (
            SELECT SECTION_ACCOUNT_ID,
                   MIN_DISPLAY_ORDER,
                   CASE WHEN HAS_ACTIVE = 1 THEN 'Y' ELSE 'N' END IS_ACTIVE
              FROM (
                    SELECT account.SECTION_ACCOUNT_ID,
                           ROW_NUMBER() OVER (
                               PARTITION BY account.SECTION_ID,
                                            account.JENIS_AKUNTING,
                                            NVL(account.IDDATA, '*'),
                                            account.KODEACC_ROOT,
                                            NVL(account.MATCH_MODE, 'TREE'),
                                            NVL(account.GRP_CODE, CHR(0))
                               ORDER BY account.DISPLAY_ORDER, account.SECTION_ACCOUNT_ID
                           ) KEEP_ROW,
                           MIN(account.DISPLAY_ORDER) OVER (
                               PARTITION BY account.SECTION_ID,
                                            account.JENIS_AKUNTING,
                                            NVL(account.IDDATA, '*'),
                                            account.KODEACC_ROOT,
                                            NVL(account.MATCH_MODE, 'TREE'),
                                            NVL(account.GRP_CODE, CHR(0))
                           ) MIN_DISPLAY_ORDER,
                           MAX(CASE WHEN account.IS_ACTIVE = 'Y' THEN 1 ELSE 0 END) OVER (
                               PARTITION BY account.SECTION_ID,
                                            account.JENIS_AKUNTING,
                                            NVL(account.IDDATA, '*'),
                                            account.KODEACC_ROOT,
                                            NVL(account.MATCH_MODE, 'TREE'),
                                            NVL(account.GRP_CODE, CHR(0))
                           ) HAS_ACTIVE
                      FROM ACCT_REPORT_SECTION_ACCOUNT account
                   )
             WHERE KEEP_ROW = 1
        ) source
           ON (target.SECTION_ACCOUNT_ID = source.SECTION_ACCOUNT_ID)
         WHEN MATCHED THEN
            UPDATE SET target.DISPLAY_ORDER = source.MIN_DISPLAY_ORDER,
                       target.IS_ACTIVE = source.IS_ACTIVE;

        DELETE FROM ACCT_REPORT_SECTION_ACCOUNT
         WHERE SECTION_ACCOUNT_ID IN (
               SELECT SECTION_ACCOUNT_ID
                 FROM (
                       SELECT account.SECTION_ACCOUNT_ID,
                              ROW_NUMBER() OVER (
                                  PARTITION BY account.SECTION_ID,
                                               account.JENIS_AKUNTING,
                                               NVL(account.IDDATA, '*'),
                                               account.KODEACC_ROOT,
                                               NVL(account.MATCH_MODE, 'TREE'),
                                               NVL(account.GRP_CODE, CHR(0))
                                  ORDER BY account.DISPLAY_ORDER, account.SECTION_ACCOUNT_ID
                              ) KEEP_ROW
                         FROM ACCT_REPORT_SECTION_ACCOUNT account
                      )
                WHERE KEEP_ROW > 1
         );

        EXECUTE IMMEDIATE 'ALTER TABLE ACCT_REPORT_SECTION_ACCOUNT DROP COLUMN TAHUN';
    END IF;

    recreate_package_body('ACCT_REPORT_ENGINE_V1');
    recreate_package_body('ACCT_LAPORAN_V2');
    recreate_package_body('ACCT_JURNAL_CLOSING_V2');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_ERRORS
     WHERE NAME IN ('ACCT_REPORT_ENGINE_V1', 'ACCT_LAPORAN_V2', 'ACCT_JURNAL_CLOSING_V2')
       AND TYPE = 'PACKAGE BODY';

    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20957, 'Report package body has compilation errors after removing TAHUN scope.');
    END IF;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('REMOVED ACCT_REPORT_SECTION_ACCOUNT.TAHUN SCOPE');
END;
/
