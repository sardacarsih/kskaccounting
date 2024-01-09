using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public class ToolsRepository : IToolsRepository
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);

        public DataTable Analisa_kesalahan_COA(string p_IDDATA, int p_tahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_TOOLS.Analisa_kesalahan_COA", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
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

        public int CekAkunBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_TOOLS.CekAkunBlok", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("iada", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;
                _command.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
                _command.ExecuteScalar();

                int result = Convert.ToInt32(_command.Parameters["iada"].Value.ToString());
                conn.Close();
                return result;
            }
        }


        public void GenerateAkunTBM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam, string p_mode)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_TOOLS.GenerateAkunTBM", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 20).Value = p_status;
                cmd.Parameters.Add(":p_divisiID", OracleDbType.Char, 2).Value = p_divisiID;
                cmd.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 50).Value = p_divisi;
                cmd.Parameters.Add(":p_blokid", OracleDbType.Char, 3).Value = p_blokid;
                cmd.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
                cmd.Parameters.Add(":p_ttanam", OracleDbType.Char, 4).Value = p_ttanam;
                cmd.Parameters.Add(":p_mode", OracleDbType.Varchar2, 20).Value = p_mode;
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public void GenerateAkunTM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam)
        {
            using (OracleCommand cmd = new OracleCommand("ACCT_TOOLS.GenerateAkunTM", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 20).Value = p_status;
                cmd.Parameters.Add(":p_divisiID", OracleDbType.Char, 2).Value = p_divisiID;
                cmd.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 50).Value = p_divisi;
                cmd.Parameters.Add(":p_blokid", OracleDbType.Char, 3).Value = p_blokid;
                cmd.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
                cmd.Parameters.Add(":p_ttanam", OracleDbType.Char, 4).Value = p_ttanam;
                
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        public DataTable GetAksesLocations(string p_userid)
        {
            string query = "SELECT MAD.IDDATA,MPH.NAMAPT,MPD.WILAYAH FROM MASTER_APPS_DETAIL MAD " +
                           "JOIN MASTER_PT_DTL MPD ON MPD.IDDATA = MAD.IDDATA " +
                           "JOIN MASTER_PT_HDR MPH ON MPH.IDPT = MPD.IDPT " +
                           "WHERE MAD.APPID = 'GL' and  MAD.USERID=:p_userid";
            using (OracleCommand _command = new OracleCommand(query, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("p_userid", OracleDbType.Varchar2,20).Value = p_userid;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }


        public DataTable GetBlokList(string p_IDDATA)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_TOOLS.GetBlokList", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        public string GetLocalIPAddress()
        {
            string ipAddress = string.Empty;

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipAddress = ip.Address.ToString();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ipAddress))
                    break;
            }

            return ipAddress;
        }

        public DataTable LoadPerkiraanBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_TOOLS.GetPerkiraanBlokList", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;
                _command.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;

                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }


        public DataTable UserLogin()
        {

            string query = "select DISTINCT ml.*,mll.nama ROLE from master_login ml " +
                          "join master_apps_detail mad on mad.userid = ml.userid " +
                          "join master_login_level mll on mll.levelid=ml.levelid where mad.appid = 'GL'";

            using (OracleCommand _command = new OracleCommand(query, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                //_command.Parameters.Add("p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }
    }
}
