using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmDeveloper : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        private SoundPlayer Player = new SoundPlayer();
        public FrmDeveloper()
        {
            InitializeComponent();
        }
        private void FrmDeveloper_Load(object sender, EventArgs e)
        {
            try
            {
                if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanAccessDeveloperTools))
                {
                    Close();
                    return;
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }
        string randomString;
        private void sbbrowse_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanAccessDeveloperTools))
            {
                return;
            }
            try
            {
                using OpenFileDialog ofd = new() { Filter = "SQL Script|*.sql" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtverifikasi.Text = string.Empty;
                    txtPath.Text = ofd.FileName;
                    sqlFilePath = ofd.FileName;

                    // Read the text from the file
                    string text = File.ReadAllText(sqlFilePath);

                    // Set the text of the control
                    memoEdit1.Text = text;

                    //random 8 karakter
                    // Display the string in a textbox
                    lblrandom.Text ="Verification Code : " +GenerateRandomString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        string sqlFilePath;
        private void SBImport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanAccessDeveloperTools))
            {
                return;
            }
            if (string.IsNullOrEmpty(sqlFilePath))
            {
                return;
            }

            if (!txtverifikasi.Text.Equals(randomString))
            {
                XtraMessageBox.Show("Verifikasi Kode Tidak Cocok", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
           
            try
            {
                string sql = File.ReadAllText(sqlFilePath);

                // execute the command and close the connection

                using OracleCommand cmd = new(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                cmd.ExecuteNonQuery();
                conn.Close();

                string logMessage = "The 'Execute' function was called at " + DateTime.Now.ToString()+" for code : "+ randomString;
                File.AppendAllText("SQL_log.txt", logMessage);
                XtraMessageBox.Show("Update Script Selesai", "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblrandom.Text = "Verification Code : " + GenerateRandomString();
                txtverifikasi.Text = string.Empty;
                memoEdit1.Text = string.Empty;
                txtPath.Text = string.Empty;
                sqlFilePath = string.Empty;

            }
            catch (OracleException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }           
        }
        private string GenerateRandomString()
        {
            // Create a new Random object
            Random rnd = new Random();

            // Create an empty string to store the random string
            randomString = "";

            // Generate 8 random characters and add them to the string
            for (int i = 0; i < 8; i++)
            {
                int num = rnd.Next(0, 36);
                if (num < 10)
                {
                    randomString += num;
                }
                else
                {
                    randomString += (char)('A' + (num - 10));
                }
            }

            // Return the random string
            return randomString;
        }

    }
}
