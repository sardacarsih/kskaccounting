-- Check script for V20260319_008__fixed_asset_core.sql

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE check_table(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'TABLE MISSING: ' || UPPER(p_table));
        END IF;
        DBMS_OUTPUT.PUT_LINE('OK TABLE: ' || UPPER(p_table));
    END;

    PROCEDURE check_index(p_index IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index);

        IF v_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'INDEX MISSING: ' || UPPER(p_index));
        END IF;
        DBMS_OUTPUT.PUT_LINE('OK INDEX: ' || UPPER(p_index));
    END;
BEGIN
    check_table('ACCT_FA_CATEGORY');
    check_table('ACCT_FA_GROUP');
    check_table('ACCT_FA_ACCOUNT_MAP');
    check_table('ACCT_FA_ASSET');
    check_table('ACCT_FA_TRX_HDR');
    check_table('ACCT_FA_APPROVAL_DTL');
    check_table('ACCT_FA_DEPR_RUN');
    check_table('ACCT_FA_DEPR_RUN_DTL');
    check_table('ACCT_FA_DEPR_HISTORY');
    check_table('ACCT_FA_DOC_SEQ');
    check_table('ACCT_FA_ATTACHMENT');
    check_table('ACCT_FA_AUDIT_LOG');
    check_table('ACCT_FA_CIP_HDR');
    check_table('ACCT_FA_CIP_COST');
    check_table('ACCT_FA_CIP_CAPITALIZATION');

    check_index('IDX_FA_AST_SCOPE');
    check_index('IDX_FA_TRX_SCOPE');
    check_index('IDX_FA_RUN_SCOPE');
    check_index('IDX_FA_HIST_SCOPE');
END;
/
