using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmBiayaBlokPusat : DevExpress.XtraEditors.XtraForm
    {
        public FrmBiayaBlokPusat()
        {
            InitializeComponent();
        }
       private readonly OracleConnection conn = new(Acct.OracleConnString);
        private void FrmBiayaBlokPusat_Load(object sender, EventArgs e)
        {
            setahun1.Properties.MinValue = Acct.TahunMin;
            setahun1.Properties.MaxValue = Acct.TahunMax;

            setahun2.Properties.MinValue = Acct.TahunMin;
            setahun2.Properties.MaxValue = Acct.TahunMax;

            setahun2.EditValue= Acct.TahunMax;
            setahun1.EditValue = Acct.TahunMax-5;

        }

        private DataTable Load_BiayaBlok(string p_IDDATA,int dari, int sampai, string p_isfrom)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.REKAP_BIAYA_BLOK", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2,20).Value = p_IDDATA;
                _command.Parameters.Add(":p_dari", OracleDbType.Int16).Value = dari;
                _command.Parameters.Add(":p_sampai", OracleDbType.Int16).Value = sampai;
                _command.Parameters.Add(":p_isfrom", OracleDbType.Varchar2, 20).Value = p_isfrom;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        private void btnproses_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            handle = SplashScreenManager.ShowOverlayForm(this);           
            var tahun1 = Convert.ToInt32(setahun1.EditValue);
            var tahun2 = Convert.ToInt32(setahun2.EditValue);
            pivotGridControl1.DataSource = Load_BiayaBlok(CompanyInfo.INIT, tahun1, tahun2,CompanyInfo.JENIS_AKUNTING);
            pivotGridControl1.BestFit();
            sbexport.Enabled = true;
            SplashScreenManager.CloseOverlayForm(handle);
        }

        private void sbexport_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            handle = SplashScreenManager.ShowOverlayForm(this);
            var pivotExportOptions = new DevExpress.XtraPivotGrid.PivotXlsxExportOptions
            {
                ExportType = DevExpress.Export.ExportType.WYSIWYG
            };
            pivotGridControl1.ExportToXlsx("pivot.xlsx", pivotExportOptions);
            SplashScreenManager.CloseOverlayForm(handle);
            //These lines will open it in Excel
            ProcessStartInfo pi = new ProcessStartInfo("pivot.xlsx");
            pi.UseShellExecute = true;
            Process.Start(pi);

        }
    }
}