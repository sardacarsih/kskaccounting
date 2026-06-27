-- Purpose: Check compile status of ACCT_LAPORAN_V2 and existence of Neraca metadata.
-- Date: 2026-06-28

SET SERVEROUTPUT ON;

DECLARE
    v_status VARCHAR2(100);
    v_count  NUMBER;
BEGIN
    -- 1. Check package validity
    SELECT STATUS INTO v_status FROM USER_OBJECTS WHERE OBJECT_NAME = 'ACCT_LAPORAN_V2' AND OBJECT_TYPE = 'PACKAGE BODY';
    IF v_status <> 'VALID' THEN
        RAISE_APPLICATION_ERROR(-20091, 'ACCT_LAPORAN_V2 is invalid.');
    END IF;

    -- 2. Check metadata
    SELECT COUNT(*) INTO v_count FROM ACCT_REPORT_DEF WHERE REPORT_CODE = 'NERACA';
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20092, 'NERACA report definition is missing.');
    END IF;

    SELECT COUNT(*) INTO v_count FROM ACCT_REPORT_SECTION WHERE REPORT_CODE = 'NERACA';
    IF v_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20093, 'NERACA report sections are missing.');
    END IF;

    DBMS_OUTPUT.PUT_LINE('CHECK SUCCESSFUL: ACCT_LAPORAN_V2 is valid and Neraca metadata is registered.');
END;
/
