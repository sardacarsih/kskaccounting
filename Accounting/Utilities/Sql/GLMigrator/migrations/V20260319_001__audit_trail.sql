-- Purpose: Add audit trail support — audit columns on HDR/DTL + ACCT_JURNAL_AUDIT + ACCT_JURNAL_AUDIT_DTL.
-- Date: 2026-03-19

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
    -- 1. Add audit columns to ACCT_JURNAL_HDR
    add_column_if_missing('ACCT_JURNAL_HDR', 'CREATED_DATE',  'TIMESTAMP DEFAULT SYSTIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_HDR', 'CREATED_BY',    'VARCHAR2(50)');
    add_column_if_missing('ACCT_JURNAL_HDR', 'MODIFIED_DATE', 'TIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_HDR', 'MODIFIED_BY',   'VARCHAR2(50)');
    add_column_if_missing('ACCT_JURNAL_HDR', 'MODIFIED_PC',   'VARCHAR2(100)');
    add_column_if_missing('ACCT_JURNAL_HDR', 'MODIFIED_IP',   'VARCHAR2(50)');

    -- 2. Add audit columns to ACCT_JURNAL_DTL
    add_column_if_missing('ACCT_JURNAL_DTL', 'CREATED_DATE',  'TIMESTAMP DEFAULT SYSTIMESTAMP');
    add_column_if_missing('ACCT_JURNAL_DTL', 'MODIFIED_DATE', 'TIMESTAMP');

    -- 3. Create ACCT_JURNAL_AUDIT table
    create_table_if_missing('ACCT_JURNAL_AUDIT', '
        CREATE TABLE ACCT_JURNAL_AUDIT (
            AUDIT_ID       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            JURNALID       NUMBER NOT NULL,
            ACTION_TYPE    VARCHAR2(20) NOT NULL,
            ACTION_DATE    TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            ACTION_BY      VARCHAR2(50) NOT NULL,
            ACTION_PC      VARCHAR2(100),
            ACTION_IP      VARCHAR2(50),
            NOJURNAL       VARCHAR2(30),
            TANGGAL        DATE,
            PERIODE        VARCHAR2(7),
            SUMBER         VARCHAR2(10),
            IDDATA         VARCHAR2(20),
            CHANGED_FIELDS VARCHAR2(1000),
            DELETE_REASON  VARCHAR2(200),
            DETAIL_ROWS_INSERTED  NUMBER DEFAULT 0,
            DETAIL_ROWS_UPDATED   NUMBER DEFAULT 0,
            DETAIL_ROWS_DELETED   NUMBER DEFAULT 0
        )
    ');

    -- 4. Create ACCT_JURNAL_AUDIT_DTL table
    create_table_if_missing('ACCT_JURNAL_AUDIT_DTL', '
        CREATE TABLE ACCT_JURNAL_AUDIT_DTL (
            AUDIT_DTL_ID   NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            AUDIT_ID       NUMBER NOT NULL,
            CHANGE_TYPE    VARCHAR2(10) NOT NULL,
            BARIS          NUMBER,
            KODE           VARCHAR2(20),
            REKENING       VARCHAR2(200),
            DEBET          NUMBER(18,2),
            KREDIT         NUMBER(18,2),
            KETERANGAN     VARCHAR2(200),
            OLD_KODE       VARCHAR2(20),
            OLD_DEBET      NUMBER(18,2),
            OLD_KREDIT     NUMBER(18,2),
            OLD_KETERANGAN VARCHAR2(200),
            CONSTRAINT FK_AUDIT_DTL_HDR FOREIGN KEY (AUDIT_ID) REFERENCES ACCT_JURNAL_AUDIT (AUDIT_ID)
        )
    ');

    -- 5. Indexes
    create_index_if_missing('IDX_AUDIT_JURNALID',      'CREATE INDEX IDX_AUDIT_JURNALID ON ACCT_JURNAL_AUDIT (JURNALID)');
    create_index_if_missing('IDX_AUDIT_ACTION_DATE',    'CREATE INDEX IDX_AUDIT_ACTION_DATE ON ACCT_JURNAL_AUDIT (ACTION_DATE)');
    create_index_if_missing('IDX_AUDIT_ACTION_BY',      'CREATE INDEX IDX_AUDIT_ACTION_BY ON ACCT_JURNAL_AUDIT (ACTION_BY)');
    create_index_if_missing('IDX_AUDIT_IDDATA',         'CREATE INDEX IDX_AUDIT_IDDATA ON ACCT_JURNAL_AUDIT (IDDATA)');
    create_index_if_missing('IDX_AUDIT_DTL_AUDIT_ID',   'CREATE INDEX IDX_AUDIT_DTL_AUDIT_ID ON ACCT_JURNAL_AUDIT_DTL (AUDIT_ID)');
END;
/

-- 6. Backfill CREATED_BY from USERID for existing rows
UPDATE ACCT_JURNAL_HDR SET CREATED_BY = USERID WHERE CREATED_BY IS NULL;
COMMIT;
