-- Purpose: Rollback marker for MASTER_ESTATE compatibility migration.
-- Date: 2026-07-21
-- Intentionally retains the master table because it can contain business and RBAC data.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: MASTER_ESTATE and its data are retained for application compatibility.');
END;
/
