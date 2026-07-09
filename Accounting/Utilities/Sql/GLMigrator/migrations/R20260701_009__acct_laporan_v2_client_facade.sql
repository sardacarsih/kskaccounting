-- Rollback: restore ACCT_LAPORAN_V2 to the pre-BukuBesar facade by reapplying the Neraca V2 migration manually if needed.
-- This rollback intentionally does not alter legacy ACCT_LAPORAN.
SET SERVEROUTPUT ON;
BEGIN
    DBMS_OUTPUT.PUT_LINE('ROLLBACK NOTE: ACCT_LAPORAN_V2 facade extension is non-destructive. Reapply V20260628_002 to remove LAP_BUKUBESAR_V2.');
END;
/