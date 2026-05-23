-- Purpose: Add enterprise-ready Fixed Asset core schema, depreciation run tables, lifecycle transactions, approval, and audit support.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
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
            DBMS_OUTPUT.PUT_LINE('SKIPPED TABLE (exists): ' || UPPER(p_table));
        END IF;
    END;

    PROCEDURE create_index_if_missing(p_index IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index);

        IF v_count = 0 THEN
            BEGIN
                EXECUTE IMMEDIATE p_sql;
                DBMS_OUTPUT.PUT_LINE('CREATED INDEX: ' || UPPER(p_index));
            EXCEPTION
                WHEN OTHERS THEN
                    IF SQLCODE = -1408 THEN
                        DBMS_OUTPUT.PUT_LINE('SKIPPED INDEX (same column list exists): ' || UPPER(p_index));
                    ELSE
                        RAISE;
                    END IF;
            END;
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED INDEX (exists): ' || UPPER(p_index));
        END IF;
    END;
BEGIN
    create_table_if_missing('ACCT_FA_CATEGORY', '
        CREATE TABLE ACCT_FA_CATEGORY
        (
            CATEGORY_ID        NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA             VARCHAR2(20) NOT NULL,
            CATEGORY_CODE      VARCHAR2(30) NOT NULL,
            CATEGORY_NAME      VARCHAR2(100) NOT NULL,
            IS_ACTIVE          CHAR(1) DEFAULT ''Y'' NOT NULL,
            CREATED_BY         VARCHAR2(50),
            CREATED_DATE       TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY        VARCHAR2(50),
            MODIFIED_DATE      TIMESTAMP,
            CONSTRAINT UK_ACCT_FA_CAT UNIQUE (IDDATA, CATEGORY_CODE)
        )
    ');

    create_table_if_missing('ACCT_FA_GROUP', '
        CREATE TABLE ACCT_FA_GROUP
        (
            GROUP_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA             VARCHAR2(20) NOT NULL,
            CATEGORY_ID        NUMBER NOT NULL,
            GROUP_CODE         VARCHAR2(30) NOT NULL,
            GROUP_NAME         VARCHAR2(100) NOT NULL,
            IS_ACTIVE          CHAR(1) DEFAULT ''Y'' NOT NULL,
            CREATED_BY         VARCHAR2(50),
            CREATED_DATE       TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY        VARCHAR2(50),
            MODIFIED_DATE      TIMESTAMP,
            CONSTRAINT FK_ACCT_FA_GRP_CAT FOREIGN KEY (CATEGORY_ID) REFERENCES ACCT_FA_CATEGORY (CATEGORY_ID),
            CONSTRAINT UK_ACCT_FA_GRP UNIQUE (IDDATA, GROUP_CODE)
        )
    ');

    create_table_if_missing('ACCT_FA_ACCOUNT_MAP', '
        CREATE TABLE ACCT_FA_ACCOUNT_MAP
        (
            MAP_ID                NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA                VARCHAR2(20) NOT NULL,
            CATEGORY_ID           NUMBER NOT NULL,
            ASSET_ACCT            VARCHAR2(20) NOT NULL,
            ACC_DEPR_ACCT         VARCHAR2(20) NOT NULL,
            DEPR_EXP_ACCT         VARCHAR2(20) NOT NULL,
            GAIN_DISP_ACCT        VARCHAR2(20),
            LOSS_DISP_ACCT        VARCHAR2(20),
            REVAL_SURPLUS_ACCT    VARCHAR2(20),
            REVAL_DEFICIT_ACCT    VARCHAR2(20),
            CIP_ACCT              VARCHAR2(20),
            EFFECTIVE_FROM        DATE DEFAULT TRUNC(SYSDATE) NOT NULL,
            IS_ACTIVE             CHAR(1) DEFAULT ''Y'' NOT NULL,
            CREATED_BY            VARCHAR2(50),
            CREATED_DATE          TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY           VARCHAR2(50),
            MODIFIED_DATE         TIMESTAMP,
            CONSTRAINT FK_ACCT_FA_MAP_CAT FOREIGN KEY (CATEGORY_ID) REFERENCES ACCT_FA_CATEGORY (CATEGORY_ID)
        )
    ');

    create_table_if_missing('ACCT_FA_ASSET', '
        CREATE TABLE ACCT_FA_ASSET
        (
            ASSET_ID                 NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA                   VARCHAR2(20) NOT NULL,
            ASSET_CODE               VARCHAR2(50) NOT NULL,
            ASSET_NAME               VARCHAR2(200) NOT NULL,
            CATEGORY_ID              NUMBER NOT NULL,
            GROUP_ID                 NUMBER,
            ACQUISITION_DATE         DATE NOT NULL,
            IN_SERVICE_DATE          DATE,
            DEPRECIATION_START_DATE  DATE,
            ACQUISITION_COST         NUMBER(18,2) NOT NULL,
            RESIDUAL_VALUE           NUMBER(18,2) DEFAULT 0 NOT NULL,
            USEFUL_LIFE_MONTHS       NUMBER(6) NOT NULL,
            DEPR_METHOD              VARCHAR2(10) NOT NULL,
            CURRENCY_CODE            VARCHAR2(10) DEFAULT ''IDR'' NOT NULL,
            EXCHANGE_RATE            NUMBER(18,8) DEFAULT 1 NOT NULL,
            STATUS                   VARCHAR2(30) DEFAULT ''DRAFT'' NOT NULL,
            DEPARTMENT_ID            VARCHAR2(30),
            COST_CENTER_ID           VARCHAR2(30),
            LOCATION_ID              VARCHAR2(30),
            VENDOR_ID                VARCHAR2(30),
            SERIAL_NO                VARCHAR2(100),
            POLICE_NO                VARCHAR2(50),
            IDENT_NO                 VARCHAR2(100),
            IMPROVEMENT_TOTAL        NUMBER(18,2) DEFAULT 0 NOT NULL,
            REVALUATION_DELTA_TOTAL  NUMBER(18,2) DEFAULT 0 NOT NULL,
            IMPAIRMENT_TOTAL         NUMBER(18,2) DEFAULT 0 NOT NULL,
            NOTES                    VARCHAR2(1000),
            IS_DELETED               CHAR(1) DEFAULT ''N'' NOT NULL,
            CREATED_BY               VARCHAR2(50),
            CREATED_PC               VARCHAR2(100),
            CREATED_IP               VARCHAR2(50),
            CREATED_DATE             TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY              VARCHAR2(50),
            MODIFIED_PC              VARCHAR2(100),
            MODIFIED_IP              VARCHAR2(50),
            MODIFIED_DATE            TIMESTAMP,
            CONSTRAINT FK_ACCT_FA_AST_CAT FOREIGN KEY (CATEGORY_ID) REFERENCES ACCT_FA_CATEGORY (CATEGORY_ID),
            CONSTRAINT FK_ACCT_FA_AST_GRP FOREIGN KEY (GROUP_ID) REFERENCES ACCT_FA_GROUP (GROUP_ID),
            CONSTRAINT UK_ACCT_FA_ASSET_CODE UNIQUE (IDDATA, ASSET_CODE),
            CONSTRAINT CK_ACCT_FA_METHOD CHECK (DEPR_METHOD IN (''SL'', ''DB'', ''NONE'')),
            CONSTRAINT CK_ACCT_FA_STATUS CHECK (STATUS IN (''DRAFT'', ''ACTIVE'', ''UNDER_CONSTRUCTION'', ''DISPOSED'', ''SOLD'', ''TRANSFERRED'', ''WRITTEN_OFF'', ''RETIRED'')),
            CONSTRAINT CK_ACCT_FA_DEL CHECK (IS_DELETED IN (''Y'', ''N''))
        )
    ');

    create_table_if_missing('ACCT_FA_TRX_HDR', '
        CREATE TABLE ACCT_FA_TRX_HDR
        (
            TRX_ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            DOC_NO              VARCHAR2(60) NOT NULL,
            TRX_TYPE            VARCHAR2(40) NOT NULL,
            DOC_DATE            DATE NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            PERIOD_KEY          NUMBER(6) NOT NULL,
            ASSET_ID            NUMBER,
            AMOUNT_BASE         NUMBER(18,2) DEFAULT 0 NOT NULL,
            OLD_AMOUNT_BASE     NUMBER(18,2),
            NEW_AMOUNT_BASE     NUMBER(18,2),
            CURRENCY_CODE       VARCHAR2(10) DEFAULT ''IDR'' NOT NULL,
            EXCHANGE_RATE       NUMBER(18,8) DEFAULT 1 NOT NULL,
            STATUS              VARCHAR2(20) DEFAULT ''DRAFT'' NOT NULL,
            SOURCE_REF_NO       VARCHAR2(80),
            REMARKS             VARCHAR2(1000),
            NOJURNAL            VARCHAR2(30),
            JURNALID            NUMBER,
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY         VARCHAR2(50),
            MODIFIED_DATE       TIMESTAMP,
            CONSTRAINT UK_ACCT_FA_TRX_DOC UNIQUE (IDDATA, DOC_NO),
            CONSTRAINT FK_ACCT_FA_TRX_AST FOREIGN KEY (ASSET_ID) REFERENCES ACCT_FA_ASSET (ASSET_ID),
            CONSTRAINT CK_ACCT_FA_TRX_STATUS CHECK (STATUS IN (''DRAFT'', ''SUBMITTED'', ''APPROVED'', ''REJECTED'', ''POSTED'', ''REVERSED''))
        )
    ');

    create_table_if_missing('ACCT_FA_APPROVAL_DTL', '
        CREATE TABLE ACCT_FA_APPROVAL_DTL
        (
            APPROVAL_DTL_ID     NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            TRX_ID              NUMBER NOT NULL,
            STEP_NO             NUMBER(5) NOT NULL,
            ROLE_CODE           VARCHAR2(30) NOT NULL,
            STATUS              VARCHAR2(20) NOT NULL,
            ACTION_BY           VARCHAR2(50),
            ACTION_DATE         TIMESTAMP,
            ACTION_COMMENT      VARCHAR2(500),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_APV_TRX FOREIGN KEY (TRX_ID) REFERENCES ACCT_FA_TRX_HDR (TRX_ID)
        )
    ');

    create_table_if_missing('ACCT_FA_DEPR_RUN', '
        CREATE TABLE ACCT_FA_DEPR_RUN
        (
            RUN_ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            PERIOD_KEY          NUMBER(6) NOT NULL,
            RUN_NO              VARCHAR2(60) NOT NULL,
            STATUS              VARCHAR2(20) NOT NULL,
            TOTAL_AMOUNT        NUMBER(18,2) DEFAULT 0 NOT NULL,
            NOJURNAL            VARCHAR2(30),
            JURNALID            NUMBER,
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            POSTED_BY           VARCHAR2(50),
            POSTED_DATE         TIMESTAMP,
            MODIFIED_BY         VARCHAR2(50),
            MODIFIED_DATE       TIMESTAMP,
            CONSTRAINT UK_ACCT_FA_RUN UNIQUE (IDDATA, RUN_NO),
            CONSTRAINT CK_ACCT_FA_RUN_STATUS CHECK (STATUS IN (''DRAFT'', ''POSTED'', ''REVERSED''))
        )
    ');

    create_table_if_missing('ACCT_FA_DEPR_RUN_DTL', '
        CREATE TABLE ACCT_FA_DEPR_RUN_DTL
        (
            RUN_DTL_ID          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            RUN_ID              NUMBER NOT NULL,
            BARIS               NUMBER(8) NOT NULL,
            ASSET_ID            NUMBER NOT NULL,
            ASSET_CODE          VARCHAR2(50) NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            OPENING_NBV         NUMBER(18,2) NOT NULL,
            DEPR_AMOUNT         NUMBER(18,2) NOT NULL,
            CLOSING_NBV         NUMBER(18,2) NOT NULL,
            RESIDUAL_VALUE      NUMBER(18,2) NOT NULL,
            DEPR_EXP_ACCT       VARCHAR2(20) NOT NULL,
            ACC_DEPR_ACCT       VARCHAR2(20) NOT NULL,
            DESCRIPTION         VARCHAR2(500),
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_RUN_DTL_RUN FOREIGN KEY (RUN_ID) REFERENCES ACCT_FA_DEPR_RUN (RUN_ID),
            CONSTRAINT FK_ACCT_FA_RUN_DTL_AST FOREIGN KEY (ASSET_ID) REFERENCES ACCT_FA_ASSET (ASSET_ID),
            CONSTRAINT UK_ACCT_FA_RUN_BARIS UNIQUE (RUN_ID, BARIS),
            CONSTRAINT UK_ACCT_FA_RUN_ASSET UNIQUE (RUN_ID, ASSET_ID)
        )
    ');

    create_table_if_missing('ACCT_FA_DEPR_HISTORY', '
        CREATE TABLE ACCT_FA_DEPR_HISTORY
        (
            HISTORY_ID          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            ASSET_ID            NUMBER NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            PERIOD_KEY          NUMBER(6) NOT NULL,
            RUN_ID              NUMBER NOT NULL,
            DEPR_AMOUNT         NUMBER(18,2) NOT NULL,
            OPENING_NBV         NUMBER(18,2) NOT NULL,
            CLOSING_NBV         NUMBER(18,2) NOT NULL,
            NOJURNAL            VARCHAR2(30),
            JURNALID            NUMBER,
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_HIST_AST FOREIGN KEY (ASSET_ID) REFERENCES ACCT_FA_ASSET (ASSET_ID),
            CONSTRAINT FK_ACCT_FA_HIST_RUN FOREIGN KEY (RUN_ID) REFERENCES ACCT_FA_DEPR_RUN (RUN_ID),
            CONSTRAINT UK_ACCT_FA_HIST UNIQUE (ASSET_ID, PERIOD)
        )
    ');

    create_table_if_missing('ACCT_FA_DOC_SEQ', '
        CREATE TABLE ACCT_FA_DOC_SEQ
        (
            DOC_SEQ_ID          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            DOC_TYPE            VARCHAR2(30) NOT NULL,
            YYYY                NUMBER(4) NOT NULL,
            MM                  NUMBER(2) NOT NULL,
            LAST_NO             NUMBER(10) DEFAULT 0 NOT NULL,
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_DATE       TIMESTAMP,
            CONSTRAINT UK_ACCT_FA_DOCSEQ UNIQUE (IDDATA, DOC_TYPE, YYYY, MM)
        )
    ');

    create_table_if_missing('ACCT_FA_ATTACHMENT', '
        CREATE TABLE ACCT_FA_ATTACHMENT
        (
            ATTACH_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            ASSET_ID            NUMBER,
            TRX_ID              NUMBER,
            FILE_NAME           VARCHAR2(255) NOT NULL,
            FILE_URI            VARCHAR2(1000) NOT NULL,
            MIME_TYPE           VARCHAR2(100),
            FILE_SIZE_BYTES     NUMBER(18),
            CHECKSUM_SHA256     VARCHAR2(100),
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_ATT_AST FOREIGN KEY (ASSET_ID) REFERENCES ACCT_FA_ASSET (ASSET_ID),
            CONSTRAINT FK_ACCT_FA_ATT_TRX FOREIGN KEY (TRX_ID) REFERENCES ACCT_FA_TRX_HDR (TRX_ID)
        )
    ');

    create_table_if_missing('ACCT_FA_AUDIT_LOG', '
        CREATE TABLE ACCT_FA_AUDIT_LOG
        (
            AUDIT_ID            NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            ENTITY_NAME         VARCHAR2(50) NOT NULL,
            ENTITY_ID           VARCHAR2(60) NOT NULL,
            ACTION_TYPE         VARCHAR2(30) NOT NULL,
            FIELD_NAME          VARCHAR2(100),
            OLD_VALUE           VARCHAR2(2000),
            NEW_VALUE           VARCHAR2(2000),
            DOC_NO              VARCHAR2(60),
            PERIOD              VARCHAR2(7),
            ACTION_BY           VARCHAR2(50) NOT NULL,
            ACTION_PC           VARCHAR2(100),
            ACTION_IP           VARCHAR2(50),
            ACTION_TS           TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL
        )
    ');

    create_table_if_missing('ACCT_FA_CIP_HDR', '
        CREATE TABLE ACCT_FA_CIP_HDR
        (
            CIP_ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            IDDATA              VARCHAR2(20) NOT NULL,
            CIP_CODE            VARCHAR2(40) NOT NULL,
            PROJECT_NAME        VARCHAR2(200) NOT NULL,
            START_DATE          DATE NOT NULL,
            TARGET_COMPLETE_DATE DATE,
            STATUS              VARCHAR2(20) DEFAULT ''OPEN'' NOT NULL,
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            MODIFIED_BY         VARCHAR2(50),
            MODIFIED_DATE       TIMESTAMP,
            CONSTRAINT UK_ACCT_FA_CIP UNIQUE (IDDATA, CIP_CODE)
        )
    ');

    create_table_if_missing('ACCT_FA_CIP_COST', '
        CREATE TABLE ACCT_FA_CIP_COST
        (
            CIP_COST_ID         NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            CIP_ID              NUMBER NOT NULL,
            IDDATA              VARCHAR2(20) NOT NULL,
            DOC_NO              VARCHAR2(60),
            COST_DATE           DATE NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            PERIOD_KEY          NUMBER(6) NOT NULL,
            ACCOUNT_CODE        VARCHAR2(20),
            AMOUNT_BASE         NUMBER(18,2) NOT NULL,
            SOURCE_REF_NO       VARCHAR2(80),
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_CIPCOST_HDR FOREIGN KEY (CIP_ID) REFERENCES ACCT_FA_CIP_HDR (CIP_ID)
        )
    ');

    create_table_if_missing('ACCT_FA_CIP_CAPITALIZATION', '
        CREATE TABLE ACCT_FA_CIP_CAPITALIZATION
        (
            CAP_ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
            CIP_ID              NUMBER NOT NULL,
            ASSET_ID            NUMBER NOT NULL,
            IDDATA              VARCHAR2(20) NOT NULL,
            DOC_NO              VARCHAR2(60) NOT NULL,
            CAPITALIZE_DATE     DATE NOT NULL,
            PERIOD              VARCHAR2(7) NOT NULL,
            PERIOD_KEY          NUMBER(6) NOT NULL,
            CAPITALIZED_AMOUNT  NUMBER(18,2) NOT NULL,
            CREATED_BY          VARCHAR2(50),
            CREATED_DATE        TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
            CONSTRAINT FK_ACCT_FA_CAP_CIP FOREIGN KEY (CIP_ID) REFERENCES ACCT_FA_CIP_HDR (CIP_ID),
            CONSTRAINT FK_ACCT_FA_CAP_AST FOREIGN KEY (ASSET_ID) REFERENCES ACCT_FA_ASSET (ASSET_ID)
        )
    ');

    create_index_if_missing('IDX_FA_AST_SCOPE', 'CREATE INDEX IDX_FA_AST_SCOPE ON ACCT_FA_ASSET (IDDATA, STATUS, CATEGORY_ID, DEPARTMENT_ID, COST_CENTER_ID)');
    create_index_if_missing('IDX_FA_AST_CODE', 'CREATE INDEX IDX_FA_AST_CODE ON ACCT_FA_ASSET (IDDATA, ASSET_CODE)');
    create_index_if_missing('IDX_FA_TRX_SCOPE', 'CREATE INDEX IDX_FA_TRX_SCOPE ON ACCT_FA_TRX_HDR (IDDATA, PERIOD_KEY, TRX_TYPE, STATUS)');
    create_index_if_missing('IDX_FA_APV_TRX', 'CREATE INDEX IDX_FA_APV_TRX ON ACCT_FA_APPROVAL_DTL (TRX_ID, STEP_NO)');
    create_index_if_missing('IDX_FA_RUN_SCOPE', 'CREATE INDEX IDX_FA_RUN_SCOPE ON ACCT_FA_DEPR_RUN (IDDATA, PERIOD_KEY, STATUS)');
    create_index_if_missing('IDX_FA_RUN_DTL_ASSET', 'CREATE INDEX IDX_FA_RUN_DTL_ASSET ON ACCT_FA_DEPR_RUN_DTL (ASSET_ID, PERIOD)');
    create_index_if_missing('IDX_FA_HIST_SCOPE', 'CREATE INDEX IDX_FA_HIST_SCOPE ON ACCT_FA_DEPR_HISTORY (IDDATA, PERIOD_KEY, ASSET_ID)');
    create_index_if_missing('IDX_FA_CIP_COST_SCOPE', 'CREATE INDEX IDX_FA_CIP_COST_SCOPE ON ACCT_FA_CIP_COST (IDDATA, PERIOD_KEY, CIP_ID)');
    create_index_if_missing('IDX_FA_AUDIT_SCOPE', 'CREATE INDEX IDX_FA_AUDIT_SCOPE ON ACCT_FA_AUDIT_LOG (IDDATA, DOC_NO, ACTION_TS)');

    -- Register module and minimum permissions when authorization tables are available.
    DECLARE
        v_module_table NUMBER := 0;
        v_perm_table   NUMBER := 0;
        v_module_id    NUMBER := 0;
        v_perm_count   NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_module_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_MODULES';
        SELECT COUNT(1) INTO v_perm_table FROM USER_TABLES WHERE TABLE_NAME = 'MASTER_PERMISSIONS';

        IF v_module_table > 0 THEN
            SELECT COUNT(1) INTO v_perm_count FROM MASTER_MODULES WHERE MODULE_NAME = 'FIXED_ASSET';
            IF v_perm_count = 0 THEN
                INSERT INTO MASTER_MODULES (MODULE_ID, MODULE_NAME)
                VALUES ((SELECT NVL(MAX(MODULE_ID), 0) + 1 FROM MASTER_MODULES), 'FIXED_ASSET');
                DBMS_OUTPUT.PUT_LINE('REGISTERED MODULE: FIXED_ASSET');
            END IF;
        END IF;

        IF v_module_table > 0 AND v_perm_table > 0 THEN
            SELECT MODULE_ID INTO v_module_id FROM MASTER_MODULES WHERE MODULE_NAME = 'FIXED_ASSET';

            SELECT COUNT(1) INTO v_perm_count
              FROM MASTER_PERMISSIONS
             WHERE MODULE_ID = v_module_id
               AND PERMISSION_NAME = 'FA_TRANSACTION_APPROVE';
            IF v_perm_count = 0 THEN
                INSERT INTO MASTER_PERMISSIONS
                    (PERMISSION_ID, MODULE_ID, PERMISSION_NAME, MENU, DESCRIPTION, URUT1, URUT2)
                VALUES
                    ((SELECT NVL(MAX(PERMISSION_ID), 0) + 1 FROM MASTER_PERMISSIONS), v_module_id,
                     'FA_TRANSACTION_APPROVE', 'Fixed Asset Approval', 'Approve/Reject revaluation, disposal, and write-off', 90, 1);
            END IF;
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED module permission registration: ' || SQLERRM);
    END;
END;
/
