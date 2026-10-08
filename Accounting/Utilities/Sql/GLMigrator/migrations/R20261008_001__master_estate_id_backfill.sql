-- Purpose: Rollback marker for the MASTER_ESTATE.ID backfill.
-- Date: 2026-10-08
-- Intentionally keeps the generated IDs: they are referenced by RBAC data (MASTER_USER_ROLES_EST.ESTATE_ID)
-- and reverting them to NULL would break the primary key added by 20260721_001_master_estate_compat.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: MASTER_ESTATE.ID values are retained for application compatibility.');
END;
/
