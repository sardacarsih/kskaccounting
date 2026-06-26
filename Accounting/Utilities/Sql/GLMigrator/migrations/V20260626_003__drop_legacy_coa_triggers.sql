-- Purpose: Drop the legacy COA mutation triggers on ACCT_JURNAL_DTL. COA balances
--          are now maintained exclusively by the idempotent async recompute
--          (ACCT_RECALLCULATIONS_V2). The triggers were already disabled by
--          V20260320_003; this removes them completely.
-- Date: 2026-06-26

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_trigger_if_present(p_trigger IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_TRIGGERS
         WHERE TRIGGER_NAME = UPPER(p_trigger);

        IF v_count = 0 THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED (missing): ' || UPPER(p_trigger));
        ELSE
            EXECUTE IMMEDIATE 'DROP TRIGGER ' || UPPER(p_trigger);
            DBMS_OUTPUT.PUT_LINE('DROPPED TRIGGER: ' || UPPER(p_trigger));
        END IF;
    END;
BEGIN
    drop_trigger_if_present('UPDATE_COA_FROM_UPDATE');
    drop_trigger_if_present('UPDATE_COA_FROM_DELETE');
END;
/
