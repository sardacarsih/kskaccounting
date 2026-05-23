-- Purpose:
--   Prevent ORA-01795 on jurnal update when BARIS list exceeds 1000 items.
--   Used by JurnalRepository.UpdateJurnalMasterDetail via GTT + NOT EXISTS.

BEGIN
    EXECUTE IMMEDIATE '
        CREATE GLOBAL TEMPORARY TABLE ACCT_JURNAL_KEEP_BARIS_TMP
        (
            SESSION_TOKEN VARCHAR2(64) NOT NULL,
            BARIS         NUMBER(10)   NOT NULL
        )
        ON COMMIT DELETE ROWS';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE '
        CREATE INDEX IDX_ACCT_JURNAL_KEEP_BARIS_TMP
            ON ACCT_JURNAL_KEEP_BARIS_TMP (SESSION_TOKEN, BARIS)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
            RAISE;
        END IF;
END;
/
