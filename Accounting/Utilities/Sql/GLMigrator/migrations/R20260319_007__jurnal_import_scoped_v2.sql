-- Purpose: Rollback scoped import package.
-- Date: 2026-03-19

SET SERVEROUTPUT ON;

BEGIN
    BEGIN
        EXECUTE IMMEDIATE 'DROP PACKAGE ACCT_JURNAL_IMPORT_V2';
        DBMS_OUTPUT.PUT_LINE('DROPPED PACKAGE ACCT_JURNAL_IMPORT_V2');
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE = -4043 THEN
                DBMS_OUTPUT.PUT_LINE('SKIPPED (not found): ACCT_JURNAL_IMPORT_V2');
            ELSE
                RAISE;
            END IF;
    END;
END;
/
