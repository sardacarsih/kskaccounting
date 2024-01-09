using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace Accounting
{
    public partial class FrmChangePass : DevExpress.XtraEditors.XtraForm
    {
       
        public FrmChangePass()
        {
            InitializeComponent();

        }

        OracleConnection con = new OracleConnection(Acct.OracleConnString);

        private void FrmChangePass_Load(object sender, EventArgs e)
        {
           
            lbluserid.Text = LoginInfo.userID;
            lblrole.Text = LoginInfo.role;
            Load_ProfileUser();
        }
        OracleDataAdapter UserDA;
        DataSet UserDS;
        string oldpasswd;
        private void Load_ProfileUser()
        {
            string query = "select nama,dept,jabatan,password from master_login WHERE userid=:p_userid";

            OracleCommand cmd = new OracleCommand(query, con)
            {
                CommandType = CommandType.Text
            };
            cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;
            UserDA = new OracleDataAdapter(cmd);
            UserDS = new DataSet();
            UserDA.Fill(UserDS);
            txtnama.Text = UserDS.Tables[0].Rows[0].Field<string>("NAMA");
            txtdept.Text = UserDS.Tables[0].Rows[0].Field<string>("DEPT");
            txtjab.Text = UserDS.Tables[0].Rows[0].Field<string>("JABATAN");
            oldpasswd = UserDS.Tables[0].Rows[0].Field<string>("PASSWORD");

        }

        IOverlaySplashScreenHandle ShowProgressPanel()
        {
            return SplashScreenManager.ShowOverlayForm(this);
        }
        void CloseProgressPanel(IOverlaySplashScreenHandle handle)
        {
            if (handle != null)
                SplashScreenManager.CloseOverlayForm(handle);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {


                var p = new PasswordCryptographyPbkdf2();
                string opwd = txtoldpass.Text;
                bool isvalid = p.IsValidPassword(opwd, oldpasswd);
                if (isvalid)
                {
                    if (txtnewpass.Text.Equals(txtpassconf.Text))
                    {
                        var newpass = txtnewpass.Text;
                        string newhashsaltpwd = p.GetHashPassword(newpass);
                        UpdateProfile(txtnama.Text, txtdept.Text, txtjab.Text, newhashsaltpwd);
                        XtraMessageBox.Show("New Password Updated.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        XtraMessageBox.Show("Konfirmasi Password tidak sama.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                }
                //XtraMessageBox.Show("Data Telah Disimpan.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                XtraMessageBox.Show("Password Lama tidak valid", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }

        private void txtnama_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtdept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtjab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtoldpass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnewpass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtpassconf_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void UpdateProfile(string text1, string text2, string text3, string newhashsaltpwd)
        {
            string query = "update master_login set nama=:pnama,dept=:pdept,jabatan=:pjab,password=:pwd WHERE userid=:p_userid";

            OracleCommand cmd = new OracleCommand(query, con)
            {
                CommandType = CommandType.Text
            };
            con.Open();
            cmd.Parameters.Add(":pnama", OracleDbType.Varchar2, 20).Value = text1;
            cmd.Parameters.Add(":pdept", OracleDbType.Varchar2, 20).Value = text2;
            cmd.Parameters.Add(":pjab", OracleDbType.Varchar2, 20).Value = text3;
            cmd.Parameters.Add(":pwd", OracleDbType.Varchar2, 100).Value = newhashsaltpwd;
            cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;
            cmd.ExecuteReader();
            con.Close();

        }

    }
}