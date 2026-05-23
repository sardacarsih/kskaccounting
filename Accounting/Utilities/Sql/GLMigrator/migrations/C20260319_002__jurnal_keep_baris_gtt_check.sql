-- Purpose: Verify GTT keep-baris migration.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
    v_errors NUMBER := 0;

    PROCEDURE fail(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[FAIL] ' || p_msg);
        v_errors := v_errors + 1;
    END;

    PROCEDURE ok(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[OK]   ' || p_msg);
    END;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== Keep BARIS GTT Migration Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_JURNAL_KEEP_BARIS_TMP';

    IF v_count = 0 THEN
        fail('Missing table ACCT_JURNAL_KEEP_BARIS_TMP');
    ELSE
        ok('Table ACCT_JURNAL_KEEP_BARIS_TMP exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_JURNAL_KEEP_BARIS_TMP'
       AND COLUMN_NAME = 'SESSION_TOKEN';

    IF v_count = 0 THEN
        fail('Missing column ACCT_JURNAL_KEEP_BARIS_TMP.SESSION_TOKEN');
    ELSE
        ok('Column SESSION_TOKEN exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_JURNAL_KEEP_BARIS_TMP'
       AND COLUMN_NAME = 'BARIS';

    IF v_count = 0 THEN
        fail('Missing column ACCT_JURNAL_KEEP_BARIS_TMP.BARIS');
    ELSE
        ok('Column BARIS exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_INDEXES
     WHERE INDEX_NAME = 'IDX_ACCT_JURNAL_KEEP_BARIS_TMP';

    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_JURNAL_KEEP_BARIS_TMP');
    ELSE
        ok('Index IDX_ACCT_JURNAL_KEEP_BARIS_TMP exists');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
