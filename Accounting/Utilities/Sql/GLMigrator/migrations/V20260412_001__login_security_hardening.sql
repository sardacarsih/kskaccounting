-- Purpose: Add persistent login hardening state on MASTER_LOGIN and ACCT_LOGIN_AUDIT event history.
-- Date: 2026-04-12

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE add_column_if_missing(p_table IN VARCHAR2, p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || UPPER(p_table) || ' ADD (' || UPPER(p_column) || ' ' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED: ' || UPPER(p_table) || '.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table) || '.' || UPPER(p_column));
        END IF;
    END;

    PROCEDURE create_table_if_missing(p_table IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED TABLE: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE create_index_if_missing(p_index_name IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED INDEX: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_index_name));
        END IF;
    END;
BEGIN
    add_column_if_missing('MASTER_LOGIN', 'FAILED_LOGIN_COUNT', 'NUMBER DEFAULT 0');
    add_column_if_missing('MASTER_LOGIN', 'LOCKOUT_UNTIL_UTC', 'TIMESTAMP');
    add_column_if_missing('MASTER_LOGIN', 'LAST_LOGIN_AT_UTC', 'TIMESTAMP');
    add_column_if_missing('MASTER_LOGIN', 'LAST_FAILED_LOGIN_AT_UTC', 'TIMESTAMP');
    add_column_if_missing('MASTER_LOGIN', 'PASSWORD_CHANGED_AT_UTC', 'TIMESTAMP');
    add_column_if_missing('MASTER_LOGIN', 'PASSWORD_RESET_REQUIRED', 'CHAR(1) DEFAULT ''N''');

    create_table_if_missing('ACCT_LOGIN_AUDIT', '
        CREATE TABLE ACCT_LOGIN_AUDIT (
            AUDIT_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            EVENT_AT_UTC       TIMESTAMP DEFAULT CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP) NOT NULL,
            USERID             VARCHAR2(30),
            MODULE_NAME        VARCHAR2(30),
            EVENT_TYPE         VARCHAR2(50) NOT NULL,
            SUCCESS_FLAG       CHAR(1) DEFAULT ''N'' NOT NULL,
            CLIENT_MACHINE     VARCHAR2(128),
            DETAIL_MESSAGE     VARCHAR2(400),
            LOCKOUT_UNTIL_UTC  TIMESTAMP,
            IDDATA             VARCHAR2(20)
        )
    ');

    create_index_if_missing('IDX_LOGIN_AUDIT_USER_EVENT_AT', 'CREATE INDEX IDX_LOGIN_AUDIT_USER_EVENT_AT ON ACCT_LOGIN_AUDIT (USERID, EVENT_AT_UTC)');
    create_index_if_missing('IDX_LOGIN_AUDIT_EVENT_TYPE', 'CREATE INDEX IDX_LOGIN_AUDIT_EVENT_TYPE ON ACCT_LOGIN_AUDIT (EVENT_TYPE)');
    create_index_if_missing('IDX_LOGIN_AUDIT_EVENT_AT', 'CREATE INDEX IDX_LOGIN_AUDIT_EVENT_AT ON ACCT_LOGIN_AUDIT (EVENT_AT_UTC)');
END;
/

UPDATE MASTER_LOGIN
   SET FAILED_LOGIN_COUNT = NVL(FAILED_LOGIN_COUNT, 0),
       PASSWORD_RESET_REQUIRED = NVL(PASSWORD_RESET_REQUIRED, 'N');

COMMIT;
