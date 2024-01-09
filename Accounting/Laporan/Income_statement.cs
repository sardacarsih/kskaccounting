using Accounting.BusinessLayer;
using DevExpress.DataAccess.Sql.DataApi;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Laporan
{
    public partial class Income_statement : DevExpress.XtraReports.UI.XtraReport
    {
        public Income_statement()
        {
            InitializeComponent();
        }

        private DataSet DSGL, DSSubRL;
        private void xrLabel10_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((XRLabel)sender).Tag = GetCurrentRow();
            
        }

        private void xrLabel10_PreviewClick(object sender, PreviewMouseEventArgs e)
        {
            try
            {
                string[] bulanbi = { "Bulan", "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };

                DataRowView dataRow = e.Brick.Value as DataRowView;
                var userid = LoginInfo.userID;
                var iddata = CompanyInfo.INIT;
                var pbulan = (int)this.Parameters["PBULAN"].Value;
                var p_sampaibulan = (int)this.Parameters["PBULAN"].Value;
                var ptahun = (int)this.Parameters["PTAHUN"].Value;
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();
                var bulan = bulanbi[pbulan].ToString() + "-" + ptahun.ToString();
                var kode = dataRow.Row["KODEACC"].ToString();
                var SUB = dataRow.Row["SUB2"].ToString();

                if (e.Brick.Text != "-" && string.IsNullOrEmpty(e.Brick.Text)==false)
                {
                    if (dataRow.Row["ISHEADER"].ToString() == "G")
                    {
                        
                        if (dataRow.Row["TIPEACC"].ToString() == "PENDAPATAN")
                        {
                            //1st generate 
                            LaporanServices.GenerateSub_LabaRugi(iddata, pbulan, ptahun, kode, userid, "LABARUGI", "K");
                            DSSubRL = LaporanServices.ViewSub_LabaRugi(iddata, userid);
                            DSSubRL.WriteXmlSchema("SubRL.xsd");
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
                        else
                        {
                            //1st generate 
                            LaporanServices.GenerateSub_LabaRugi(iddata, pbulan, ptahun, kode, userid, "LABARUGI", "D");
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
                        
                    }
                    else //if (dataRow.Row["ISHEADER"].ToString() == "D")
                    {
                        var darikode = kode;
                        var sampaikode = kode;
                        //get data for report
                        DSGL = LaporanServices.ViewLap_BukuBesar(iddata, ptahun,pbulan,p_sampaibulan, darikode, sampaikode, userid, "LABARUGI");
                        //DSGL.WriteXmlSchema("GeneralLedger.xsd");

                        if (dataRow.Row["TIPEACC"].ToString() != "PENDAPATAN")
                        {
                            GeneralLedgerD2 laporan = new ()
                            {
                                DataSource = DSGL
                            };

                            laporan.Parameters["PBULAN"].Value = pbulan;
                            laporan.Parameters["PTAHUN"].Value = ptahun;
                            laporan.Parameters["BULAN"].Value = bulan;
                            laporan.Parameters["PERIODE"].Value = periode;
                            laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                            laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                            laporan.Parameters["USERID"].Value = userid;
                            laporan.RequestParameters = true;
                            ReportPrintTool tool = new (laporan);
                            tool.ShowPreview();
                        }
                        else
                        {
                           
                            GeneralLedgerK2 laporan = new ()
                            {
                                DataSource = DSGL
                            };

                            laporan.Parameters["PBULAN"].Value = pbulan;
                            laporan.Parameters["PTAHUN"].Value = ptahun;
                            laporan.Parameters["BULAN"].Value = bulan;
                            laporan.Parameters["PERIODE"].Value = periode;
                            laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                            laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                            laporan.Parameters["USERID"].Value = userid;
                            laporan.RequestParameters = true;
                            ReportPrintTool tool = new (laporan);
                            tool.ShowPreview();
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void xrLabel10_PreviewMouseMove(object sender, PreviewMouseEventArgs e)
        {
           
        }
    }
}
