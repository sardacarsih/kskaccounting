using Accounting._1.Interface;
using Accounting.Model;
using Accounting.Utilities;
using Dapper;
using DevExpress.Mvvm.Native;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting._2.DataAcces
{
    public class JurnalimportService : IJurnalimport
    {

        public async Task<List<Division>> GetDivisionsAsync(string idData, string estateId, int periode, int remise)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            try
            {
                dbConnection.Open();
                string query = @"
                    SELECT DISTINCT
                        M.ISBORONGAN,
                        D.DIVISIID,
                        D.DIVISI,
                        :P_PERIODE || '/R' || :P_REMISE || 
                        CASE WHEN M.ISBORONGAN = 1 THEN '/BORONGAN/' ELSE '/HARIAN/' END || 
                        :P_ESTATE || '/' || D.DIVISI AS NOMOR,
                        NULL AS Jurnal,
                        NULL AS NoJurnal
                    FROM 
                        AIS_BKMMASTER M
                    JOIN 
                        MASTER_DIVISI D ON D.DIVISIID = M.DIVISI
                    WHERE 
                        M.SIGN = 1 
                        AND M.PERIODE = :P_PERIODE 
                        AND M.REMISE = :P_REMISE 
                        AND M.IDDATA = :P_IDDATA 
                        AND M.ESTATE = :P_ESTATE 
                    ORDER BY D.DIVISI, M.ISBORONGAN";

                var parameters = new
                {
                    P_PERIODE = periode,
                    P_REMISE = remise,
                    P_IDDATA = idData,
                    P_ESTATE = estateId
                };

                var divisions = await dbConnection.QueryAsync<Division>(query, parameters);
                return divisions.AsList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        

        public async Task<List<Estate>> GetEstateAsync(string idData)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            try
            {
                dbConnection.Open();
                string query = @"SELECT ESTATEID ID,NAMA FROM MASTER_ESTATE WHERE IDDATA=:P_IDDATA";

                var parameters = new { P_IDDATA = idData };
                var divisions = await dbConnection.QueryAsync<Estate>(query, parameters);
                return divisions.AsList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<AIS_JURNAL>> GetAISforJurnalAsync(string idData, string estateId, int periode, int remise, int tahun, string periodes)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            try
            {
                dbConnection.Open();
                string query = @"
    WITH CTE_AggregatedData AS (
        SELECT 
            M.ISBORONGAN,
            M.DIVISI,
            J.JENIS,
            P.KATEGORI,
            D.ACKODE AS KODE,
            CASE 
                WHEN D.KETERANGAN IS NULL THEN P.KERJA 
                ELSE P.KERJA || ',' || D.KETERANGAN 
            END AS PEKERJAAN,       
            CASE 
                WHEN MB.BLOK IS NULL THEN '' 
                ELSE TRIM(MB.BLOK)
            END AS BLOK,
            CASE 
                WHEN D.PEKERJAAN = '41001' AND D.KETERANGAN IS NULL THEN D.SATP2
                WHEN D.PEKERJAAN = '41001' AND UPPER(D.KETERANGAN) IN ('PANEN BASIS', 'PANEN HARIAN') THEN D.SATP2
                ELSE D.SATP1
            END AS SATUAN,
            SUM(CASE 
                WHEN D.PEKERJAAN = '41001' AND D.KETERANGAN IS NULL THEN D.QTYP2
                WHEN D.PEKERJAAN = '41001' AND UPPER(D.KETERANGAN) IN ('PANEN BASIS', 'PANEN HARIAN') THEN D.QTYP2
                ELSE D.QTYP1
            END) AS QTY,
            SUM(CASE 
                WHEN D.SATUAN = 'HK' THEN D.QTY
                ELSE 0
            END) AS HK,
            SUM(D.QTY * D.TARIF) AS JUMLAH,
            SUM(D.PREMI) AS PREMI,
            SUM(D.DENDA) AS DENDA,
            SUM(D.JUMLAH) AS TOTAL
        FROM 
            AIS_BKMMASTER M
        JOIN 
            AIS_BKMDETAIL D ON D.MASTERID = M.MASTERID
        JOIN 
            AIS_PEKERJAAN P ON P.KERJAID = D.PEKERJAAN
        JOIN 
            AIS_JENIS J ON J.ID = M.JENISBKM
        LEFT JOIN 
            MASTER_BLOK MB ON MB.BLOKID = D.BLOK
        WHERE 
            M.SIGN = 1 
            AND M.PERIODE = :P_PERIODE 
            AND M.REMISE = :P_REMISE 
            AND M.IDDATA = :P_IDDATA 
            AND M.ESTATE = :P_ESTATE 
        GROUP BY 
            M.ISBORONGAN,
            M.DIVISI,
            J.JENIS,
            P.KATEGORI,
            D.ACKODE,
            CASE 
                WHEN D.KETERANGAN IS NULL THEN P.KERJA 
                ELSE P.KERJA || ',' || D.KETERANGAN 
            END,
            CASE 
                WHEN MB.BLOK IS NULL THEN '' 
                ELSE TRIM(MB.BLOK) 
            END,
            CASE 
                WHEN D.PEKERJAAN = '41001' AND D.KETERANGAN IS NULL THEN D.SATP2
                WHEN D.PEKERJAAN = '41001' AND UPPER(D.KETERANGAN) IN ('PANEN BASIS', 'PANEN HARIAN') THEN D.SATP2
                ELSE D.SATP1
            END
    )
    SELECT 
        CTE.ISBORONGAN,
        CTE.DIVISI,
        CTE.JENIS,
        CTE.PEKERJAAN,
        CTE.BLOK,
        CTE.KODE,   
        C.NAMAACC REKENING,
        CTE.TOTAL DEBET,
        CTE.TOTAL DEBETPPH,
        0 KREDIT,
        CTE.BLOK||' '||D.DIVISI||' '||D.ESTATEID||' '||:P_PERIODES||';'|| CTE.PEKERJAAN||'='||TO_CHAR(CTE.QTY, 'FM999990.00')||' '||CTE.SATUAN ||' '||
        CASE 
        WHEN CTE.SATUAN = 'HK' THEN
            CASE 
                WHEN CTE.QTY != 0 THEN '' 
            END
        ELSE 
            CASE 
                WHEN CTE.HK != 0 THEN TO_CHAR(CTE.HK, 'FM999990.00') || ' HK' 
                ELSE ''
            END
        END AS Keterangan,
        'TRUE' POSTED,
        SUBSTR(:P_PERIODE,-2)||'/'||SUBSTR(:P_PERIODE,1,4) PERIODE
        
    FROM 
        CTE_AggregatedData CTE
    JOIN MASTER_DIVISI D ON D.DIVISIID=CTE.DIVISI
    LEFT JOIN ACCT_COA C ON C.KODEACC=CTE.KODE AND C.TAHUN=:P_TAHUN AND C.IDDATA=:P_IDDATA
    ORDER BY 
        CTE.DIVISI,
        CTE.JENIS,
        CTE.PEKERJAAN,
        CTE.BLOK";

                var parameters = new
                {
                    P_PERIODE = periode,
                    P_REMISE = remise,
                    P_IDDATA = idData,
                    P_ESTATE = estateId,
                    P_TAHUN = tahun,
                    P_PERIODES = periodes
                };


                var result = await dbConnection.QueryAsync<AIS_JURNAL>(query, parameters);
                return result.ToList();
            
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "An error occurred while fetching AIS Jurnal data.");
                throw;
            }
        }

        public async Task<IEnumerable<BKM_KOMPONEN_KHT>> GetKOMPONEN_KHTAsync(string idData, int periode, int remise, string divisiid)
        {
            using IDbConnection db = new OracleConnection(LoginInfo.OracleConnString);

            // Check the count from FIN_KOMPONEN_KHT
            string countSql = "SELECT COUNT(*) FROM FIN_KOMPONEN_KHT WHERE IDDATA=:P_IDDATA AND PERIODE = :P_PERIODE AND REMISE = :P_REMISE";
            int count = await db.ExecuteScalarAsync<int>(countSql, new { P_IDDATA = idData, P_PERIODE = periode, P_REMISE = remise });

            var handler = new KOMPONEN_KHTHandler();

            // Define the query based on the count
            if (count == 0)
            {
                if (remise == 1)
                {
                    return await handler.HandleTrueConditionAsyncR1(db, idData, periode, remise, divisiid);
                }
                else
                {
                    return await handler.HandleTrueConditionAsyncR2(db, idData, periode, remise, divisiid);
                }
            }
            else
            {
                return await handler.HandleFalseConditionAsync(db, idData, periode, remise, divisiid);
            }
        }

        public async Task<IEnumerable<BPJS_INFO_DTO>> GetBPJSInfoListAsync(int periode, string divisiid)
        {
            try
            {
                using var dbConnection = new OracleConnection(LoginInfo.OracleConnString);
                await dbConnection.OpenAsync(); // Use asynchronous Open method

                // Query to check the count in FIN_BPJS_HARIAN
                string countSql = @"
                    SELECT COUNT(*)
                    FROM FIN_BPJS_HARIAN
                    WHERE PERIODE = :periode AND DIVISIID = :divisiid";

                var countParameters = new
                {
                    periode,
                    divisiid
                };

                int count = await dbConnection.ExecuteScalarAsync<int>(countSql, countParameters);

                // Depending on the count, query FIN_BPJS_HARIAN or HRD_KARYAWAN
                string querySql = count > 0
                    ? @"SELECT B.NIK, B.NAMA, B.STATUS,K.NO_BPJS_TK,K.NO_BPJS_KESEHATAN, B.POT_BPJS_TK, B.POT_BPJS_KESEHATAN, B.POT_BPJS_TK_PENSIUN
                        FROM FIN_BPJS_HARIAN B
                        JOIN HRD_KARYAWAN K ON K.NIK = B.NIK AND K.DIVISIID=B.DIVISIID
                        WHERE B.PERIODE = :periode AND B.DIVISIID = :divisiid"
                    : @"
                        SELECT K.DIVISIID, K.NIK, K.NAMA, S.STATUS, K.NO_BPJS_TK, K.NO_BPJS_KESEHATAN, 
                               K.POT_BPJS_TK, K.POT_BPJS_KESEHATAN, K.POT_BPJS_TK_PENSIUN
                        FROM HRD_KARYAWAN K
                        JOIN HRD_STATUS_KARYAWAN S ON S.KODESTATUS = K.STATUS_KARYAWAN
                        WHERE K.STATUS_KARYAWAN != 1  AND K.DIVISIID=:divisiid
                          AND (K.POT_BPJS_TK > 0 OR K.POT_BPJS_KESEHATAN > 0 OR K.POT_BPJS_TK_PENSIUN > 0)";

                return (await dbConnection.QueryAsync<BPJS_INFO_DTO>(querySql, countParameters)).AsList();
            }
            catch (Exception ex)
            {
                // Handle exceptions (log, rethrow, or return an empty list)
                // For example, logging the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<BPJS_INFO_DTO>(); // Return an empty list or handle as needed
            }
        }

        public async Task<List<SlipGaji_DTO>> viewDaftarGajidanTunjangan_BulananAsync(string idData, string estateId, int periode)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();
            string query = @"SELECT  NO,NIK, NAMA, REKENING, DIVISI, JABATAN, GOL, GAJI_POKOK, TUNJANGAN, LEMBUR_PREMI, POT_TUNJANGAN,NO_BPJS_TK,BPJS_KES,BPJS_JHT,BPJS_JP,KANTOR,KENDARAAN,LAIN, GAJI_BERSIH, 
                            TJG_JABATAN, TJG_LAPANGAN, TJG_NONLAPANGAN, TJG_LUASAN, TJG_TELP, TJG_PERABOT, TJG_KEBERSIHAN, INSENTIF, UMAKAN, SKENDARAAN,ALOKASI_JURNAL FROM FIN_PAYROLL_STAFF 
                            WHERE PERIODE = :periode AND IDDATA = :idData AND ESTATE = :estateId
                            ORDER BY NO";

            var result = dbConnection.Query<SlipGaji_DTO>(query, new { periode, idData,estateId });
            return result.AsList();
        }

        public async Task<List<FIN_POTONGAN_KANTOR>> viewPotonganKantorAsync(string idData, string estateId, int periode)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();
            string query = @"SELECT M.TANGGAL,M.NIK,S.NAMA,D.POT_KE||' of '||M.X_BAYAR as POTONGAN,ROUND(M.JLH_POTONGAN,2)JUMLAH, 'POTONGAN KANTOR' KETERANGAN,ACKODE FROM FIN_PIUTANG_KANTOR M
                        JOIN FIN_PIUTANG_KANTOR_DTL D ON D.PARENTID=M.MASTERID
                        JOIN FIN_PAYROLL_STAFF S ON S.NIK=M.NIK AND S.IDDATA=M.IDDATA AND S.ESTATE=M.ESTATE AND S.PERIODE=D.PERIODE
                        WHERE D.PERIODE=:P_PERIODE AND M.IDDATA=:P_IDDATA AND M.ESTATE=:P_ESTATE
                        UNION ALL
                        SELECT M.TANGGAL,M.NIK,S.NAMA,D.POT_KE||' of '||M.X_BAYAR as POTONGAN,ROUND(M.JLH_POTONGAN,2)JUMLAH, 'POTONGAN KENDARAAN' KETERANGAN,ACKODE FROM FIN_PIUTANG_KENDARAAN M
                        JOIN FIN_PIUTANG_KENDARAAN_DTL D ON D.PARENTID=M.MASTERID
                        JOIN FIN_PAYROLL_STAFF S ON S.NIK=M.NIK AND S.IDDATA=M.IDDATA AND S.ESTATE=M.ESTATE AND S.PERIODE=D.PERIODE
                        WHERE D.PERIODE=:P_PERIODE AND M.IDDATA=:P_IDDATA AND M.ESTATE=:P_ESTATE";
            var result = dbConnection.Query<FIN_POTONGAN_KANTOR>(query, new { P_PERIODE = periode, P_IDDATA = idData, P_ESTATE = estateId });
            return result.AsList();
        }
               

        public async Task<List<ALOKASI_JURNAL_DTO>> AlokasiJurnalAsync(string idData)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            string query = @"SELECT KODE,KET KETERANGAN,ACKODE ACJURNAL,BIAYA_UMUM FROM FIN_ALOKASI_JURNAL WHERE IDDATA=:P_IDDATA"
            ;

            var parameters = new
            {
                P_IDDATA = idData,
            };

            var results = connection.Query<ALOKASI_JURNAL_DTO>(query, parameters).ToList();

            return results;
        }
        public async Task<List<AIS_JURNAL_FINAL>> HitungLampiranKASAsync(
            List<SlipGaji_DTO> sLIPGAJIlist,
            List<FIN_POTONGAN_KANTOR> pot_KANTOR,
            List<ALOKASI_JURNAL_DTO> alokasijurnal,
            IEnumerable<DTOCOAAktif> ListCoaAktif,
            string p_periodeket,
            string p_periode_str,
            string p_estate,
            DateTime TanggalJurnal)
         { 
            string ket = $"Alokasi Payroll {p_periodeket}, ";
            // STEP1: Calculate BIAYA_UMUM and its components
            var BIAYA_UMUM = from sg in sLIPGAJIlist
                             join j in alokasijurnal on sg.ALOKASI_JURNAL equals j.KODE
                             where j.BIAYA_UMUM == "Y"
                             select sg;

            var jumlahgpumakan = BIAYA_UMUM.Sum(gp => gp.GAJI_POKOK + gp.UMAKAN);
            var insentif = BIAYA_UMUM.Sum(gp => gp.INSENTIF + gp.LEMBUR_PREMI);
            var jabatan = BIAYA_UMUM.Sum(gp => gp.TJG_JABATAN);
            var perumahan = BIAYA_UMUM.Sum(gp => gp.TJG_KEBERSIHAN + gp.TJG_PERABOT);
            var operasional = BIAYA_UMUM.Sum(gp => gp.TJG_LAPANGAN + gp.TJG_NONLAPANGAN);
            var telp = BIAYA_UMUM.Sum(gp => gp.TJG_TELP);
            var luasan = BIAYA_UMUM.Sum(gp => gp.TJG_LUASAN);
            var sewa = BIAYA_UMUM.Sum(gp => gp.SKENDARAAN);

            List<FIN_LAMPIRANKAS_DTO> LAMPIRAN = new()
            {
                new FIN_LAMPIRANKAS_DTO { NO = 1, ACCOUNT="70.10001.001", KETERANGAN = ket+"Gaji Pokok dan Uang Makan", DEBET = jumlahgpumakan, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 2, ACCOUNT="70.10001.003", KETERANGAN = ket+"Premi Lembur dan Insentif", DEBET = insentif, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 3, ACCOUNT="70.10001.004", KETERANGAN = ket+"Tunjangan Jabatan", DEBET = jabatan, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 4, ACCOUNT="70.10001.999", KETERANGAN = ket+"Tunjangan Perumahan", DEBET = perumahan, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 5, ACCOUNT="70.10001.013", KETERANGAN = ket+"Tunjangan Operasional", DEBET = operasional, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 6, ACCOUNT="70.10001.999", KETERANGAN = ket+"Tunjangan Telpon", DEBET = telp, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 7, ACCOUNT="70.10001.999", KETERANGAN = ket+"Tunjangan Luasan", DEBET = luasan, KREDIT = 0 },
                new FIN_LAMPIRANKAS_DTO { NO = 8, ACCOUNT="72.03001.004", KETERANGAN = ket+"Sewa Kendaraan", DEBET = sewa, KREDIT = 0 }
            };


            // STEP2: Calculate BIAYA_ALOKASI
            var BIAYA_ALOKASI = from sg in sLIPGAJIlist
                                join j in alokasijurnal on sg.ALOKASI_JURNAL equals j.KODE
                                where j.BIAYA_UMUM != "Y"
                                group sg by new { j.ACJURNAL, j.KETERANGAN } into grouped
                                orderby grouped.Key.ACJURNAL
                                select new FIN_LAMPIRANKAS_DTO
                                {
                                    ACCOUNT = grouped.Key.ACJURNAL,
                                    KETERANGAN = $"{ket} {grouped.Key.KETERANGAN}",
                                    DEBET = grouped.Sum(sg => sg.TUNJANGAN + sg.GAJI_POKOK + sg.LEMBUR_PREMI),
                                    KREDIT = 0 // Assuming KREDIT calculation is not needed here
                                };


            int currentMaxNo = LAMPIRAN.Max(l => l.NO);
            foreach (var item in BIAYA_ALOKASI)
            {
                item.NO = ++currentMaxNo;
                LAMPIRAN.Add(item);
            }

            // STEP3: Handle pot_KANTOR
            var pot_kant = from k in pot_KANTOR
                           select new FIN_LAMPIRANKAS_DTO
                           {
                               ACCOUNT = k.ACKODE,
                               KETERANGAN = $"{p_periodeket}, {k.NAMA} {k.KETERANGAN} ({k.POTONGAN})",
                               DEBET = 0,
                               KREDIT = k.JUMLAH
                           };

            var kantor = pot_kant.Sum(total => total.KREDIT);

            int currentMaxNokantor = LAMPIRAN.Max(l => l.NO);
            foreach (var item in pot_kant)
            {
                item.NO = ++currentMaxNokantor;
                LAMPIRAN.Add(item);
            }

            // STEP4: Calculate additional components
            var kesehatan = sLIPGAJIlist.Sum(pot => pot.BPJS_KES);
            var jht = sLIPGAJIlist.Sum(pot => pot.BPJS_JHT);
            var jp = sLIPGAJIlist.Sum(pot => pot.BPJS_JP);
            var pot_tunj = sLIPGAJIlist.Sum(pot => pot.POT_TUNJANGAN);
            var alokasi = BIAYA_ALOKASI.Sum(total => total.DEBET);

            var gajiymhd = jumlahgpumakan + insentif + jabatan + perumahan + operasional + telp + luasan + sewa + alokasi - (kesehatan + jht + jp + pot_tunj + kantor);

            List<FIN_LAMPIRANKAS_DTO> LAMPIRANnext = new()
            {
                new FIN_LAMPIRANKAS_DTO { NO = 9, ACCOUNT="70.10001.005",KETERANGAN = $"{p_periodeket}, Potongan BPJS", DEBET = 0, KREDIT = kesehatan },
                new FIN_LAMPIRANKAS_DTO { NO = 10, ACCOUNT="70.10001.005",KETERANGAN = $"{p_periodeket}, Potongan JHT", DEBET = 0, KREDIT = jht },
                new FIN_LAMPIRANKAS_DTO { NO = 11, ACCOUNT="70.10001.005",KETERANGAN = $"{p_periodeket}, Potongan PENSIUN", DEBET = 0, KREDIT = jp },
                new FIN_LAMPIRANKAS_DTO { NO = 12, ACCOUNT="70.10001.005",KETERANGAN = $"{p_periodeket}, Potongan Tunjangan", DEBET = 0, KREDIT = pot_tunj },
                new FIN_LAMPIRANKAS_DTO { NO = 13, ACCOUNT="33.00001.001",KETERANGAN = "Alokasi Payroll "+p_periodeket, DEBET = 0, KREDIT = gajiymhd }
            };

            int currentMaxNo2 = LAMPIRAN.Max(l => l.NO);
            foreach (var item in LAMPIRANnext)
            {
                item.NO = ++currentMaxNo2;
                LAMPIRAN.Add(item);
            }

            // STEP5: Final join with ListCoaAktif and return result
            var jurnalfinal = (from j in LAMPIRAN
                               join coa in ListCoaAktif on j.ACCOUNT equals coa.KODE into coaGroup
                               from coa in coaGroup.DefaultIfEmpty()
                               select new AIS_JURNAL_FINAL
                               {
                                   NOJURNAL = $"{p_estate}/{p_periode_str}",
                                   TANGGAL = TanggalJurnal,
                                   NO = j.NO,
                                   KODE = j.ACCOUNT,
                                   REKENING = coa?.PERKIRAAN ?? j.ACCOUNT,
                                   DEBET = j.DEBET,
                                   KREDIT = j.KREDIT,
                                   KETERANGAN = j.KETERANGAN,
                                   POSTED = true,
                                   PERIODE = p_periode_str
                               }).ToList();

            return jurnalfinal;
        }

        public async Task<List<AIS_JURNAL_FINAL>> GetPayrollforJurnalAsync(string idData, int periode, int remise, int tahun)
        {
            try
            {
                using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
                dbConnection.Open();

                string query = @"SELECT 
                            U.ACKODE AS KODE, 
                            A.NAMAACC AS REKENING, 
                            U.KERJA || ',' || SUM(TOTAL_HK) || ' HK,' AS KETERANGAN, 
                            SUM(U.BRUTO) AS DEBET 
                        FROM 
                            HRD_PAYROLL_NONSTAFF U
                        LEFT JOIN 
                            ACCT_COA A 
                        ON 
                            A.KODEACC = U.ACKODE 
                            AND A.IDDATA = U.IDDATA 
                            AND A.TAHUN = :tahun
                        WHERE 
                            U.IDDATA = :idData 
                            AND U.PERIODE = :periode 
                            AND U.REMISE = :remise
                        GROUP BY 
                            U.ACKODE, 
                            A.NAMAACC, 
                            U.KERJA
                        ORDER BY 
                            U.KERJA";

                var result = await dbConnection.QueryAsync<AIS_JURNAL_FINAL>(query, new { tahun,idData, periode, remise  });

                return result.AsList();
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging library)
                Console.WriteLine($"Error fetching payroll for journal: {ex.Message}");
                throw;
            }
        }


        public async Task<List<BPJS_INFO_DTO>> GetBPJSUmumAsync(string idData, int periode, int remise)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string query = @"SELECT B.NIK, B.NAMA,B.POT_BPJS_TK, B.POT_BPJS_KESEHATAN, B.POT_BPJS_TK_PENSIUN
                        FROM HRD_PAYROLL_NONSTAFF B                       
                        WHERE B.PERIODE = :periode AND B.IDDATA = :idData AND B.REMISE = :remise";

            var result = await dbConnection.QueryAsync<BPJS_INFO_DTO>(query, new { periode,idData, remise });

            return result.AsList();
        }

        public async Task<List<JurnalKomponen>> GetAISforJurnalKomponenAsync(string idData, string estateId, int periode, int remise)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string query = @"SELECT ISBORONGAN, DIVISI, KETERANGAN, JUMLAH,SISI
                    FROM FIN_JURNAL_KOMPONEN
                    WHERE PERIODE = :P_PERIODE 
                      AND REMISE = :P_REMISE 
                      AND ESTATE = :P_ESTATE";

            var result = await dbConnection.QueryAsync<JurnalKomponen>(query, new { P_PERIODE = periode, P_REMISE = remise, P_ESTATE = estateId });

            return result.AsList();
        }

    }
}
