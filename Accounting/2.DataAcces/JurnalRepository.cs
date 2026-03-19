using Accounting.BusinessLayer;
using Accounting.Model;
using Dapper;
using DevExpress.Data.ODataLinq;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Accounting.DataLayer
{
    public class JurnalRepository : IJurnalRepository
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        private static readonly HashSet<string> OracleTextReservedKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "AND", "OR", "NOT", "NEAR", "WITHIN", "MINUS", "ACCUM", "ABOUT", "BT", "NT", "RT", "SQE"
        };
        private static readonly Regex OracleTextTokenRegex = new("[A-Za-z0-9]+", RegexOptions.Compiled);
        
        public DataTable GetJurnalHeader(string piddata, string periode)
        {
            using OracleCommand _command = new("ACCT_JURNAL.GetJurnalList", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable GetJurnalDetails(string piddata, string periode)
        {
            using OracleCommand _command = new("ACCT_JURNAL.GetJurnalDetails", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable GetJurnalDetailsV2(string piddata, string periode)
        {
            using OracleCommand _command = new("ACCT_JURNAL.GetJurnalDetailsV2", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }
        public string GetLockStatus(string piddata, string periode)
        {
            using OracleCommand cmd = new("ACCT_JURNAL.GetStatusLock", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Parameters.Add("LockStatus", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            cmd.ExecuteReader();

            string result = cmd.Parameters["LockStatus"].Value.ToString();
            conn.Close();
            return result;
        }

        public void HapusJurnal(string p_nomorHID)
        {
            using OracleCommand cmd = new("ACCT_JURNAL.HapusJurnal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Parameters.Add(":p_nomorHID", OracleDbType.Varchar2, 40).Value = p_nomorHID;
            cmd.ExecuteReader();
        }
        public int ImportJurnalGlobal(string piddata, int p_bulan, int p_tahun, string periode)
        {
            try
            {

                using OracleCommand cmd = new("ACCT_JURNAL.ImportJurnalGlobal", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
                cmd.ExecuteReader();

                int result = Convert.ToInt16(cmd.Parameters["ISSUKSES"].Value.ToString());
                conn.Close();
                return result;
            }
            catch (Exception )
            {
                conn.Close();
                throw ;
            }
        }

        public int ImportJurnalParsial(string piddata, int p_bulan, int p_tahun, string periode)
        {
            try
            {

                using OracleCommand cmd = new("ACCT_JURNAL.ImportJurnalParsial", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
                cmd.ExecuteReader();

                int result = Convert.ToInt16(cmd.Parameters["ISSUKSES"].Value.ToString());
                conn.Close();
                return result;
            }
            catch (Exception )
            {
                conn.Close();
                throw ;
            }
        }
        public void SaveUsingOracleBulkCopy(string destTableName, DataTable dt)
        {
            try
            {

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (var bulkCopy = new OracleBulkCopy(conn, OracleBulkCopyOptions.UseInternalTransaction))
                {
                    bulkCopy.DestinationTableName = destTableName;
                    bulkCopy.BulkCopyTimeout = 600;
                    bulkCopy.WriteToServer(dt);
                }
                conn.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable CekAkunMaster(int ptahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.CekAkunMaster", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        public DataTable CekDuplikasiJurnal()
        {
            using OracleCommand _command = new("ACCT_JURNAL.CekDuplikasiJurnal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;

            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable CekNoJurnalExist()
        {
            using OracleCommand _command = new("ACCT_JURNAL.CekNoJurnalExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;

            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public int CekRecordJurnalExist(string piddata, string periode)
        {
            using OracleCommand cmd = new("ACCT_JURNAL.CekRecordJurnalExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Parameters.Add("RecordExist", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            cmd.ExecuteReader();

            int result = Convert.ToInt32(cmd.Parameters["RecordExist"].Value.ToString());
            conn.Close();
            return result;
        }

        public void ImportCOAOracleBulkCopy(string destTableName, DataTable dt, string jenisakunting)
        {
            try
            {

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using (var bulkCopy = new OracleBulkCopy(conn, OracleBulkCopyOptions.UseInternalTransaction))
                {
                    if (jenisakunting == "KEBUN")
                    {
                        bulkCopy.DestinationTableName = destTableName;
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.ColumnMappings.Add("Account", "ACCOUNT");
                        bulkCopy.ColumnMappings.Add("Nama Perkiraan", "PERKIRAAN");
                        bulkCopy.ColumnMappings.Add("Jenis", "JENIS");
                        bulkCopy.ColumnMappings.Add("Level", "LVL");
                        bulkCopy.ColumnMappings.Add("Induk", "INDUK");
                        bulkCopy.ColumnMappings.Add("Gen", "ISHEADER");
                        bulkCopy.ColumnMappings.Add("Saldo Normal", "POSISI");
                        bulkCopy.ColumnMappings.Add("Awal Tahun", "SALDOAWAL");
                        bulkCopy.ColumnMappings.Add("Blok", "BLOK");
                        bulkCopy.ColumnMappings.Add("Divisi", "DIVISI");
                        bulkCopy.ColumnMappings.Add("TahunTanam", "TAHUNTANAM");
                    }
                    else
                    {
                        bulkCopy.DestinationTableName = destTableName;
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.ColumnMappings.Add("Account", "ACCOUNT");
                        bulkCopy.ColumnMappings.Add("Nama Perkiraan", "PERKIRAAN");
                        bulkCopy.ColumnMappings.Add("Jenis", "JENIS");
                        bulkCopy.ColumnMappings.Add("Level", "LVL");
                        bulkCopy.ColumnMappings.Add("Induk", "INDUK");
                        bulkCopy.ColumnMappings.Add("Gen", "ISHEADER");
                        bulkCopy.ColumnMappings.Add("Saldo Normal", "POSISI");
                        bulkCopy.ColumnMappings.Add("Awal Tahun", "SALDOAWAL");
                    }

                    bulkCopy.WriteToServer(dt);
                }
                conn.Close();
            }
            catch (Exception )
            {
                throw;
            }
        }

        public bool CekNoJurnalExist_input(string piddata, string nojurnal, string periode)
        {
            bool result = false;
            using OracleCommand cmd = new("ACCT_JURNAL.CekNoJurnalExist_input", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Parameters.Add("NoJurnalExist", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":nojurnal", OracleDbType.Varchar2, 30).Value = nojurnal;
            cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            cmd.ExecuteReader();

            int ada = Convert.ToInt32(cmd.Parameters["NoJurnalExist"].Value.ToString());
            if (ada == 1)
            {
                result = true;
            }
            conn.Close();
            return result;
        }
        public DataTable EditJurnalDT(string p_nomorHID)
        {
            using OracleCommand _command = new("ACCT_JURNAL.GetJurnalListEdit", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_nomorHID", OracleDbType.Varchar2, 40).Value = p_nomorHID;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataTable PeriodeList(string piddata, string ptahun)
        {
            using (OracleCommand _command = new OracleCommand("select periode from acct_periode where iddata=:piddata and tahun=:ptahun order by periode asc", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 40).Value = piddata;
                _command.Parameters.Add(":ptahun", OracleDbType.Int16).Value = ptahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        public int CekPeriodeExist(string piddata, string p_periode)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_JURNAL.CekPeriodeExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("RecordExist", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = p_periode;
                cmd.ExecuteReader();

                int result = Convert.ToInt32(cmd.Parameters["RecordExist"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public DataSet GetNotaDebet(string piddata, string pperiode, string pkodeacc)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.NOTADEBET", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 30).Value = piddata;
                _command.Parameters.Add(":pperiode", OracleDbType.Varchar2, 30).Value = pperiode;
                _command.Parameters.Add(":pkodeacc", OracleDbType.Varchar2, 30).Value = pkodeacc;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "NotaDebet");
                // return dataset result
                return _ds;
            }
        }

        public DataSet GetNotaKredit(string piddata, string pperiode, string pkodeacc)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.NOTAKREDIT", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 30).Value = piddata;
                _command.Parameters.Add(":pperiode", OracleDbType.Varchar2, 30).Value = pperiode;
                _command.Parameters.Add(":pkodeacc", OracleDbType.Varchar2, 30).Value = pkodeacc;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "NotaKredit");
                // return dataset result
                return _ds;
            }
        }

        public void JurnalRE(string p_iddata, string p_periode, string p_userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_JURNAL.JurnalRE", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                cmd.Parameters.Add(":p_periode", OracleDbType.Char, 7).Value = p_periode;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                cmd.ExecuteReader();
            }
        }

        public IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string p_iddata, int P_TAHUN, int P_DARIBULAN, int P_SAMPAIBULAN)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("P_TAHUN", P_TAHUN, DbType.Int32);
            parameters.Add("P_DARIBULAN", P_DARIBULAN, DbType.Int32);
            parameters.Add("P_SAMPAIBULAN", P_SAMPAIBULAN, DbType.Int32);

            var sql = @"
        SELECT 
            REFFID, HIDREFF, NOJURNAL, TANGGAL, BARIS, 
            KODE, REKENING, DEBET, KREDIT, KETERANGAN, 
            Posted, Periode 
        FROM ACCT_JURNAL_DTL
        WHERE IDDATA = :p_iddata
          AND GLYEAR = :P_TAHUN
          AND GLMONTH BETWEEN :P_DARIBULAN AND :P_SAMPAIBULAN";

            using (var conn = new OracleConnection(LoginInfo.OracleConnString))
            {
                if (conn.State == ConnectionState.Closed)
                    conn.Open();

                var list = conn.Query<JurnalDetailDTO>(sql, parameters).ToList(); // Immediate execution

                return list.AsQueryable(); // Allows further LINQ queries
            }
        }



        public IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string p_iddata, string p_periode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("p_periode", p_periode, DbType.String);
            var sql1 = string.Empty;
            IEnumerable<JurnalHeaderDTO> JurnalHeader;
            using (var contol = new OracleConnection( LoginInfo.OracleConnString))
            {
                sql1 = "SELECT JURNALID,HID,NOJURNAL,TANGGAL FROM ACCT_JURNAL_HDR WHERE IDDATA=:p_iddata and PERIODE=:p_periode order by nojurnal";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    //KodePerkiraanSaldo = contol.Query<COADaftarPerkiraanSaldoDTO>(sql1, parameters, commandType: CommandType.Text);
                    JurnalHeader = contol.Query<JurnalHeaderDTO>(sql1, parameters, commandType: CommandType.Text);
                }
                catch (Exception)
                {
                    throw;
                }
                //finally
                //{
                //    if (contol.State == ConnectionState.Open)
                //        contol.Close();
                //}
            }
            return JurnalHeader.AsQueryable();
        }

        public IEnumerable<JurnalDetailDTO> SearchJurnal(string p_iddata, int p_daritahunbulan, int p_sampaitahunbulan, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah)
        {
            IEnumerable<JurnalDetailDTO> SearchJurnal = Array.Empty<JurnalDetailDTO>();
            using (var contol = new OracleConnection( LoginInfo.OracleConnString))
            {
                if (contol.State == ConnectionState.Closed)
                    contol.Open();

                bool preferOracleText = true;
                while (true)
                {
                    var dynamicParams = new DynamicParameters();
                    string sql = BuildSearchJurnalSql(
                        p_iddata,
                        p_daritahunbulan,
                        p_sampaitahunbulan,
                        p_nojurnal,
                        p_tanggal,
                        p_kode,
                        p_keterangan,
                        p_jumlah,
                        preferOracleText,
                        dynamicParams);

                    try
                    {
                        SearchJurnal = contol.Query<JurnalDetailDTO>(sql, dynamicParams);
                        break;
                    }
                    catch (OracleException ex) when (preferOracleText && IsOracleTextUnavailable(ex))
                    {
                        preferOracleText = false;
                    }
                }
            }
            return SearchJurnal;
        }

        public IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string p_iddata, string p_periode, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah)
        {
            IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan = Array.Empty<JurnalDetailDTO>();
            using (var contol = new OracleConnection( LoginInfo.OracleConnString))
            {
                if (contol.State == ConnectionState.Closed)
                    contol.Open();

                bool preferOracleText = true;
                while (true)
                {
                    var dynamicParams = new DynamicParameters();
                    string sql = BuildSearchJurnalBulanSql(
                        p_iddata,
                        p_periode,
                        p_nojurnal,
                        p_tanggal,
                        p_kode,
                        p_keterangan,
                        p_jumlah,
                        preferOracleText,
                        dynamicParams);

                    try
                    {
                        SearchJurnal_Bulan = contol.Query<JurnalDetailDTO>(sql, dynamicParams);
                        break;
                    }
                    catch (OracleException ex) when (preferOracleText && IsOracleTextUnavailable(ex))
                    {
                        preferOracleText = false;
                    }
                }
            }
            return SearchJurnal_Bulan;
        }

        private static string BuildSearchJurnalSql(
            string p_iddata,
            int p_daritahunbulan,
            int p_sampaitahunbulan,
            string p_nojurnal,
            string p_tanggal,
            string p_kode,
            string p_keterangan,
            decimal p_jumlah,
            bool preferOracleText,
            DynamicParameters dynamicParams)
        {
            StringBuilder sql = new("SELECT REFFID,HIDREFF,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL WHERE 1=1");

            if (p_iddata != null)
            {
                sql.Append(" AND IDDATA =:p_iddata");
                dynamicParams.Add("p_iddata", p_iddata, DbType.String);
            }

            if (p_daritahunbulan != 0 && p_sampaitahunbulan != 0)
            {
                int dariYear = p_daritahunbulan / 100;
                int dariMonth = p_daritahunbulan % 100;
                int sampaiYear = p_sampaitahunbulan / 100;
                int sampaiMonth = p_sampaitahunbulan % 100;

                sql.Append(@" AND (
                                  (GLYEAR > :p_dari_year OR (GLYEAR = :p_dari_year AND GLMONTH >= :p_dari_month))
                              AND (GLYEAR < :p_sampai_year OR (GLYEAR = :p_sampai_year AND GLMONTH <= :p_sampai_month))
                             )");
                dynamicParams.Add("p_dari_year", dariYear, DbType.Int32);
                dynamicParams.Add("p_dari_month", dariMonth, DbType.Int32);
                dynamicParams.Add("p_sampai_year", sampaiYear, DbType.Int32);
                dynamicParams.Add("p_sampai_month", sampaiMonth, DbType.Int32);
            }

            AppendKeywordFilter("NOJURNAL", "p_nojurnal", p_nojurnal, preferOracleText, sql, dynamicParams);

            if (!string.IsNullOrEmpty(p_tanggal))
            {
                sql.Append(" AND TANGGAL=:p_tanggal");
                dynamicParams.Add("p_tanggal", Convert.ToDateTime(p_tanggal), DbType.Date);
            }

            if (!string.IsNullOrEmpty(p_kode))
            {
                sql.Append(" AND KODE LIKE :p_kode || '%'");
                dynamicParams.Add("p_kode", p_kode, DbType.String);
            }

            AppendKeywordFilter("KETERANGAN", "p_keterangan", p_keterangan, preferOracleText, sql, dynamicParams);

            if (p_jumlah > 0)
            {
                sql.Append(" AND (DEBET = :p_jumlah OR  KREDIT = :p_jumlah)");
                dynamicParams.Add("p_jumlah", p_jumlah, DbType.Decimal);
            }

            sql.Append(" ORDER BY PERIODE, NOJURNAL, BARIS");
            return sql.ToString();
        }

        private static string BuildSearchJurnalBulanSql(
            string p_iddata,
            string p_periode,
            string p_nojurnal,
            string p_tanggal,
            string p_kode,
            string p_keterangan,
            decimal p_jumlah,
            bool preferOracleText,
            DynamicParameters dynamicParams)
        {
            StringBuilder sql = new("SELECT REFFID,HIDREFF,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL WHERE 1=1");

            if (p_iddata != null && !string.IsNullOrEmpty(p_periode))
            {
                sql.Append(" AND IDDATA =:p_iddata");
                dynamicParams.Add("p_iddata", p_iddata, DbType.String);
            }

            if (!string.IsNullOrEmpty(p_periode) && p_iddata != null)
            {
                sql.Append(" AND PERIODE =:p_periode");
                dynamicParams.Add("p_periode", p_periode, DbType.String);
            }

            AppendKeywordFilter("NOJURNAL", "p_nojurnal", p_nojurnal, preferOracleText, sql, dynamicParams);

            if (!string.IsNullOrEmpty(p_tanggal))
            {
                sql.Append(" AND TANGGAL=:p_tanggal");
                dynamicParams.Add("p_tanggal", Convert.ToDateTime(p_tanggal), DbType.Date);
            }

            if (!string.IsNullOrEmpty(p_kode))
            {
                sql.Append(" AND KODE LIKE :p_kode || '%'");
                dynamicParams.Add("p_kode", p_kode, DbType.String);
            }

            AppendKeywordFilter("KETERANGAN", "p_keterangan", p_keterangan, preferOracleText, sql, dynamicParams);

            if (p_jumlah > 0)
            {
                sql.Append(" AND (DEBET = :p_jumlah OR  KREDIT = :p_jumlah)");
                dynamicParams.Add("p_jumlah", p_jumlah, DbType.Decimal);
            }

            sql.Append(" ORDER BY NOJURNAL, BARIS");
            return sql.ToString();
        }

        private static void AppendKeywordFilter(
            string columnName,
            string parameterName,
            string keyword,
            bool preferOracleText,
            StringBuilder sql,
            DynamicParameters dynamicParams)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            string normalizedKeyword = keyword.Trim().ToLowerInvariant();
            if (preferOracleText && TryBuildOracleTextQuery(normalizedKeyword, out string textQuery))
            {
                sql.Append(" AND CONTAINS(")
                   .Append(columnName)
                   .Append(", :")
                   .Append(parameterName)
                   .Append("_ctx) > 0");
                dynamicParams.Add(parameterName + "_ctx", textQuery, DbType.String);
                return;
            }

            sql.Append(" AND LOWER(")
               .Append(columnName)
               .Append(") LIKE '%' || :")
               .Append(parameterName)
               .Append(" || '%'");
            dynamicParams.Add(parameterName, normalizedKeyword, DbType.String);
        }

        private static bool TryBuildOracleTextQuery(string keyword, out string textQuery)
        {
            textQuery = string.Empty;
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 3)
            {
                return false;
            }

            string[] tokens = OracleTextTokenRegex.Matches(keyword)
                .Cast<Match>()
                .Select(match => match.Value)
                .Where(token => token.Length >= 3 && !OracleTextReservedKeywords.Contains(token))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToArray();

            if (tokens.Length == 0)
            {
                return false;
            }

            textQuery = string.Join(" AND ", tokens.Select(token => $"%{token}%"));
            return true;
        }

        private static bool IsOracleTextUnavailable(OracleException ex)
        {
            string message = ex.Message ?? string.Empty;
            return message.Contains("DRG-", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ORA-29855", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ORA-29902", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ORA-20000", StringComparison.OrdinalIgnoreCase);
        }

        public bool InsertJurnalDetail(BindingList<JurnalDetailAdd> inputJurnalDetail)
        {
            string sql = @" INSERT INTO ACCT_JURNAL_DTL (BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,POSTED,IDDATA,SUMBER,USERID,DID,REFFID,HIDREFF,NOJURNAL,TANGGAL, PERIODE,GLYEAR,GLMONTH) VALUES
                            (:BARIS,:KODE,:REKENING,:DEBET,:KREDIT,:KETERANGAN,:POSTED,:IDDATA,:SUMBER,:USERID,:DID,:REFFID,:HIDREFF,:NOJURNAL,:TANGGAL, :PERIODE,:GLYEAR,:GLMONTH)";
            
            //var result=conn.Execute("ACCT_JURNAL.NewJurnalDetail", parameters, commandType: CommandType.StoredProcedure);
            conn.Execute(sql, inputJurnalDetail);
            return true;
        }
        
        public bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom)
        {
            // memeriksa jumlah kolom dalam datatable sama dengan jumlah nama kolom yang ditentukan
            if (dataTable.Columns.Count != NamaKolom.Length)
            {
                XtraMessageBox.Show($"Jumlah Kolom Seharusnya: {NamaKolom.Length}, Jumlah Kolom Aktual : {dataTable.Columns.Count}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            // memeriksa setiap nama kolom di datatable sama dengan nama kolom yang ditentukan
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].ColumnName != NamaKolom[i])
                {
                    XtraMessageBox.Show($"Urutan Nama Kolom Seharusnya: {NamaKolom[i]}, Nama Kolom Aktual: {dataTable.Columns[i].ColumnName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

        public DataTable CekJurnal_KODENULL()
        {
            string sql1 = "select  nojurnal,tanggal,KODE from acct_jurnal_tmp WHERE KODE IS NULL order by nojurnAL,TANGGAL";
            using OracleCommand _command = new(sql1, conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public bool CekjURNALRJE(double p_jurnalID)
        {
            using OracleCommand _command = new("SELECT ISRE FROM ACCT_JURNAL_HDR WHERE JURNALID=:p_jurnalID", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            bool jurnal = false;
            string result = string.Empty;

            _command.Parameters.Add(":p_nomorbukti", OracleDbType.Double).Value = p_jurnalID;
            OracleDataReader dr = _command.ExecuteReader();

            while (dr.Read())
            {
                result = dr.GetString(0);
            }
            if (result == "Y")
            {
                jurnal = true;
            }
            conn.Close();
            return jurnal;
        }

        public void InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var masterInsertQuery = @"INSERT INTO ACCT_JURNAL_HDR
                (IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, HID, PC, IP_ADD, ISRE, CREATED_DATE, CREATED_BY)
                VALUES(:IDData, :NoJurnal, :Tanggal, :Periode, :Sumber, :UserID,:HID, :PC, :IP_Add, :ISRE, SYSTIMESTAMP, :CREATED_BY)
                RETURNING JURNALID INTO :jurnalid";

                jurnalHeader.CREATED_BY = jurnalHeader.USERID;
                var masterParameters = new DynamicParameters(jurnalHeader);
                masterParameters.Add("jurnalid", dbType: DbType.Double, direction: ParameterDirection.Output);

                int rowsAffected = connection.Execute(masterInsertQuery, masterParameters, transaction);

                double newjurnalid = masterParameters.Get<double>("jurnalid");

                var detailInsertQuery = @"INSERT INTO ACCT_JURNAL_DTL
             (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID)
             VALUES (:NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT, :KETERANGAN, :POSTED, :PERIODE, :IDDATA, :USERID, :SUMBER, :DID, :GLYEAR, :GLMONTH, :HIDREFF, :REFFID)";

                int glMonth = int.Parse(jurnalHeader.PERIODE[..2]);
                int glYear = int.Parse(jurnalHeader.PERIODE.Substring(3, 4));

                foreach (var detailData in jurnalDetail)
                {
                    detailData.NoJurnal = jurnalHeader.NOJURNAL;
                    detailData.Tanggal = jurnalHeader.TANGGAL;
                    detailData.Periode = jurnalHeader.PERIODE;
                    detailData.IDDATA = jurnalHeader.IDDATA;
                    detailData.USERID = jurnalHeader.USERID;
                    detailData.SUMBER = jurnalHeader.SUMBER;
                    detailData.GLMONTH = glMonth;
                    detailData.GLYEAR = glYear;
                    detailData.DID = newjurnalid.ToString() + detailData.BARIS;
                    detailData.Posted = "True";
                    detailData.REFFID = newjurnalid;
                    detailData.HIDREFF = jurnalHeader.HID;
                }
                connection.Execute(detailInsertQuery, jurnalDetail, transaction);

                transaction.Commit();

                AccountServices.RekalkulasiByJurnalID(jurnalHeader.IDDATA, glMonth, glYear, newjurnalid, jurnalHeader.PERIODE, LoginInfo.userID);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Snapshot old detail rows BEFORE any changes for audit comparison
                var oldDetails = connection.Query<dynamic>(
                    "SELECT BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid",
                    new { reffid = oldJurnalId }, transaction).ToList();
                var oldDetailMap = oldDetails.ToDictionary(d => (int)(decimal)d.BARIS);

                var headerUpdateQuery = @"UPDATE ACCT_JURNAL_HDR
                    SET NOJURNAL=:NoJurnal, TANGGAL=:Tanggal, PERIODE=:Periode,
                        SUMBER=:Sumber, USERID=:UserID, HID=:HID, PC=:PC, IP_ADD=:IP_Add, ISRE=:ISRE,
                        MODIFIED_DATE=SYSTIMESTAMP, MODIFIED_BY=:ModifiedBy, MODIFIED_PC=:ModifiedPC, MODIFIED_IP=:ModifiedIP
                    WHERE JURNALID=:JurnalId";
                connection.Execute(headerUpdateQuery, new
                {
                    NoJurnal = jurnalHeader.NOJURNAL,
                    Tanggal = jurnalHeader.TANGGAL,
                    Periode = jurnalHeader.PERIODE,
                    Sumber = jurnalHeader.SUMBER,
                    UserID = jurnalHeader.USERID,
                    HID = jurnalHeader.HID,
                    PC = jurnalHeader.PC,
                    IP_Add = jurnalHeader.IP_ADD,
                    ISRE = jurnalHeader.ISRE,
                    ModifiedBy = jurnalHeader.USERID,
                    ModifiedPC = jurnalHeader.PC,
                    ModifiedIP = jurnalHeader.IP_ADD,
                    JurnalId = oldJurnalId
                }, transaction);

                int glMonth = int.Parse(jurnalHeader.PERIODE[..2]);
                int glYear = int.Parse(jurnalHeader.PERIODE.Substring(3, 4));

                foreach (var detailData in jurnalDetail)
                {
                    detailData.NoJurnal = jurnalHeader.NOJURNAL;
                    detailData.Tanggal = jurnalHeader.TANGGAL;
                    detailData.Periode = jurnalHeader.PERIODE;
                    detailData.IDDATA = jurnalHeader.IDDATA;
                    detailData.USERID = jurnalHeader.USERID;
                    detailData.SUMBER = jurnalHeader.SUMBER;
                    detailData.GLMONTH = glMonth;
                    detailData.GLYEAR = glYear;
                    detailData.DID = oldJurnalId.ToString() + detailData.BARIS;
                    detailData.Posted = "True";
                    detailData.REFFID = oldJurnalId;
                    detailData.HIDREFF = jurnalHeader.HID;
                }

                var mergeQuery = @"MERGE INTO ACCT_JURNAL_DTL target
                    USING (SELECT :DID as DID, :NOJURNAL as NOJURNAL, :TANGGAL as TANGGAL,
                                  :BARIS as BARIS, :KODE as KODE, :REKENING as REKENING,
                                  :DEBET as DEBET, :KREDIT as KREDIT, :KETERANGAN as KETERANGAN,
                                  :POSTED as POSTED, :PERIODE as PERIODE, :IDDATA as IDDATA,
                                  :USERID as USERID, :SUMBER as SUMBER, :GLYEAR as GLYEAR,
                                  :GLMONTH as GLMONTH, :HIDREFF as HIDREFF, :REFFID as REFFID
                           FROM DUAL) source
                    ON (target.DID = source.DID)
                    WHEN MATCHED THEN UPDATE SET
                        NOJURNAL=source.NOJURNAL, TANGGAL=source.TANGGAL,
                        KODE=source.KODE, REKENING=source.REKENING,
                        DEBET=source.DEBET, KREDIT=source.KREDIT,
                        KETERANGAN=source.KETERANGAN, POSTED=source.POSTED,
                        PERIODE=source.PERIODE, USERID=source.USERID,
                        SUMBER=source.SUMBER, GLYEAR=source.GLYEAR,
                        GLMONTH=source.GLMONTH, HIDREFF=source.HIDREFF,
                        MODIFIED_DATE=SYSTIMESTAMP
                    WHEN NOT MATCHED THEN INSERT
                        (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN,
                         POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID, CREATED_DATE)
                    VALUES (source.NOJURNAL, source.TANGGAL, source.BARIS, source.KODE, source.REKENING,
                            source.DEBET, source.KREDIT, source.KETERANGAN, source.POSTED, source.PERIODE,
                            source.IDDATA, source.USERID, source.SUMBER, source.DID, source.GLYEAR,
                            source.GLMONTH, source.HIDREFF, source.REFFID, SYSTIMESTAMP)";
                connection.Execute(mergeQuery, jurnalDetail, transaction);

                var newBarisSet = new HashSet<int>(jurnalDetail.Select(d => d.BARIS));
                int deletedRows = connection.Execute(
                    "DELETE FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid AND BARIS NOT IN :barisList",
                    new { reffid = oldJurnalId, barisList = newBarisSet.ToList() }, transaction);

                // Build detail-level audit rows
                var auditDetailRows = new List<object>();
                int addedCount = 0, modifiedCount = 0;

                foreach (var newDetail in jurnalDetail)
                {
                    if (oldDetailMap.TryGetValue(newDetail.BARIS, out var oldRow))
                    {
                        // MODIFIED — row existed before
                        auditDetailRows.Add(new
                        {
                            CHANGE_TYPE = "MODIFIED",
                            BARIS = newDetail.BARIS,
                            KODE = newDetail.Kode,
                            REKENING = newDetail.Rekening,
                            DEBET = newDetail.Debet,
                            KREDIT = newDetail.Kredit,
                            KETERANGAN = newDetail.Keterangan,
                            OLD_KODE = (string)oldRow.KODE,
                            OLD_DEBET = (decimal?)oldRow.DEBET,
                            OLD_KREDIT = (decimal?)oldRow.KREDIT,
                            OLD_KETERANGAN = (string)oldRow.KETERANGAN
                        });
                        modifiedCount++;
                    }
                    else
                    {
                        // ADDED — new row
                        auditDetailRows.Add(new
                        {
                            CHANGE_TYPE = "ADDED",
                            BARIS = newDetail.BARIS,
                            KODE = newDetail.Kode,
                            REKENING = newDetail.Rekening,
                            DEBET = newDetail.Debet,
                            KREDIT = newDetail.Kredit,
                            KETERANGAN = newDetail.Keterangan,
                            OLD_KODE = (string)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string)null
                        });
                        addedCount++;
                    }
                }

                // DELETED — rows that were removed
                foreach (var oldRow in oldDetails)
                {
                    int oldBaris = (int)(decimal)oldRow.BARIS;
                    if (!newBarisSet.Contains(oldBaris))
                    {
                        auditDetailRows.Add(new
                        {
                            CHANGE_TYPE = "DELETED",
                            BARIS = oldBaris,
                            KODE = (string)oldRow.KODE,
                            REKENING = (string)oldRow.REKENING,
                            DEBET = (decimal?)oldRow.DEBET,
                            KREDIT = (decimal?)oldRow.KREDIT,
                            KETERANGAN = (string)oldRow.KETERANGAN,
                            OLD_KODE = (string)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string)null
                        });
                    }
                }

                double auditId = InsertAuditLog(connection, transaction, oldJurnalId, "UPDATE", jurnalHeader,
                    detailRowsInserted: addedCount, detailRowsUpdated: modifiedCount, detailRowsDeleted: deletedRows);

                InsertAuditDetailRows(connection, transaction, auditId, auditDetailRows);

                transaction.Commit();

                AccountServices.RekalkulasiByJurnalID(
                    jurnalHeader.IDDATA,
                    glMonth, glYear,
                    oldJurnalId,
                    jurnalHeader.PERIODE,
                    LoginInfo.userID);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void HapusJurnal(double p_JurnalID)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Snapshot header data for audit before deleting
                var header = connection.QueryFirstOrDefault<JurnalHeaderAdd>(
                    "SELECT NOJURNAL, TANGGAL, PERIODE, SUMBER, IDDATA, USERID FROM ACCT_JURNAL_HDR WHERE JURNALID=:jid",
                    new { jid = p_JurnalID }, transaction);

                // Snapshot detail rows before deleting
                var details = connection.Query<dynamic>(
                    "SELECT BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid",
                    new { reffid = p_JurnalID }, transaction).ToList();

                double auditId = InsertAuditLog(connection, transaction, p_JurnalID, "DELETE",
                    header: header,
                    userId: LoginInfo.userID, pc: System.Net.Dns.GetHostName(), ip: ToolsServices.GetLocalIPAddress(),
                    detailRowsDeleted: details.Count);

                var auditDetailRows = details.Select(d => (object)new
                {
                    CHANGE_TYPE = "SNAPSHOT",
                    BARIS = (int)(decimal)d.BARIS,
                    KODE = (string)d.KODE,
                    REKENING = (string)d.REKENING,
                    DEBET = (decimal?)d.DEBET,
                    KREDIT = (decimal?)d.KREDIT,
                    KETERANGAN = (string)d.KETERANGAN,
                    OLD_KODE = (string)null,
                    OLD_DEBET = (decimal?)null,
                    OLD_KREDIT = (decimal?)null,
                    OLD_KETERANGAN = (string)null
                }).ToList();
                InsertAuditDetailRows(connection, transaction, auditId, auditDetailRows);

                connection.Execute("DELETE FROM ACCT_JURNAL_HDR WHERE JURNALID=:jid",
                    new { jid = p_JurnalID }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void HapusJurnalRange(List<double> selectedValues)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Snapshot header data for audit before deleting
                var headers = connection.Query<dynamic>(
                    "SELECT JURNALID, NOJURNAL, TANGGAL, PERIODE, SUMBER, IDDATA FROM ACCT_JURNAL_HDR WHERE JURNALID IN :RecordIds",
                    new { RecordIds = selectedValues }, transaction);

                // Snapshot all detail rows for all journals being deleted
                var allDetails = connection.Query<dynamic>(
                    "SELECT REFFID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN FROM ACCT_JURNAL_DTL WHERE REFFID IN :RecordIds",
                    new { RecordIds = selectedValues }, transaction).ToList();
                var detailsByJurnal = allDetails.GroupBy(d => (double)(decimal)d.REFFID).ToDictionary(g => g.Key, g => g.ToList());

                string pc = System.Net.Dns.GetHostName();
                string ip = ToolsServices.GetLocalIPAddress();
                foreach (var h in headers)
                {
                    double jurnalId = (double)h.JURNALID;
                    detailsByJurnal.TryGetValue(jurnalId, out var details);
                    int detailCount = details?.Count ?? 0;

                    double auditId = InsertAuditLog(connection, transaction, jurnalId, "DELETE",
                        userId: LoginInfo.userID, pc: pc, ip: ip,
                        nojurnal: (string)h.NOJURNAL, tanggal: (DateTime?)h.TANGGAL,
                        periode: (string)h.PERIODE, sumber: (string)h.SUMBER, iddata: (string)h.IDDATA,
                        detailRowsDeleted: detailCount);

                    if (details != null && details.Count > 0)
                    {
                        var auditDetailRows = details.Select(d => (object)new
                        {
                            CHANGE_TYPE = "SNAPSHOT",
                            BARIS = (int)(decimal)d.BARIS,
                            KODE = (string)d.KODE,
                            REKENING = (string)d.REKENING,
                            DEBET = (decimal?)d.DEBET,
                            KREDIT = (decimal?)d.KREDIT,
                            KETERANGAN = (string)d.KETERANGAN,
                            OLD_KODE = (string)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string)null
                        }).ToList();
                        InsertAuditDetailRows(connection, transaction, auditId, auditDetailRows);
                    }
                }

                connection.Execute("DELETE FROM ACCT_JURNAL_HDR WHERE JURNALID IN :RecordIds",
                    new { RecordIds = selectedValues }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static double InsertAuditLog(
            OracleConnection connection, OracleTransaction transaction,
            double jurnalId, string actionType, JurnalHeaderAdd header = null,
            string userId = null, string pc = null, string ip = null,
            string nojurnal = null, DateTime? tanggal = null, string periode = null,
            string sumber = null, string iddata = null,
            string changedFields = null, string deleteReason = null,
            int detailRowsInserted = 0, int detailRowsUpdated = 0, int detailRowsDeleted = 0)
        {
            var auditSql = @"INSERT INTO ACCT_JURNAL_AUDIT
                (JURNALID, ACTION_TYPE, ACTION_DATE, ACTION_BY, ACTION_PC, ACTION_IP,
                 NOJURNAL, TANGGAL, PERIODE, SUMBER, IDDATA,
                 CHANGED_FIELDS, DELETE_REASON,
                 DETAIL_ROWS_INSERTED, DETAIL_ROWS_UPDATED, DETAIL_ROWS_DELETED)
                VALUES
                (:JURNALID, :ACTION_TYPE, SYSTIMESTAMP, :ACTION_BY, :ACTION_PC, :ACTION_IP,
                 :NOJURNAL, :TANGGAL, :PERIODE, :SUMBER, :IDDATA,
                 :CHANGED_FIELDS, :DELETE_REASON,
                 :DETAIL_ROWS_INSERTED, :DETAIL_ROWS_UPDATED, :DETAIL_ROWS_DELETED)
                RETURNING AUDIT_ID INTO :auditId";

            var parameters = new DynamicParameters();
            parameters.Add("JURNALID", jurnalId);
            parameters.Add("ACTION_TYPE", actionType);
            parameters.Add("ACTION_BY", header?.USERID ?? userId);
            parameters.Add("ACTION_PC", header?.PC ?? pc);
            parameters.Add("ACTION_IP", header?.IP_ADD ?? ip);
            parameters.Add("NOJURNAL", header?.NOJURNAL ?? nojurnal);
            parameters.Add("TANGGAL", (object)header?.TANGGAL ?? tanggal);
            parameters.Add("PERIODE", header?.PERIODE ?? periode);
            parameters.Add("SUMBER", header?.SUMBER ?? sumber);
            parameters.Add("IDDATA", header?.IDDATA ?? iddata);
            parameters.Add("CHANGED_FIELDS", changedFields);
            parameters.Add("DELETE_REASON", deleteReason);
            parameters.Add("DETAIL_ROWS_INSERTED", detailRowsInserted);
            parameters.Add("DETAIL_ROWS_UPDATED", detailRowsUpdated);
            parameters.Add("DETAIL_ROWS_DELETED", detailRowsDeleted);
            parameters.Add("auditId", dbType: DbType.Double, direction: ParameterDirection.Output);

            connection.Execute(auditSql, parameters, transaction);
            return parameters.Get<double>("auditId");
        }

        private static void InsertAuditDetailRows(
            OracleConnection connection, OracleTransaction transaction,
            double auditId, List<object> auditDetailRows)
        {
            if (auditDetailRows == null || auditDetailRows.Count == 0) return;

            var auditDetailSql = @"INSERT INTO ACCT_JURNAL_AUDIT_DTL
                (AUDIT_ID, CHANGE_TYPE, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN,
                 OLD_KODE, OLD_DEBET, OLD_KREDIT, OLD_KETERANGAN)
                VALUES
                (:AUDIT_ID, :CHANGE_TYPE, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT, :KETERANGAN,
                 :OLD_KODE, :OLD_DEBET, :OLD_KREDIT, :OLD_KETERANGAN)";

            foreach (var row in auditDetailRows)
            {
                var p = new DynamicParameters(row);
                p.Add("AUDIT_ID", auditId);
                connection.Execute(auditDetailSql, p, transaction);
            }
        }

        public void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DragDropEventArgs e)
        {
            if (e.Action == DragDropActions.None || targetGrid != sourceGrid)
                return;

            BindingList<JurnalDetailAdd> sourceTable = sourceGrid.GridControl.DataSource as BindingList<JurnalDetailAdd>;

            Point hitPoint = targetGrid.GridControl.PointToClient(Cursor.Position);
            GridHitInfo hitInfo = targetGrid.CalcHitInfo(hitPoint);

            int[] sourceHandles = e.GetData<int[]>();

            int targetRowHandle = hitInfo.RowHandle;
            int targetRowIndex = targetGrid.GetDataSourceRowIndex(targetRowHandle);

            List<JurnalDetailAdd> draggedRows = new List<JurnalDetailAdd>();
            foreach (int sourceHandle in sourceHandles)
            {
                int oldRowIndex = sourceGrid.GetDataSourceRowIndex(sourceHandle);
                JurnalDetailAdd oldRow = sourceTable[oldRowIndex];
                draggedRows.Add(oldRow);
            }

            int newRowIndex;

            switch (e.InsertType)
            {
                case InsertType.Before:
                    newRowIndex = targetRowIndex > sourceHandles[sourceHandles.Length - 1] ? targetRowIndex - 1 : targetRowIndex;
                    for (int i = draggedRows.Count - 1; i >= 0; i--)
                    {
                        JurnalDetailAdd oldRow = draggedRows[i];
                        JurnalDetailAdd newRow = new JurnalDetailAdd()
                        {
                            Kode = oldRow.Kode,
                            Rekening = oldRow.Rekening,
                            Debet = oldRow.Debet,
                            Kredit = oldRow.Kredit,
                            Keterangan = oldRow.Keterangan
                        };
                        sourceTable.Remove(oldRow);
                        sourceTable.Insert(newRowIndex, newRow);
                    }
                    break;
                case InsertType.After:
                    newRowIndex = targetRowIndex < sourceHandles[0] ? targetRowIndex + 1 : targetRowIndex;
                    for (int i = 0; i < draggedRows.Count; i++)
                    {
                        JurnalDetailAdd oldRow = draggedRows[i];
                        JurnalDetailAdd newRow = new JurnalDetailAdd()
                        {
                            Kode = oldRow.Kode,
                            Rekening = oldRow.Rekening,
                            Debet = oldRow.Debet,
                            Kredit = oldRow.Kredit,
                            Keterangan = oldRow.Keterangan
                        };
                        sourceTable.Remove(oldRow);
                        sourceTable.Insert(newRowIndex, newRow);
                    }
                    break;
                default:
                    newRowIndex = -1;
                    break;
            }

            int insertedIndex = targetGrid.GetRowHandle(newRowIndex);
            targetGrid.FocusedRowHandle = insertedIndex;
            targetGrid.SelectRow(targetGrid.FocusedRowHandle);
        }

        public List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            string sql1 = "select kodeacc as KODE, namaacc as PERKIRAAN from acct_coa where iddata=:p_iddata and tahun=:p_tahun AND ISHEADER = 'D' AND isAKTIF <>'T' order by kodeacc asc";

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var parameters = new { p_iddata = piddata, p_tahun = ptahun };

            List<DTOCOAAktif> resultList = connection.Query<DTOCOAAktif>(sql1, parameters).ToList();

            connection.Close();

            return resultList;
        }
    }
}
