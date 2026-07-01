-- Purpose: Roll back server-side V2 year closing orchestrator.
-- Date: 2026-07-01

BEGIN
    EXECUTE IMMEDIATE 'DROP PACKAGE ACCT_CLOSING_YEAR_V2';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4043 THEN
            RAISE;
        END IF;
END;
/
