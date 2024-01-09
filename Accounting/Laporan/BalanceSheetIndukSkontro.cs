using DevExpress.DataAccess.Sql.DataApi;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    public partial class BalanceSheetIndukSkontro : DevExpress.XtraReports.UI.XtraReport
    {
        public BalanceSheetIndukSkontro()
        {
            InitializeComponent();
        }
        private void xrSubreport1_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.FilterString = "[KAT] = 'AKTIVA'";
        }

        private void xrSubreport2_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((XRSubreport)sender).ReportSource.FilterString = "[KAT] = 'PASIVA'";
        }
    }
}
