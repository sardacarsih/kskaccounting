using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    // Laba Rugi V2 report bound to ACCT_REPORT_ENGINE_V1 via LaporanServices.ViewLap_LabaRugi_V2.
    public partial class Income_statement2 : DevExpress.XtraReports.UI.XtraReport
    {
        private static readonly string[] BulanIndonesia =
        {
            "Bulan", "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember"
        };

        public Income_statement2()
        {
            InitializeComponent();
        }

        private DataSet DSGL;

        private void xrLabel10_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }

        private void xrLabel1_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Indent account detail rows under the section header; keep subtotal/result lines flush-left.
            string rowKind = GetCurrentColumnValue("ROWKIND") as string;
            bool isAggregate = rowKind == LabaRugiRow.SubtotalRowKind || rowKind == LabaRugiRow.TotalRowKind;
            int left = isAggregate ? 6 : 24;
            ((XRLabel)sender).Padding = new DevExpress.XtraPrinting.PaddingInfo(left, 2, 0, 0, 100F);
        }

        private void xrLabel10_PreviewClick(object sender, PreviewMouseEventArgs e)
        {
            try
            {
                if (e.Brick.Value is not DataRowView dataRow)
                {
                    return;
                }

                if (e.Brick.Text == "-" || string.IsNullOrEmpty(e.Brick.Text))
                {
                    return;
                }

                string rowKind = GetString(dataRow.Row, "ROWKIND");
                if (rowKind == LabaRugiRow.SubtotalRowKind || rowKind == LabaRugiRow.TotalRowKind)
                {
                    return;
                }

                string userid = LoginInfo.userID;
                string iddata = CompanyInfo.IDDATA;
                int pbulan = (int)Parameters["PBULAN"].Value;
                int ptahun = (int)Parameters["PTAHUN"].Value;
                string periode = pbulan.ToString("00") + "/" + ptahun;
                string bulan = BulanIndonesia[pbulan] + "-" + ptahun;
                string kode = GetString(dataRow.Row, "KODEACC");
                string tipeAcc = GetString(dataRow.Row, "TIPEACC");
                string posisi = tipeAcc == "PENDAPATAN" || tipeAcc == "PENDAPATAN DILUAR USAHA" ? "K" : "D";

                // Drill the clicked account and its whole COA subtree (header rows have no direct
                // postings; the tree is linked by PARENTACC, so a code range cannot reach children).
                DSGL = LaporanServices.ViewLap_BukuBesar_Tree(iddata, ptahun, pbulan, pbulan, kode);
                XtraReport laporan = posisi == "D"
                    ? new GeneralLedgerD2 { DataSource = DSGL }
                    : new GeneralLedgerK2 { DataSource = DSGL };

                laporan.Parameters["PBULAN"].Value = pbulan;
                laporan.Parameters["PTAHUN"].Value = ptahun;
                laporan.Parameters["BULAN"].Value = bulan;
                laporan.Parameters["PERIODE"].Value = periode;
                laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                laporan.Parameters["USERID"].Value = userid;
                laporan.RequestParameters = true;

                ReportPrintTool tool = new(laporan);
                tool.ShowPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void xrLabel10_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
        }

        private static string GetString(DataRow row, string column)
        {
            return row.Table.Columns.Contains(column) && row[column] != DBNull.Value ? row[column].ToString() : string.Empty;
        }
    }
}
