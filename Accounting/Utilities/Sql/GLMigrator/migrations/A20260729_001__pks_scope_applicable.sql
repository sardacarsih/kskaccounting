-- Purpose: Applicability gate for the PKS-only report migrations (20260729_001 / 20260729_002).
--          Prints APPLICABLE only when this database holds PKS scoped report mappings, or when an
--          interrupted 20260729_001 run left its backup rows behind (resume case).
--          Plantation databases without PKS data print NOT_APPLICABLE so up/verify skip the migration
--          instead of failing the PKS baseline check (ORA-20866).
-- Date: 2026-10-08
-- Read-only: only SELECTs; safe to run any number of times.

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    c_backup_migration_id CONSTANT VARCHAR2(128) := '20260729_001_pks_global_report_settings';
    v_table_count  NUMBER := 0;
    v_scoped_count NUMBER := 0;
    v_backup_count NUMBER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_table_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_REPORT_SECTION_ACCOUNT';

    IF v_table_count = 1 THEN
        EXECUTE IMMEDIATE q'[
            SELECT COUNT(*)
              FROM ACCT_REPORT_SECTION_ACCOUNT
             WHERE UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'
               AND TRIM(IDDATA) IS NOT NULL]'
          INTO v_scoped_count;
    END IF;

    SELECT COUNT(*)
      INTO v_table_count
      FROM USER_TABLES
     WHERE TABLE_NAME = 'ACCT_RPT_PKS_SCOPE_BAK';

    IF v_table_count = 1 THEN
        EXECUTE IMMEDIATE q'[
            SELECT COUNT(*)
              FROM ACCT_RPT_PKS_SCOPE_BAK
             WHERE MIGRATION_ID = :migration_id]'
          INTO v_backup_count
         USING c_backup_migration_id;
    END IF;

    IF v_scoped_count > 0 OR v_backup_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('APPLICABLE');
    ELSE
        DBMS_OUTPUT.PUT_LINE('NOT_APPLICABLE: database has no PKS scoped report mappings');
    END IF;
END;
/
