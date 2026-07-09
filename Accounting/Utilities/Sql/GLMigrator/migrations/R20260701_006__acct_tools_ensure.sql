-- Purpose: NO-OP rollback. ACCT_TOOLS is a shared legacy package; this migration only (re)deploys the
--   canonical spec + body. There is no safe automatic prior version to restore per environment, so rolling
--   back is a no-op. Restore from a database backup if a specific prior ACCT_TOOLS is required.
-- Date: 2026-07-01

BEGIN
    DBMS_OUTPUT.PUT_LINE('NO-OP rollback: ACCT_TOOLS ensure only (re)deploys the canonical package');
END;
/
