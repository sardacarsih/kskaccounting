-- Purpose: Verify audit trail migration was applied correctly.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
    v_errors NUMBER := 0;

    PROCEDURE check_column(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
        v_cnt NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_cnt
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table) AND COLUMN_NAME = UPPER(p_column);

        IF v_cnt = 0 THEN
            DBMS_OUTPUT.PUT_LINE('[FAIL] Missing column: ' || UPPER(p_table) || '.' || UPPER(p_column));
            v_errors := v_errors + 1;
        ELSE
            DBMS_OUTPUT.PUT_LINE('[OK]   ' || UPPER(p_table) || '.' || UPPER(p_column));
        END IF;
    END;

    PROCEDURE check_table(p_table IN VARCHAR2) IS
        v_cnt NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_cnt FROM USER_TABLES WHERE TABLE_NAME = UPPER(p_table);

        IF v_cnt = 0 THEN
            DBMS_OUTPUT.PUT_LINE('[FAIL] Missing table: ' || UPPER(p_table));
            v_errors := v_errors + 1;
        ELSE
            DBMS_OUTPUT.PUT_LINE('[OK]   Table ' || UPPER(p_table) || ' exists');
        END IF;
    END;

    PROCEDURE check_index(p_index IN VARCHAR2) IS
        v_cnt NUMBER := 0;
    BEGIN
        SELECT COUNT(1) INTO v_cnt FROM USER_INDEXES WHERE INDEX_NAME = UPPER(p_index);

        IF v_cnt = 0 THEN
            DBMS_OUTPUT.PUT_LINE('[FAIL] Missing index: ' || UPPER(p_index));
            v_errors := v_errors + 1;
        ELSE
            DBMS_OUTPUT.PUT_LINE('[OK]   Index ' || UPPER(p_index) || ' exists');
        END IF;
    END;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Audit Trail Migration Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    -- HDR columns
    check_column('ACCT_JURNAL_HDR', 'CREATED_DATE');
    check_column('ACCT_JURNAL_HDR', 'CREATED_BY');
    check_column('ACCT_JURNAL_HDR', 'MODIFIED_DATE');
    check_column('ACCT_JURNAL_HDR', 'MODIFIED_BY');
    check_column('ACCT_JURNAL_HDR', 'MODIFIED_PC');
    check_column('ACCT_JURNAL_HDR', 'MODIFIED_IP');

    -- DTL columns
    check_column('ACCT_JURNAL_DTL', 'CREATED_DATE');
    check_column('ACCT_JURNAL_DTL', 'MODIFIED_DATE');

    -- Tables
    check_table('ACCT_JURNAL_AUDIT');
    check_table('ACCT_JURNAL_AUDIT_DTL');

    -- Indexes
    check_index('IDX_AUDIT_JURNALID');
    check_index('IDX_AUDIT_ACTION_DATE');
    check_index('IDX_AUDIT_ACTION_BY');
    check_index('IDX_AUDIT_IDDATA');
    check_index('IDX_AUDIT_DTL_AUDIT_ID');

    -- FK check
    SELECT COUNT(1) INTO v_count
      FROM USER_CONSTRAINTS
     WHERE TABLE_NAME = 'ACCT_JURNAL_AUDIT_DTL'
       AND CONSTRAINT_TYPE = 'R'
       AND CONSTRAINT_NAME = 'FK_AUDIT_DTL_HDR';

    IF v_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('[FAIL] Missing FK: FK_AUDIT_DTL_HDR');
        v_errors := v_errors + 1;
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   FK FK_AUDIT_DTL_HDR exists');
    END IF;

    -- Backfill check
    SELECT COUNT(1) INTO v_count FROM ACCT_JURNAL_HDR WHERE CREATED_BY IS NULL;
    IF v_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[WARN] ' || v_count || ' rows in ACCT_JURNAL_HDR still have NULL CREATED_BY');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   All ACCT_JURNAL_HDR rows have CREATED_BY populated');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
