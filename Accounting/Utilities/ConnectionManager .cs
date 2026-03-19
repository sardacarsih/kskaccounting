using Accounting;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Accounting.Utilities
{
    public class ConnectionManager
    {
        private static string LokasiServer = string.Empty;
        private static string ServerAddress = string.Empty;

        private static readonly Dictionary<string, (string UserId, string Password, string ServiceName)> ServerConfigs = new()
        {
            { "LOCAL_KSKGROUP", ("KSKG", "KSKGboss2022", "KSKGROUP") },
            { "LOCAL_FSLGROUP", ("FSLG", "FSLGboss2022", "FSLGROUP") },
            { "LOCAL_FBM", ("FBM", "FBMboss", "KEBUN") },
            { "LOCAL_KSKKEBUN", ("KSK", "KSKboss", "KEBUN") },
            { "LOCAL_MSLKEBUN", ("MSL", "MSLboss", "MSLKEBUN") },
            { "LOCAL_FSLKEBUN", ("FSL", "FSLboss", "KEBUN") },           
            { "FBMKANPUS", ("FBM", "FBMboss", "XEPDB1") },
            { "FBMKEBUN", ("FBM", "FBMboss", "KSKKEBUN") }, 
            { "PKSFSL", ("FSL", "FSLboss", "XEPDB1") },
            { "KSKKEBUN", ("KSK", "KSKboss", "KSKKEBUN") },
            { "MSLKEBUN", ("MSL", "MSLboss", "MSLKEBUN") },
            { "FSLKEBUN", ("FSL", "FSLboss", "FSLKEBUN") },
            { "FAKKEBUN", ("FAK", "FAKboss", "XEPDB1") },
             { "FSKKEBUN", ("FSKKEBUN", "FSKboss", "ORCLPDB") },
             //KANPUS
            { "KSKGROUP", ("KSKG", "KSKGboss2022", "KSKGROUP") },
            { "FSLGROUP", ("FSLG", "FSLGboss2022", "FSLGROUP") },
            { "PAJAK", ("PAJAK", "PAJAKboss2022", "PAJAK") },
             { "FBM", ("KSKG", "KSKGboss2022", "KSKGROUP") },
            //KANPUS
        };

        public static string GetOracleConnection()
        {
            LoadConfiguration();

            if (!string.IsNullOrEmpty(LokasiServer) && !string.IsNullOrEmpty(ServerAddress))
            {
                if (ServerConfigs.TryGetValue(LokasiServer, out var config))
                {
                    return BuildConnectionString(ServerAddress, config.UserId, config.Password, config.ServiceName);
                }
                else
                {
                    ShowMessage("Koneksi ke server gagal", "Info", MessageBoxIcon.Error);
                }
            }
            else
            {
                ShowMessage("Setting Koneksi Server tidak ditemukan", "Info", MessageBoxIcon.Information);
            }

            return string.Empty;
        }

        private static void LoadConfiguration()
        {
            try
            {
                string configFilePath = Path.Combine("Utilities", "config.json");

                if (File.Exists(configFilePath))
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    string json = string.Join(Environment.NewLine, lines.Where(line => !line.TrimStart().StartsWith("//")));

                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    if (config != null)
                    {
                        LokasiServer = config.LokasiServer;
                        ServerAddress = config.ServerAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading configuration: {ex.Message}", "Error", MessageBoxIcon.Error);
            }
        }

        private static string BuildConnectionString(string host, string userId, string password, string serviceName)
        {
            return $@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={host})(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={userId};Password={password}";
        }

        private static void ShowMessage(string message, string caption, MessageBoxIcon icon)
        {
            XtraMessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }
    }
}
