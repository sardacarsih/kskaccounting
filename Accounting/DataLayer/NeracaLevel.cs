using Accounting.Model;
using Accounting;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using Dapper;
using DevExpress.XtraGantt.Scheduling;
using System;
using System.Linq;

public class NeracaLevel
{
    public List<ACCT_KATEGORI> GetLapNeraca(int month, string dataId, int year)
    {
        List<ACCT_KATEGORI> results = new ();
        using (OracleConnection connection = new ( LoginInfo.OracleConnString))
        {
            connection.Open();

            string query = @"SELECT KODE, KETERANGAN, KELOMPOK, SISI FROM ACCT_KATEGORI WHERE KETERANGAN IS NOT NULL AND KELOMPOK = 'N' ORDER BY KODE";
            using (OracleCommand command = new (query, connection))
            {
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ACCT_KATEGORI KATEGORI = new()
                        {
                            KODE = reader.GetString(0),
                            KETERANGAN = reader.GetString(1),
                            KELOMPOK = reader.GetString(2),
                            SISI = reader.GetString(3)
                        };
                        // Mengambil akun neraca untuk transaksi
                        List<LaporanNeraca> AKUN_NERACA = GetLapNeracaAkun(reader["KODE"].ToString(),dataId,month,year);
                        KATEGORI.DetailsAKUN = AKUN_NERACA;

                        decimal totalbulanini = AKUN_NERACA.Sum(d => d.BULANINI);
                        decimal totalbulanlalu = AKUN_NERACA.Sum(d => d.BULANLALU);
                        decimal totalawaltahun = AKUN_NERACA.Sum(d => d.AWALTAHUN);

                        KATEGORI.BULANINI = totalbulanini;
                        KATEGORI.BULANLALU = totalbulanlalu;
                        KATEGORI.AWALTAHUN = totalawaltahun;
                        results.Add(KATEGORI);
                    }
                }
            }
        }

        return results;
    }
    private List<LaporanNeraca> GetLapNeracaAkun(string kategori, string dataId, int month, int year)
    {
        using OracleConnection connection = new( LoginInfo.OracleConnString);

        string query = @"SELECT K.KODE,
                       'NERACA' CAT1,
                       CASE WHEN K.SISI='D' THEN 'AKTIVA' ELSE 'PASIVA' END KAT,
                       K.KETERANGAN CAT2,
                       S.KODEACC AKUN,
                       S.PARENTACC PARENTAKUN,
                       S.NAMAACC TIPE,
                       K.SISI POSISI,
                       CASE
                           WHEN :p_bulan = 1 THEN S.""1S""
                           WHEN :p_bulan = 2 THEN S.""2S""
                           WHEN :p_bulan = 3 THEN S.""3S""
                           WHEN :p_bulan = 4 THEN S.""4S""
                           WHEN :p_bulan = 5 THEN S.""5S""
                           WHEN :p_bulan = 6 THEN S.""6S""
                           WHEN :p_bulan = 7 THEN S.""7S""
                           WHEN :p_bulan = 8 THEN S.""8S""
                           WHEN :p_bulan = 9 THEN S.""9S""
                           WHEN :p_bulan = 10 THEN S.""10S""
                           WHEN :p_bulan = 11 THEN S.""11S""
                           ELSE S.""12S""
                       END BULANINI,
                       CASE
                           WHEN :p_bulan = 1 THEN S.SALDOAWAL
                           WHEN :p_bulan = 2 THEN S.""1S""
                           WHEN :p_bulan = 3 THEN S.""2S""
                           WHEN :p_bulan = 4 THEN S.""3S""
                           WHEN :p_bulan = 5 THEN S.""4S""
                           WHEN :p_bulan = 6 THEN S.""5S""
                           WHEN :p_bulan = 7 THEN S.""6S""
                           WHEN :p_bulan = 8 THEN S.""7S""
                           WHEN :p_bulan = 9 THEN S.""8S""
                           WHEN :p_bulan = 10 THEN S.""9S""
                           WHEN :p_bulan = 11 THEN S.""10S""
                           ELSE S.""11S""
                       END BULANLALU,
                       S.SALDOAWAL AWALTAHUN
                FROM ACCT_KATEGORI K
                INNER JOIN ACCT_COA S ON K.KODE = S.GRP
                WHERE K.KELOMPOK = 'N' 
                    AND K.KODE=:p_kategori
                    AND S.IDDATA = :p_IDDATA 
                    AND S.TAHUN = :p_tahun 
                    AND (S.SALDOAWAL <> 0 OR
                        (:p_bulan = 1 AND S.""1S"" <> 0) OR
                        (:p_bulan = 2 AND S.""2S"" <> 0) OR
                        (:p_bulan = 3 AND S.""3S"" <> 0) OR
                        (:p_bulan = 4 AND S.""4S"" <> 0) OR
                        (:p_bulan = 5 AND S.""5S"" <> 0) OR
                        (:p_bulan = 6 AND S.""6S"" <> 0) OR
                        (:p_bulan = 7 AND S.""7S"" <> 0) OR
                        (:p_bulan = 8 AND S.""8S"" <> 0) OR
                        (:p_bulan = 9 AND S.""9S"" <> 0) OR
                        (:p_bulan = 10 AND S.""10S"" <> 0) OR
                        (:p_bulan = 11 AND S.""11S"" <> 0) OR
                        (:p_bulan = 12 AND S.""12S"" <> 0))
                ORDER BY KAT, KODE, AKUN";

        var parameters = new
        {
            p_kategori = kategori,
            p_IDDATA = dataId,
            p_bulan = month,           
            p_tahun = year            
        };

        return connection.Query<LaporanNeraca>(query, parameters).AsList();
    }

}
