using Accounting.BusinessLayer;
using Accounting.Model;
using Dapper;
using DevExpress.Data.ODataLinq;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        private const string KeepBarisTempTableName = "ACCT_JURNAL_KEEP_BARIS_TMP";
        private const string DetailStageTempTableName = "ACCT_JURNAL_DTL_STAGE_TMP";
        private const string JurnalAsyncRecalcClientIdentifier = "JURNAL_ASYNC_RECALC";
        private const int KeepBarisArrayBindBatchSize = 500;
        private const int DetailStageArrayBindBatchSize = 500;

        private sealed class ExistingDetailAuditRow
        {
            public int Baris { get; set; }
            public string Kode { get; set; } = string.Empty;
            public string Rekening { get; set; } = string.Empty;
            public decimal? Debet { get; set; }
            public decimal? Kredit { get; set; }
            public string Keterangan { get; set; } = string.Empty;
        }
        
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

        public int ImportJurnalParsialScoped(string piddata, int p_bulan, int p_tahun, string periode, string userid)
        {
            try
            {
                using OracleCommand cmd = new("ACCT_JURNAL_IMPORT_V2.ImportJurnalParsial", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                cmd.BindByName = true;
                cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                cmd.ExecuteReader();

                int result = Convert.ToInt16(cmd.Parameters["ISSUKSES"].Value.ToString());
                conn.Close();
                return result;
            }
            catch (Exception)
            {
                conn.Close();
                throw;
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

        public DataTable CekAkunMasterScoped(int ptahun, string piddata, string periode, string userid)
        {
            using OracleCommand command = new("ACCT_JURNAL_IMPORT_V2.CekAkunMaster", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            command.BindByName = true;
            command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = piddata;
            command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;

            OracleDataReader dr = command.ExecuteReader();
            DataTable dt = new();
            dt.Load(dr);
            dr.Close();
            conn.Close();
            return dt;
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

        public DataTable CekNoJurnalExistScoped(string piddata, string periode, string userid)
        {
            using OracleCommand command = new("ACCT_JURNAL_IMPORT_V2.CekNoJurnalExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            command.BindByName = true;
            command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = piddata;
            command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;

            OracleDataReader dr = command.ExecuteReader();
            DataTable dt = new();
            dt.Load(dr);
            dr.Close();
            conn.Close();
            return dt;
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

        public bool CekNoJurnalExistExceptJurnalId(string piddata, string nojurnal, string periode, double exceptJurnalId)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            const string sql = @"SELECT COUNT(1)
                FROM ACCT_JURNAL_HDR
                WHERE IDDATA = :p_iddata
                  AND PERIODE = :p_periode
                  AND UPPER(NOJURNAL) = :p_nojurnal
                  AND JURNALID <> :p_except_jurnal_id";

            connection.Open();
            int count = connection.ExecuteScalar<int>(sql, new
            {
                p_iddata = piddata,
                p_periode = periode,
                p_nojurnal = (nojurnal ?? string.Empty).ToUpperInvariant(),
                p_except_jurnal_id = exceptJurnalId
            });

            return count > 0;
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
          AND GLMONTH BETWEEN :P_DARIBULAN AND :P_SAMPAIBULAN
        ORDER BY NOJURNAL ASC, BARIS ASC";

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
                sql1 = @"SELECT JURNALID,HID,NOJURNAL,TANGGAL,NVL(MODIFIED_DATE, CREATED_DATE) AS HeaderVersionUtc
                         FROM ACCT_JURNAL_HDR
                         WHERE IDDATA=:p_iddata and PERIODE=:p_periode
                         ORDER BY nojurnal";

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

            sql.Append(" ORDER BY NOJURNAL ASC, BARIS ASC");
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

            sql.Append(" ORDER BY NOJURNAL ASC, BARIS ASC");
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

        public DataTable CekJurnal_KODENULL_Scoped(string piddata, string periode, string userid)
        {
            using OracleCommand command = new("ACCT_JURNAL_IMPORT_V2.CekJurnalKodeNull", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            command.BindByName = true;
            command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = piddata;
            command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;

            OracleDataReader dr = command.ExecuteReader();
            DataTable dt = new();
            dt.Load(dr);
            dr.Close();
            conn.Close();
            return dt;
        }

        public void DeleteJurnalTmpByScope(string piddata, string periode, string userid)
        {
            using OracleCommand command = new(
                "DELETE FROM ACCT_JURNAL_TMP WHERE IDDATA=:p_iddata AND PERIODE=:p_periode AND USERID=:p_userid",
                conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            command.BindByName = true;
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = piddata;
            command.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = periode;
            command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            command.ExecuteNonQuery();
            conn.Close();
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

        public JurnalPersistResult InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
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

                connection.Execute(masterInsertQuery, masterParameters, transaction);
                double newjurnalid = masterParameters.Get<double>("jurnalid");
                PrepareDetailRows(jurnalHeader, newjurnalid, jurnalDetail);
                UpsertDetailRowsUsingStage(connection, transaction, jurnalHeader, newjurnalid, jurnalDetail, isUpdate: false);
                JurnalRecalcHint recalcHint = BuildInsertRecalcHint(jurnalDetail);

                transaction.Commit();
                return JurnalPersistResult.Success(newjurnalid, recalcHint);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public JurnalPersistResult UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail, DateTime? expectedHeaderVersionUtc)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            bool clientIdentifierSet = false;
            try
            {
                SetClientIdentifier(connection, transaction, JurnalAsyncRecalcClientIdentifier);
                clientIdentifierSet = true;

                // Snapshot old detail rows BEFORE any changes for audit comparison
                List<ExistingDetailAuditRow> oldDetails = connection.Query<ExistingDetailAuditRow>(
                    @"SELECT BARIS AS Baris,
                             KODE AS Kode,
                             REKENING AS Rekening,
                             DEBET AS Debet,
                             KREDIT AS Kredit,
                             KETERANGAN AS Keterangan
                      FROM ACCT_JURNAL_DTL
                      WHERE REFFID=:reffid",
                    new { reffid = oldJurnalId }, transaction).ToList();
                var oldDetailMap = oldDetails.ToDictionary(d => d.Baris);

                var headerUpdateQuery = @"UPDATE ACCT_JURNAL_HDR
                    SET NOJURNAL=:NoJurnal, TANGGAL=:Tanggal, PERIODE=:Periode,
                        SUMBER=:Sumber, USERID=:UserID, HID=:HID, PC=:PC, IP_ADD=:IP_Add, ISRE=:ISRE,
                        MODIFIED_DATE=SYSTIMESTAMP, MODIFIED_BY=:ModifiedBy, MODIFIED_PC=:ModifiedPC, MODIFIED_IP=:ModifiedIP
                    WHERE JURNALID=:JurnalId
                      AND (:ExpectedVersion IS NULL OR NVL(MODIFIED_DATE, CREATED_DATE)=:ExpectedVersion)";
                int updatedHeaderRows = connection.Execute(headerUpdateQuery, new
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
                    JurnalId = oldJurnalId,
                    ExpectedVersion = expectedHeaderVersionUtc
                }, transaction);
                if (updatedHeaderRows == 0)
                {
                    transaction.Rollback();
                    return JurnalPersistResult.Conflict(oldJurnalId);
                }

                PrepareDetailRows(jurnalHeader, oldJurnalId, jurnalDetail);
                int deletedRows = UpsertDetailRowsUsingStage(connection, transaction, jurnalHeader, oldJurnalId, jurnalDetail, isUpdate: true);
                var newBarisSet = new HashSet<int>(jurnalDetail.Select(d => d.BARIS));
                HashSet<string> impactedAccounts = new(StringComparer.OrdinalIgnoreCase);
                int impactedRows = 0;

                // Build detail-level audit rows
                var auditDetailRows = new List<object>();
                int addedCount = 0, modifiedCount = 0;

                foreach (var newDetail in jurnalDetail)
                {
                    if (oldDetailMap.TryGetValue(newDetail.BARIS, out var oldRow))
                    {
                        if (AreDetailValuesEqual(newDetail, oldRow))
                        {
                            continue;
                        }

                        if (HasFinancialImpact(newDetail, oldRow))
                        {
                            impactedRows++;
                            AddImpactedAccount(impactedAccounts, newDetail.Kode);
                            AddImpactedAccount(impactedAccounts, oldRow.Kode);
                        }

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
                            OLD_KODE = oldRow.Kode,
                            OLD_DEBET = oldRow.Debet,
                            OLD_KREDIT = oldRow.Kredit,
                            OLD_KETERANGAN = oldRow.Keterangan
                        });
                        modifiedCount++;
                    }
                    else
                    {
                        if (HasFinancialImpactAdded(newDetail))
                        {
                            impactedRows++;
                            AddImpactedAccount(impactedAccounts, newDetail.Kode);
                        }

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
                            OLD_KODE = (string?)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string?)null
                        });
                        addedCount++;
                    }
                }

                // DELETED — rows that were removed
                foreach (var oldRow in oldDetails)
                {
                    int oldBaris = oldRow.Baris;
                    if (!newBarisSet.Contains(oldBaris))
                    {
                        if (HasFinancialImpactDeleted(oldRow))
                        {
                            impactedRows++;
                            AddImpactedAccount(impactedAccounts, oldRow.Kode);
                        }

                        auditDetailRows.Add(new
                        {
                            CHANGE_TYPE = "DELETED",
                            BARIS = oldBaris,
                            KODE = oldRow.Kode,
                            REKENING = oldRow.Rekening,
                            DEBET = oldRow.Debet,
                            KREDIT = oldRow.Kredit,
                            KETERANGAN = oldRow.Keterangan,
                            OLD_KODE = (string?)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string?)null
                        });
                    }
                }

                double auditId = InsertAuditLog(connection, transaction, oldJurnalId, "UPDATE", jurnalHeader,
                    detailRowsInserted: addedCount, detailRowsUpdated: modifiedCount, detailRowsDeleted: deletedRows);

                InsertAuditDetailRows(connection, transaction, auditId, auditDetailRows);
                JurnalRecalcHint recalcHint = impactedRows > 0
                    ? JurnalRecalcHint.Create(
                        JurnalRecalcScopes.UpdateDelta,
                        impactedRows,
                        impactedAccounts.Count,
                        impactedAccounts)
                    : JurnalRecalcHint.None();

                transaction.Commit();
                return JurnalPersistResult.Success(oldJurnalId, recalcHint);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (clientIdentifierSet)
                {
                    TryClearClientIdentifier(connection, transaction);
                }
            }
        }

        public JurnalPersistResult HapusJurnal(double p_JurnalID)
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
                HashSet<string> impactedAccounts = new(StringComparer.OrdinalIgnoreCase);
                int impactedRows = 0;
                foreach (var detail in details)
                {
                    var oldRow = new ExistingDetailAuditRow
                    {
                        Baris = (int)(decimal)detail.BARIS,
                        Kode = (string?)detail.KODE,
                        Rekening = (string?)detail.REKENING,
                        Debet = (decimal?)detail.DEBET,
                        Kredit = (decimal?)detail.KREDIT,
                        Keterangan = (string?)detail.KETERANGAN
                    };

                    if (!HasFinancialImpactDeleted(oldRow))
                    {
                        continue;
                    }

                    impactedRows++;
                    AddImpactedAccount(impactedAccounts, oldRow.Kode);
                }

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
                    OLD_KODE = (string?)null,
                    OLD_DEBET = (decimal?)null,
                    OLD_KREDIT = (decimal?)null,
                    OLD_KETERANGAN = (string?)null
                }).ToList();
                InsertAuditDetailRows(connection, transaction, auditId, auditDetailRows);

                connection.Execute("DELETE FROM ACCT_JURNAL_HDR WHERE JURNALID=:jid",
                    new { jid = p_JurnalID }, transaction);

                transaction.Commit();
                JurnalRecalcHint recalcHint = impactedRows > 0
                    ? JurnalRecalcHint.Create(
                        JurnalRecalcScopes.DeleteDelta,
                        impactedRows,
                        impactedAccounts.Count,
                        impactedAccounts)
                    : JurnalRecalcHint.None();
                return JurnalPersistResult.Success(p_JurnalID, recalcHint);
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
                            OLD_KODE = (string?)null,
                            OLD_DEBET = (decimal?)null,
                            OLD_KREDIT = (decimal?)null,
                            OLD_KETERANGAN = (string?)null
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

        private static void PrepareDetailRows(JurnalHeaderAdd jurnalHeader, double reffId, List<JurnalDetailAdd> jurnalDetail)
        {
            int glMonth = int.Parse(jurnalHeader.PERIODE[..2]);
            int glYear = int.Parse(jurnalHeader.PERIODE.Substring(3, 4));
            string reffIdToken = reffId.ToString("0", CultureInfo.InvariantCulture);

            foreach (JurnalDetailAdd detailData in jurnalDetail)
            {
                detailData.NoJurnal = jurnalHeader.NOJURNAL;
                detailData.Tanggal = jurnalHeader.TANGGAL;
                detailData.Periode = jurnalHeader.PERIODE;
                detailData.IDDATA = jurnalHeader.IDDATA;
                detailData.USERID = jurnalHeader.USERID;
                detailData.SUMBER = jurnalHeader.SUMBER;
                detailData.GLMONTH = glMonth;
                detailData.GLYEAR = glYear;
                detailData.DID = reffIdToken + detailData.BARIS.ToString(CultureInfo.InvariantCulture);
                detailData.Posted = "True";
                detailData.REFFID = reffId;
                detailData.HIDREFF = jurnalHeader.HID;
            }
        }

        private static JurnalRecalcHint BuildInsertRecalcHint(List<JurnalDetailAdd> jurnalDetail)
        {
            HashSet<string> impactedAccounts = new(StringComparer.OrdinalIgnoreCase);
            int impactedRows = 0;

            foreach (JurnalDetailAdd detail in jurnalDetail)
            {
                if (!HasFinancialImpactAdded(detail))
                {
                    continue;
                }

                impactedRows++;
                AddImpactedAccount(impactedAccounts, detail.Kode);
            }

            return impactedRows > 0
                ? JurnalRecalcHint.Create(
                    JurnalRecalcScopes.InsertDelta,
                    impactedRows,
                    impactedAccounts.Count,
                    impactedAccounts)
                : JurnalRecalcHint.None();
        }

        private static decimal NormalizeAmount(decimal? value) => value ?? 0m;
        private static string NormalizeText(string? value) => (value ?? string.Empty).Trim();

        private static bool AreDetailValuesEqual(JurnalDetailAdd newDetail, ExistingDetailAuditRow oldRow)
        {
            return string.Equals(NormalizeText(newDetail.Kode), NormalizeText(oldRow.Kode), StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizeText(newDetail.Rekening), NormalizeText(oldRow.Rekening), StringComparison.OrdinalIgnoreCase)
                && NormalizeAmount(newDetail.Debet) == NormalizeAmount(oldRow.Debet)
                && NormalizeAmount(newDetail.Kredit) == NormalizeAmount(oldRow.Kredit)
                && string.Equals(NormalizeText(newDetail.Keterangan), NormalizeText(oldRow.Keterangan), StringComparison.Ordinal);
        }

        private static bool HasFinancialImpact(JurnalDetailAdd newDetail, ExistingDetailAuditRow oldRow)
        {
            return !string.Equals(NormalizeText(newDetail.Kode), NormalizeText(oldRow.Kode), StringComparison.OrdinalIgnoreCase)
                || NormalizeAmount(newDetail.Debet) != NormalizeAmount(oldRow.Debet)
                || NormalizeAmount(newDetail.Kredit) != NormalizeAmount(oldRow.Kredit);
        }

        private static bool HasFinancialImpactAdded(JurnalDetailAdd detail)
        {
            return !string.IsNullOrWhiteSpace(detail.Kode)
                && (NormalizeAmount(detail.Debet) != 0m || NormalizeAmount(detail.Kredit) != 0m);
        }

        private static bool HasFinancialImpactDeleted(ExistingDetailAuditRow oldRow)
        {
            return !string.IsNullOrWhiteSpace(oldRow.Kode)
                && (NormalizeAmount(oldRow.Debet) != 0m || NormalizeAmount(oldRow.Kredit) != 0m);
        }

        private static void AddImpactedAccount(HashSet<string> impactedAccounts, string? kode)
        {
            string normalizedKode = NormalizeText(kode);
            if (!string.IsNullOrEmpty(normalizedKode))
            {
                impactedAccounts.Add(normalizedKode);
            }
        }

        private static int UpsertDetailRowsUsingStage(
            OracleConnection connection,
            OracleTransaction transaction,
            JurnalHeaderAdd jurnalHeader,
            double reffId,
            List<JurnalDetailAdd> jurnalDetail,
            bool isUpdate)
        {
            string sessionToken = Guid.NewGuid().ToString("N");
            Stopwatch stopwatch = Stopwatch.StartNew();
            long stageElapsedMs = 0;
            long writeElapsedMs = 0;
            long deleteElapsedMs = 0;

            try
            {
                StageDetailRows(connection, transaction, sessionToken, jurnalDetail);
                stageElapsedMs = stopwatch.ElapsedMilliseconds;
                string reffIdToken = reffId.ToString("0", CultureInfo.InvariantCulture);
                decimal reffIdNumber = Convert.ToDecimal(reffId, CultureInfo.InvariantCulture);

                if (isUpdate)
                {
                    const string mergeSql = @"MERGE INTO ACCT_JURNAL_DTL target
                        USING (
                            SELECT DID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN
                            FROM ACCT_JURNAL_DTL_STAGE_TMP
                            WHERE SESSION_TOKEN = :sessionToken
                        ) source
                        ON (target.DID = source.DID)
                        WHEN MATCHED THEN UPDATE SET
                            NOJURNAL = :nojurnal,
                            TANGGAL = :tanggal,
                            KODE = source.KODE,
                            REKENING = source.REKENING,
                            DEBET = source.DEBET,
                            KREDIT = source.KREDIT,
                            KETERANGAN = source.KETERANGAN,
                            POSTED = :posted,
                            PERIODE = :periode,
                            USERID = :userId,
                            SUMBER = :sumber,
                            GLYEAR = :glYear,
                            GLMONTH = :glMonth,
                            HIDREFF = :hidReff,
                            MODIFIED_DATE = SYSTIMESTAMP
                        WHEN NOT MATCHED THEN INSERT
                            (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN,
                             POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID, CREATED_DATE)
                        VALUES
                            (:nojurnal, :tanggal, source.BARIS, source.KODE, source.REKENING, source.DEBET, source.KREDIT, source.KETERANGAN,
                             :posted, :periode, :idData, :userId, :sumber, source.DID, :glYear, :glMonth, :hidReff, :reffid, SYSTIMESTAMP)";

                    DynamicParameters mergeParameters = new();
                    mergeParameters.Add("sessionToken", sessionToken);
                    mergeParameters.Add("reffid", reffIdNumber, DbType.Decimal);
                    mergeParameters.Add("nojurnal", jurnalHeader.NOJURNAL);
                    mergeParameters.Add("tanggal", jurnalHeader.TANGGAL);
                    mergeParameters.Add("posted", "True");
                    mergeParameters.Add("periode", jurnalHeader.PERIODE);
                    mergeParameters.Add("idData", jurnalHeader.IDDATA);
                    mergeParameters.Add("userId", jurnalHeader.USERID);
                    mergeParameters.Add("sumber", jurnalHeader.SUMBER);
                    mergeParameters.Add("glYear", jurnalDetail.FirstOrDefault()?.GLYEAR ?? int.Parse(jurnalHeader.PERIODE.Substring(3, 4)));
                    mergeParameters.Add("glMonth", jurnalDetail.FirstOrDefault()?.GLMONTH ?? int.Parse(jurnalHeader.PERIODE[..2]));
                    mergeParameters.Add("hidReff", jurnalHeader.HID);
                    int mergedRows = connection.Execute(mergeSql, mergeParameters, transaction);
                    writeElapsedMs = stopwatch.ElapsedMilliseconds - stageElapsedMs;

                    int deletedRows = connection.Execute(
                        @"DELETE FROM ACCT_JURNAL_DTL target
                          WHERE target.REFFID = :reffid
                            AND NOT EXISTS (
                                SELECT 1
                                FROM ACCT_JURNAL_DTL_STAGE_TMP stage
                                WHERE stage.SESSION_TOKEN = :sessionToken
                                  AND stage.DID = target.DID
                            )",
                        new { reffid = reffIdNumber, sessionToken },
                        transaction);
                    deleteElapsedMs = stopwatch.ElapsedMilliseconds - stageElapsedMs - writeElapsedMs;

                    Log.Information(
                        "PERF JurnalRepository.UpsertDetailRowsUsingStage elapsed_ms={ElapsedMs} stage_ms={StageMs} merge_ms={MergeMs} delete_ms={DeleteMs} reffid={ReffId} detail_count={DetailCount} merged_rows={MergedRows} deleted_rows={DeletedRows} mode=update",
                        stopwatch.ElapsedMilliseconds,
                        stageElapsedMs,
                        writeElapsedMs,
                        deleteElapsedMs,
                        reffId,
                        jurnalDetail.Count,
                        mergedRows,
                        deletedRows);
                    return deletedRows;
                }

                const string insertSql = @"INSERT INTO ACCT_JURNAL_DTL
                    (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN,
                     POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID, CREATED_DATE)
                    SELECT
                        :nojurnal, :tanggal, stage.BARIS, stage.KODE, stage.REKENING, stage.DEBET, stage.KREDIT, stage.KETERANGAN,
                        :posted, :periode, :idData, :userId, :sumber, :reffidToken || TO_CHAR(stage.BARIS), :glYear, :glMonth, :hidReff, :reffid, SYSTIMESTAMP
                    FROM ACCT_JURNAL_DTL_STAGE_TMP stage
                    WHERE stage.SESSION_TOKEN = :sessionToken";

                DynamicParameters insertParameters = new();
                insertParameters.Add("sessionToken", sessionToken);
                insertParameters.Add("nojurnal", jurnalHeader.NOJURNAL);
                insertParameters.Add("tanggal", jurnalHeader.TANGGAL);
                insertParameters.Add("posted", "True");
                insertParameters.Add("periode", jurnalHeader.PERIODE);
                insertParameters.Add("idData", jurnalHeader.IDDATA);
                insertParameters.Add("userId", jurnalHeader.USERID);
                insertParameters.Add("sumber", jurnalHeader.SUMBER);
                insertParameters.Add("glYear", jurnalDetail.FirstOrDefault()?.GLYEAR ?? int.Parse(jurnalHeader.PERIODE.Substring(3, 4)));
                insertParameters.Add("glMonth", jurnalDetail.FirstOrDefault()?.GLMONTH ?? int.Parse(jurnalHeader.PERIODE[..2]));
                insertParameters.Add("hidReff", jurnalHeader.HID);
                insertParameters.Add("reffid", reffIdNumber, DbType.Decimal);
                insertParameters.Add("reffidToken", reffIdToken);

                connection.Execute(insertSql, insertParameters, transaction);
                writeElapsedMs = stopwatch.ElapsedMilliseconds - stageElapsedMs;

                Log.Information(
                    "PERF JurnalRepository.UpsertDetailRowsUsingStage elapsed_ms={ElapsedMs} stage_ms={StageMs} insert_ms={InsertMs} reffid={ReffId} detail_count={DetailCount} mode=insert",
                    stopwatch.ElapsedMilliseconds,
                    stageElapsedMs,
                    writeElapsedMs,
                    reffId,
                    jurnalDetail.Count);
                return 0;
            }
            catch (OracleException ex) when (IsDetailStageTempTableUnavailable(ex))
            {
                Log.Warning(ex, "PERF JurnalRepository.UpsertDetailRowsUsingStage fallback_to_legacy reffid={ReffId}", reffId);
                return UpsertDetailRowsLegacy(connection, transaction, jurnalDetail, reffId, isUpdate);
            }
            finally
            {
                try
                {
                    connection.Execute(
                        $"DELETE FROM {DetailStageTempTableName} WHERE SESSION_TOKEN=:sessionToken",
                        new { sessionToken },
                        transaction);
                }
                catch
                {
                    // Best effort cleanup only.
                }
            }
        }

        private static void StageDetailRows(
            OracleConnection connection,
            OracleTransaction transaction,
            string sessionToken,
            List<JurnalDetailAdd> jurnalDetail)
        {
            if (jurnalDetail.Count == 0)
            {
                return;
            }

            List<JurnalDetailAdd> ordered = jurnalDetail.OrderBy(x => x.BARIS).ToList();
            const string insertSql = @"INSERT INTO ACCT_JURNAL_DTL_STAGE_TMP
                (SESSION_TOKEN, DID, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN)
                VALUES (:sessionToken, :did, :baris, :kode, :rekening, :debet, :kredit, :keterangan)";

            for (int offset = 0; offset < ordered.Count; offset += DetailStageArrayBindBatchSize)
            {
                int batchCount = Math.Min(DetailStageArrayBindBatchSize, ordered.Count - offset);
                JurnalDetailAdd[] batch = ordered.Skip(offset).Take(batchCount).ToArray();

                using OracleCommand command = new(insertSql, connection)
                {
                    CommandType = CommandType.Text,
                    BindByName = true,
                    ArrayBindCount = batchCount,
                    Transaction = transaction
                };

                command.Parameters.Add(":sessionToken", OracleDbType.Varchar2, Enumerable.Repeat(sessionToken, batchCount).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":did", OracleDbType.Varchar2, batch.Select(x => x.DID).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":baris", OracleDbType.Int32, batch.Select(x => x.BARIS).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":kode", OracleDbType.Varchar2, batch.Select(x => x.Kode).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":rekening", OracleDbType.Varchar2, batch.Select(x => x.Rekening).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":debet", OracleDbType.Decimal, batch.Select(x => x.Debet).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":kredit", OracleDbType.Decimal, batch.Select(x => x.Kredit).ToArray(), ParameterDirection.Input);
                command.Parameters.Add(":keterangan", OracleDbType.Varchar2, batch.Select(x => x.Keterangan).ToArray(), ParameterDirection.Input);
                command.ExecuteNonQuery();
            }
        }

        private static int UpsertDetailRowsLegacy(
            OracleConnection connection,
            OracleTransaction transaction,
            List<JurnalDetailAdd> jurnalDetail,
            double reffId,
            bool isUpdate)
        {
            if (!isUpdate)
            {
                const string detailInsertQuery = @"INSERT INTO ACCT_JURNAL_DTL
                    (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID)
                    VALUES (:NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT, :KETERANGAN, :POSTED, :PERIODE, :IDDATA, :USERID, :SUMBER, :DID, :GLYEAR, :GLMONTH, :HIDREFF, :REFFID)";
                connection.Execute(detailInsertQuery, jurnalDetail, transaction);
                return 0;
            }

            const string mergeQuery = @"MERGE INTO ACCT_JURNAL_DTL target
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
            HashSet<int> newBarisSet = new(jurnalDetail.Select(d => d.BARIS));
            if (newBarisSet.Count == 0)
            {
                return connection.Execute("DELETE FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid", new { reffid = reffId }, transaction);
            }

            return DeleteObsoleteDetailRowsUsingTempTable(connection, transaction, reffId, newBarisSet);
        }

        private static bool IsDetailStageTempTableUnavailable(OracleException ex)
        {
            string message = ex.Message ?? string.Empty;
            return message.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ORA-00904", StringComparison.OrdinalIgnoreCase);
        }

        private static void SetClientIdentifier(OracleConnection connection, OracleTransaction transaction, string identifier)
        {
            connection.Execute(
                "BEGIN DBMS_SESSION.SET_IDENTIFIER(:identifier); END;",
                new { identifier },
                transaction);
        }

        private static void TryClearClientIdentifier(OracleConnection connection, OracleTransaction transaction)
        {
            try
            {
                connection.Execute("BEGIN DBMS_SESSION.CLEAR_IDENTIFIER; END;", transaction: transaction);
            }
            catch
            {
                // Best effort cleanup only.
            }
        }

        private static double InsertAuditLog(
            OracleConnection connection, OracleTransaction transaction,
            double jurnalId, string actionType, JurnalHeaderAdd? header = null,
            string? userId = null, string? pc = null, string? ip = null,
            string? nojurnal = null, DateTime? tanggal = null, string? periode = null,
            string? sumber = null, string? iddata = null,
            string? changedFields = null, string? deleteReason = null,
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

            List<DynamicParameters> parameters = auditDetailRows
                .Select(row =>
                {
                    DynamicParameters p = new(row);
                    p.Add("AUDIT_ID", auditId);
                    return p;
                })
                .ToList();

            connection.Execute(auditDetailSql, parameters, transaction);
        }

        private static int DeleteObsoleteDetailRowsUsingTempTable(
            OracleConnection connection,
            OracleTransaction transaction,
            double reffId,
            HashSet<int> keepBarisSet)
        {
            string sessionToken = Guid.NewGuid().ToString("N");
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                StageKeepBarisRows(connection, transaction, sessionToken, keepBarisSet);

                const string deleteSql = @"DELETE FROM ACCT_JURNAL_DTL target
                    WHERE target.REFFID = :reffid
                      AND NOT EXISTS (
                          SELECT 1
                          FROM ACCT_JURNAL_KEEP_BARIS_TMP keep
                          WHERE keep.SESSION_TOKEN = :sessionToken
                            AND keep.BARIS = target.BARIS
                      )";

                int deletedRows = connection.Execute(deleteSql, new { reffid = reffId, sessionToken }, transaction);
                Log.Information(
                    "PERF JurnalRepository.DeleteObsoleteRows elapsed_ms={ElapsedMs} reffid={ReffId} keep_rows={KeepRows} deleted_rows={DeletedRows} strategy=GTT",
                    stopwatch.ElapsedMilliseconds,
                    reffId,
                    keepBarisSet.Count,
                    deletedRows);
                return deletedRows;
            }
            catch (OracleException ex) when (IsKeepBarisTempTableUnavailable(ex))
            {
                Log.Warning(ex, "PERF JurnalRepository.DeleteObsoleteRows fallback_to_chunking reffid={ReffId}", reffId);
                return DeleteObsoleteDetailRowsByChunking(connection, transaction, reffId, keepBarisSet);
            }
            finally
            {
                try
                {
                    connection.Execute(
                        $"DELETE FROM {KeepBarisTempTableName} WHERE SESSION_TOKEN=:sessionToken",
                        new { sessionToken },
                        transaction);
                }
                catch
                {
                    // Best effort cleanup only.
                }
            }
        }

        private static void StageKeepBarisRows(
            OracleConnection connection,
            OracleTransaction transaction,
            string sessionToken,
            HashSet<int> keepBarisSet)
        {
            if (keepBarisSet.Count == 0)
            {
                return;
            }

            List<int> barisList = keepBarisSet.OrderBy(x => x).ToList();
            const string insertSql = "INSERT INTO ACCT_JURNAL_KEEP_BARIS_TMP (SESSION_TOKEN, BARIS) VALUES (:sessionToken, :baris)";

            for (int offset = 0; offset < barisList.Count; offset += KeepBarisArrayBindBatchSize)
            {
                int batchCount = Math.Min(KeepBarisArrayBindBatchSize, barisList.Count - offset);
                int[] barisBatch = barisList.Skip(offset).Take(batchCount).ToArray();
                string[] sessionTokenBatch = Enumerable.Repeat(sessionToken, batchCount).ToArray();

                using OracleCommand command = new(insertSql, connection)
                {
                    CommandType = CommandType.Text,
                    BindByName = true,
                    ArrayBindCount = batchCount,
                    Transaction = transaction
                };

                command.Parameters.Add(":sessionToken", OracleDbType.Varchar2, sessionTokenBatch, ParameterDirection.Input);
                command.Parameters.Add(":baris", OracleDbType.Int32, barisBatch, ParameterDirection.Input);
                command.ExecuteNonQuery();
            }
        }

        private static int DeleteObsoleteDetailRowsByChunking(
            OracleConnection connection,
            OracleTransaction transaction,
            double reffId,
            HashSet<int> keepBarisSet)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<int> existingBaris = connection.Query<int>(
                "SELECT BARIS FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid",
                new { reffid = reffId },
                transaction).ToList();

            List<int> deleteBaris = existingBaris.Where(baris => !keepBarisSet.Contains(baris)).ToList();
            if (deleteBaris.Count == 0)
            {
                return 0;
            }

            int deletedRows = 0;
            foreach (int[] chunk in deleteBaris.Chunk(900))
            {
                deletedRows += connection.Execute(
                    "DELETE FROM ACCT_JURNAL_DTL WHERE REFFID=:reffid AND BARIS IN :barisList",
                    new { reffid = reffId, barisList = chunk.ToList() },
                    transaction);
            }

            Log.Information(
                "PERF JurnalRepository.DeleteObsoleteRows elapsed_ms={ElapsedMs} reffid={ReffId} keep_rows={KeepRows} deleted_rows={DeletedRows} strategy=ChunkingFallback",
                stopwatch.ElapsedMilliseconds,
                reffId,
                keepBarisSet.Count,
                deletedRows);
            return deletedRows;
        }

        private static bool IsKeepBarisTempTableUnavailable(OracleException ex)
        {
            string message = ex.Message ?? string.Empty;
            return message.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ORA-00904", StringComparison.OrdinalIgnoreCase);
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

        public List<JurnalAuditSummary> SearchAuditTrail(string iddata, DateTime fromDate, DateTime toDate, string actionType, string userId, string nojurnal)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();

            var sb = new StringBuilder();
            sb.Append("SELECT MIN(JURNALID) AS JURNALID, NOJURNAL, MIN(TANGGAL) AS TANGGAL, PERIODE, COUNT(*) AS JUMLAH_AKSI, MAX(ACTION_DATE) AS LAST_ACTION_DATE, LISTAGG(DISTINCT ACTION_TYPE, ', ') WITHIN GROUP (ORDER BY ACTION_TYPE) AS ACTION_TYPES FROM ACCT_JURNAL_AUDIT WHERE IDDATA=:p_iddata AND ACTION_DATE BETWEEN :p_from AND :p_to");

            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", iddata);
            parameters.Add("p_from", fromDate);
            parameters.Add("p_to", toDate.Date.AddDays(1).AddSeconds(-1));

            if (!string.IsNullOrEmpty(actionType) && actionType != "All")
            {
                sb.Append(" AND ACTION_TYPE=:p_action_type");
                parameters.Add("p_action_type", actionType);
            }
            if (!string.IsNullOrEmpty(userId))
            {
                sb.Append(" AND UPPER(ACTION_BY) LIKE :p_user");
                parameters.Add("p_user", "%" + userId.ToUpper() + "%");
            }
            if (!string.IsNullOrEmpty(nojurnal))
            {
                sb.Append(" AND UPPER(NOJURNAL) LIKE :p_nojurnal");
                parameters.Add("p_nojurnal", "%" + nojurnal.ToUpper() + "%");
            }

            sb.Append(" GROUP BY NOJURNAL, PERIODE ORDER BY MAX(ACTION_DATE) DESC");

            return connection.Query<JurnalAuditSummary>(sb.ToString(), parameters).ToList();
        }

        public List<JurnalAuditLog> GetAuditByJurnal(string nojurnal, string periode, string iddata, DateTime fromDate, DateTime toDate)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();

            const string sql = "SELECT AUDIT_ID, JURNALID, ACTION_TYPE, ACTION_DATE, ACTION_BY, ACTION_PC, ACTION_IP, NOJURNAL, TANGGAL, PERIODE, SUMBER, IDDATA, CHANGED_FIELDS, DELETE_REASON, DETAIL_ROWS_INSERTED, DETAIL_ROWS_UPDATED, DETAIL_ROWS_DELETED FROM ACCT_JURNAL_AUDIT WHERE NOJURNAL=:p_nojurnal AND PERIODE=:p_periode AND IDDATA=:p_iddata AND ACTION_DATE BETWEEN :p_from AND :p_to ORDER BY ACTION_DATE DESC";

            return connection.Query<JurnalAuditLog>(sql, new
            {
                p_nojurnal = nojurnal,
                p_periode = periode,
                p_iddata = iddata,
                p_from = fromDate,
                p_to = toDate.Date.AddDays(1).AddSeconds(-1)
            }).ToList();
        }

        public List<JurnalAuditDetailDTO> GetAuditDetail(double auditId)
        {
            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();

            const string sql = "SELECT AUDIT_DTL_ID, AUDIT_ID, CHANGE_TYPE, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, OLD_KODE, OLD_DEBET, OLD_KREDIT, OLD_KETERANGAN FROM ACCT_JURNAL_AUDIT_DTL WHERE AUDIT_ID=:p_id ORDER BY BARIS";

            return connection.Query<JurnalAuditDetailDTO>(sql, new { p_id = auditId }).ToList();
        }
    }
}

