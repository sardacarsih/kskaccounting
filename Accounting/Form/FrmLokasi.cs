using System;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Media;
using Accounting.BusinessLayer;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using DevExpress.XtraSplashScreen;

namespace Accounting.Form
{
    public partial class FrmLokasi : DevExpress.XtraEditors.XtraForm
    {
       readonly OracleConnection conn = new(Acct.OracleConnString);
        public FrmLokasi()
        {
            InitializeComponent();
        }
        private SoundPlayer Player = new SoundPlayer();
        private void FrmLokasi_Load(object sender, EventArgs e)
        {
            
            try
            {
                this.CenterToScreen();
                conn.Open();
                string SQL = "SELECT NAMAPT,IDPT,WILAYAH,IDDATA,JENIS_AKUNTANSI from VLOGIN WHERE USERID=:USERID and APPID='GL' order by NAMAPT,IDPT,IDDATA asc ";
                OracleCommand cmd = new OracleCommand(SQL, conn);
                cmd.Parameters.Add(":USERID", OracleDbType.Varchar2, 25).Value = LoginInfo.userID;
                OracleDataReader dr = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dr);
                gridControl1.DataSource = dt;
                dr.Close();
                conn.Close();
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\iddata.wav";
                this.Player.Play();
            }            
                 catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                       
        }
        

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                INIT_PT();
            }          


        }

        private void INIT_PT()
        {
            try
            {
                
                var rowhandle = gridView1.FocusedRowHandle;
                CompanyInfo.INIT = gridView1.GetRowCellValue(rowhandle, "IDDATA").ToString();
                CompanyInfo.JENIS_AKUNTING = gridView1.GetRowCellValue(rowhandle, "JENIS_AKUNTANSI").ToString();
                CompanyInfo.IDPT = gridView1.GetRowCellValue(rowhandle, "IDPT").ToString();
                CompanyInfo.NAMAPT = gridView1.GetRowCellValue(rowhandle, "NAMAPT").ToString();
                CompanyInfo.WILAYAH = gridView1.GetRowCellValue(rowhandle, "WILAYAH").ToString();

                Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.INIT);
                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.INIT);
                Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.INIT);
                Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.INIT);               

                this.Hide();
                new MainView().Show();
                //_ = RunAsync();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //create daftar perkiraan jurnal dan laporan
                //LoadDataAsync();
               
            }

        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            INIT_PT();
        }

    }
}