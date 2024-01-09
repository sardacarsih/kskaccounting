using Accounting.Model;
using Dapper;
using DevExpress.Data.ODataLinq;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.DataLayer
{
    public class AccountRepository : IAccountRepository
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        public DataTable GetCOA(string piddata, int pbulan, int ptahun)
        {
            using OracleCommand _command = new("ACCOUNTING.COA2", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
            //OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
            //DataSet _ds = new DataSet();
            //_ds.Clear();
            ////Get the data in disconnected mode
            //sqlAdapter.Fill(_ds);
            //// return dataset result
            //return _ds;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public DataSet PeriodeAkuntansi(string piddata,  int ptahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.PERIODELIST", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();                
                OracleDataAdapter sqlAdapter = new OracleDataAdapter();
                sqlcmdbuilder.DataAdapter = sqlAdapter;
                sqlAdapter.SelectCommand = _command;
                DataSet _ds = new DataSet();
                _ds.Clear();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds,"Periode");
                // return dataset result
                return _ds;

            }
        }
        public DataTable GetCOAWithoutSaldo(string piddata)
        {
             using (OracleCommand _command = new OracleCommand("ACCOUNTING.COA", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                    _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                    OracleDataReader dr;
                    dr = _command.ExecuteReader();
                    DataTable _dt = new DataTable();
                    _dt.Load(dr);
                    dr.Close();
                    conn.Close();
                    return _dt;
                }           
        }

        public DataTable GetParentAccount(string piddata, int p_tahun,string ptipe)
        {
               using (OracleCommand _command = new OracleCommand("ACCOUNTING.PARENT_AKUN", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                    _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_TIPEID", OracleDbType.Varchar2,2).Value = ptipe;
                    OracleDataReader dr;
                    dr = _command.ExecuteReader();
                    DataTable _dt = new DataTable();
                    _dt.Load(dr);
                    dr.Close();
                    conn.Close();
                    return _dt;
                }          
        }

        public DataTable GetTipeAkun(string dariform)
        {
           using (OracleCommand _command = new OracleCommand("ACCOUNTING.TIPE_AKUN", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":dariform", OracleDbType.Varchar2, 20).Value = dariform;
                OracleDataReader dr;
                    dr = _command.ExecuteReader();
                    DataTable _dt = new DataTable();
                    _dt.Load(dr);
                    dr.Close();
                    conn.Close();
                    return _dt;
                }
          
        }

        public int GetMinPeriode(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.MINPERIODE", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("MINP", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["MINP"].Value.ToString());
                conn.Close();
                return result;
            }
        }
        public int GetMaxPeriode(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.MAXPERIODE", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("MAXp", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["MAXp"].Value.ToString());                
                conn.Close();
                return result;
            }
        }

        public string GetNamaPeriode(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.NAMAPERIODE", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("l_NAMABULAN", OracleDbType.Varchar2,20).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();

                string result = cmd.Parameters["l_NAMABULAN"].Value.ToString();
                conn.Close();
                return result;
            }
        }
        public int RekalkulasiByNoJurnal(string piddata, int p_bulan, int p_tahun, string p_NoJurnal, string p_Periode, string p_Userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.ReCalcByNoJurnal", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("Record", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_NoJurnal", OracleDbType.Varchar2, 30).Value = p_NoJurnal;
                cmd.Parameters.Add(":p_Periode", OracleDbType.Varchar2, 7).Value = p_Periode;
                cmd.Parameters.Add(":p_Userid", OracleDbType.Varchar2, 20).Value = p_Userid;
                cmd.ExecuteScalar();
                int result = Convert.ToInt32(cmd.Parameters["Record"].Value.ToString());
                conn.Close();
                return result;
            }
        }
        
            public int ReCalcByNoHID(string piddata, int p_bulan, int p_tahun, string p_HID, string p_Periode, string p_Userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.ReCalcByNoHID", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("Record", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_HID", OracleDbType.Varchar2, 40).Value = p_HID;
                cmd.Parameters.Add(":p_Periode", OracleDbType.Varchar2, 7).Value = p_Periode;
                cmd.Parameters.Add(":p_Userid", OracleDbType.Varchar2, 20).Value = p_Userid;
                cmd.ExecuteScalar();
                int result = Convert.ToInt32(cmd.Parameters["Record"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public void RekalkulasiSaldo(string piddata, int p_bulan, int p_tahun, string p_Userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.RecalkulasiSaldoDetail", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_Userid", OracleDbType.Varchar2, 20).Value = p_Userid;
                
                cmd.ExecuteScalar();
                conn.Close();
            }
        }
        public void RekalkulasiSaldoV2(string piddata, int p_bulan, int p_tahun, string p_Userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.RecalkulasiSaldoDetailV2", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_Userid", OracleDbType.Varchar2, 20).Value = p_Userid;

                cmd.ExecuteScalar();
                conn.Close();
            }
        }
        public void CreateNextPeriode(string piddata, int p_bulan, int p_tahun)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.CreateNextPeriode", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public void CreateClosingAcct(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.CreateClosingAcct", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }
        public void ClosingEndYear(string piddata, int p_tahun, string p_userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.ClosingEndYear", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public int CekPeriodeExist(string piddata, int pbulan, int ptahun)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.CekPeriodeExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("ExistPeriode", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["ExistPeriode"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public int ImportCOA(string iddata, int ptahun)
        {
            try
            {

                using (OracleCommand cmd = new OracleCommand("ACCOUNTING.ImportCOA", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = iddata;
                    cmd.Parameters.Add(":ptahun", OracleDbType.Varchar2, 7).Value = ptahun;
                    cmd.ExecuteReader();

                    int result = Convert.ToInt16(cmd.Parameters["ISSUKSES"].Value.ToString());
                    conn.Close();
                    return result;
                }
            }
            catch (Exception)
            {
                conn.Close();
                throw ;
            }
        }

        public int CekCOAExist(string iddata, int ptahun)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.CekCOAExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("Exist", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":iddata", OracleDbType.Varchar2, 200).Value = iddata;
                cmd.Parameters.Add(":ptahun", OracleDbType.Int16).Value = ptahun;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["Exist"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public void ClosingEndYearUpdateOnly(string piddata, int p_tahun, string p_userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.ClosingEndYearUpdateOnly", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public void InsertCOA(string piddata, int p_tahun, string pgrp, string pinduk, char pgd, string pkode, int plvl, 
            char pposisi, string pnama, decimal psawal)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.COAInsert", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":pgrp", OracleDbType.Varchar2, 2).Value = pgrp;
                cmd.Parameters.Add(":pinduk", OracleDbType.Varchar2,30).Value = pinduk;
                cmd.Parameters.Add(":pgd", OracleDbType.Char, 2).Value = pgd;
                cmd.Parameters.Add(":pkode", OracleDbType.Varchar2,30).Value = pkode;
                cmd.Parameters.Add(":plvl", OracleDbType.Int16).Value = plvl;
                cmd.Parameters.Add(":pposisi", OracleDbType.Char,1).Value = pposisi;
                cmd.Parameters.Add(":pnama", OracleDbType.Varchar2,100).Value = pnama;
                cmd.Parameters.Add(":psawal", OracleDbType.Decimal).Value = psawal;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public int CekCountKode(string piddata, int ptahun, string pinduk, string pGD)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.CheckCountAkun", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("Jumlah", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
                cmd.Parameters.Add(":pinduk", OracleDbType.Varchar2, 25).Value = pinduk;
                cmd.Parameters.Add(":pGD", OracleDbType.Varchar2, 25).Value = pGD;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["Jumlah"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public void UpdateSaldoAwalHeader(string piddata, int p_bulan, int p_tahun, string p_userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.ReCalcSaldoAwalHeader", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public DataTable CekParentNotExist()
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.CekParentNotExist", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
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
        }

        public DataTable CekParentNotExist2()
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.CekParentNotExist2", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
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
        }

        public DataTable CekParentNotExist3(string p_iddata, int p_tahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.CekParentNotExist3", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;

                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }
        public int ImportCOAbyMerge(string piddata, int p_tahun)
        {
            try
            {

                using (OracleCommand cmd = new("ACCOUNTING.ImportCOAbyMerge", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = piddata;
                    cmd.Parameters.Add(":ptahun", OracleDbType.Varchar2, 7).Value = p_tahun;
                    cmd.ExecuteReader();

                    int result = Convert.ToInt16(cmd.Parameters["ISSUKSES"].Value.ToString());
                    conn.Close();
                    return result;
                }
            }
            catch (Exception)
            {
                conn.Close();
                throw;
            }
        }

        public int MaxTahunCOA(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.MAXTAHUNCOA", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("MAXYEAR", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["MAXYEAR"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public DataTable CekSalahInduk()
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.CekSalahInduk", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
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
        }

        public int MinTahunCOA(string piddata)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.MINTAHUNCOA", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add("MINYEAR", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.ExecuteScalar();

                int result = Convert.ToInt32(cmd.Parameters["MINYEAR"].Value.ToString());
                conn.Close();
                return result;
            }
        }

        public void UpdateLevelAccount(string piddata, int p_tahun)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.UpdateLevel", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public void ReclassLabaRugi(string piddata, int p_tahun, string p_userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCOUNTING.ReClassLabaRugi", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public IQueryable<COADaftarPerkiraanSaldoDTO> GetPerkiraanSaldo_Dapper(string p_iddata, int p_tahun, int p_bulan)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("p_tahun", p_tahun, DbType.Int32);
            var sql1 = string.Empty;
            IEnumerable<COADaftarPerkiraanSaldoDTO> KodePerkiraanSaldo;
            using (var contol = new OracleConnection(Acct.OracleConnString))
            {
                if (p_bulan == 1)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",SALDOAWAL SALDOAWAL ,\"1D\" DEBET ,\"1K\" KREDIT ,\"1S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 2)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"1S\"  SALDOAWAL ,\"2D\" DEBET ,\"2K\" KREDIT ,\"2S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 3)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"2S\"  SALDOAWAL ,\"3D\" DEBET ,\"3K\" KREDIT ,\"3S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 4)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"3S\"  SALDOAWAL ,\"4D\" DEBET ,\"4K\" KREDIT ,\"4S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 5)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"4S\"  SALDOAWAL ,\"5D\" DEBET ,\"5K\" KREDIT ,\"5S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 6)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"5S\"  SALDOAWAL ,\"6D\" DEBET ,\"6K\" KREDIT ,\"6S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 7)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"6S\"  SALDOAWAL ,\"7D\" DEBET ,\"7K\" KREDIT ,\"7S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 8)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"7S\"  SALDOAWAL ,\"8D\" DEBET ,\"8K\" KREDIT ,\"8S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 9)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"8S\"  SALDOAWAL ,\"9D\" DEBET ,\"9K\" KREDIT ,\"9S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 10)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"9S\"  SALDOAWAL ,\"10D\" DEBET ,\"10K\" KREDIT ,\"10S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 11)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"10S\"  SALDOAWAL ,\"11D\" DEBET ,\"11K\" KREDIT ,\"11S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
                }
                else if (p_bulan == 12)
                {
                    sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                        ",\"11S\"  SALDOAWAL ,\"12D\" DEBET ,\"12K\" KREDIT ,\"12S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";

                }

                if (contol.State == ConnectionState.Closed)
                    contol.Open();
                try
                {
                    KodePerkiraanSaldo = contol.Query<COADaftarPerkiraanSaldoDTO>(sql1, parameters);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (contol.State == ConnectionState.Open)
                        contol.Close();
                }
            }
            return KodePerkiraanSaldo.AsQueryable();
        }

        public DataTable GetPerkiraanSaldo_ADO(string p_iddata, int p_tahun, int p_bulan)
        {
            using OracleCommand _command = new("ACCOUNTING.COA_DAPPER", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("COA", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        public void RekalkulasiByJurnalID(string piddata, int p_bulan, int p_tahun, double p_JurnalID, string p_Periode, string p_Userid)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_RECALLCULATIONS.ReCalcByNoJurnalID", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = piddata;
                cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_HID", OracleDbType.Double).Value = p_JurnalID;
                cmd.Parameters.Add(":p_Periode", OracleDbType.Varchar2, 7).Value = p_Periode;
                cmd.Parameters.Add(":p_Userid", OracleDbType.Varchar2, 20).Value = p_Userid;
                cmd.ExecuteReader();
                conn.Close();
            }
        }

        public IEnumerable<coaHIA> GetPerkiraanSaldo_TreeView(string p_iddata, int p_tahun, int p_bulan)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_iddata", p_iddata, DbType.String);
            parameters.Add("p_tahun", p_tahun, DbType.Int32);
            using (var contol = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KSKGROUP)));User Id=KSKG;Password=KSKGboss2022"))
            {
                    var sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  "+
                        "from acct_coa where iddata=:p_iddata and tahun=:p_tahun order by kodeacc";
                if (contol.State == ConnectionState.Closed)
                    contol.Open();

                return contol.Query<coaHIA>(sql1, parameters);
            }
           
        }

        public bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom)
        {
            if (dataTable.Columns.Count==16 && NamaKolom.Length==13)
            {
                XtraMessageBox.Show($"Akun Kebun tidak dapat digunakan dipusat", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (dataTable.Columns.Count == 13 && NamaKolom.Length == 16)
            {
                XtraMessageBox.Show($"Akun Pusat tidak dapat digunakan diKebun", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

        public DataTable Akun_Agronomy(string p_iddata, int p_tahun, string p_kelompok)
        {
            using OracleCommand _command = new("ACCOUNTING.AGRONOMI_AKUN", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_kelompok", OracleDbType.Varchar2, 20).Value = p_kelompok;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }
    }
}
