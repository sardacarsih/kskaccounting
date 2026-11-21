-- Purpose: Remove only the ACCT_DEFAULT AIS_KOMPONEN keys inserted by migration 20261121_001
--          (keys added or edited by users keep their own KETERANGAN and are left alone).
-- Date: 2026-11-21

SET SERVEROUTPUT ON;
SET DEFINE OFF;

DECLARE
    v_deleted NUMBER := 0;
BEGIN
    DELETE FROM ACCT_DEFAULT
     WHERE NAMA LIKE 'AIS\_KOMPONEN\_%' ESCAPE '\'
       AND KETERANGAN = 'Migrasi 20261121_001: dari riwayat jurnal AIS';

    v_deleted := SQL%ROWCOUNT;
    COMMIT;

    DBMS_OUTPUT.PUT_LINE('AIS_KOMPONEN keys removed from ACCT_DEFAULT: ' || v_deleted);
END;
/
