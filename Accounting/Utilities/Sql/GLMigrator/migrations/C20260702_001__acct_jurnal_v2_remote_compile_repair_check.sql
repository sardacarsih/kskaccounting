SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
    l_error_count INTEGER;
    l_messages VARCHAR2(4000);

    PROCEDURE require_column(p_table IN VARCHAR2, p_column IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO l_count
          FROM USER_TAB_COLUMNS
         WHERE TABLE_NAME = UPPER(p_table)
           AND COLUMN_NAME = UPPER(p_column);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, UPPER(p_table) || '.' || UPPER(p_column) || ' is missing');
        END IF;
    END;

    PROCEDURE require_object(p_name IN VARCHAR2, p_type IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(1)
          INTO l_count
          FROM USER_OBJECTS
         WHERE OBJECT_NAME = UPPER(p_name)
           AND OBJECT_TYPE = UPPER(p_type);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, UPPER(p_name) || ' ' || UPPER(p_type) || ' is missing');
        END IF;
    END;
BEGIN
    require_column('ACCT_JURNAL_TMP', 'SUMBER');
    require_column('ACCT_JURNAL_TMP', 'DID');
    require_object('ACCT_JURNAL_V2', 'PACKAGE');
    require_object('ACCT_JURNAL_V2', 'PACKAGE BODY');

    SELECT COUNT(1)
      INTO l_error_count
      FROM USER_OBJECTS
     WHERE OBJECT_NAME = 'ACCT_JURNAL_V2'
       AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
       AND STATUS <> 'VALID';

    IF l_error_count > 0 THEN
        FOR err IN (
            SELECT NAME, TYPE, LINE, POSITION, TEXT
              FROM USER_ERRORS
             WHERE NAME = 'ACCT_JURNAL_V2'
             ORDER BY SEQUENCE
        ) LOOP
            DBMS_OUTPUT.PUT_LINE(err.NAME || ' ' || err.TYPE || ' line ' || err.LINE || ':' || err.POSITION || ' - ' || err.TEXT);
            l_messages := SUBSTR(l_messages || err.TYPE || ' line ' || err.LINE || ':' || err.POSITION || ' - ' || err.TEXT || '; ', 1, 3500);
        END LOOP;

        RAISE_APPLICATION_ERROR(-20001, 'ACCT_JURNAL_V2 package is invalid: ' || l_messages);
    END IF;
END;
/
