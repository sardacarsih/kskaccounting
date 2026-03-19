-- ============================================================================
-- Rollback: Remove Audit Trail Support
-- Date: 2026-03-19
-- ============================================================================

-- Drop audit tables (detail first due to FK)
DROP TABLE ACCT_JURNAL_AUDIT_DTL;
DROP TABLE ACCT_JURNAL_AUDIT;

-- Remove audit columns from ACCT_JURNAL_HDR
ALTER TABLE ACCT_JURNAL_HDR DROP (CREATED_DATE, CREATED_BY, MODIFIED_DATE, MODIFIED_BY, MODIFIED_PC, MODIFIED_IP);

-- Remove audit columns from ACCT_JURNAL_DTL
ALTER TABLE ACCT_JURNAL_DTL DROP (CREATED_DATE, MODIFIED_DATE);
