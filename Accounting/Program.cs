using Accounting.Form;
using Accounting.Services;
using DevExpress.XtraEditors;
using Serilog;
using System;
using System.Windows.Forms;

namespace Accounting
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console() // Log to console
                .WriteTo.Debug()   // Log to Visual Studio debug output
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Log to file
                .CreateLogger();

            try
            {
                if (args.Length == 2 && args[0].Equals("--reset-password", StringComparison.OrdinalIgnoreCase))
                {
                    string userId = args[1];
                    string temporaryPassword = UserManager_Services.AdminResetPassword(userId, LoginInfo.MODULE, bypassAuthorization: true);
                    Log.Information("Password reset for user {UserId}", userId);
                    Console.WriteLine($"Password sementara untuk user '{userId}': {temporaryPassword}");
                    Console.WriteLine("Password ini wajib diganti saat login berikutnya.");
                    return;
                }

                if (args.Length >= 1 && args[0].Equals("--reset-password", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Gunakan format: --reset-password <userid>");
                    return;
                }

                Log.Information("Starting the Accounting application");

                // Initialize WinForms settings
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                WindowsFormsSettings.LoadApplicationSettings();

                // Run the login form
                Application.Run(new Frmlogin());
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                // Ensure all logs are flushed
                Log.CloseAndFlush();
            }
        }
    }
}
