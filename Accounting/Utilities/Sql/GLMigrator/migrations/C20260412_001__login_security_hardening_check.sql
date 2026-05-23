-- Purpose: Verify login security hardening schema objects exist.
-- Date: 2026-04-12

SET SERVEROUTPUT ON;

DECLARE
    v_errors NUMBER := 0;
    v_count NUMBER := 0;

    PROCEDURE fail(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[FAIL] ' || p_msg);
        v_errors := v_errors + 1;
    END;

    PROCEDURE ok(p_msg IN VARCHAR2) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('[OK]   ' || p_msg);
    END;

    PROCEDURE expect_column(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF v_count = 1 THEN
            ok(UPPER(p_table) || '.' || UPPER(p_column) || ' exists');
        ELSE
            fail(UPPER(p_table) || '.' || UPPER(p_column) || ' missing');
        END IF;
    END;
BEGIN
    expect_column('MASTER_LOGIN', 'FAILED_LOGIN_COUNT');
    expect_column('MASTER_LOGIN', 'LOCKOUT_UNTIL_UTC');
    expect_column('MASTER_LOGIN', 'LAST_LOGIN_AT_UTC');
    expect_column('MASTER_LOGIN', 'LAST_FAILED_LOGIN_AT_UTC');
    expect_column('MASTER_LOGIN', 'PASSWORD_CHANGED_AT_UTC');
    expect_column('MASTER_LOGIN', 'PASSWORD_RESET_REQUIRED');

    SELECT COUNT(1)
      INTO v_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_LOGIN_AUDIT';

    IF v_count = 1 THEN
        ok('ACCT_LOGIN_AUDIT exists');
    ELSE
        fail('ACCT_LOGIN_AUDIT missing');
    END IF;

    IF v_errors > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[RESULT] ' || v_errors || ' check(s) FAILED');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[RESULT] All checks PASSED');
    END IF;
END;
/
