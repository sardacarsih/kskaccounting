-- Purpose: Rollback Oracle Text strategy for jurnal keyword search.
-- Date: 2026-03-18

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE drop_index_if_exists(p_index_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count > 0 THEN
            EXECUTE IMMEDIATE 'DROP INDEX ' || UPPER(p_index_name);
            DBMS_OUTPUT.PUT_LINE('DROPPED INDEX: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('INDEX NOT FOUND: ' || UPPER(p_index_name));
        END IF;
    END;

    PROCEDURE drop_preference_if_exists(p_preference_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM CTX_USER_PREFERENCES
         WHERE PRE_NAME = UPPER(p_preference_name);

        IF v_count > 0 THEN
            BEGIN
                CTX_DDL.DROP_PREFERENCE(p_preference_name);
                DBMS_OUTPUT.PUT_LINE('DROPPED PREFERENCE: ' || UPPER(p_preference_name));
            EXCEPTION
                WHEN OTHERS THEN
                    DBMS_OUTPUT.PUT_LINE('SKIPPED PREFERENCE ' || UPPER(p_preference_name) || ': ' || SQLERRM);
            END;
        ELSE
            DBMS_OUTPUT.PUT_LINE('PREFERENCE NOT FOUND: ' || UPPER(p_preference_name));
        END IF;
    END;
BEGIN
    drop_index_if_exists('IDX_CTX_ACCT_JD_NOJURNAL');
    drop_index_if_exists('IDX_CTX_ACCT_JD_KETERANGAN');

    drop_preference_if_exists('JURNAL_TEXT_WORDLIST');
    drop_preference_if_exists('JURNAL_TEXT_LEXER');
END;
/

SELECT INDEX_NAME, INDEX_TYPE, STATUS
  FROM USER_INDEXES
 WHERE INDEX_NAME IN ('IDX_CTX_ACCT_JD_NOJURNAL', 'IDX_CTX_ACCT_JD_KETERANGAN')
 ORDER BY INDEX_NAME;

SELECT PRE_NAME, PRE_CLASS
  FROM CTXSYS.CTX_USER_PREFERENCES
 WHERE PRE_NAME IN ('JURNAL_TEXT_LEXER', 'JURNAL_TEXT_WORDLIST')
 ORDER BY PRE_NAME;
