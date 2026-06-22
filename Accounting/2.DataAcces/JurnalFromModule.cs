using Accounting.Model;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.DataLayer
{
    public class JurnalFromModule : IJurnalFromModule
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);

        public DataTable AIS_Jurnal_Header(int p_periode, string p_ptlokasi, string p_estate, int p_remise)
        {
            string sql1 = "SELECT DIVISI,NOMOR,JURNALSTATUS JURNAL,DIVISI_ID,TIPE_BOR,NOJURNAL FROM BKM_JURNAL@DATABASE_LINK WHERE   periode=:p_periode and ptlokasi=:p_ptlokasi and estate=:p_estate and remise=:p_remise order by divisi_ID,NOMOR";
            using (OracleCommand _command = new(sql1, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_periode", OracleDbType.Int16).Value = p_periode;
                _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
                _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
                _command.Parameters.Add(":p_remise", OracleDbType.Int16).Value = p_remise;

                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        public DataTable AIS_Jurnal_Detail_BOR(string p_NOBUKTI, DateTime TanggalJurnal,int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi)
        {
           
            using (OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_AIS_BORONGAN", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("BORONGANONLY", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_NOBUKTI", OracleDbType.Varchar2, 50).Value = p_NOBUKTI;
                _command.Parameters.Add(":TanggalJurnal", OracleDbType.Date).Value = TanggalJurnal;
                _command.Parameters.Add(":p_periode", OracleDbType.Int16).Value = p_periode;
                _command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
                _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
                _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
                _command.Parameters.Add(":p_remise", OracleDbType.Int16).Value = p_remise;
                _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;

                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        
        public double CEK_TOTAL_TRANSAKSI(int p_periode, string p_ptlokasi, string p_module)
        {
            using OracleCommand _command = new("ACCT_IMPORT_MODULE.CEK_NILAI_TOTAL_TRANSAKSI", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("TOTAL", OracleDbType.Double).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_periode_int", OracleDbType.Int16).Value = p_periode;
            _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 5).Value = p_ptlokasi;
            _command.Parameters.Add(":p_module", OracleDbType.Varchar2, 20).Value = p_module;
            OracleDataReader dr;
            dr = _command.ExecuteReader();

            Double result = Convert.ToDouble(_command.Parameters["TOTAL"].Value.ToString());
            conn.Close();
            return result;
        }


        public void InserJurnal_FromKasir(List<JurnalKasirDetailDTO> JurnalFromKasir)
        {
            using OracleConnection conn = new( LoginInfo.OracleConnString);
            conn.Open();
            using (OracleTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    // Insert master records
                    string insertKasirsql = "INSERT INTO ACCT_JURNAL_TMP (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA,USERID,GLYEAR,GLMONTH) VALUES " +
                        "(:NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT, :KETERANGAN, :POSTED, :PERIODE, :IDDATA,:USERID,:GLYEAR,:GLMONTH) ";

                    conn.Execute(insertKasirsql, JurnalFromKasir, trans);
                    trans.Commit();
                    // return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    // return false;
                }
            }
        }

        public DataTable JurnalKasirDetail_DapperKasir(
                 DateTime p_dari,
                 DateTime p_sampai,
                 string p_iddata,
                 string p_estate,
                 string p_posted,
                 string p_periode,
                 string p_userid,
                 int p_glyear,
                 int p_glmonth)
                    {
            string sql = @"SELECT 
           j.kasno AS NoJurnal,
           j.kastgl AS Tanggal, 
           ROW_NUMBER() OVER (PARTITION BY j.KASNO ORDER BY j.BARIS ASC) AS Baris,
           j.ackode AS Kode,
           P.NAMAACC AS Rekening,
           j.debet AS Debet,
           j.kredit AS Kredit,
           j.ket AS Keterangan,
           :p_posted AS Posted,
           :p_periode AS Periode,
           :p_iddata AS IdData,
           :p_userid AS UserId,
           :p_glyear AS Glyear,
           :p_glmonth AS Glmonth 
       FROM kasir_jurnal_dtl j
       LEFT JOIN ACCT_COA P
           ON P.KODEACC = j.ACKODE AND P.IDDATA = j.IDDATA AND P.TAHUN = :p_tahun
       WHERE j.kastgl BETWEEN :p_dari AND :p_sampai AND j.ESTATEID = :p_estate AND j.IDDATA = :p_iddata
       ORDER BY j.kastgl, j.KASNO, j.baris ASC";

            using OracleConnection conn = new(LoginInfo.OracleConnString);
            using OracleCommand command = new(sql, conn);
            try
            {
                conn.Open();
                command.CommandType = CommandType.Text;
                command.BindByName = true;

                // Add parameters
                command.Parameters.Add(":p_posted", OracleDbType.Varchar2, 20).Value = p_posted;
                command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 20).Value = p_periode;
                command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                command.Parameters.Add(":p_glyear", OracleDbType.Int16).Value = p_glyear;
                command.Parameters.Add(":p_glmonth", OracleDbType.Int16).Value = p_glmonth;
                command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_glyear;
                command.Parameters.Add(":p_dari", OracleDbType.Date).Value = p_dari;
                command.Parameters.Add(":p_sampai", OracleDbType.Date).Value = p_sampai;
                command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;

                using OracleDataReader reader = command.ExecuteReader();
                DataTable dt = new();
                dt.Load(reader);
                return dt;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in JurnalKasirDetail with parameters {Dari}, {Sampai}, {Estate}, {IdData}", p_dari, p_sampai, p_estate, p_iddata);
                throw new Exception($"Error fetching data: {ex.Message}", ex);
            }
        } 

        public DataTable Jurnal_Inventori(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            using OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_INVENTORY", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_periode_int", OracleDbType.Int16).Value = p_periode_int;
            _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 5).Value = p_ptlokasi;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_posted", OracleDbType.Varchar2, 20).Value = p_posted;
            _command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
            _command.Parameters.Add(":p_glyear", OracleDbType.Int16).Value = p_glyear;
            _command.Parameters.Add(":p_glmonth", OracleDbType.Int16).Value = p_glmonth;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable AIS_Jurnal_Detail_HARIAN(string p_NOBUKTI, DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi)
        {
            using OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_AIS_HARIAN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("HARIANONLY", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_NOBUKTI", OracleDbType.Varchar2, 50).Value = p_NOBUKTI;
            _command.Parameters.Add(":TanggalJurnal", OracleDbType.Date).Value = TanggalJurnal;
            _command.Parameters.Add(":p_periode", OracleDbType.Int16).Value = p_periode;
            _command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
            _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
            _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
            _command.Parameters.Add(":p_remise", OracleDbType.Int16).Value = p_remise;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;

            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public string CekSumber_Jurnal(double p_jurnalid)
        {
            string result = string.Empty;
            using OracleCommand _command = new("SELECT SUMBER FROM ACCT_JURNAL_HDR WHERE JURNALID=:p_jurnalid", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_jurnalid", OracleDbType.Double).Value = p_jurnalid;
            OracleDataReader  dr = _command.ExecuteReader();
            while(dr.Read()) 
            {
                result = dr.GetString(0);
            }
            conn.Close();
            return result;
        }

        public DataTable AIS_Jurnal_Detail_ALL_HARIAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata)
        {
            using OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_AIS_ALL_HARIAN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("ALL", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":TanggalJurnal", OracleDbType.Date).Value = TanggalJurnal;
            _command.Parameters.Add(":p_periode", OracleDbType.Int16).Value = p_periode;
            _command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
            _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
            _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
            _command.Parameters.Add(":p_remise", OracleDbType.Int16).Value = p_remise;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable AIS_Jurnal_Detail_ALL_BORONGAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata)
        {
            using OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_AIS_ALL_BORONGAN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("ALL", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":TanggalJurnal", OracleDbType.Date).Value = TanggalJurnal;
            _command.Parameters.Add(":p_periode", OracleDbType.Int16).Value = p_periode;
            _command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
            _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
            _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
            _command.Parameters.Add(":p_remise", OracleDbType.Int16).Value = p_remise;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }
        public IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int p_periode_int, string p_ptlokasi)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_periode_int", p_periode_int, DbType.Int32);
            parameters.Add("p_ptlokasi", p_ptlokasi, DbType.String);

            var sql = @"
        SELECT nomor, tanggal 
        FROM (
            SELECT DISTINCT nomor, tanggal 
            FROM inv_terima@DATABASE_LINK 
            WHERE periode = :p_periode_int AND ptlokasi = :p_ptlokasi
            UNION
            SELECT DISTINCT nomor, tanggal 
            FROM inv_keluar@DATABASE_LINK 
            WHERE periode = :p_periode_int AND ptlokasi = :p_ptlokasi
        ) 
        ORDER BY nomor, tanggal";

            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            try
            {
                var result = connection.Query<JurnalInventoryHeaderDTO>(sql, parameters, commandType: CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                // Logging bisa ditambahkan di sini jika perlu
                throw new Exception("Gagal mengambil data jurnal inventori", ex);
            }
        }

        public IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_InventoryBaru(int p_periode_int, string p_ptlokasi, string? p_source_filter = null)
        {
            (DateTime dari, DateTime nextMonth) = MonthRange(p_periode_int / 100, p_periode_int % 100);

            var parameters = new DynamicParameters();
            parameters.Add("p_dari", dari, DbType.Date);
            parameters.Add("p_next_month", nextMonth, DbType.Date);
            parameters.Add("p_iddata", CompanyInfo.IDDATA, DbType.String);

            const string sql = @"
        SELECT IR.""ReceptionNumber"" AS NOMOR,
               MIN(IR.""ReceptionDate"") AS TANGGAL
        FROM ""InvReceptions"" IR
        JOIN ""InvLocations"" IL ON IL.""Id"" = IR.""LocationId""
        WHERE IR.""ReceptionDate"" >= :p_dari AND IR.""ReceptionDate"" < :p_next_month
          AND IL.""Name"" = :p_iddata
        GROUP BY IR.""ReceptionNumber""
        ORDER BY NOMOR, TANGGAL";

            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            try
            {
                return connection.Query<JurnalInventoryHeaderDTO>(sql, parameters, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal mengambil data jurnal inventory baru", ex);
            }
        }

        // Live LT (penerimaan barang) detail journal, sourced from the inventory app schema (Inv* tables).
        // Debit: each reception item to its item-type account; Credit: one aggregated line per reception
        // to the BARANG_DALAM_PERJALANAN default account (ACCT_DEFAULT) for the current IDDATA.
        private const string InventoryBaruDetailSql = @"
        SELECT X.NOJURNAL,
               X.TANGGAL,
               ROW_NUMBER() OVER (PARTITION BY X.NOJURNAL ORDER BY X.SORT_ORDER, X.KODE) AS BARIS,
               X.KODE,
               X.REKENING,
               X.DEBET,
               X.KREDIT,
               X.KETERANGAN,
               :p_posted AS POSTED,
               :p_periode_str AS PERIODE,
               :p_iddata AS IDDATA,
               :p_userid AS USERID,
               :p_glyear AS GLYEAR,
               :p_glmonth AS GLMONTH
        FROM (
            SELECT IR.""ReceptionNumber"" AS NOJURNAL,
                   IR.""ReceptionDate"" AS TANGGAL,
                   CAST(IIT.""CreditAccountNumber"" AS VARCHAR2(50)) AS KODE,
                   CAST(IIT.""Name"" AS NVARCHAR2(200)) AS REKENING,
                   ROUND(IRI.""Quantity"" * IRI.""UnitPrice"", 2) AS DEBET,
                   ROUND(0, 2) AS KREDIT,
                   TO_NCHAR(IR.""DeliveryNumber"") || N', ' || TO_NCHAR(II.""Name"") || N' = '
                       || TO_NCHAR(IRI.""Quantity"") || N' ' || TO_NCHAR(IU.""Name"") AS KETERANGAN,
                   1 AS SORT_ORDER
            FROM ""InvReceptions"" IR
            JOIN ""InvReceptionItems"" IRI ON IRI.""ReceptionId"" = IR.""Id""
            JOIN ""InvItems"" II ON II.""Id"" = IRI.""ItemId""
            JOIN ""InvUnits"" IU ON IU.""Id"" = II.""UnitId""
            JOIN ""InvItemTypes"" IIT ON IIT.""Id"" = II.""ItemTypeId""
            JOIN ""InvLocations"" IL ON IL.""Id"" = IR.""LocationId""
            WHERE IR.""ReceptionDate"" >= :p_dari AND IR.""ReceptionDate"" < :p_next_month
              AND IL.""Name"" = :p_iddata
            UNION ALL
            SELECT IR.""ReceptionNumber"",
                   IR.""ReceptionDate"",
                   CAST((SELECT MAX(kodeacc) FROM ACCT_DEFAULT WHERE iddata = :p_iddata AND nama = 'BARANG_DALAM_PERJALANAN') AS VARCHAR2(50)) AS KODE,
                   CAST((SELECT MAX(keterangan) FROM ACCT_DEFAULT WHERE iddata = :p_iddata AND nama = 'BARANG_DALAM_PERJALANAN') AS NVARCHAR2(200)) AS REKENING,
                   ROUND(0, 2) AS DEBET,
                   ROUND(SUM(IRI.""Quantity"" * IRI.""UnitPrice""), 2) AS KREDIT,
                   TO_NCHAR((SELECT MAX(keterangan) FROM ACCT_DEFAULT WHERE iddata = :p_iddata AND nama = 'BARANG_DALAM_PERJALANAN'))
                       || N', ' || TO_NCHAR(IR.""ReceptionNumber"") AS KETERANGAN,
                   2 AS SORT_ORDER
            FROM ""InvReceptions"" IR
            JOIN ""InvReceptionItems"" IRI ON IRI.""ReceptionId"" = IR.""Id""
            JOIN ""InvLocations"" IL ON IL.""Id"" = IR.""LocationId""
            WHERE IR.""ReceptionDate"" >= :p_dari AND IR.""ReceptionDate"" < :p_next_month
              AND IL.""Name"" = :p_iddata
            GROUP BY IR.""ReceptionNumber"", IR.""ReceptionDate""
        ) X
        ORDER BY X.TANGGAL, X.NOJURNAL, X.SORT_ORDER, X.KODE";

        private static (DateTime dari, DateTime nextMonth) MonthRange(int year, int month)
        {
            DateTime dari = new(year, month, 1);
            return (dari, dari.AddMonths(1));
        }

        private static void AddInventoryBaruDetailParameters(OracleCommand command, DateTime dari, DateTime nextMonth,
            string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            command.Parameters.Add(":p_posted", OracleDbType.Varchar2, 20).Value = p_posted;
            command.Parameters.Add(":p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
            command.Parameters.Add(":p_glyear", OracleDbType.Int16).Value = p_glyear;
            command.Parameters.Add(":p_glmonth", OracleDbType.Int16).Value = p_glmonth;
            command.Parameters.Add(":p_dari", OracleDbType.Date).Value = dari;
            command.Parameters.Add(":p_next_month", OracleDbType.Date).Value = nextMonth;
        }

        public IEnumerable<JurnalInventoryDetailDTO> GetJurnalDetails_Inventory(
        int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted,
        string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            var result = new List<JurnalInventoryDetailDTO>();

            try
            {
                using var command = new OracleCommand("ACCT_IMPORT_MODULE.JURNAL_INVENTORY", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add("p_periode_int", OracleDbType.Int16).Value = p_periode_int;
                command.Parameters.Add("p_ptlokasi", OracleDbType.Varchar2, 5).Value = p_ptlokasi;
                command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                command.Parameters.Add("p_posted", OracleDbType.Varchar2, 20).Value = p_posted;
                command.Parameters.Add("p_periode_str", OracleDbType.Varchar2, 20).Value = p_periode_str;
                command.Parameters.Add("p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                command.Parameters.Add("p_glyear", OracleDbType.Int16).Value = p_glyear;
                command.Parameters.Add("p_glmonth", OracleDbType.Int16).Value = p_glmonth;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = new JurnalInventoryDetailDTO
                    {
                        NOJURNAL = reader["NOJURNAL"]?.ToString(),
                        TANGGAL = Convert.ToDateTime(reader["TANGGAL"]),
                        BARIS = Convert.ToInt16(reader["BARIS"]),
                        KODE = reader["KODE"]?.ToString(),
                        REKENING = reader["REKENING"]?.ToString(),
                        DEBET = Convert.ToDouble(reader["DEBET"]),
                        KREDIT = Convert.ToDouble(reader["KREDIT"]),
                        KETERANGAN = reader["KETERANGAN"]?.ToString(),
                        POSTED = reader["POSTED"]?.ToString(),
                        PERIODE = reader["PERIODE"]?.ToString(),
                        IDDATA = reader["IDDATA"]?.ToString(),
                        USERID = reader["USERID"]?.ToString(),
                        GLYEAR = Convert.ToInt16(reader["GLYEAR"]),
                        GLMONTH = Convert.ToInt16(reader["GLMONTH"])
                    };
                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal mengambil detail jurnal inventori", ex);
            }

            return result;
        }

        public IEnumerable<JurnalInventoryDetailDTO> GetJurnalDetails_InventoryBaru(
        int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted,
        string p_periode_str, string p_userid, int p_glyear, int p_glmonth, string? p_source_filter = null)
        {
            var result = new List<JurnalInventoryDetailDTO>();

            (DateTime dari, DateTime nextMonth) = MonthRange(p_glyear, p_glmonth);

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new(InventoryBaruDetailSql, connection)
            {
                CommandType = CommandType.Text,
                BindByName = true
            };

            try
            {
                connection.Open();
                AddInventoryBaruDetailParameters(command, dari, nextMonth, p_iddata, p_posted, p_periode_str, p_userid, p_glyear, p_glmonth);

                using OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = new JurnalInventoryDetailDTO
                    {
                        NOJURNAL = reader["NOJURNAL"]?.ToString(),
                        TANGGAL = Convert.ToDateTime(reader["TANGGAL"]),
                        BARIS = Convert.ToInt16(reader["BARIS"]),
                        KODE = reader["KODE"]?.ToString(),
                        REKENING = reader["REKENING"]?.ToString(),
                        DEBET = Convert.ToDouble(reader["DEBET"]),
                        KREDIT = Convert.ToDouble(reader["KREDIT"]),
                        KETERANGAN = reader["KETERANGAN"]?.ToString(),
                        POSTED = reader["POSTED"]?.ToString(),
                        PERIODE = reader["PERIODE"]?.ToString(),
                        IDDATA = reader["IDDATA"]?.ToString(),
                        USERID = reader["USERID"]?.ToString(),
                        GLYEAR = Convert.ToInt16(reader["GLYEAR"]),
                        GLMONTH = Convert.ToInt16(reader["GLMONTH"])
                    };
                    result.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal mengambil detail jurnal inventory baru", ex);
            }

            return result;
        }

        public DataTable Jurnal_InventoriBaru(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth, string? p_source_filter = null)
        {
            (DateTime dari, DateTime nextMonth) = MonthRange(p_glyear, p_glmonth);

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new(InventoryBaruDetailSql, connection)
            {
                CommandType = CommandType.Text,
                BindByName = true
            };

            connection.Open();
            AddInventoryBaruDetailParameters(command, dari, nextMonth, p_iddata, p_posted, p_periode_str, p_userid, p_glyear, p_glmonth);

            using OracleDataReader reader = command.ExecuteReader();
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        private static string? NormalizeInventorySourceFilter(string? p_source_filter)
        {
            if (string.IsNullOrWhiteSpace(p_source_filter))
            {
                return null;
            }

            return "%" + p_source_filter.Trim().ToUpperInvariant() + "%";
        }

        public IEnumerable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int p_periode_int, string p_estate, string p_iddata)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_periode_int", p_periode_int, DbType.Int32);
            parameters.Add("p_estate", p_estate, DbType.String);
            parameters.Add("p_iddata", p_iddata, DbType.String);

            using (var connection = new OracleConnection(LoginInfo.OracleConnString))
            {
                const string sql = @"
            SELECT DISTINCT KASNO AS NOMOR, KASTGL AS TANGGAL
            FROM kasir_jurnal_dtl
            WHERE PERIODE = :p_periode_int
              AND ESTATEID = :p_estate
              AND IDDATA = :p_iddata
            ORDER BY NOMOR, TANGGAL";

                return connection.Query<JurnalKasirHeaderDTO>(sql, parameters, commandType: CommandType.Text);
            }
        }

    }
}
