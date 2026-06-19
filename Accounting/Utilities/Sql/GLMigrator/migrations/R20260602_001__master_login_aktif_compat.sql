-- Purpose: Rollback marker for MASTER_LOGIN.AKTIF compatibility migration.
-- Date: 2026-06-02
-- Intentionally keeps MASTER_LOGIN.AKTIF because application login and user management depend on it.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: MASTER_LOGIN.AKTIF is retained for application compatibility.');
END;
/
