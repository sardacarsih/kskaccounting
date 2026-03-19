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
    public partial class Jurnal_NotBalanced : SplashScreen
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public string Myperiode { get; set; }
        public int ibulan { get; set; }
        public int itahun { get; set; }
        public Jurnal_NotBalanced()
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

        private void Jurnal_NotBalanced_Load(object sender, EventArgs e)
        {
            var jurnal = Jurnal_tidak_imbang();
            gridControl1.DataSource = jurnal;
                gridView1.BestFitColumns();
         }

        private DataTable Jurnal_tidak_imbang()
        {
            string query = "SELECT NOJURNAL,TANGGAL,SUM(DEBET)DEBET,SUM(KREDIT)KREDIT,SUM(DEBET)-SUM(KREDIT)SELISIH FROM ACCT_JURNAL_TMP HAVING SUM(DEBET)-SUM(KREDIT)<>0 GROUP BY NOJURNAL,TANGGAL order by nojurnal";
            using OracleCommand _command = new(query, conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}