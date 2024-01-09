using Accounting.Model;
using Dapper;
using DevExpress.Data.ODataLinq;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.DataLayer
{
    public class JurnalRepository : IJurnalRepository
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        
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

        public IQueryable<JurnalDetailDTO> GetJurnalDetails_Dapper(string p_iddata, string p_periode)
        {

            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("p_periode", p_periode, DbType.String);
            var sql1 = string.Empty;
            IEnumerable<JurnalDetailDTO> JurnalDetail;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                sql1 = "SELECT REFFID,HIDREFF,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL WHERE IDDATA=:p_iddata and PERIODE=:p_periode order by nojurnal,baris";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    JurnalDetail = contol.Query<JurnalDetailDTO>(sql1, parameters, commandType: CommandType.Text);
                }
                catch (Exception)
                {
                    throw;
                }
                
            }
            return JurnalDetail.AsQueryable();
        }

        public IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string p_iddata, string p_periode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("p_periode", p_periode, DbType.String);
            var sql1 = string.Empty;
            IEnumerable<JurnalHeaderDTO> JurnalHeader;
            using (var contol = new OracleConnection(Acct.OracleConnString))
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

            IEnumerable<JurnalDetailDTO> SearchJurnal;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                var dynamicParams = new DynamicParameters();

                string sql = "SELECT REFFID,HIDREFF,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL  WHERE 1=1";

                if (p_iddata != null)
                {
                    sql += " AND IDDATA =:p_iddata";
                    dynamicParams.Add("p_iddata", p_iddata, DbType.String);
                }
                if (p_daritahunbulan != 0 && p_sampaitahunbulan != 0)
                {
                    sql += " AND CAST(GLYEAR||TRIM(TO_CHAR(GLMONTH,'00')) AS INT) BETWEEN :p_daritahunbulan AND :p_sampaitahunbulan";
                    dynamicParams.Add("p_daritahunbulan", p_daritahunbulan, DbType.Int32);
                    dynamicParams.Add("p_sampaitahunbulan", p_sampaitahunbulan, DbType.Int32);
                }
                if (!string.IsNullOrEmpty(p_nojurnal))
                {
                    sql += " AND LOWER(NOJURNAL) LIKE '%' || :p_nojurnal || '%'";
                    dynamicParams.Add("p_nojurnal", p_nojurnal, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_tanggal))
                {
                    sql += " AND TANGGAL=:p_tanggal";
                    dynamicParams.Add("p_tanggal", Convert.ToDateTime(p_tanggal), DbType.Date);
                }

                if (!string.IsNullOrEmpty(p_kode))
                {
                    sql += " AND KODE LIKE :p_kode || '%'";
                    dynamicParams.Add("p_kode", p_kode, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_keterangan))
                {
                    sql += " AND LOWER(KETERANGAN) LIKE '%' || :p_keterangan || '%'";
                    dynamicParams.Add("p_keterangan", p_keterangan, DbType.String);
                }
                if (p_jumlah > 0)
                {
                    sql += " AND (DEBET = :p_jumlah OR  KREDIT = :p_jumlah)";
                    dynamicParams.Add("p_jumlah", p_jumlah, DbType.Decimal);
                }
                sql += " order by periode,nojurnal,baris";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    SearchJurnal = contol.Query<JurnalDetailDTO>(sql, dynamicParams);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return SearchJurnal;
        }

        public IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string p_iddata, string p_periode, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah)
        {
            IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                var dynamicParams = new DynamicParameters();

                string sql = "SELECT REFFID,HIDREFF,NOJURNAL,TANGGAL,BARIS ,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL  WHERE 1=1";

                if (p_iddata != null && !string.IsNullOrEmpty(p_periode))
                {
                    sql += " AND IDDATA =:p_iddata";
                    dynamicParams.Add("p_iddata", p_iddata, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_periode) && p_iddata != null)
                {
                    sql += " AND PERIODE =:p_periode";
                    dynamicParams.Add("p_periode", p_periode, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_nojurnal))
                {
                    sql += " AND LOWER(NOJURNAL) LIKE '%' || :p_nojurnal || '%'";
                    dynamicParams.Add("p_nojurnal", p_nojurnal, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_tanggal))
                {
                    sql += " AND TANGGAL=:p_tanggal";
                    dynamicParams.Add("p_tanggal", Convert.ToDateTime(p_tanggal), DbType.Date);
                }

                if (!string.IsNullOrEmpty(p_kode))
                {
                    sql += " AND KODE LIKE :p_kode || '%'";
                    dynamicParams.Add("p_kode", p_kode, DbType.String);
                }
                if (!string.IsNullOrEmpty(p_keterangan))
                {
                    sql += " AND LOWER(KETERANGAN) LIKE '%' || :p_keterangan || '%'";
                    dynamicParams.Add("p_keterangan", p_keterangan, DbType.String);
                }
                if (p_jumlah > 0)
                {
                    sql += " AND (DEBET = :p_jumlah OR  KREDIT = :p_jumlah)";
                    dynamicParams.Add("p_jumlah", p_jumlah, DbType.Decimal);
                }
                sql += " order by nojurnal,baris";

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    SearchJurnal_Bulan = contol.Query<JurnalDetailDTO>(sql, dynamicParams);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return SearchJurnal_Bulan;
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
    }
}
