-- Purpose: Verify MASTER_ESTATE has no NULL ID values (required for PRIMARY KEY (ID)).
-- Date: 2026-10-08

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
    l_nulls INTEGER;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tab_columns
     WHERE table_name = 'MASTER_ESTATE'
       AND column_name = 'ID';

    -- Table or column absent: 20260721_001_master_estate_compat creates it with a NOT NULL ID.
    IF l_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('OK: MASTER_ESTATE.ID not present yet, nothing to backfill');
        RETURN;
    END IF;

    EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM MASTER_ESTATE WHERE ID IS NULL' INTO l_nulls;
    IF l_nulls > 0 THEN
        RAISE_APPLICATION_ERROR(-20131, 'MASTER_ESTATE.ID still has ' || l_nulls || ' NULL value(s)');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: MASTER_ESTATE.ID has no NULL values');
END;
/
