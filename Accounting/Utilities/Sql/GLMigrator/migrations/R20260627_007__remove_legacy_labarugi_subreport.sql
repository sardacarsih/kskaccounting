-- Purpose: Rollback placeholder for removing legacy Laba Rugi sub-report dependency.
-- Keeping the non-legacy ACCT_LAPORAN_V2 body is intentional.

BEGIN
    DBMS_OUTPUT.PUT_LINE('NO-OP: keeping non-legacy ACCT_LAPORAN_V2 body');
END;
/