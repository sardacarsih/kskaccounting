using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using Serilog;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    internal static class LabaRugiDrillDownPreview
    {
        private static readonly string[] BulanIndonesia =
        {
            "Bulan", "Januari", "Pebruari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember"
        };

        public static void Show(XtraReport sourceReport, PreviewMouseEventArgs e)
        {
            try
            {
                if (e.Brick.Value is not DataRowView dataRow)
                {
                    return;
                }

                var userid = LoginInfo.userID;
                var iddata = CompanyInfo.IDDATA;
                var pbulan = (int)sourceReport.Parameters["PBULAN"].Value;
                var pSampaiBulan = pbulan;
                var ptahun = (int)sourceReport.Parameters["PTAHUN"].Value;
                var periode = pbulan.ToString("00") + "/" + ptahun;
                var bulan = BulanIndonesia[pbulan] + "-" + ptahun;
                LabaRugiDrillDownRow row = LabaRugiDrillDownRow.FromDataRow(dataRow.Row);

                if (row.Header == "G")
                {
                    XtraMessageBox.Show("Silahkan klik pada Detail", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dariLaporan = row.Kelompok == "NERACA" ? "NERACA" : "LABARUGI";
                DataSet dsGl = LaporanServices.ViewLap_BukuBesar(iddata, ptahun, pbulan, pSampaiBulan, row.KodeAcc, row.KodeAcc, userid, dariLaporan);
                XtraReport laporan = row.Posisi == "D"
                    ? new GeneralLedgerD2 { DataSource = dsGl }
                    : new GeneralLedgerK2 { DataSource = dsGl };

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
                Log.Error(ex, "LabaRugi drilldown_preview_failed");
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
