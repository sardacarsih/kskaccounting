-- Purpose: Rollback marker for jurnal import schema compatibility migration.
-- Date: 2026-05-28

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('NO-OP: compatibility columns are shared by legacy and current jurnal import flows.');
END;
/
