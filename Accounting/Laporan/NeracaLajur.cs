using DevExpress.DataAccess.ObjectBinding;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Accounting.Laporan
{
    public partial class NeracaLajur : DevExpress.XtraReports.UI.XtraReport
    {
        public NeracaLajur()
        {
            InitializeComponent();
            this.Parameters["Bulan"].Value = DateTime.Today.Month;
            this.Parameters["Tahun"].Value = DateTime.Today.Year;
            
        }

        private void NeracaLajur_BeforePrint(object sender, CancelEventArgs e)
        {
            
        }

        private void NeracaLajur_DataSourceDemanded(object sender, EventArgs e)
        {
            
        }
    }
}
