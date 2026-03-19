-- Purpose: Enable Oracle Text strategy for NOJURNAL and KETERANGAN search at scale.
-- Date: 2026-03-18
-- Notes:
-- 1) Run as schema owner of ACCT_JURNAL_DTL.
-- 2) Oracle Text component must be installed and valid.
-- 3) Safe to re-run (idempotent by object name).

SET SERVEROUTPUT ON;

DECLARE
    v_ctx_valid NUMBER := 0;
BEGIN
    BEGIN
        SELECT COUNT(1)
          INTO v_ctx_valid
          FROM DBA_REGISTRY
         WHERE COMP_ID = 'CONTEXT'
           AND STATUS = 'VALID';

        IF v_ctx_valid = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Oracle Text component is not VALID in DBA_REGISTRY.');
        END IF;
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('WARN: Unable to validate DBA_REGISTRY (continuing): ' || SQLERRM);
    END;
END;
/

DECLARE
    PROCEDURE create_preference_if_missing(p_preference_name IN VARCHAR2, p_preference_type IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM CTX_USER_PREFERENCES
         WHERE PRE_NAME = UPPER(p_preference_name);

        IF v_count = 0 THEN
            CTX_DDL.CREATE_PREFERENCE(p_preference_name, p_preference_type);
            DBMS_OUTPUT.PUT_LINE('CREATED PREFERENCE: ' || UPPER(p_preference_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('PREFERENCE EXISTS: ' || UPPER(p_preference_name));
        END IF;
    END;

    PROCEDURE set_preference_attribute(p_preference_name IN VARCHAR2, p_attribute_name IN VARCHAR2, p_attribute_value IN VARCHAR2) IS
    BEGIN
        CTX_DDL.SET_ATTRIBUTE(p_preference_name, p_attribute_name, p_attribute_value);
        DBMS_OUTPUT.PUT_LINE('SET ATTRIBUTE: ' || UPPER(p_preference_name) || '.' || UPPER(p_attribute_name) || '=' || p_attribute_value);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('WARN: Failed to set attribute ' || UPPER(p_preference_name) || '.' || UPPER(p_attribute_name) || ': ' || SQLERRM);
    END;

    PROCEDURE create_context_index_if_missing(p_index_name IN VARCHAR2, p_column_name IN VARCHAR2) IS
        v_count NUMBER := 0;
        v_sql CLOB;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count = 0 THEN
            v_sql :=
                'CREATE INDEX ' || UPPER(p_index_name) ||
                ' ON ACCT_JURNAL_DTL(' || p_column_name || ')' ||
                ' INDEXTYPE IS CTXSYS.CONTEXT ' ||
                ' PARAMETERS(''LEXER JURNAL_TEXT_LEXER WORDLIST JURNAL_TEXT_WORDLIST SYNC (ON COMMIT)'')';
            EXECUTE IMMEDIATE v_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED INDEX: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('INDEX EXISTS: ' || UPPER(p_index_name));
        END IF;
    END;
BEGIN
    create_preference_if_missing('JURNAL_TEXT_LEXER', 'BASIC_LEXER');
    set_preference_attribute('JURNAL_TEXT_LEXER', 'BASE_LETTER', 'YES');
    set_preference_attribute('JURNAL_TEXT_LEXER', 'MIXED_CASE', 'NO');
    set_preference_attribute('JURNAL_TEXT_LEXER', 'PRINTJOINS', '._-/');

    create_preference_if_missing('JURNAL_TEXT_WORDLIST', 'BASIC_WORDLIST');
    set_preference_attribute('JURNAL_TEXT_WORDLIST', 'PREFIX_INDEX', 'TRUE');
    set_preference_attribute('JURNAL_TEXT_WORDLIST', 'PREFIX_MIN_LENGTH', '2');
    set_preference_attribute('JURNAL_TEXT_WORDLIST', 'PREFIX_MAX_LENGTH', '20');
    set_preference_attribute('JURNAL_TEXT_WORDLIST', 'SUBSTRING_INDEX', 'TRUE');
    set_preference_attribute('JURNAL_TEXT_WORDLIST', 'WILDCARD_MAXTERMS', '10000');

    create_context_index_if_missing('IDX_CTX_ACCT_JD_NOJURNAL', 'NOJURNAL');
    create_context_index_if_missing('IDX_CTX_ACCT_JD_KETERANGAN', 'KETERANGAN');
END;
/

SELECT INDEX_NAME, INDEX_TYPE, STATUS
  FROM USER_INDEXES
 WHERE INDEX_NAME IN ('IDX_CTX_ACCT_JD_NOJURNAL', 'IDX_CTX_ACCT_JD_KETERANGAN')
 ORDER BY INDEX_NAME;

SELECT IDX_NAME, IDX_STATUS, IDX_SYNC_TYPE
  FROM CTXSYS.CTX_USER_INDEXES
 WHERE IDX_NAME IN ('IDX_CTX_ACCT_JD_NOJURNAL', 'IDX_CTX_ACCT_JD_KETERANGAN')
 ORDER BY IDX_NAME;
