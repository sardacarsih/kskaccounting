using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Accounting.Utilities
{
    /// <summary>
    /// Log diagnostik untuk cek duplikat nomor jurnal. Merekam parameter persis
    /// yang dikirim ke ACCT_JURNAL_V2.CekNoJurnalExist_input beserta hasilnya,
    /// untuk melacak error "Nomor Jurnal sudah ada" palsu.
    /// </summary>
    public static class JurnalDupCheckLog
    {
        public static void Write(string idData, string nomor, string periode, bool exists, string source)
        {
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(dir);
                string line = string.Join("\t",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    "source=" + source,
                    "iddata=" + idData,
                    "nomor=[" + nomor + "]",
                    "periode=[" + periode + "]",
                    "exists=" + (exists ? "1" : "0"),
                    "db=" + ExtractDataSource(LoginInfo.OracleConnString));
                File.AppendAllText(Path.Combine(dir, "jurnal_dupcheck.log"), line + Environment.NewLine);
            }
            catch
            {
                // Logging tidak boleh mengganggu alur input.
            }
        }

        private static string ExtractDataSource(string connString)
        {
            if (string.IsNullOrEmpty(connString))
            {
                return "(null)";
            }

            // Ambil HOST dan SERVICE_NAME saja; jangan pernah menulis kredensial ke log.
            string host = Regex.Match(connString, @"HOST=([^)\s]+)", RegexOptions.IgnoreCase).Groups[1].Value;
            string service = Regex.Match(connString, @"SERVICE_NAME=([^)\s]+)", RegexOptions.IgnoreCase).Groups[1].Value;
            return host + "/" + service;
        }
    }
}
