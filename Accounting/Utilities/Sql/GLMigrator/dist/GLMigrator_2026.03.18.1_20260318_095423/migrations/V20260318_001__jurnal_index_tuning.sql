-- Purpose: Add Oracle indexes to accelerate jurnal loading/search paths used by FrmJurnal.
-- Date: 2026-03-18
-- Notes:
-- 1) Run as schema owner of ACCT_JURNAL_HDR / ACCT_JURNAL_DTL.
-- 2) Safe to re-run (index creation is idempotent by index name).

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE create_index_if_missing(p_index_name IN VARCHAR2, p_sql IN CLOB) IS
        v_count NUMBER := 0;
    BEGIN
        SELECT COUNT(1)
          INTO v_count
          FROM USER_INDEXES
         WHERE INDEX_NAME = UPPER(p_index_name);

        IF v_count = 0 THEN
            EXECUTE IMMEDIATE p_sql;
            DBMS_OUTPUT.PUT_LINE('CREATED: ' || UPPER(p_index_name));
        ELSE
            DBMS_OUTPUT.PUT_LINE('SKIPPED (exists): ' || UPPER(p_index_name));
        END IF;
    END;
BEGIN
    -- Supports: GetJurnalHeader_Dapper (IDDATA + PERIODE, order by NOJURNAL)
    create_index_if_missing(
        'IDX_ACCT_JH_ID_PRD_NJ',
        'CREATE INDEX IDX_ACCT_JH_ID_PRD_NJ ON ACCT_JURNAL_HDR (IDDATA, PERIODE, NOJURNAL)'
    );

    -- Supports: SearchJurnal_Bulan and period exports (IDDATA + PERIODE, order by NOJURNAL/BARIS)
    create_index_if_missing(
        'IDX_ACCT_JD_ID_PRD_NJ_BR',
        'CREATE INDEX IDX_ACCT_JD_ID_PRD_NJ_BR ON ACCT_JURNAL_DTL (IDDATA, PERIODE, NOJURNAL, BARIS)'
    );

    -- Supports: SearchJurnal year/month range (IDDATA + GLYEAR/GLMONTH) with sorted journal rows
    create_index_if_missing(
        'IDX_ACCT_JD_ID_GY_GM_NJ_BR',
        'CREATE INDEX IDX_ACCT_JD_ID_GY_GM_NJ_BR ON ACCT_JURNAL_DTL (IDDATA, GLYEAR, GLMONTH, NOJURNAL, BARIS)'
    );

    -- Supports: Exact date filter in monthly view
    create_index_if_missing(
        'IDX_ACCT_JD_ID_PRD_TGL',
        'CREATE INDEX IDX_ACCT_JD_ID_PRD_TGL ON ACCT_JURNAL_DTL (IDDATA, PERIODE, TANGGAL)'
    );

    -- Supports: Prefix KODE filter (KODE LIKE :kode || ''%'')
    create_index_if_missing(
        'IDX_ACCT_JD_ID_PRD_KODE',
        'CREATE INDEX IDX_ACCT_JD_ID_PRD_KODE ON ACCT_JURNAL_DTL (IDDATA, PERIODE, KODE)'
    );

    BEGIN
        DBMS_STATS.GATHER_TABLE_STATS(OWNNAME => USER, TABNAME => 'ACCT_JURNAL_HDR', CASCADE => TRUE);
        DBMS_STATS.GATHER_TABLE_STATS(OWNNAME => USER, TABNAME => 'ACCT_JURNAL_DTL', CASCADE => TRUE);
        DBMS_OUTPUT.PUT_LINE('STATS refreshed for ACCT_JURNAL_HDR and ACCT_JURNAL_DTL');
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('STATS refresh skipped: ' || SQLERRM);
    END;
END;
/

-- Optional note:
-- NOJURNAL/KETERANGAN queries using "%keyword%" patterns are not b-tree friendly.
-- If full contains-search must be fast for large data, evaluate Oracle Text (CONTEXT index).
