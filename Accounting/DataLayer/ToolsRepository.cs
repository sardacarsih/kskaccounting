using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Net.NetworkInformation;

namespace Accounting.DataLayer
{
    public class ToolsRepository : IToolsRepository
    {
        // Stateless: every method opens and disposes its own connection per-call.

        public DataTable Analisa_kesalahan_COA(string p_IDDATA, int p_tahun)
        {
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new("ACCT_TOOLS.Analisa_kesalahan_COA", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
        }

        public int CekAkunBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok)
        {
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new("ACCT_TOOLS.CekAkunBlok", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("iada", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;
            _command.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
            _command.ExecuteScalar();
            return Convert.ToInt32(_command.Parameters["iada"].Value.ToString());
        }

        public void GenerateAkunTBM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam, string p_mode)
        {
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand cmd = new("ACCT_TOOLS.GenerateAkunTBM", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
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
        }

        public void GenerateAkunTM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam)
        {
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand cmd = new("ACCT_TOOLS.GenerateAkunTM", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 20).Value = p_status;
            cmd.Parameters.Add(":p_divisiID", OracleDbType.Char, 2).Value = p_divisiID;
            cmd.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 50).Value = p_divisi;
            cmd.Parameters.Add(":p_blokid", OracleDbType.Char, 3).Value = p_blokid;
            cmd.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
            cmd.Parameters.Add(":p_ttanam", OracleDbType.Char, 4).Value = p_ttanam;
            cmd.ExecuteScalar();
        }

        public DataTable GetAksesLocations(string p_userid)
        {
            string query = "SELECT MAD.IDDATA,MPH.NAMAPT,MPD.WILAYAH FROM MASTER_APPS_DETAIL MAD " +
                           "JOIN MASTER_PT_DTL MPD ON MPD.IDDATA = MAD.IDDATA " +
                           "JOIN MASTER_PT_HDR MPH ON MPH.IDPT = MPD.IDPT " +
                           "WHERE MAD.APPID = 'GL' and  MAD.USERID=:p_userid";
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new(query, conn)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add("p_userid", OracleDbType.Varchar2, 20).Value = p_userid;
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
        }

        public DataTable GetBlokList(string p_IDDATA)
        {
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new("ACCT_TOOLS.GetBlokList", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
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
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new("ACCT_TOOLS.GetPerkiraanBlokList", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_divisi", OracleDbType.Varchar2, 20).Value = p_divisi;
            _command.Parameters.Add(":p_blok", OracleDbType.Varchar2, 20).Value = p_blok;
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
        }

        public DataTable UserLogin()
        {
            string query = "select DISTINCT ml.*,mll.nama ROLE from master_login ml " +
                          "join master_apps_detail mad on mad.userid = ml.userid " +
                          "join master_login_level mll on mll.levelid=ml.levelid where mad.appid = 'GL'";

            using OracleConnection conn = new(LoginInfo.OracleConnString);
            conn.Open();
            using OracleCommand _command = new(query, conn)
            {
                CommandType = CommandType.Text
            };
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
        }
    }
}
