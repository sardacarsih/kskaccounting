using Accounting.BusinessLayer;
using Accounting.Form;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    public partial class BalanceSheetDetail : DevExpress.XtraReports.UI.XtraReport
    {
        public BalanceSheetDetail()
        {
            InitializeComponent();
        }
        DataSet DSSubRL;
        private void xrLabel6_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
        }


        private void xrLabel6_PreviewClick(object sender, PreviewMouseEventArgs e)
        {

            try
            {
                SplashScreenManager.ShowForm(typeof(WaitForm_Prosess));
                Cursor.Current = Cursors.WaitCursor;
                string[] bulanbi = { "Bulan", "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };

                DataRowView dataRow = e.Brick.Value as DataRowView;
                var userid = LoginInfo.userID;
                var iddata =CompanyInfo.IDDATA;
                var pbulan = (int)this.Parameters["PBULAN"].Value;
                var ptahun = (int)this.Parameters["PTAHUN"].Value;
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();
                var bulan = bulanbi[pbulan].ToString() + "-" + ptahun.ToString();
                var kode = dataRow.Row["AKUN"].ToString();
                var SUB = dataRow.Row["TIPE"].ToString();
                var kategori = dataRow.Row["KAT"].ToString();

                if (e.Brick.Text != "-" && string.IsNullOrEmpty(e.Brick.Text) == false)
                {

                    if (kategori == "AKTIVA")
                    {
                        //1st generate 
                        LaporanServices.GenerateSub_LabaRugi(iddata, pbulan, ptahun, kode, userid, "NERACA", "D");
                        //MessageBox.Show("this is g");
                        //2nd view data
                        DSSubRL = LaporanServices.ViewSub_LabaRugi(iddata, userid);
                        //DSSubRL.WriteXmlSchema("SubRL.xsd");

                        rsub_rl_DetailD detailReport = new rsub_rl_DetailD
                        {
                            DataSource = DSSubRL
                        };
                        detailReport.Parameters["SUB"].Value = SUB;
                        detailReport.Parameters["PBULAN"].Value = pbulan;
                        detailReport.Parameters["PTAHUN"].Value = ptahun;
                        detailReport.Parameters["BULAN"].Value = bulan;
                        detailReport.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        detailReport.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        detailReport.RequestParameters = true;
                        detailReport.ShowPreviewDialog();
                    }
                    else
                    {
                        //1st generate 
                        LaporanServices.GenerateSub_LabaRugi(iddata, pbulan, ptahun, kode, userid, "NERACA", "K");
                        //MessageBox.Show("this is g");
                        //2nd view data
                        DSSubRL = LaporanServices.ViewSub_LabaRugi(iddata, userid);
                        //DSSubRL.WriteXmlSchema("SubRL.xsd");

                        rsub_rl_DetailK detailReport = new rsub_rl_DetailK
                        {
                            DataSource = DSSubRL
                        };
                        detailReport.Parameters["SUB"].Value = SUB;
                        detailReport.Parameters["PBULAN"].Value = pbulan;
                        detailReport.Parameters["PTAHUN"].Value = ptahun;
                        detailReport.Parameters["BULAN"].Value = bulan;
                        detailReport.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        detailReport.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        detailReport.RequestParameters = true;
                        detailReport.ShowPreviewDialog();
                    }

                }
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
