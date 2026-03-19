
using DevExpress.XtraEditors;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.DBConnection
{
    public static class ConnectionManager
    {
        //public static IDbConnection GetOracleConnection()
        public static string GetOracleConnection()
        {
            string OracleConnString = string.Empty;
            var settings = Properties.Settings.Default;
            if (!String.IsNullOrEmpty(settings.Server))
            {
                var DATA_server = settings.Server;
                if (DATA_server == "LOCAL_KSKGROUP")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KSKGROUP)));User Id=KSKG;Password=KSKGboss2022";
                }
                else if (DATA_server == "LOCAL_FSLGROUP")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FSLGROUP)));User Id=FSLG;Password=FSLGboss2022";
                }
                else if (DATA_server == "LOCAL_FBM") //FAJAR BALAI MANDIRI, LIEM POE MONG, user SYS pass Pontianak2022
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KSKKEBUN)));User Id=FBM;Password=FBMboss";
                }
                else if (DATA_server == "LOCAL_KSKKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KEBUN)));User Id=KSK;Password=KSKboss";
                }
                else if (DATA_server == "LOCAL_MSLKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KEBUN)));User Id=MSL;Password=MSLboss";
                }
                else if (DATA_server == "LOCAL_FSLKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KEBUN)));User Id=FSL;Password=FSLboss";
                }

                else if (DATA_server == "KSKGROUP")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.10.10.41)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KSKGROUP)));User Id=KSKG;Password=KSKGboss2022";
                }
                else if (DATA_server == "FSLGROUP")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.10.10.41)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FSLGROUP)));User Id=FSLG;Password=FSLGboss2022";
                }
                else if (DATA_server == "PAJAK")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.10.10.41)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PAJAK)));User Id=PAJAK;Password=PAJAKboss2022";
                }
                else if (DATA_server == "FBM") //FAJAR BALAI MANDIRI, LIEM POE MONG, user SYS pass Pontianak2022
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=LOCALHOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=FBM;Password=FBMboss";
                }
                else if (DATA_server == "PKS_FSL") 
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.10)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=FSL;Password=FSLboss";
                }
                else if (DATA_server == "KSKKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=200.208.100.201)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=KSKKEBUN)));User Id=KSK;Password=KSKboss";
                }
                else if (DATA_server == "MSLKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=200.208.100.201)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=MSLKEBUN)));User Id=MSL;Password=MSLboss";
                }
                else if (DATA_server == "FSLKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=10.10.20.13)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FSLKEBUN)));User Id=FSL;Password=FSLboss";
                }
                else if (DATA_server == "FAKKEBUN")
                {
                    OracleConnString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));User Id=FAK;Password=FAKboss";
                }
                else
                {
                    XtraMessageBox.Show("Koneksi ke server gagal", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            }
            else
            {
                XtraMessageBox.Show("Setting Koneksi Server tidak ditemukan", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            //var conn = new OracleConnection(OracleConnString);
            //if (conn.State == ConnectionState.Closed)
            //{
            //    conn.Open();
            //}
            //return conn;
            Acct.OracleConnString = OracleConnString;
            return OracleConnString;
        }
    }
}
