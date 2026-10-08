-- Purpose: Ensure MASTER_ESTATE exists with the minimum contract required by Accounting.
-- Date: 2026-07-21

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE add_column_if_missing(p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_tab_columns
         WHERE table_name = 'MASTER_ESTATE'
           AND column_name = UPPER(p_column);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE MASTER_ESTATE ADD (' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED MASTER_ESTATE.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): MASTER_ESTATE.' || UPPER(p_column));
        END IF;
    END;

    PROCEDURE create_index_if_missing(p_index IN VARCHAR2, p_sql IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*)
          INTO l_count
          FROM user_indexes
         WHERE index_name = UPPER(p_index);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED INDEX: ' || UPPER(p_index));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_index));
        END IF;
    END;
BEGIN
    SELECT COUNT(*)
      INTO l_count
      FROM user_tables
     WHERE table_name = 'MASTER_ESTATE';

    IF l_count = 0 THEN
        EXECUTE IMMEDIATE '
            CREATE TABLE MASTER_ESTATE
            (
                ID       NUMBER(10) NOT NULL,
                ESTATEID VARCHAR2(20) NOT NULL,
                NAMA     VARCHAR2(100) NOT NULL,
                IDDATA   VARCHAR2(20) NOT NULL,
                CONSTRAINT PK_MASTER_ESTATE PRIMARY KEY (ID),
                CONSTRAINT UK_MASTER_ESTATE_COMPANY UNIQUE (IDDATA, ESTATEID)
            )';
        DBMS_OUTPUT.PUT_LINE('CREATED TABLE: MASTER_ESTATE');
    ELSE
        add_column_if_missing('ID', 'ID NUMBER(10)');
        add_column_if_missing('ESTATEID', 'ESTATEID VARCHAR2(20)');
        add_column_if_missing('NAMA', 'NAMA VARCHAR2(100)');
        add_column_if_missing('IDDATA', 'IDDATA VARCHAR2(20)');

        SELECT COUNT(*)
          INTO l_count
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
            EXECUTE IMMEDIATE 'ALTER TABLE MASTER_ESTATE ADD CONSTRAINT PK_MASTER_ESTATE PRIMARY KEY (ID)';
            DBMS_OUTPUT.PUT_LINE('CREATED CONSTRAINT: PK_MASTER_ESTATE');
        END IF;

        SELECT COUNT(*)
          INTO l_count
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
            EXECUTE IMMEDIATE 'ALTER TABLE MASTER_ESTATE ADD CONSTRAINT UK_MASTER_ESTATE_COMPANY UNIQUE (IDDATA, ESTATEID)';
            DBMS_OUTPUT.PUT_LINE('CREATED CONSTRAINT: UK_MASTER_ESTATE_COMPANY');
        END IF;
    END IF;

    create_index_if_missing(
        'IDX_MASTER_ESTATE_IDDATA',
        'CREATE INDEX IDX_MASTER_ESTATE_IDDATA ON MASTER_ESTATE (IDDATA)');
END;
/
