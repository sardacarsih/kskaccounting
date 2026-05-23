using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting
{
    public partial class FrmChangePass : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection con;
        private OracleDataAdapter? userDA;
        private DataSet? userDS;

        public FrmChangePass()
        {
            InitializeComponent();
            con = new OracleConnection(LoginInfo.OracleConnString);
        }

        public bool RequireCurrentPassword { get; set; } = true;
        public bool ForcePasswordChange { get; set; }

        private void FrmChangePass_Load(object sender, EventArgs e)
        {
            lbluserid.Text = LoginInfo.userID;
            lblrole.Text = LoginInfo.role;
            txtnama.Properties.ReadOnly = true;
            txtdept.Properties.ReadOnly = true;
            txtjab.Properties.ReadOnly = true;
            LoadProfileUser();
            ApplyMode();
        }

        private void ApplyMode()
        {
            labelControl3.Visible = RequireCurrentPassword;
            txtoldpass.Visible = RequireCurrentPassword;

            if (ForcePasswordChange)
            {
                Text = "Ganti Password Sementara";
                simpleButton1.Text = "Update Password";
            }
        }

        private void LoadProfileUser()
        {
            const string query = "SELECT nama, dept, jabatan FROM master_login WHERE userid = :p_userid";

            using OracleCommand cmd = new(query, con);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;
            userDA = new OracleDataAdapter(cmd);
            userDS = new DataSet();
            userDA.Fill(userDS);

            if (userDS.Tables.Count == 0 || userDS.Tables[0].Rows.Count == 0)
            {
                return;
            }

            DataRow userRow = userDS.Tables[0].Rows[0];
            txtnama.Text = userRow.Field<string>("NAMA");
            txtdept.Text = userRow.Field<string>("DEPT");
            txtjab.Text = userRow.Field<string>("JABATAN");
        }

        private IOverlaySplashScreenHandle ShowProgressPanel()
        {
            return SplashScreenManager.ShowOverlayForm(this);
        }

        private void CloseProgressPanel(IOverlaySplashScreenHandle? handle)
        {
            if (handle != null)
            {
                SplashScreenManager.CloseOverlayForm(handle);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle? handle = null;

            try
            {
                handle = ShowProgressPanel();
                ValidateAndChangePassword();
            }
            finally
            {
                CloseProgressPanel(handle);
            }
        }

        private void ValidateAndChangePassword()
        {
            if (!txtnewpass.Text.Equals(txtpassconf.Text, StringComparison.Ordinal))
            {
                XtraMessageBox.Show("Konfirmasi password tidak sama.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtpassconf.Focus();
                return;
            }

            bool changed;
            string validationMessage;
            if (RequireCurrentPassword)
            {
                changed = UserManager_Services.TryChangePassword(LoginInfo.userID, txtoldpass.Text, txtnewpass.Text, out validationMessage);
            }
            else
            {
                changed = UserManager_Services.TryChangePasswordAfterVerifiedLogin(LoginInfo.userID, txtnewpass.Text, out validationMessage);
            }

            if (!changed)
            {
                XtraMessageBox.Show(validationMessage, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (RequireCurrentPassword)
                {
                    txtoldpass.Focus();
                }
                else
                {
                    txtnewpass.Focus();
                }

                return;
            }

            XtraMessageBox.Show("Password berhasil diperbarui.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnama_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
        private void txtdept_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
        private void txtjab_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
        private void txtoldpass_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
        private void txtnewpass_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
        private void txtpassconf_KeyDown(object sender, KeyEventArgs e) => TextBox_KeyDown(sender, e);
    }
}
