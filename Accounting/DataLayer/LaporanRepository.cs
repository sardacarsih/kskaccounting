using Accounting.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Accounting.DataLayer
{
    public class LaporanRepository : ILaporanRepository
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);

        public Decimal Generate_LabaRugi(string piddata, int pbulan, int ptahun, string userid, string jenisakunting)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_LAPORAN.LAP_LABARUGI", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {                    
                    conn.Open();
                }
                //conn.Open();
                cmd.Parameters.Add("LabaRugi", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                cmd.Parameters.Add(":jenisakunting", OracleDbType.Varchar2, 20).Value = jenisakunting;
                cmd.ExecuteNonQuery();
                Decimal result = Convert.ToDecimal(cmd.Parameters["LabaRugi"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public decimal Generate_Jurnal_Closing(string piddata, int pbulan, int ptahun, string userid, string jenisakunting)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_JURNAL.JURNAL_CLOSING", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                //conn.Open();
                cmd.Parameters.Add("LabaRugi", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                cmd.Parameters.Add(":jenisakunting", OracleDbType.Varchar2, 20).Value = jenisakunting;
                cmd.ExecuteNonQuery();
                Decimal result = Convert.ToDecimal(cmd.Parameters["LabaRugi"].Value.ToString());
                conn.Close();
                return result;
            }
        }
        public int GenerateSub_LabaRugi(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string lap, string posisi)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_LAPORAN.ACC_GENREP_LRNR_SUB", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("SubLabaRugi", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_kodeacc", OracleDbType.Varchar2, 30).Value = p_kodeacc;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                cmd.Parameters.Add(":lap", OracleDbType.Varchar2, 20).Value = lap;
                cmd.Parameters.Add(":posisi", OracleDbType.Varchar2, 20).Value = posisi;
                cmd.ExecuteNonQuery();
                int result = Convert.ToInt32(cmd.Parameters["SubLabaRugi"].Value.ToString());
                conn.Close();
                return result;
            }
        }
        public DataSet ViewLap_LabaRugi(string piddata, string userid)
        {
            using (OracleCommand _command = new OracleCommand("select * from ACC_TMPLRNR where iddata=:p_IDDATA and usergen=:p_userid", conn)
            {
                CommandType = CommandType.Text
            })
            {
                //conn.Open();                
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "LabaRugi");
                // return dataset result
                return _ds;

            }
        }

        public DataSet ViewLap_Neraca(string piddata, int p_bulan, int p_tahun, string userid)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAPORAN.LAP_NERACA", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Neraca");
                // return dataset result
                return _ds;
            }
        }

        public DataSet ViewSub_LabaRugi(string piddata, string userid)
        {
            using (OracleCommand _command = new OracleCommand("select * from ACC_SUB_REPORT where iddata=:p_IDDATA and genuser=:p_userid ", conn)
            {
                CommandType = CommandType.Text
            })
            {
                //conn.Open();                
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "SubLabaRugi");
                // return dataset result
                return _ds;

            }
        }

        public DataSet View_Jurnal(string piddata, string periode, string kode)
        {
            using (OracleCommand _command = new OracleCommand("select * from acct_jurnal_dtl where iddata=:iddata and periode=:periode and kode=:kode", conn)
            {
                CommandType = CommandType.Text
            })
            {
                //conn.Open();                
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":periode", OracleDbType.Varchar2, 20).Value = periode;
                _command.Parameters.Add(":kode", OracleDbType.Varchar2, 20).Value = kode;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Jurnal");
                // return dataset result
                return _ds;

            }
        }

        public DataSet ViewLap_NeracaHalfYear(string piddata, int p_tahun, string userid, int ishalf)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAPORAN.LAP_NERACA_HALFYEAR", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                _command.Parameters.Add(":ishalf", OracleDbType.Int16).Value = ishalf;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Neraca");
                // return dataset result
                return _ds;
            }
        }

        public DataTable ViewLap_NeracaKonsolidasi(int p_tahun, string p_pt,int p_bulan, string userid)
        {
            using (OracleCommand _command = new ("ACCT_LAPORAN.LAP_NERACA_KONSOLIDASI", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_pt", OracleDbType.Varchar2, 20).Value = p_pt;
                _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;               
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new ();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        public DataSet ViewLap_BukuBesar(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            //using (OracleCommand _command = new OracleCommand("ACCT_LAPORAN.LAP_GENERAL_LEDGER", conn)
            using (OracleCommand _command = new OracleCommand("ACCT_LAPORAN.LAP_DYNAMIC_GL", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("BukuBesar", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":P_IDDATA", OracleDbType.Varchar2, 20).Value = P_IDDATA;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                _command.Parameters.Add(":p_sampaibulan", OracleDbType.Int16).Value = p_sampaibulan;
                _command.Parameters.Add(":DARIKODE", OracleDbType.Varchar2, 20).Value = DARIKODE;
                _command.Parameters.Add(":SAMPAIKODE", OracleDbType.Varchar2, 20).Value = SAMPAIKODE;
                _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_Userid;
                _command.Parameters.Add(":DARILAPORAN", OracleDbType.Varchar2, 20).Value = DARILAPORAN;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "BukuBesar");
                // return dataset result
                return _ds;
            }
        }

        public DataSet ViewLap_BukuBesarMultiTahun(string P_IDDATA, int p_tahundari, int p_tahunsampai, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_DYNAMIC_GL_Multi_YEAR", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            //conn.Open();                
            _command.Parameters.Add("BukuBesar", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":P_IDDATA", OracleDbType.Varchar2, 20).Value = P_IDDATA;
            _command.Parameters.Add(":p_tahundari", OracleDbType.Int16).Value = p_tahundari;
            _command.Parameters.Add(":p_tahunsampai", OracleDbType.Int16).Value = p_tahunsampai;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_sampaibulan", OracleDbType.Int16).Value = p_sampaibulan;
            _command.Parameters.Add(":DARIKODE", OracleDbType.Varchar2, 20).Value = DARIKODE;
            _command.Parameters.Add(":SAMPAIKODE", OracleDbType.Varchar2, 20).Value = SAMPAIKODE;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_Userid;
            _command.Parameters.Add(":DARILAPORAN", OracleDbType.Varchar2, 20).Value = DARILAPORAN;
            OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
            DataSet _ds = new DataSet();
            //Get the data in disconnected mode
            sqlAdapter.Fill(_ds, "BukuBesar");
            // return dataset result
            return _ds;
        }


        public DataSet ViewLap_NeracaLajur(string piddata, int p_bulan, int p_tahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAPORAN.LAP_NERACA_LAJUR", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Neraca");
                // return dataset result
                return _ds;
            }
        }

        public decimal Balanced_Check(string piddata, int pbulan, int ptahun)
        {
            using OracleCommand cmd = new("ACCT_LAPORAN.BALANCED_CHECK", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            //conn.Open();
            cmd.Parameters.Add("Selisih", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
            cmd.ExecuteNonQuery();
            Decimal result = Convert.ToDecimal(cmd.Parameters["Selisih"].Value.ToString());
            conn.Close();
            return result;
        }

        public List<AccountSummary> NeracaSaldoTahun(string piddata, int p_tahun)
        {
            List<AccountSummary> accountDataList = new();

            using (OracleConnection connection = new(Acct.OracleConnString))
            {
                connection.Open();

                string sqlQuery = @"
                SELECT KODEACC, NAMAACC, LVL, SALDOAWAL, 
                ""1S"" AS JAN, ""2S"" AS FEB, ""3S"" AS MAR, ""4S"" AS APR, 
                ""5S"" AS MAY, ""6S"" AS JUN, ""7S"" AS JUL, ""8S"" AS AUG, 
                ""9S"" AS SEP, ""10S"" AS OCT, ""11S"" AS NOV, ""12S"" AS DEC
                FROM ACCT_COA
                WHERE IDDATA = :piddata AND TAHUN = :p_tahun
                ORDER BY KODEACC";

                using OracleCommand command = new(sqlQuery, connection);
                command.Parameters.Add(new OracleParameter(":piddata", OracleDbType.Varchar2)).Value = piddata;
                command.Parameters.Add(new OracleParameter(":p_tahun", OracleDbType.Int32)).Value = p_tahun;

                OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    AccountSummary accountData = new()
                    {
                        KODEACC = reader["KODEACC"].ToString(),
                        NAMAACC = reader["NAMAACC"].ToString(),
                        LVL = Convert.ToInt32(reader["LVL"]),
                        SALDOAWAL = Convert.ToDecimal(reader["SALDOAWAL"]),
                        JAN = Convert.ToDecimal(reader["JAN"]),
                        FEB = Convert.ToDecimal(reader["FEB"]),
                        MAR = Convert.ToDecimal(reader["MAR"]),
                        APR = Convert.ToDecimal(reader["APR"]),
                        MAY = Convert.ToDecimal(reader["MAY"]),
                        JUN = Convert.ToDecimal(reader["JUN"]),
                        JUL = Convert.ToDecimal(reader["JUL"]),
                        AUG = Convert.ToDecimal(reader["AUG"]),
                        SEP = Convert.ToDecimal(reader["SEP"]),
                        OCT = Convert.ToDecimal(reader["OCT"]),
                        NOV = Convert.ToDecimal(reader["NOV"]),
                        DEC = Convert.ToDecimal(reader["DEC"])
                    };
                    accountDataList.Add(accountData);
                }
            }
            return accountDataList;
        }
    }
    
}
