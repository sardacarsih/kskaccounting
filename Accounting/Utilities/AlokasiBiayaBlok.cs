using Accounting.Models;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounting.Utilities
{
    public class AlokasiBiayaBlok
    {
        public static async Task<IEnumerable<JurnalBlok>> GetJournalDetailsAsync(int dari, int sampai, string idData)
        {
            var useSpecialQuery = idData == "FSLKEBUN";

            var sql = useSpecialQuery
                ? @"
            SELECT J.IDDATA, D.ESTATEID AS ESTATE, D.DIVISI, B.KODE AS KODEBLOK, B.BLOK,
                   K.KATEGORI, G.GROUPID, J.PERIODE, J.NOJURNAL, J.TANGGAL, J.KODE,
                   J.REKENING, J.DEBET, J.KREDIT, J.KETERANGAN
            FROM acct_jurnal_dtl J
            JOIN MASTER_BLOK B ON B.KODE = SUBSTR(J.kode, 6, 3) AND B.IDDATA = J.IDDATA
            JOIN MASTER_DIVISI D ON D.DIVISIID = B.DIVISIID AND D.ESTATEID = 'MBE'
            LEFT JOIN AGRO_GROUPS G ON G.GROUPID = SUBSTR(J.kode, 1, 3) || SUBSTR(J.kode, 10, 3)
            LEFT JOIN AGRO_KATEGORI K ON K.KATEGORIID = G.kategoriid
            WHERE TO_DATE(J.GLYEAR || J.GLMONTH, 'YYYYMM') 
                  BETWEEN TO_DATE(:DARI, 'YYYYMM') AND TO_DATE(:SAMPAI, 'YYYYMM')                  
              AND SUBSTR(J.kode, 1, 2) IN ('20', '80', '81')
              AND J.IDDATA = :IDDATA
              AND SUBSTR(J.KODE, 4, 2) = '21'

            UNION ALL

            SELECT J.IDDATA, D.ESTATEID AS ESTATE, D.DIVISI, B.KODE AS KODEBLOK, B.BLOK,
                   K.KATEGORI, G.GROUPID, J.PERIODE, J.NOJURNAL, J.TANGGAL, J.KODE,
                   J.REKENING, J.DEBET, J.KREDIT, J.KETERANGAN
            FROM acct_jurnal_dtl J
            JOIN MASTER_BLOK B ON B.KODE = SUBSTR(J.kode, 6, 3) AND B.IDDATA = J.IDDATA
            JOIN MASTER_DIVISI D ON D.DIVISIID = B.DIVISIID AND D.ESTATEID = 'GLE'
            LEFT JOIN AGRO_GROUPS G ON G.GROUPID = SUBSTR(J.kode, 1, 3) || SUBSTR(J.kode, 10, 3)
            LEFT JOIN AGRO_KATEGORI K ON K.KATEGORIID = G.kategoriid
            WHERE TO_DATE(J.GLYEAR || J.GLMONTH, 'YYYYMM') 
                  BETWEEN TO_DATE(:DARI, 'YYYYMM') AND TO_DATE(:SAMPAI, 'YYYYMM')                  
              AND SUBSTR(J.kode, 1, 2) IN ('20', '80', '81')
              AND J.IDDATA = :IDDATA
              AND SUBSTR(J.KODE, 4, 2) = '22'"
                : @"
            SELECT J.IDDATA, D.ESTATEID AS ESTATE, D.DIVISI, B.KODE AS KODEBLOK, B.BLOK,
                   K.KATEGORI, G.GROUPID, J.PERIODE, J.NOJURNAL, J.TANGGAL, J.KODE,
                   J.REKENING, J.DEBET, J.KREDIT, J.KETERANGAN
            FROM acct_jurnal_dtl J
            JOIN MASTER_BLOK B ON B.KODE = SUBSTR(J.kode, 6, 3) AND B.IDDATA = J.IDDATA
            JOIN MASTER_DIVISI D ON D.DIVISIID = B.DIVISIID
            LEFT JOIN AGRO_GROUPS G ON G.GROUPID = SUBSTR(J.kode, 1, 3) || SUBSTR(J.kode, 10, 3)
            LEFT JOIN AGRO_KATEGORI K ON K.KATEGORIID = G.kategoriid
            WHERE TO_DATE(J.GLYEAR || J.GLMONTH, 'YYYYMM') 
                  BETWEEN TO_DATE(:DARI, 'YYYYMM') AND TO_DATE(:SAMPAI, 'YYYYMM')                  
              AND SUBSTR(J.kode, 1, 2) IN ('20', '80', '81')
              AND J.IDDATA = :IDDATA";

            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            var result = await connection.QueryAsync<JurnalBlok>(sql, new
            {
                DARI = dari.ToString(),
                SAMPAI = sampai.ToString(),
                IDDATA = idData
            });

            return result.AsList();
        }


        public static async Task<IEnumerable<JurnalBlokRekap>> GetJournalRekapAsync(int dari, int sampai, string idData)
        {
            var sql = @"SELECT 
                    J.IDDATA, 
                    D.ESTATEID AS ESTATE, 
                    J.PERIODE,  
                    SUM(J.DEBET) AS DEBET, 
                    SUM(J.KREDIT) AS KREDIT
                FROM 
                    ACCT_JURNAL_DTL J
                JOIN 
                    MASTER_BLOK B ON B.KODE = SUBSTR(J.KODE, 6, 3) AND B.IDDATA = J.IDDATA
                JOIN 
                    MASTER_DIVISI D ON D.DIVISIID = B.DIVISIID
                LEFT JOIN 
                    AGRO_GROUPS G ON G.GROUPID = SUBSTR(J.KODE, 1, 3) || SUBSTR(J.KODE, 10, 3)
                LEFT JOIN 
                    AGRO_KATEGORI K ON K.KATEGORIID = G.KATEGORIID
                WHERE 
                    TO_DATE(J.GLYEAR || J.GLMONTH, 'YYYYMM') BETWEEN TO_DATE(:DARI, 'YYYYMM') AND TO_DATE(:SAMPAI, 'YYYYMM')  
                    AND SUBSTR(J.KODE, 1, 2) IN ('20', '80', '81')
                    AND J.IDDATA = :IDDATA
                GROUP BY 
                    J.IDDATA, D.ESTATEID, J.PERIODE
                ORDER BY 
                    D.ESTATEID, J.PERIODE";

            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            var result = await connection.QueryAsync<JurnalBlokRekap>(sql, new
            {
                DARI = dari.ToString(),
                SAMPAI = sampai.ToString(),
                IDDATA = idData
            });

            return result.AsList();
        }
    }
}
