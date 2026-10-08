-- Purpose: Backfill MASTER_ESTATE.ID on legacy tables before 20260721_001_master_estate_compat
--          adds PRIMARY KEY (ID). Without it, a MASTER_ESTATE that already holds rows fails with
--          ORA-01449 (column contains NULL values) because the ID column was just added empty.
-- Date: 2026-10-08
-- Idempotent: does nothing when the table is missing (created later by 20260721_001), when no ID is NULL,
--             and on databases where 20260721_001 is already applied (ID is the primary key, never NULL).

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;
    l_nulls INTEGER;
    l_dups  INTEGER;
    l_max   NUMBER;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tables
     WHERE table_name = 'MASTER_ESTATE';

    IF l_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('SKIPPED: MASTER_ESTATE does not exist yet (created by 20260721_001_master_estate_compat)');
        RETURN;
    END IF;

    SELECT COUNT(*)
      INTO l_count
      FROM user_tab_columns
     WHERE table_name = 'MASTER_ESTATE'
       AND column_name = 'ID';

    IF l_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE MASTER_ESTATE ADD (ID NUMBER(10))';
        DBMS_OUTPUT.PUT_LINE('ADDED MASTER_ESTATE.ID');
    END IF;

    EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM MASTER_ESTATE WHERE ID IS NULL' INTO l_nulls;
    IF l_nulls = 0 THEN
        DBMS_OUTPUT.PUT_LINE('SKIPPED: MASTER_ESTATE.ID has no NULL values');
        RETURN;
    END IF;

    -- A PRIMARY KEY can never be added over duplicates; fail with a clear message instead of ORA-02437.
    EXECUTE IMMEDIATE
        'SELECT COUNT(*) FROM (SELECT ID FROM MASTER_ESTATE WHERE ID IS NOT NULL GROUP BY ID HAVING COUNT(*) > 1)'
        INTO l_dups;
    IF l_dups > 0 THEN
        RAISE_APPLICATION_ERROR(-20130,
            'MASTER_ESTATE.ID contains duplicate values; resolve them manually before PRIMARY KEY (ID) can be added');
    END IF;

    EXECUTE IMMEDIATE 'SELECT NVL(MAX(ID), 0) FROM MASTER_ESTATE' INTO l_max;

    EXECUTE IMMEDIATE
        'MERGE INTO MASTER_ESTATE target ' ||
        'USING (SELECT ROWID AS rid, :base_id + ROW_NUMBER() OVER (ORDER BY ROWID) AS new_id ' ||
        '         FROM MASTER_ESTATE WHERE ID IS NULL) source ' ||
        'ON (target.ROWID = source.rid) ' ||
        'WHEN MATCHED THEN UPDATE SET target.ID = source.new_id'
        USING l_max;

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('BACKFILLED MASTER_ESTATE.ID for ' || l_nulls || ' row(s), starting after ' || l_max);
END;
/
