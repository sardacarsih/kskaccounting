using Accounting.BusinessLayer;
using Accounting.Laporan;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Drawing.Printing;
using System.Media;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmReportParamKebun : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public FrmReportParamKebun()
        {
            InitializeComponent();
        }
        DataSet DSLapEstate = new DataSet();
        DataSet TBMJeniskerja = new DataSet();
        DataSet TMJeniskerja = new DataSet();
        int bulan;
        //string iddata;

        private void ApplyAuthorizationState()
        {
            sbcetak.Enabled = AuthorizationService.CanViewEstateReports();
            sbexport.Enabled = AuthorizationService.CanExportReports();
        }

        private void FrmReportParamEstate_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewEstateReports))
            {
                Close();
                return;
            }
            string[] Bulan = { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };
            cmbbulan.Properties.Items.AddRange(Bulan);
            bulan = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));

            cmbbulan.SelectedIndex = bulan - 1;

            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            ApplyAuthorizationState();
        }


        private void sbexport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanExportReports))
            {
                return;
            }
            try
            {
                int pbulan = cmbbulan.SelectedIndex + 1;
                int ptahun = Convert.ToInt32(setahun.Value);
                var lastDayOfMonth = DateTime.DaysInMonth(ptahun, pbulan);
                var bulan = "Periode : " + cmbbulan.Text + "-" + setahun.Value.ToString();
                var bulanneraca = lastDayOfMonth + " " + cmbbulan.Text + "-" + setahun.Value.ToString();
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();

                var iddata =CompanyInfo.IDDATA;
                var jenis = CompanyInfo.JENIS_AKUNTING;
                var userid = LoginInfo.userID;
                SplashScreenManager.ShowForm(typeof(WaitForm_Load));
                Cursor.Current = Cursors.WaitCursor;

                var adacoa = AccountServices.CekCOAExist(iddata, ptahun);
                if (adacoa == 1) //1 tidak ada coa 
                {
                    XtraMessageBox.Show("Daftar Perkiraan belum tersedia", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DataTable dt = new ();
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage package = new ();
                // number formats
                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "[Red](#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                if (radioGroup1.SelectedIndex == 3)
                {

                }
                if (radioGroup1.SelectedIndex == 4)
                {


                }
                if (radioGroup1.SelectedIndex == 6)
                {

                }
            }
            catch (SystemException ex)
            {
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
            }
        }


        private void subreport_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XRSubreport subreport = sender as XRSubreport;
            XtraReport report = subreport.ReportSource;
            report.DataSource = subreport.RootReport.DataSource; //Master report's DataSource

        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroup1.SelectedIndex == 0)
            {
                this.Text = "Laba / Rugi";
                cmbbulan.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 1)
            {
                this.Text = "Neraca";
                cmbbulan.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 2)
            {
                this.Text = "Neraca (Skontro)";
                cmbbulan.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 3)
            {
                this.Text = "Neraca (Semester 1)";
                cmbbulan.SelectedIndex = 0;
                cmbbulan.Enabled = false;
                cmbbulan.SelectedIndex = 0;
            }
            if (radioGroup1.SelectedIndex == 4)
            {
                this.Text = "Neraca (Semester 2)";
                cmbbulan.SelectedIndex = 6;
                cmbbulan.Enabled = false;
            }
            if (radioGroup1.SelectedIndex == 5)
            {
                this.Text = "Buku Besar";
                cmbbulan.Enabled = true;
                cmbbulan.SelectedIndex = 0;
            }

            sbexport.Enabled = sbexport.Enabled && AuthorizationService.CanExportReports();
        }
        private SoundPlayer Player = new SoundPlayer();

        private void sbcetak_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewEstateReports))
            {
                return;
            }
            try
            {
                int pbulan = cmbbulan.SelectedIndex + 1;
                int ptahun = Convert.ToInt32(setahun.Value);
                var lastDayOfMonth = DateTime.DaysInMonth(ptahun, pbulan);
                var bulan = "Periode : " + cmbbulan.Text + "-" + setahun.Value.ToString();
                var bulanneraca = lastDayOfMonth + " " + cmbbulan.Text + "-" + setahun.Value.ToString();
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();

                var iddata =CompanyInfo.IDDATA;
                var jenis = CompanyInfo.JENIS_AKUNTING;
                var userid = LoginInfo.userID;
                SplashScreenManager.ShowForm(typeof(WaitForm_Load));
                Cursor.Current = Cursors.WaitCursor;


                if (radioGroup1.SelectedIndex == 0)
                {
                    //cek record jurnal exist ?
                    var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    if (record == 0)
                    {
                        XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }

                    //generate data
                    var divisi = Generate_Divisi(iddata, periode);
                    //divisi.WriteXmlSchema("RekapPerDivisi.xsd");


                    RekapTMperDivisi laporan = new RekapTMperDivisi
                    {
                        DataSource = divisi
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = ptahun;
                    laporan.Parameters["BULAN"].Value = bulan;                    
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new ReportPrintTool(laporan);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 1)
                {
                    //generate data
                    TBMJeniskerja = Generate_TBM(iddata, pbulan, ptahun);
                    //TBMJeniskerja.WriteXmlSchema("TBMJeniskerja.xsd");


                    TBMINTI laporan = new TBMINTI
                    {
                        DataSource = TBMJeniskerja
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = ptahun;
                    laporan.Parameters["BULAN"].Value = bulan;
                    laporan.Parameters["JUDUL"].Value = "RINCIAN PEKERJAAN (TBM)";
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.RequestParameters = true;
                    laporan.CreateDocument();
                    TBMKKPA laporan2 = new TBMKKPA
                    {
                        DataSource = TBMJeniskerja
                    };

                    laporan2.Parameters["PBULAN"].Value = pbulan;
                    laporan2.Parameters["PTAHUN"].Value = ptahun;
                    laporan2.Parameters["BULAN"].Value = bulan;
                    laporan2.Parameters["JUDUL"].Value = "RINCIAN PEKERJAAN (TBM)";
                    laporan2.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan2.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan2.RequestParameters = true;
                    laporan2.CreateDocument();
                    // Add all pages of the 2nd report to the end of the 1st report.
                    laporan.Pages.AddRange(laporan2.Pages);

                    // Reset all page numbers in the resulting document.
                    laporan.PrintingSystem.ContinuousPageNumbering = true;

                    laporan.ShowPreview();
                    //ReportPrintTool tool = new ReportPrintTool(laporan);
                    //tool.ShowPreview();

                }
                if (radioGroup1.SelectedIndex == 2)
                {
                    //generate data
                    TMJeniskerja = Generate_TMJeniskerja(iddata, pbulan,ptahun,"81");
                    //TMJeniskerja.WriteXmlSchema("TMJenisKerja.xsd");


                    TMJenisKerjaINTI laporan = new TMJenisKerjaINTI
                    {
                        DataSource = TMJeniskerja
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = ptahun;
                    laporan.Parameters["BULAN"].Value = bulan;
                    laporan.Parameters["JUDUL"].Value = "RINCIAN PEKERJAAN PEMELIHARAAN (TM)";
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.RequestParameters = true;
                    laporan.CreateDocument();
                    TMJenisKerjaKKPA laporan2 = new TMJenisKerjaKKPA
                    {
                        DataSource = TMJeniskerja
                    };

                    laporan2.Parameters["PBULAN"].Value = pbulan;
                    laporan2.Parameters["PTAHUN"].Value = ptahun;
                    laporan2.Parameters["BULAN"].Value = bulan;
                    laporan2.Parameters["JUDUL"].Value = "RINCIAN PEKERJAAN PEMELIHARAAN (TM)";
                    laporan2.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan2.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan2.RequestParameters = true;
                    laporan2.CreateDocument();
                    // Add all pages of the 2nd report to the end of the 1st report.
                    laporan.Pages.AddRange(laporan2.Pages);

                    // Reset all page numbers in the resulting document.
                    laporan.PrintingSystem.ContinuousPageNumbering = true;

                    laporan.ShowPreview();
                    //ReportPrintTool tool = new ReportPrintTool(laporan);
                    //tool.ShowPreview();

                }
                if (radioGroup1.SelectedIndex == 3)
                {
                    //generate data
                    TMJeniskerja = Generate_TMJeniskerja(iddata, pbulan, ptahun,"80");


                    TMPanen laporan = new TMPanen
                    {
                        DataSource = TMJeniskerja
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = ptahun;
                    laporan.Parameters["BULAN"].Value = bulan;
                    laporan.Parameters["JUDUL"].Value = "RINCIAN PEKERJAAN PANEN";
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new ReportPrintTool(laporan);
                    tool.ShowPreview();
                }
            }
            catch (SystemException ex)
            {
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
            }
        }

        private DataSet Generate_TBM(string iddata, int pbulan, int ptahun)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.REKAP_BIAYA_TBM", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = iddata;
                _command.Parameters.Add("pbulan", OracleDbType.Int16).Value = pbulan;
                _command.Parameters.Add("ptahun", OracleDbType.Int16).Value = ptahun;
                OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();
                OracleDataAdapter sqlAdapter = new OracleDataAdapter();
                sqlcmdbuilder.DataAdapter = sqlAdapter;
                sqlAdapter.SelectCommand = _command;
                DataSet _ds = new DataSet();
                _ds.Clear();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "TMJenis");
                // return dataset result
                return _ds;
            }
        }

        private DataSet Generate_TMJeniskerja(string iddata, int pbulan, int ptahun,string pjenis)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.REKAP_BIAYA_TM_JENIS", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = iddata;
                _command.Parameters.Add("pbulan", OracleDbType.Int16).Value = pbulan;
                _command.Parameters.Add("ptahun", OracleDbType.Int16).Value = ptahun;
                _command.Parameters.Add("pjenis", OracleDbType.Varchar2, 20).Value = pjenis;
                OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();
                OracleDataAdapter sqlAdapter = new OracleDataAdapter();
                sqlcmdbuilder.DataAdapter = sqlAdapter;
                sqlAdapter.SelectCommand = _command;
                DataSet _ds = new DataSet();
                _ds.Clear();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "TMJenis");
                // return dataset result
                return _ds;
            }
        }

        private DataSet Generate_Divisi(string p_iddata, string p_periode)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.REKAP_BIAYA_TM_PERDIVISI", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add("p_periode", OracleDbType.Char, 7).Value = p_periode;
                OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();
                OracleDataAdapter sqlAdapter = new OracleDataAdapter();
                sqlcmdbuilder.DataAdapter = sqlAdapter;
                sqlAdapter.SelectCommand = _command;
                DataSet _ds = new DataSet();
                _ds.Clear();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Divisi");
                // return dataset result
                return _ds;

            }
        }
    }

}
