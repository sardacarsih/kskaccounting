using System;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System.Media;
using Accounting.BusinessLayer;

namespace Accounting.Form
{
    public partial class FrmloginOld : DevExpress.XtraEditors.XtraForm
    {
        OracleConnection con = new(ConnectionManager.GetOracleConnection());
        private SoundPlayer Player = new();
        static int kesempatan = 3;
        public FrmloginOld()
        {
            InitializeComponent();
        }
        DataTable dt;
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void Frmlogin_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Acct.AppVersion = fvi.FileVersion;
            lblversi.Text= "Version : "+ Acct.AppVersion;
           // lblversi.Text= "Version " + Application.ProductVersion;
        }

        private void txtuserid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }


        private void txtpwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
                Login.PerformClick();
            }
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
        private void Login_Click(object sender, EventArgs e)
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                if (string.IsNullOrEmpty(txtuserid.Text) | (string.IsNullOrEmpty(txtpwd.Text)))
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\userid.wav";
                    this.Player.Play();
                    MessageBox.Show("UserID atau Password belum diisi", "Konfirmasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (kesempatan == 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\hub_manager.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("3 Kesempatan Gagal digunakan - Hubungi Manager Anda", "Info",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //jika user dan password admin masuk menu
                if (txtuserid.Text == "admin" && txtpwd.Text == "admin2021")
                {
                    LoginInfo.role = "Supervisor";
                    LoginInfo.userID = "admin";

                    Hide();
                    new MainView().Show();
                }
                else
                {             
                    string pwd = txtpwd.Text;
                    using OracleCommand cmd = new("SELECT * FROM VLOGIN where userid=:USERID  AND APPID='GL' ", con);

                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    cmd.Parameters.Add(":USERID", OracleDbType.Varchar2, 25).Value = txtuserid.Text.ToLower();

                    OracleDataReader dr;
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dt = new DataTable();
                        dt.Load(dr);
                        string hashPass = dt.Rows[0]["PASSWORD"].ToString();

                        var p = new PasswordCryptographyPbkdf2();

                        bool masuk = p.IsValidPassword(pwd, hashPass);
                        // bool masuk = true;
                        if (masuk)
                        {

                            if (dt.Rows.Count > 1)
                            {
                                LoginInfo.role = dt.Rows[0]["AKSESROLE"].ToString();
                                LoginInfo.userID = dt.Rows[0]["USERID"].ToString();

                                Hide();
                                new FrmLokasi().Show();
                            }
                            else
                            {
                                LoginInfo.role = dt.Rows[0]["AKSESROLE"].ToString();
                                LoginInfo.userID = dt.Rows[0]["USERID"].ToString();
                                CompanyInfo.INIT = dt.Rows[0]["IDDATA"].ToString();
                                CompanyInfo.JENIS_AKUNTING = dt.Rows[0]["JENIS_AKUNTANSI"].ToString();
                                CompanyInfo.NAMAPT = dt.Rows[0]["NAMAPT"].ToString();
                                CompanyInfo.WILAYAH = dt.Rows[0]["WILAYAH"].ToString();

                                Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.INIT);
                                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.INIT);
                                Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.INIT);
                                Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.INIT);

                                Hide();
                                new MainView().Show();
                            }
                        }
                        else
                        {
                            --kesempatan;
                            XtraMessageBox.Show("Password Salah. " +
                               "\nAnda Memiliki " + Convert.ToString(kesempatan) + "X Kesempatan untuk Mencoba", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                    }
                    else
                    {
                        this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\nore_userid.wav";
                        this.Player.Play();

                        XtraMessageBox.Show("UserID tidak terdaftar. " +
                            "\nAnda Memiliki " + Convert.ToString(kesempatan) + "X Kesempatan untuk Mencoba", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        --kesempatan;
                        txtuserid.Focus();
                        txtpwd.Focus();
                    }
                    dr.Close();
                    con.Close();

                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                --kesempatan;
            }

        }
        private void SimpleButton1_Click(object sender, EventArgs e)
        {
            //con.Close();
            this.Close();
        }

    }
}
