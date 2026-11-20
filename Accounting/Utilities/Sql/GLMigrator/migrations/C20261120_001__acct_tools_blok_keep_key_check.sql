-- Purpose: Verify ACCT_TOOLS is valid and AddorUpdateBlok no longer rewrites BLOKID.
-- Date: 2026-11-20
SET SERVEROUTPUT ON;

DECLARE
    v_count NUMBER;
    v_errors NUMBER := 0;
BEGIN
    SELECT COUNT(1) INTO v_count FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_TOOLS' AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY') AND STATUS <> 'VALID';
    IF v_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[FAIL] ACCT_TOOLS has invalid package/body');
        v_errors := v_errors + 1;
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   ACCT_TOOLS package and body are valid');
    END IF;

    SELECT COUNT(1) INTO v_count FROM USER_SOURCE
     WHERE NAME = 'ACCT_TOOLS' AND TYPE = 'PACKAGE BODY'
       AND REPLACE(LOWER(TEXT), ' ', '') LIKE '%blokid=sblockid%';
    IF v_count > 0 THEN
        DBMS_OUTPUT.PUT_LINE('[FAIL] AddorUpdateBlok still rewrites BLOKID');
        v_errors := v_errors + 1;
    ELSE
        DBMS_OUTPUT.PUT_LINE('[OK]   AddorUpdateBlok keeps BLOKID on UPDATE');
    END IF;

    IF v_errors > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'ACCT_TOOLS blok check failed');
    END IF;
END;
/
