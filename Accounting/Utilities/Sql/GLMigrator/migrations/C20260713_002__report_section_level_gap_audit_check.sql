-- Purpose: Verify the report section level audit can run and the report engine remains valid.
-- INACTIVE REFERENCE ONLY: the corresponding diagnostic migration is intentionally excluded from
-- migrations.manifest.json. Use FrmSettingRL > Validate Section for company/year-scoped validation.
DECLARE
    v_invalid_packages INTEGER := 0;
BEGIN
    SELECT COUNT(*)
      INTO v_invalid_packages
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_REPORT_ENGINE_V1'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF v_invalid_packages > 0 THEN
        RAISE_APPLICATION_ERROR(-20962, 'ACCT_REPORT_ENGINE_V1 is INVALID after report section level audit migration.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: report engine is valid; use Validate Section in the client to inspect company/year-specific gaps.');
END;
/
