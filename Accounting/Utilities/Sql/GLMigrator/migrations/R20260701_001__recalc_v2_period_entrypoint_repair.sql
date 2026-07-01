-- Purpose: Rollback placeholder for ReCalcPeriod repair migration.
-- Date: 2026-07-01

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('NO-OP rollback: ReCalcPeriod repair only reapplies ACCT_RECALLCULATIONS_V2 package body.');
END;
/
