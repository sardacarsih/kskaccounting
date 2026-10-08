-- Purpose: Ensure HRD_PAYROLL_NONSTAFF matches the canonical HRD staging contract.
-- Date: 2026-07-21

SET SERVEROUTPUT ON;

DECLARE
    l_count INTEGER;

    PROCEDURE add_column_if_missing(p_column IN VARCHAR2, p_definition IN VARCHAR2) IS
    BEGIN
        SELECT COUNT(*) INTO l_count
          FROM user_tab_columns
         WHERE table_name = 'HRD_PAYROLL_NONSTAFF'
           AND column_name = UPPER(p_column);

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE HRD_PAYROLL_NONSTAFF ADD (' || p_definition || ')';
            DBMS_OUTPUT.PUT_LINE('ADDED HRD_PAYROLL_NONSTAFF.' || UPPER(p_column));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): HRD_PAYROLL_NONSTAFF.' || UPPER(p_column));
        END IF;
    END;

    PROCEDURE require_not_null(p_column IN VARCHAR2) IS
    BEGIN
        EXECUTE IMMEDIATE 'SELECT COUNT(*) FROM HRD_PAYROLL_NONSTAFF WHERE ' || p_column || ' IS NULL'
            INTO l_count;

        IF l_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20130, 'HRD_PAYROLL_NONSTAFF.' || p_column || ' contains NULL values; correct the data before applying NOT NULL');
        END IF;

        SELECT COUNT(*) INTO l_count
          FROM user_tab_columns
         WHERE table_name = 'HRD_PAYROLL_NONSTAFF'
           AND column_name = p_column
           AND nullable = 'N';

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE HRD_PAYROLL_NONSTAFF MODIFY (' || p_column || ' NOT NULL)';
            DBMS_OUTPUT.PUT_LINE('SET NOT NULL: HRD_PAYROLL_NONSTAFF.' || p_column);
        END IF;
    END;

    PROCEDURE create_index_if_missing IS
    BEGIN
        SELECT COUNT(*) INTO l_count
          FROM user_indexes
         WHERE index_name = 'IDX_HRD_PAYROLL_NS_SCOPE';

        IF l_count = 0 THEN
            EXECUTE IMMEDIATE 'CREATE INDEX IDX_HRD_PAYROLL_NS_SCOPE ON HRD_PAYROLL_NONSTAFF (IDDATA, PERIODE, REMISE)';
            DBMS_OUTPUT.PUT_LINE('CREATED INDEX: IDX_HRD_PAYROLL_NS_SCOPE');
        END IF;
    END;
