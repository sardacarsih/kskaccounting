-- Purpose: Verify the MASTER_ESTATE compatibility contract required by Accounting.
-- Date: 2026-07-21

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE require_column(p_column IN VARCHAR2, p_data_type IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tab_columns
         WHERE table_name = 'MASTER_ESTATE'
           AND column_name = UPPER(p_column)
           AND data_type = UPPER(p_data_type);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20121, 'MASTER_ESTATE.' || UPPER(p_column) || ' is missing or has an incompatible type');
        END IF;
    END;
BEGIN
    SELECT COUNT(*) INTO l_count FROM user_tables WHERE table_name = 'MASTER_ESTATE';
    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20120, 'MASTER_ESTATE is missing');
    END IF;

    require_column('ID', 'NUMBER');
    require_column('ESTATEID', 'VARCHAR2');
    require_column('NAMA', 'VARCHAR2');
    require_column('IDDATA', 'VARCHAR2');

    SELECT COUNT(*) INTO l_count
      FROM user_constraints constraint_info
      JOIN user_cons_columns constraint_column
        ON constraint_column.constraint_name = constraint_info.constraint_name
       AND constraint_column.table_name = constraint_info.table_name
     WHERE constraint_info.table_name = 'MASTER_ESTATE'
       AND constraint_info.constraint_type = 'P'
       AND constraint_column.column_name = 'ID'
       AND constraint_column.position = 1
       AND NOT EXISTS (
           SELECT 1
             FROM user_cons_columns additional_column
            WHERE additional_column.constraint_name = constraint_info.constraint_name
              AND additional_column.table_name = constraint_info.table_name
              AND additional_column.position > 1
       );
    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20122, 'MASTER_ESTATE primary key on ID is missing');
    END IF;

    SELECT COUNT(*) INTO l_count
      FROM user_constraints uc
      JOIN user_cons_columns first_column
        ON first_column.constraint_name = uc.constraint_name
       AND first_column.table_name = uc.table_name
       AND first_column.position = 1
      JOIN user_cons_columns second_column
        ON second_column.constraint_name = uc.constraint_name
       AND second_column.table_name = uc.table_name
       AND second_column.position = 2
     WHERE uc.table_name = 'MASTER_ESTATE'
       AND uc.constraint_type = 'U'
       AND first_column.column_name = 'IDDATA'
       AND second_column.column_name = 'ESTATEID';
    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20123, 'MASTER_ESTATE unique company/estate constraint is missing');
    END IF;

    SELECT COUNT(*) INTO l_count
      FROM user_indexes
     WHERE index_name = 'IDX_MASTER_ESTATE_IDDATA'
       AND table_name = 'MASTER_ESTATE';
    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20124, 'IDX_MASTER_ESTATE_IDDATA is missing');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: MASTER_ESTATE compatibility contract is installed');
END;
/
