-- Purpose: Rollback marker for HRD_PAYROLL_NONSTAFF compatibility migration.
-- Date: 2026-07-21
-- Intentionally retains the HRD staging table because it can contain payroll data.

SET SERVEROUTPUT ON;

BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK SKIPPED: HRD_PAYROLL_NONSTAFF and its data are retained for application compatibility.');
END;
/
