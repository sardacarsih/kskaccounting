-- Purpose: Verify jurnal update index migration.
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
    DBMS_OUTPUT.PUT_LINE('=== Jurnal Update Index Migration Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_INDEXES
     WHERE INDEX_NAME = 'IDX_ACCT_JD_REFFID_BARIS';

    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_JD_REFFID_BARIS');
    ELSE
        ok('Index IDX_ACCT_JD_REFFID_BARIS exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_IND_COLUMNS
     WHERE INDEX_NAME = 'IDX_ACCT_JD_REFFID_BARIS'
       AND COLUMN_NAME = 'REFFID';

    IF v_count = 0 THEN
        fail('IDX_ACCT_JD_REFFID_BARIS missing column REFFID');
    ELSE
        ok('IDX_ACCT_JD_REFFID_BARIS has column REFFID');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_IND_COLUMNS
     WHERE INDEX_NAME = 'IDX_ACCT_JD_REFFID_BARIS'
       AND COLUMN_NAME = 'BARIS';

    IF v_count = 0 THEN
        fail('IDX_ACCT_JD_REFFID_BARIS missing column BARIS');
    ELSE
        ok('IDX_ACCT_JD_REFFID_BARIS has column BARIS');
    END IF;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
