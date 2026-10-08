-- Purpose: Verify the canonical HRD_PAYROLL_NONSTAFF staging contract.
-- Date: 2026-07-21

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE require_column(
        p_column IN VARCHAR2,
        p_data_type IN VARCHAR2,
        p_data_length IN INTEGER DEFAULT NULL,
        p_nullable IN VARCHAR2 DEFAULT NULL) IS
    BEGIN
        SELECT COUNT(*) INTO l_count
          FROM user_tab_columns
         WHERE table_name = 'HRD_PAYROLL_NONSTAFF'
           AND column_name = p_column
           AND data_type = p_data_type
           AND (p_data_length IS NULL OR data_length = p_data_length)
           AND (p_nullable IS NULL OR nullable = p_nullable);

        IF l_count = 0 THEN
            RAISE_APPLICATION_ERROR(-20131, 'HRD_PAYROLL_NONSTAFF.' || p_column || ' is missing or incompatible');
        END IF;
    END;
BEGIN
    SELECT COUNT(*) INTO l_count FROM user_tables WHERE table_name = 'HRD_PAYROLL_NONSTAFF';
    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20132, 'HRD_PAYROLL_NONSTAFF is missing');
    END IF;

    require_column('PERIODE', 'NUMBER', NULL, 'N');
    require_column('REMISE', 'NUMBER', NULL, 'N');
    require_column('NOABSEN', 'VARCHAR2', 50, 'N');
    require_column('NIK', 'VARCHAR2', 50, 'N');
    require_column('NAMA', 'VARCHAR2', 100);
    require_column('DIVISIID', 'VARCHAR2', 20);
    require_column('DIVISI', 'VARCHAR2', 100);
    require_column('IDJAB', 'VARCHAR2', 20);
    require_column('JAB', 'VARCHAR2', 100);
    require_column('GOL', 'VARCHAR2', 5);
    require_column('IDBAG', 'VARCHAR2', 20);
    require_column('BAG', 'VARCHAR2', 100);
    require_column('KERJAID', 'VARCHAR2', 20);
    require_column('KERJA', 'VARCHAR2', 100);
    require_column('ACKODE', 'VARCHAR2', 100);
    require_column('KATEGORI_TK', 'VARCHAR2', 5);
    require_column('ESTATE', 'VARCHAR2', 3);
    require_column('ACBANK', 'NUMBER');
    require_column('HARIKERJANORMAL', 'NUMBER');
    require_column('HKKOREKSI', 'NUMBER');
    require_column('TOTAL_HK', 'NUMBER');
    require_column('LIBURDIBAYAR', 'NUMBER');
    require_column('UMAKAN', 'NUMBER');
    require_column('TJGPERABOT', 'NUMBER');
    require_column('JAMLEMBUR', 'NUMBER');
    require_column('LEMBUR', 'NUMBER');
    require_column('PREMI', 'NUMBER');
    require_column('BRUTO', 'NUMBER');
    require_column('POTONGAN', 'NUMBER');
    require_column('NETTO', 'NUMBER');
    require_column('KANTOR', 'NUMBER');
    require_column('KOPERASI', 'NUMBER');
    require_column('LAINNYA', 'NUMBER');
    require_column('POT_BPJS_TK', 'NUMBER');
    require_column('POT_BPJS_KESEHATAN', 'NUMBER');
    require_column('POT_BPJS_TK_PENSIUN', 'NUMBER');
    require_column('IDPT', 'VARCHAR2', 10);
    require_column('IDDATA', 'VARCHAR2', 20);
    require_column('PENDAPATAN_HK', 'NUMBER');
    require_column('JUMLAH_HKNORMAL', 'NUMBER');
    require_column('JUMLAH_LBAYAR', 'NUMBER');

    SELECT COUNT(*) INTO l_count
      FROM user_indexes index_info
      JOIN user_ind_columns first_column
        ON first_column.index_name = index_info.index_name
       AND first_column.table_name = index_info.table_name
       AND first_column.column_position = 1
      JOIN user_ind_columns second_column
        ON second_column.index_name = index_info.index_name
       AND second_column.table_name = index_info.table_name
       AND second_column.column_position = 2
      JOIN user_ind_columns third_column
        ON third_column.index_name = index_info.index_name
       AND third_column.table_name = index_info.table_name
       AND third_column.column_position = 3
     WHERE index_info.table_name = 'HRD_PAYROLL_NONSTAFF'
       AND first_column.column_name = 'IDDATA'
       AND second_column.column_name = 'PERIODE'
       AND third_column.column_name = 'REMISE';

    IF l_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20133, 'HRD_PAYROLL_NONSTAFF scope index is missing');
    END IF;

    DBMS_OUTPUT.PUT_LINE('OK: HRD_PAYROLL_NONSTAFF canonical compatibility contract is installed');
END;
/
