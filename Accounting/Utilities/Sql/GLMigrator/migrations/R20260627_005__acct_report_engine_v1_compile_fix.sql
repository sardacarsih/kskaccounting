-- Purpose: Rollback placeholder for ACCT_REPORT_ENGINE_V1 package compile fix.
-- The previous package body was invalid in Oracle because SQL cursors could not call
-- a private PL/SQL function. This rollback intentionally leaves the fixed package in place.

BEGIN
    DBMS_OUTPUT.PUT_LINE('NO-OP: keeping valid ACCT_REPORT_ENGINE_V1 package');
END;
/