BEGIN
    SELECT COUNT(*) INTO l_count
      FROM user_tables
     WHERE table_name = 'HRD_PAYROLL_NONSTAFF';

    IF l_count = 0 THEN
        EXECUTE IMMEDIATE '
            CREATE TABLE HRD_PAYROLL_NONSTAFF
            (
                PERIODE               NUMBER NOT NULL,
                REMISE                NUMBER NOT NULL,
                NOABSEN               VARCHAR2(50) NOT NULL,
                NIK                   VARCHAR2(50) NOT NULL,
                NAMA                  VARCHAR2(100),
                DIVISIID              VARCHAR2(20),
                DIVISI                VARCHAR2(100),
                IDJAB                 VARCHAR2(20),
                JAB                   VARCHAR2(100),
                GOL                   VARCHAR2(5),
                IDBAG                 VARCHAR2(20),
                BAG                   VARCHAR2(100),
                KERJAID               VARCHAR2(20),
                KERJA                 VARCHAR2(100),
                ACKODE                VARCHAR2(100),
                KATEGORI_TK           VARCHAR2(5),
                ESTATE                VARCHAR2(3),
                ACBANK                NUMBER,
                HARIKERJANORMAL       NUMBER,
                HKKOREKSI             NUMBER,
                TOTAL_HK              NUMBER,
                LIBURDIBAYAR          NUMBER,
                UMAKAN                NUMBER,
                TJGPERABOT            NUMBER,
                JAMLEMBUR             NUMBER,
                LEMBUR                NUMBER,
                PREMI                 NUMBER,
                BRUTO                 NUMBER,
                POTONGAN              NUMBER,
                NETTO                 NUMBER,
                KANTOR                NUMBER,
                KOPERASI              NUMBER,
                LAINNYA               NUMBER,
                POT_BPJS_TK           NUMBER,
                POT_BPJS_KESEHATAN    NUMBER,
                POT_BPJS_TK_PENSIUN   NUMBER,
                IDPT                  VARCHAR2(10),
                IDDATA                VARCHAR2(20),
                PENDAPATAN_HK         NUMBER,
                JUMLAH_HKNORMAL       NUMBER,
                JUMLAH_LBAYAR         NUMBER
            )';
        DBMS_OUTPUT.PUT_LINE('CREATED TABLE: HRD_PAYROLL_NONSTAFF');
    ELSE
        add_column_if_missing('PERIODE', 'PERIODE NUMBER');
        add_column_if_missing('REMISE', 'REMISE NUMBER');
        add_column_if_missing('NOABSEN', 'NOABSEN VARCHAR2(50)');
        add_column_if_missing('NIK', 'NIK VARCHAR2(50)');
        add_column_if_missing('NAMA', 'NAMA VARCHAR2(100)');
        add_column_if_missing('DIVISIID', 'DIVISIID VARCHAR2(20)');
        add_column_if_missing('DIVISI', 'DIVISI VARCHAR2(100)');
        add_column_if_missing('IDJAB', 'IDJAB VARCHAR2(20)');
        add_column_if_missing('JAB', 'JAB VARCHAR2(100)');
        add_column_if_missing('GOL', 'GOL VARCHAR2(5)');
        add_column_if_missing('IDBAG', 'IDBAG VARCHAR2(20)');
        add_column_if_missing('BAG', 'BAG VARCHAR2(100)');
        add_column_if_missing('KERJAID', 'KERJAID VARCHAR2(20)');
        add_column_if_missing('KERJA', 'KERJA VARCHAR2(100)');
        add_column_if_missing('ACKODE', 'ACKODE VARCHAR2(100)');
        add_column_if_missing('KATEGORI_TK', 'KATEGORI_TK VARCHAR2(5)');
        add_column_if_missing('ESTATE', 'ESTATE VARCHAR2(3)');
        add_column_if_missing('ACBANK', 'ACBANK NUMBER');
        add_column_if_missing('HARIKERJANORMAL', 'HARIKERJANORMAL NUMBER');
        add_column_if_missing('HKKOREKSI', 'HKKOREKSI NUMBER');
        add_column_if_missing('TOTAL_HK', 'TOTAL_HK NUMBER');
        add_column_if_missing('LIBURDIBAYAR', 'LIBURDIBAYAR NUMBER');
        add_column_if_missing('UMAKAN', 'UMAKAN NUMBER');
        add_column_if_missing('TJGPERABOT', 'TJGPERABOT NUMBER');
        add_column_if_missing('JAMLEMBUR', 'JAMLEMBUR NUMBER');
        add_column_if_missing('LEMBUR', 'LEMBUR NUMBER');
        add_column_if_missing('PREMI', 'PREMI NUMBER');
        add_column_if_missing('BRUTO', 'BRUTO NUMBER');
        add_column_if_missing('POTONGAN', 'POTONGAN NUMBER');
        add_column_if_missing('NETTO', 'NETTO NUMBER');
        add_column_if_missing('KANTOR', 'KANTOR NUMBER');
        add_column_if_missing('KOPERASI', 'KOPERASI NUMBER');
        add_column_if_missing('LAINNYA', 'LAINNYA NUMBER');
        add_column_if_missing('POT_BPJS_TK', 'POT_BPJS_TK NUMBER');
        add_column_if_missing('POT_BPJS_KESEHATAN', 'POT_BPJS_KESEHATAN NUMBER');
        add_column_if_missing('POT_BPJS_TK_PENSIUN', 'POT_BPJS_TK_PENSIUN NUMBER');
        add_column_if_missing('IDPT', 'IDPT VARCHAR2(10)');
        add_column_if_missing('IDDATA', 'IDDATA VARCHAR2(20)');
        add_column_if_missing('PENDAPATAN_HK', 'PENDAPATAN_HK NUMBER');
        add_column_if_missing('JUMLAH_HKNORMAL', 'JUMLAH_HKNORMAL NUMBER');
        add_column_if_missing('JUMLAH_LBAYAR', 'JUMLAH_LBAYAR NUMBER');

        require_not_null('PERIODE');
        require_not_null('REMISE');
        require_not_null('NOABSEN');
        require_not_null('NIK');
    END IF;

    create_index_if_missing;
END;
/
