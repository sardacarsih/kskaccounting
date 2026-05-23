-- Rollback: Drop Fixed Asset core objects introduced by V20260319_008__fixed_asset_core.sql

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_table_if_exists(p_table IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TABLES
         WHERE TABLE_NAME = UPPER(p_table);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP TABLE ' || UPPER(p_table) || ' CASCADE CONSTRAINTS PURGE';
            DBMS_OUTPUT.PUT_LINE('DROPPED TABLE: ' || UPPER(p_table));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED TABLE (not found): ' || UPPER(p_table));
        END IF;
    END;
BEGIN
    drop_table_if_exists('ACCT_FA_CIP_CAPITALIZATION');
    drop_table_if_exists('ACCT_FA_CIP_COST');
    drop_table_if_exists('ACCT_FA_CIP_HDR');
    drop_table_if_exists('ACCT_FA_AUDIT_LOG');
    drop_table_if_exists('ACCT_FA_ATTACHMENT');
    drop_table_if_exists('ACCT_FA_DOC_SEQ');
    drop_table_if_exists('ACCT_FA_DEPR_HISTORY');
    drop_table_if_exists('ACCT_FA_DEPR_RUN_DTL');
    drop_table_if_exists('ACCT_FA_DEPR_RUN');
    drop_table_if_exists('ACCT_FA_APPROVAL_DTL');
    drop_table_if_exists('ACCT_FA_TRX_HDR');
    drop_table_if_exists('ACCT_FA_ASSET');
    drop_table_if_exists('ACCT_FA_ACCOUNT_MAP');
    drop_table_if_exists('ACCT_FA_GROUP');
    drop_table_if_exists('ACCT_FA_CATEGORY');
END;
/
