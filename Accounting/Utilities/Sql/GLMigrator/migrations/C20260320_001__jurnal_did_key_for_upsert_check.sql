-- Purpose: Verify DID key support for jurnal upsert migration.
-- Date: 2026-03-20

SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
    v_errors NUMBER := 0;
    v_equivalent_index_name USER_INDEXES.INDEX_NAME%TYPE;

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
    DBMS_OUTPUT.PUT_LINE('=== Jurnal DID Key Migration Check ===');
    DBMS_OUTPUT.PUT_LINE('');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TAB_COLUMNS
     WHERE TABLE_NAME = 'ACCT_JURNAL_DTL_STAGE_TMP'
       AND COLUMN_NAME = 'DID';
    IF v_count = 0 THEN
        fail('Missing column ACCT_JURNAL_DTL_STAGE_TMP.DID');
    ELSE
        ok('Column ACCT_JURNAL_DTL_STAGE_TMP.DID exists');
    END IF;

    SELECT COUNT(1)
      INTO v_count
      FROM USER_INDEXES
     WHERE INDEX_NAME = 'IDX_ACCT_JD_STAGE_TMP_DID';
    IF v_count = 0 THEN
        fail('Missing index IDX_ACCT_JD_STAGE_TMP_DID');
    ELSE
        ok('Index IDX_ACCT_JD_STAGE_TMP_DID exists');
    END IF;

    BEGIN
        SELECT index_name
          INTO v_equivalent_index_name
          FROM (
                SELECT ui.index_name
                  FROM USER_INDEXES ui
                 WHERE ui.TABLE_NAME = 'ACCT_JURNAL_DTL'
                   AND (
                        SELECT COUNT(1)
                          FROM USER_IND_COLUMNS uic
                         WHERE uic.INDEX_NAME = ui.INDEX_NAME
                           AND uic.TABLE_NAME = ui.TABLE_NAME
                       ) = 1
                   AND EXISTS (
                        SELECT 1
                          FROM USER_IND_COLUMNS uic
                         WHERE uic.INDEX_NAME = ui.INDEX_NAME
                           AND uic.TABLE_NAME = ui.TABLE_NAME
                           AND uic.COLUMN_POSITION = 1
                           AND uic.COLUMN_NAME = 'DID'
                       )
               )
         WHERE ROWNUM = 1;

        ok('Equivalent single-column DID index exists: ' || v_equivalent_index_name);
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            fail('Missing equivalent single-column index on ACCT_JURNAL_DTL(DID)');
    END;

    DBMS_OUTPUT.PUT_LINE('');
    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
