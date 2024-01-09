using Accounting.BusinessLayer;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraSpreadsheet.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class COAError : SplashScreen
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        public string Myperiode { get; set; }
        public int ibulan { get; set; }
        public int itahun { get; set; }
        public COAError()
        {
            InitializeComponent();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
        }

        private void COAError_Load(object sender, EventArgs e)
        {
            
                var errorcoa = ToolsServices.Analisa_kesalahan_COA(CompanyInfo.INIT, itahun);

            gridControl1.DataSource = errorcoa;
                gridView1.BestFitColumns();
               // XtraMessageBox.Show("Biaya Produksi Belum selesai direclass", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
              
         }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void sbexport_Click(object sender, EventArgs e)
        {
            try
            {
               
                gridControl1.ExportToText("Error COA.txt");
                ProcessStartInfo psi = new ProcessStartInfo(@"Error COA.txt");
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}