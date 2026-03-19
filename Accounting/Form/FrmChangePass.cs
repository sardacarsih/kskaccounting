using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting
{
    public partial class FrmChangePass : DevExpress.XtraEditors.XtraForm
    {
        private OracleConnection con;
        private OracleDataAdapter UserDA;
        private DataSet UserDS;
        private string oldpasswd;

        public FrmChangePass()
        {
            InitializeComponent();
            con = new OracleConnection(LoginInfo.OracleConnString);
        }

        private void FrmChangePass_Load(object sender, EventArgs e)
        {
            lbluserid.Text = LoginInfo.userID;
            lblrole.Text = LoginInfo.role;
            Load_ProfileUser();
        }

        private void Load_ProfileUser()
        {
            string query = "select nama, dept, jabatan, password from master_login WHERE userid = :p_userid";

            using (OracleCommand cmd = new(query, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;
                UserDA = new OracleDataAdapter(cmd);
                UserDS = new DataSet();
                UserDA.Fill(UserDS);

                if (UserDS.Tables[0].Rows.Count > 0)
                {
                    DataRow userRow = UserDS.Tables[0].Rows[0];
                    txtnama.Text = userRow.Field<string>("NAMA");
                    txtdept.Text = userRow.Field<string>("DEPT");
                    txtjab.Text = userRow.Field<string>("JABATAN");
                    oldpasswd = userRow.Field<string>("PASSWORD");
                }
            }
        }

        private IOverlaySplashScreenHandle ShowProgressPanel()
        {
            return SplashScreenManager.ShowOverlayForm(this);
        }

        private void CloseProgressPanel(IOverlaySplashScreenHandle handle)
        {
            if (handle != null)
                SplashScreenManager.CloseOverlayForm(handle);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;

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
            var p = new PasswordCryptographyPbkdf2();
            string opwd = txtoldpass.Text;
            bool isValid = p.IsValidPassword(opwd, oldpasswd);

            if (isValid)
            {
                if (txtnewpass.Text.Equals(txtpassconf.Text))
                {
                    string newhashsaltpwd = p.GetHashPassword(txtnewpass.Text);
                    UpdateProfile(txtnama.Text, txtdept.Text, txtjab.Text, newhashsaltpwd);
                    XtraMessageBox.Show("New Password Updated.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    XtraMessageBox.Show("Konfirmasi Password tidak sama.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                XtraMessageBox.Show("Password Lama tidak valid", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateProfile(string nama, string dept, string jabatan, string newhashsaltpwd)
        {
            string query = "update master_login set nama = :pnama, dept = :pdept, jabatan = :pjab, password = :pwd WHERE userid = :p_userid";

            using (OracleCommand cmd = new OracleCommand(query, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(":pnama", OracleDbType.Varchar2, 20).Value = nama;
                cmd.Parameters.Add(":pdept", OracleDbType.Varchar2, 20).Value = dept;
                cmd.Parameters.Add(":pjab", OracleDbType.Varchar2, 20).Value = jabatan;
                cmd.Parameters.Add(":pwd", OracleDbType.Varchar2, 100).Value = newhashsaltpwd;
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
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
