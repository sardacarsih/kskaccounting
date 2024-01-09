using Accounting.Model;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.DataLayer
{
    public class JurnalFromModule : IJurnalFromModule
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);

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
            using OracleConnection conn = new(Acct.OracleConnString);
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

       

        public DataTable JurnalKasirDetail_DapperKasir(DateTime p_dari, DateTime p_sampai, string p_ptlokasi, string p_iddata, string p_estate, string p_posted, string p_periode, string p_userid, int p_glyear, int p_glmonth)
        {
            string sql1 = "SELECT j.kasno NoJurnal,j.kastgl Tanggal, " +
                   "ROW_NUMBER() OVER(PARTITION BY j.KASNO ORDER BY j.BARIS ASC) Baris,j.ackode Kode,P.KET Rekening,j.debet Debet,j.kredit Kredit,j.ket Keterangan " +
                   ",:p_posted Posted ,:p_periode Periode,:p_iddata iddata,:p_userid userid,:p_glyear glyear,:p_glmonth glmonth from kasir_jurnal_dtl@DATABASE_LINK j " +
                   "LEFT JOIN ACCT_ACKODE@DATABASE_LINK P ON P.KODE=j.ACKODE AND P.PTLOKASI=j.PTLOKASI " +
                   "where j.kastgl between :p_dari and :p_sampai and j.PTLOKASI=:p_ptlokasi and IDPAY=:p_estate order by j.kastgl,j.KASNO,j.baris asc";
            using (OracleCommand _command = new (sql1, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":Posted", OracleDbType.Varchar2, 20).Value = p_posted;
                _command.Parameters.Add(":Periode", OracleDbType.Varchar2, 20).Value = p_periode;
                _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":userid", OracleDbType.Varchar2, 20).Value = p_userid;
                _command.Parameters.Add(":glyear", OracleDbType.Int16).Value = p_glyear;
                _command.Parameters.Add(":glmonth", OracleDbType.Int16).Value = p_glmonth;
                _command.Parameters.Add(":p_dari", OracleDbType.Date).Value = p_dari;
                _command.Parameters.Add(":p_sampai", OracleDbType.Date).Value = p_sampai;
                _command.Parameters.Add(":p_ptlokasi", OracleDbType.Varchar2, 20).Value = p_ptlokasi;
                _command.Parameters.Add(":p_estate", OracleDbType.Varchar2, 20).Value = p_estate;
                
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new ();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
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

        public IQueryable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int p_periode_int, string p_ptlokasi)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_periode_int", p_periode_int, DbType.Int32);
            parameters.Add("p_ptlokasi", p_ptlokasi, DbType.String);
            var sql1 = string.Empty;
            IEnumerable<JurnalInventoryHeaderDTO> InventoryJurnalHeader;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                sql1 = $"select nomor, tanggal from(" +
                    $"select DISTINCT nomor,tanggal from inv_terima@DATABASE_LINK where periode=:p_periode_int and ptlokasi=:p_ptlokasi " +
                    $"union " +
                    $"select DISTINCT nomor,tanggal from inv_keluar@DATABASE_LINK where periode=:p_periode_int and ptlokasi=:p_ptlokasi " +
                    $") order by nomor,tanggal";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    //KodePerkiraanSaldo = contol.Query<COADaftarPerkiraanSaldoDTO>(sql1, parameters, commandType: CommandType.Text);
                    InventoryJurnalHeader = contol.Query<JurnalInventoryHeaderDTO>(sql1, parameters, commandType: CommandType.Text);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return InventoryJurnalHeader.AsQueryable();
        }

        public IQueryable<JurnalInventoryDetailDTO> GetJurnalDetails_Inventory(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            using (OracleCommand _command = new("ACCT_IMPORT_MODULE.JURNAL_INVENTORY", conn))
            {
                _command.CommandType = CommandType.StoredProcedure;

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

                using OracleDataReader dr = _command.ExecuteReader();
                List<JurnalInventoryDetailDTO> InventoryJurnalDetails = new();

                while (dr.Read())
                {
                    JurnalInventoryDetailDTO jurnalDetail = new()
                    {
                        // Populate the properties of JurnalInventoryDetailDTO from the reader's columns
                        
                        NOJURNAL = dr["NOJURNAL"].ToString(),
                        TANGGAL= Convert.ToDateTime(dr["TANGGAL"]),
                        BARIS= Convert.ToInt16(dr["BARIS"]),
                        KODE= dr["KODE"].ToString(),
                        REKENING = dr["REKENING"].ToString(),
                        DEBET = Convert.ToDouble(dr["DEBET"]),
                        KREDIT = Convert.ToDouble(dr["KREDIT"]),
                        KETERANGAN = dr["KETERANGAN"].ToString(),
                        POSTED = dr["POSTED"].ToString(),
                        PERIODE = dr["PERIODE"].ToString(),
                        IDDATA = dr["IDDATA"].ToString(),
                        USERID = dr["USERID"].ToString(),
                        GLYEAR = Convert.ToInt16(dr["GLYEAR"]),
                        GLMONTH = Convert.ToInt16(dr["GLMONTH"])

                    };

                    InventoryJurnalDetails.Add(jurnalDetail);
                }

                dr.Close();
                conn.Close();

                return InventoryJurnalDetails.AsQueryable();
            }
        }

        public IQueryable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int p_periode_int, string p_ptlokasi)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_periode_int", p_periode_int, DbType.Int16);
            parameters.Add("p_ptlokasi", p_ptlokasi, DbType.String);
            var sql1 = string.Empty;
            IEnumerable<JurnalKasirHeaderDTO> JurnalHeader;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                sql1 = $"SELECT DISTINCT KASNO NOMOR,KASTGL TANGGAL from kasir_jurnal_dtl@DATABASE_LINK  "+
                        "WHERE PERIODE = :p_periode_int AND PTLOKASI = :p_ptlokasi ORDER BY NOMOR,TANGGAL";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    //KodePerkiraanSaldo = contol.Query<COADaftarPerkiraanSaldoDTO>(sql1, parameters, commandType: CommandType.Text);
                    JurnalHeader = contol.Query<JurnalKasirHeaderDTO>(sql1, parameters, commandType: CommandType.Text);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return JurnalHeader.AsQueryable();
        }
    }
}
