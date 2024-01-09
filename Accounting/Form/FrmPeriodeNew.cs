using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmPeriodeNew : DevExpress.XtraEditors.XtraForm
    {
        OracleConnection con = new OracleConnection(Acct.OracleConnString);
        public FrmPeriodeNew()
        {
            InitializeComponent();
        }

        private void FrmPeriodeNew_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
            cmbbulan.SelectedIndex = 0;
            setahun.Value = DateTime.Today.Year;

            lblpt.Text = CompanyInfo.NAMAPT;
            lbldata.Text = CompanyInfo.INIT;
            lblwilayah.Text = CompanyInfo.WILAYAH;

        }

        private void sbcetak_Click(object sender, EventArgs e)
        {
            try
            {
                var p_bulan = cmbbulan.SelectedIndex;
                var p_tahun = (int)setahun.Value;
                AccountServices.CreateNextPeriode(CompanyInfo.INIT, p_bulan, p_tahun);
                Acct.PeriodeMin = Convert.ToInt32(AccountServices.GetMinPeriode(CompanyInfo.INIT).ToString());
                Acct.PeriodeMax = Convert.ToInt32(AccountServices.GetMaxPeriode(CompanyInfo.INIT).ToString());
                //create default perkiraan closing
                AccountServices.CreateClosingAcct(CompanyInfo.INIT);

                XtraMessageBox.Show("Periode Akuntansi Baru Telah dibuat.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                   
                    XtraMessageBox.Show("Periode ini sudah ada, silahkan pilih periode lainnya", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }
    }
}