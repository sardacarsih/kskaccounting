using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    public partial class rsub_rl_DetailD : DevExpress.XtraReports.UI.XtraReport
    {
        public rsub_rl_DetailD()
        {
            InitializeComponent();
        }

        private void xrTableCell1_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void xrTableCell1_PreviewClick(object sender, PreviewMouseEventArgs e)
        {
            LabaRugiDrillDownPreview.Show(this, e);
        }

        private void xrTableCell1_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
          
        }

        private void tableCell12_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void tableCell13_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void tableCell14_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void tableCell14_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
           
        }

        private void tableCell13_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
           
        }

        private void tableCell12_PreviewClick(object sender, PreviewMouseEventArgs e)
        {
            LabaRugiDrillDownPreview.Show(this, e);
        }

        private void tableCell13_PreviewClick(object sender, PreviewMouseEventArgs e)
        {
            LabaRugiDrillDownPreview.Show(this, e);
        }

        private void xrTableCell4_BeforePrint(object sender, CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void tableCell12_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
           
        }

    }
}
