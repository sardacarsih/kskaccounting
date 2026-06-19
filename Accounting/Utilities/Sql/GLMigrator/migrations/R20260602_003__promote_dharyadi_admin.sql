-- Purpose: Rollback marker for dharyadi ADMIN promotion.
-- Date: 2026-06-02
-- Intentionally keeps the ADMIN assignment to avoid destructive access changes.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: dharyadi ADMIN assignment is retained to avoid destructive access changes.');
END;
/
