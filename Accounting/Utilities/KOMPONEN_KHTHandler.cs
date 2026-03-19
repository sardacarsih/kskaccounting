using Accounting._1.Interface;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Accounting.Utilities
{
    public class KOMPONEN_KHTHandler
    {
        /// <summary>
        /// jika table FIN_KOMPONEN_KHT 0 di r1 ambil ini
        /// </summary>
        /// <param name="db"></param>
        /// <param name="P_IDDATA"></param>
        /// <param name="P_PERIODE"></param>
        /// <param name="P_REMISE"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BKM_KOMPONEN_KHT>> HandleTrueConditionAsyncR1(IDbConnection db, string P_IDDATA, int P_PERIODE, int P_REMISE, string P_DIVISI)
        {
            // Define the query for the true condition
            string sql = @"WITH KomponenKHT_CTE AS (
                            SELECT 
                            K.IDDATA,
                            K.DIVISIID,
                            K.NIK,
                            K.NAMA,
                            K.ACBANK REKENING,
                            K.CBTK AS CATUBERAS,
                            C.NILAI AS NILAICATU,
                            0 TJGPERABOT,
                            (SELECT UPAH_HARIAN 
                                 FROM HRD_UMK 
                                 WHERE IDDATA = :P_IDDATA
                                   AND STATUS_SK = 'Y') AS UPAH_HARIAN
                        FROM 
                            HRD_KARYAWAN K
                            JOIN HRD_SK_CATUBERAS C ON C.KATEGORI_CATUBERAS = K.CBTK
                        WHERE 
                            K.DIVISIID = :P_DIVISI
                            AND K.STATUS_KARYAWAN = 2 
                            AND K.AKTIF = 'Y'
                        ),
                ALLHK_CTE AS (
                    SELECT M.IDDATA, D.NIK, SUM(D.QTY) AS JUMLAH
                    FROM AIS_BKMDETAIL D
                    JOIN AIS_BKMMASTER M ON M.MASTERID = D.MASTERID
                    WHERE D.SATUAN = 'HK' AND M.IDDATA = :P_IDDATA AND M.PERIODE = :P_PERIODE AND M.REMISE = :P_REMISE
                    GROUP BY M.IDDATA, D.NIK
                ),
                Lembur_CTE AS (
                    SELECT IDDATA, NIK, SUM(TOTAL) AS PREMIDANLEMBUR
                    FROM HRD_LEMBUR_TR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK
                ),
                LiburNasional_CTE AS (
                    SELECT PERIODE, REMISE, COUNT(TGLLIBUR) AS HKLIBURDIBAYAR
                    FROM HRD_LIBUR_DIBAYAR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY PERIODE, REMISE
                ),                    
                Kompensasi_CTE AS (
                    SELECT IDDATA, NIK, JENIS, SUM(JUMLAH) AS HARI
                    FROM HRD_IZIN
                    WHERE POTUMAKAN = 'Y' AND IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK, JENIS
                )
                SELECT 
                    KHT.*, COALESCE(HK.JUMLAH, 0) AS TOTALHK, COALESCE(LN.HKLIBURDIBAYAR, 0) AS HKLIBUR,
                    CASE WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS SAKIT,
                    CASE WHEN KO.JENIS = 'P1' OR KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS IZIN,
                    CASE WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS CUTI,
                    (COALESCE(HK.JUMLAH, 0) * KHT.NILAICATU) AS UMAKAN,
                    COALESCE(L.PREMIDANLEMBUR, 0) AS PREMIDANLEMBUR,
                   (
                        CASE 
                            WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END
                    ) * KHT.UPAH_HARIAN AS KOMPENSASI,
                    COALESCE(LN.HKLIBURDIBAYAR, 0) * KHT.UPAH_HARIAN AS LIBURDIBAYAR
                FROM KomponenKHT_CTE KHT
                LEFT JOIN ALLHK_CTE HK ON HK.NIK = KHT.NIK AND HK.IDDATA = KHT.IDDATA
                LEFT JOIN Lembur_CTE L ON L.NIK = KHT.NIK AND L.IDDATA = KHT.IDDATA
                LEFT JOIN LiburNasional_CTE LN ON LN.PERIODE = :P_PERIODE AND LN.REMISE = :P_REMISE                   
                LEFT JOIN Kompensasi_CTE KO ON KO.NIK = KHT.NIK AND KO.IDDATA = KHT.IDDATA";

            // Execute the query and return the results
            return await db.QueryAsync<BKM_KOMPONEN_KHT>(sql, new { P_DIVISI, P_IDDATA, P_PERIODE, P_REMISE });
        }
        /// <summary>
        /// jika table FIN_KOMPONEN_KHT 0 di r2 ambil ini
        /// </summary>
        /// <param name="db"></param>
        /// <param name="P_IDDATA"></param>
        /// <param name="P_PERIODE"></param>
        /// <param name="P_REMISE"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BKM_KOMPONEN_KHT>> HandleTrueConditionAsyncR2(IDbConnection db, string P_IDDATA, int P_PERIODE, int P_REMISE, string P_DIVISI)
        {
            // Define the query for the true condition
            string sql = @"WITH KomponenKHT_CTE AS (
                            SELECT 
                            K.IDDATA,
                            K.DIVISIID,
                            K.NIK,
                            K.NAMA,
                            K.ACBANK REKENING,
                            K.CBTK AS CATUBERAS,
                            C.NILAI AS NILAICATU,
                            K.TJGPERABOT,
                            (SELECT UPAH_HARIAN 
                                 FROM HRD_UMK 
                                 WHERE IDDATA = :P_IDDATA
                                   AND STATUS_SK = 'Y') AS UPAH_HARIAN
                        FROM 
                            HRD_KARYAWAN K
                            JOIN HRD_SK_CATUBERAS C ON C.KATEGORI_CATUBERAS = K.CBTK
                        WHERE 
                            K.DIVISIID = :P_DIVISI
                            AND K.STATUS_KARYAWAN = 2 
                            AND K.AKTIF = 'Y'
                        ),
                ALLHK_CTE AS (
                    SELECT M.IDDATA, D.NIK, SUM(D.QTY) AS JUMLAH
                    FROM AIS_BKMDETAIL D
                    JOIN AIS_BKMMASTER M ON M.MASTERID = D.MASTERID
                    WHERE D.SATUAN = 'HK' AND M.IDDATA = :P_IDDATA AND M.PERIODE = :P_PERIODE AND M.REMISE = :P_REMISE
                    GROUP BY M.IDDATA, D.NIK
                ),
                Lembur_CTE AS (
                    SELECT IDDATA, NIK, SUM(TOTAL) AS PREMIDANLEMBUR
                    FROM HRD_LEMBUR_TR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK
                ),
                LiburNasional_CTE AS (
                    SELECT PERIODE, REMISE, COUNT(TGLLIBUR) AS HKLIBURDIBAYAR
                    FROM HRD_LIBUR_DIBAYAR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY PERIODE, REMISE
                ),                    
                Kompensasi_CTE AS (
                    SELECT IDDATA, NIK, JENIS, SUM(JUMLAH) AS HARI
                    FROM HRD_IZIN
                    WHERE POTUMAKAN = 'Y' AND IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK, JENIS
                )
                SELECT 
                    KHT.*, COALESCE(HK.JUMLAH, 0) AS TOTALHK, COALESCE(LN.HKLIBURDIBAYAR, 0) AS HKLIBUR,
                    CASE WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS SAKIT,
                    CASE WHEN KO.JENIS = 'P1' OR KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS IZIN,
                    CASE WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS CUTI,
                    (COALESCE(HK.JUMLAH, 0) * KHT.NILAICATU) AS UMAKAN,
                    COALESCE(L.PREMIDANLEMBUR, 0) AS PREMIDANLEMBUR,
                   (
                        CASE 
                            WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END
                    ) * KHT.UPAH_HARIAN AS KOMPENSASI,
                    COALESCE(LN.HKLIBURDIBAYAR, 0) * KHT.UPAH_HARIAN AS LIBURDIBAYAR
                FROM KomponenKHT_CTE KHT
                LEFT JOIN ALLHK_CTE HK ON HK.NIK = KHT.NIK AND HK.IDDATA = KHT.IDDATA
                LEFT JOIN Lembur_CTE L ON L.NIK = KHT.NIK AND L.IDDATA = KHT.IDDATA
                LEFT JOIN LiburNasional_CTE LN ON LN.PERIODE = :P_PERIODE AND LN.REMISE = :P_REMISE                   
                LEFT JOIN Kompensasi_CTE KO ON KO.NIK = KHT.NIK AND KO.IDDATA = KHT.IDDATA";

            // Execute the query and return the results
            return await db.QueryAsync<BKM_KOMPONEN_KHT>(sql, new { P_DIVISI,P_IDDATA, P_PERIODE, P_REMISE });
        }
        /// <summary>
        /// jika table FIN_KOMPONEN_KHT sudah ada isnya ambil dari sini
        /// </summary>
        /// <param name="db"></param>
        /// <param name="P_IDDATA"></param>
        /// <param name="P_PERIODE"></param>
        /// <param name="P_REMISE"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BKM_KOMPONEN_KHT>> HandleFalseConditionAsync(IDbConnection db, string P_IDDATA, int P_PERIODE, int P_REMISE, string P_DIVISI)
        {
            // Define the query for the false condition
            string sql = @"WITH KomponenKHT_CTE AS (
                    SELECT 
                        IDDATA, DIVISIID, NIK, NAMA, REKENING, CATUBERAS, NILAICATU, TJGPERABOT, 
                        UPAH_HARIAN
                    FROM FIN_KOMPONEN_KHT 
                    WHERE DIVISIID=:P_DIVISI AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                ),
                ALLHK_CTE AS (
                    SELECT M.IDDATA, D.NIK, SUM(D.QTY) AS JUMLAH
                    FROM AIS_BKMDETAIL D
                    JOIN AIS_BKMMASTER M ON M.MASTERID = D.MASTERID
                    WHERE D.SATUAN = 'HK' AND M.IDDATA = :P_IDDATA AND M.PERIODE = :P_PERIODE AND M.REMISE = :P_REMISE
                    GROUP BY M.IDDATA, D.NIK
                ),
                Lembur_CTE AS (
                    SELECT IDDATA, NIK, SUM(TOTAL) AS PREMIDANLEMBUR
                    FROM HRD_LEMBUR_TR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK
                ),
                LiburNasional_CTE AS (
                    SELECT PERIODE, REMISE, COUNT(TGLLIBUR) AS HKLIBURDIBAYAR
                    FROM HRD_LIBUR_DIBAYAR 
                    WHERE IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY PERIODE, REMISE
                ),                    
                Kompensasi_CTE AS (
                    SELECT IDDATA, NIK, JENIS, SUM(JUMLAH) AS HARI
                    FROM HRD_IZIN
                    WHERE POTUMAKAN = 'Y' AND IDDATA = :P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE
                    GROUP BY IDDATA, NIK, JENIS
                )
                SELECT 
                    KHT.*, COALESCE(HK.JUMLAH, 0) AS TOTALHK, COALESCE(LN.HKLIBURDIBAYAR, 0) AS HKLIBUR,
                    CASE WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS SAKIT,
                    CASE WHEN KO.JENIS = 'P1' OR KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS IZIN,
                    CASE WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) ELSE 0 END AS CUTI,
                    (COALESCE(HK.JUMLAH, 0) * KHT.NILAICATU) AS UMAKAN,
                    COALESCE(L.PREMIDANLEMBUR, 0) AS PREMIDANLEMBUR,
                   (
                        CASE 
                            WHEN KO.JENIS = 'S' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'P2' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END + 
                        CASE 
                            WHEN KO.JENIS = 'C' THEN COALESCE(KO.HARI, 0) 
                            ELSE 0 
                        END
                    ) * KHT.UPAH_HARIAN AS KOMPENSASI,
                    COALESCE(LN.HKLIBURDIBAYAR, 0) * KHT.UPAH_HARIAN AS LIBURDIBAYAR
                FROM KomponenKHT_CTE KHT
                LEFT JOIN ALLHK_CTE HK ON HK.NIK = KHT.NIK AND HK.IDDATA = KHT.IDDATA
                LEFT JOIN Lembur_CTE L ON L.NIK = KHT.NIK AND L.IDDATA = KHT.IDDATA
                LEFT JOIN LiburNasional_CTE LN ON LN.PERIODE = :P_PERIODE AND LN.REMISE = :P_REMISE                   
                LEFT JOIN Kompensasi_CTE KO ON KO.NIK = KHT.NIK AND KO.IDDATA = KHT.IDDATA";

            // Execute the query and return the results
            return await db.QueryAsync<BKM_KOMPONEN_KHT>(sql, new { P_DIVISI,P_IDDATA, P_PERIODE, P_REMISE });
        }
    }

}
