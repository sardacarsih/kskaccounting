-- Purpose: Rollback marker for RBAC user assignment compatibility migration.
-- Date: 2026-06-02
-- Intentionally keeps RBAC assignment tables and migrated access rows for application compatibility.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: RBAC assignment tables are retained for application compatibility.');
END;
/
