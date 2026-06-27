using Accounting.BusinessLayer;
using Accounting.Laporan;
using Accounting.Model;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table.PivotTable;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmReportParam : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public FrmReportParam()
        {
            InitializeComponent();
            searchLookUpEdit2.Properties.View.ActiveFilterEnabled = true;
        }
        DataSet DSLabaRugi, DSNeraca, DSGL =new DataSet();
        int bulan;
        string iddata, periode, posisi;

        private void ApplyAuthorizationState()
        {
            sbcetak.Enabled = AuthorizationService.CanViewReports();
            sbexport.Enabled = AuthorizationService.CanExportReports();
        }

        private void FrmReportParam_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewReports))
            {
                Close();
                return;
            }
            string [] Bulan ={ "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };
            cmbbulan.Properties.Items.AddRange(Bulan);
            cmbbulan2.Properties.Items.AddRange(Bulan);
            bulan = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));
           
            cmbbulan.SelectedIndex = bulan - 1;
            cmbbulan2.SelectedIndex = - 1;
            cmbbulan2.Enabled = false;
            daritahun.Properties.MinValue = Acct.TahunMin;
            daritahun.Properties.MaxValue = Acct.TahunMax;
            daritahun.Value = Acct.TahunMax;
            sampaitahun.Value = 0;
            sampaitahun.Enabled = false;
            Load_Perkiraan();
            ApplyAuthorizationState();
            
        }

        private void searchLookUpEdit2_Popup(object sender, EventArgs e)
        {
            //SearchLookUpEdit lookup = sender as SearchLookUpEdit;
            //ColumnView view = lookup.Properties.View;
            //view.ActiveFilterCriteria = DevExpress.Data.Filtering.CriteriaOperator.Parse("[KEL] == ?", kelompok);
            //searchLookUpEdit2.Properties.View.ActiveFilterEnabled = true;
        }

        private void Load_Perkiraan()
        {
            int ptahun = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
            string p_iddata =CompanyInfo.IDDATA;
            string sql = @"select kodeacc KODE,namaacc PERKIRAAN,POSISI,GRP from acct_coa 
                            where iddata=:iddata and tahun=:ptahun AND ISHEADER = 'D' order by kodeacc asc";
            using (OracleCommand _command = new(sql, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":ptahun", OracleDbType.Int16).Value = ptahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();

                this.searchLookUpEdit1.Properties.DataSource = _dt;
                searchLookUpEdit1.Properties.ValueMember = "KODE";
                searchLookUpEdit1.Properties.DisplayMember = "PERKIRAAN";
                
                searchLookUpEdit1.Properties.PopulateViewColumns();
                searchLookUpEdit1.Properties.View.Columns["POSISI"].Visible = false;
                searchLookUpEdit1.Properties.View.Columns["GRP"].Visible = false;

                //searchLookUpEdit1.Properties.BestFitMode(true);

                this.searchLookUpEdit2.Properties.DataSource = _dt;
                searchLookUpEdit2.Properties.ValueMember = "KODE";
                searchLookUpEdit2.Properties.DisplayMember = "PERKIRAAN";                
                searchLookUpEdit2.Properties.PopulateViewColumns();
                searchLookUpEdit2.Properties.View.Columns["POSISI"].Visible = false;
                searchLookUpEdit2.Properties.View.Columns["GRP"].Visible = false;

            }
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbbulan2.SelectedIndex = cmbbulan.SelectedIndex;
        }
        // Laba Rugi V2 export source: reuses the single-round-trip V2 generator and
        // projects the three export columns (KETERANGAN, BULANINI, TAHUNINI) from the
        // returned "LabaRugi" table instead of reading acc_tmplrnr directly.
        private DataTable ExportLR(string piddata, int pbulan, int ptahun, string puserid, string jenisakunting)
        {
            DataSet ds = LaporanServices.ViewLap_LabaRugi_V2(piddata, pbulan, ptahun, puserid, jenisakunting);
            DataTable src = ds.Tables["LabaRugi"];

            DataTable _dt = new();
            _dt.Columns.Add("KETERANGAN", typeof(string));
            _dt.Columns.Add("BULANINI", typeof(decimal));
            _dt.Columns.Add("TAHUNINI", typeof(decimal));

            if (src != null)
            {
                foreach (DataRow r in src.Rows)
                {
                    _dt.Rows.Add(
                        r["SUB1"] == DBNull.Value ? string.Empty : r["SUB1"].ToString(),
                        r["BULANINI"] == DBNull.Value ? 0m : Convert.ToDecimal(r["BULANINI"]),
                        r["TAHUNINI"] == DBNull.Value ? 0m : Convert.ToDecimal(r["TAHUNINI"]));
                }
            }
            return _dt;
        }
        private void sbexport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanExportReports))
            {
                return;
            }
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                int pbulan = cmbbulan.SelectedIndex + 1;
                int p_sampaibulan = cmbbulan2.SelectedIndex + 1;
                int p_daritahun = Convert.ToInt32(daritahun.Value);
                int p_sampaitahun = Convert.ToInt32(sampaitahun.Value);
                var lastDayOfMonth = DateTime.DaysInMonth(p_daritahun, pbulan);
                var bulan = "Periode : " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                var bulanneraca = lastDayOfMonth + " " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                var periode = pbulan.ToString("00") + "/" + p_daritahun.ToString();

                var iddata =CompanyInfo.IDDATA;
                var jenis = CompanyInfo.JENIS_AKUNTING;
                var userid = LoginInfo.userID;
                var adacoa = AccountServices.CekCOAExist(iddata, p_daritahun);
                if (adacoa == 1) //1 tidak ada coa 
                {
                    XtraMessageBox.Show("Daftar Perkiraan belum tersedia", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
               

                // number formats
                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "[Red](#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                if (radioGroup1.SelectedIndex == 0)
                {
                    //cek record jurnal exist ?
                    var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    if (record == 0)
                    {
                        XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        return;
                    }

                    //generate + get data in a single round-trip (Laba Rugi V2)
                    DataTable dt = ExportLR(iddata, pbulan, p_daritahun, userid, jenis);

                    using ExcelPackage package = new ();


                    if (dt.Rows.Count > 0)
                    {
                        var wsDt = package.Workbook.Worksheets.Add("LabaRugi");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].Value = CompanyInfo.NAMAPT;
                        wsDt.Cells["A2"].Value = CompanyInfo.WILAYAH;
                        wsDt.Cells["A3"].Value = "LAPORAN LABA/RUGI";
                        wsDt.Cells["A4"].Value = "Periode :" + cmbbulan.Text + " " + daritahun.Text;
                        wsDt.Cells["A3:C3"].Merge = true;
                        wsDt.Cells["A4:C4"].Merge = true;
                        wsDt.Cells["A3:C4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        wsDt.Cells["A3:C4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsDt.Cells["A3"].Style.Font.Bold = true;
                        wsDt.Cells["A3"].Style.Font.Size = 14;
                        wsDt.Cells["A6"].LoadFromDataTable(dt, true, TableStyle: OfficeOpenXml.Table.TableStyles.Medium17);
                        wsDt.Cells[3, 2, dt.Rows.Count + 1, 3].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        string file = tempPath + "LabaRugi.xlsx";
                        File.WriteAllBytes(file, bin);

                        //These lines will open it in Excel
                        ProcessStartInfo pi = new(file)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(pi);

                    }
                }
                if (radioGroup1.SelectedIndex == 1)
                {
                    ////cek record jurnal exist ?

                    //var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    //if (record == 0) { XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_Neraca(iddata, pbulan, p_daritahun, userid);
                    DataTable dt = new DataTable();
                    dt = DSNeraca.Tables[0];

                    using ExcelPackage package = new();

                    if (dt.Rows.Count > 0)
                    {
                        var wsDt = package.Workbook.Worksheets.Add("Neraca");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A6"].LoadFromDataTable(dt, true, TableStyle: OfficeOpenXml.Table.TableStyles.Medium17);
                        wsDt.DeleteColumn(1, 2);
                        wsDt.Cells[2, 6, dt.Rows.Count + 1, 8].Style.Numberformat.Format = fullNumberFormat;
                        wsDt.Cells["A1"].Value = CompanyInfo.NAMAPT;
                        wsDt.Cells["A2"].Value = CompanyInfo.WILAYAH;
                        wsDt.Cells["A3"].Value = "LAPORAN NERACA";
                        wsDt.Cells["A4"].Value = "Periode :" + cmbbulan.Text + " " + daritahun.Text;
                        wsDt.Cells["A3:H3"].Merge = true;
                        wsDt.Cells["A4:H4"].Merge = true;
                        wsDt.Cells["A3:H4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        wsDt.Cells["A3:H4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        wsDt.Cells["A3"].Style.Font.Bold = true;
                        wsDt.Cells["A3"].Style.Font.Size = 14;


                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        string file = tempPath + "Neraca.xlsx";
                        File.WriteAllBytes(file, bin);

                        //These lines will open it in Excel
                        ProcessStartInfo pi = new(file)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(pi);

                    }

                }
                if (radioGroup1.SelectedIndex == 2)
                {
                    ////cek record jurnal exist ?

                    //var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    //if (record == 0) { XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_Neraca(iddata, pbulan, p_daritahun, userid);
                    //DSNeraca.WriteXmlSchema("Neraca.xsd");


                    BalanceSheetIndukSkontro neraca = new BalanceSheetIndukSkontro
                    {
                        DataSource = DSNeraca
                    };
                    foreach (var subreport in neraca.AllControls<XRSubreport>())
                    {
                        subreport.BeforePrint += subreport_BeforePrint;
                    }


                    neraca.Parameters["PBULAN"].Value = pbulan;
                    neraca.Parameters["PTAHUN"].Value = p_daritahun;
                    neraca.Parameters["BULAN"].Value = bulanneraca;
                    neraca.Parameters["PERIODE"].Value = periode;
                    neraca.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    neraca.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    neraca.Parameters["USERID"].Value = userid;
                    neraca.RequestParameters = true;
                    ReportPrintTool tool = new (neraca);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 3)
                {
                    var neracasaldo = LaporanServices.NeracaSaldoTahun(iddata, p_daritahun);
                    using ExcelPackage package = new();

                    if (neracasaldo.Any())
                    {
                        var wsDt = package.Workbook.Worksheets.Add("Neraca Saldo");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A6"].LoadFromCollection(neracasaldo, true, TableStyle: OfficeOpenXml.Table.TableStyles.Medium17);
                     
                        wsDt.Cells[7, 4, neracasaldo.Count + 6, 16].Style.Numberformat.Format = fullNumberFormat;
                        wsDt.Cells["A1"].Value = CompanyInfo.NAMAPT;
                        wsDt.Cells["A2"].Value = CompanyInfo.WILAYAH;
                        wsDt.Cells["A3"].Value = "NERACA SALDO";
                        wsDt.Cells["A4"].Value = "Periode :" +  daritahun.Text;
                        //wsDt.Cells["A3:H3"].Merge = true;
                        //wsDt.Cells["A4:H4"].Merge = true;
                        //wsDt.Cells["A3:H4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //wsDt.Cells["A3:H4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        //wsDt.Cells["A3"].Style.Font.Bold = true;
                        //wsDt.Cells["A3"].Style.Font.Size = 14;


                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        // Obtain the Excel file data as a byte array
                        byte[] excelData = package.GetAsByteArray();

                        // Generate a temporary file path
                        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                        // Write the byte array to the temporary file
                        File.WriteAllBytes(tempFilePath, excelData);

                        // Open the temporary file with the default associated Excel program
                        ProcessStartInfo psi = new(tempFilePath)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(psi);

                    }

                }
                if (radioGroup1.SelectedIndex == 4)
                {
                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_NeracaHalfYear(iddata, p_daritahun, userid, 2);
                    //DSNeraca.WriteXmlSchema("NeracaH2.xsd");


                    BalanceSheetHalf2 laporan = new()
                    {
                        DataSource = DSNeraca
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = p_daritahun;
                    laporan.Parameters["BULAN"].Value = "Tahun : " + p_daritahun;
                    laporan.Parameters["PERIODE"].Value = periode;
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.Parameters["USERID"].Value = userid;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new (laporan);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 5)
                {
                    ExportNeracaKonsolidasi(pbulan, p_daritahun,periode);
                }
                if (radioGroup1.SelectedIndex == 6)
                {
                    if (searchLookUpEdit1.EditValue == null || searchLookUpEdit2.EditValue == null)
                    {
                        XtraMessageBox.Show("Silahkan pilih kode Akun", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string rangebulan;

                    if (daritahun.Value > sampaitahun.Value)
                    {
                        XtraMessageBox.Show("Pilihan tahun salah", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (daritahun.Value == sampaitahun.Value)
                    {
                        if (cmbbulan.SelectedIndex > cmbbulan2.SelectedIndex)
                        {
                            XtraMessageBox.Show("Pilihan bulan salah", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                    }

                    if (daritahun.Value == sampaitahun.Value)
                    {
                        if (cmbbulan.SelectedIndex == cmbbulan2.SelectedIndex)
                        {
                            rangebulan = "Periode : " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                        }
                        else
                        {
                            rangebulan = "Periode : " + cmbbulan.Text + " s/d " + cmbbulan2.Text + " " + daritahun.Value.ToString();
                        }
                    }
                    else
                    {
                        rangebulan = "Periode : " + cmbbulan.Text + " " + daritahun.Value.ToString() + " s/d " + cmbbulan2.Text + " " + sampaitahun.Value.ToString();

                    }

                    //cek record jurnal exist ?

                    var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    if (record == 0) { XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                    var darikode = searchLookUpEdit1.EditValue.ToString();
                    var sampaikode = searchLookUpEdit2.EditValue.ToString();

                    string[] myArray_neraca = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };

                    bool isneraca = myArray_neraca.Contains(kelompok);
                    var isakun_neraca = string.Empty;
                    if (isneraca)
                    {
                        isakun_neraca = "NERACA";
                    }
                    else
                    {
                        isakun_neraca = "NON NERACA";
                    }
                    DSGL = LaporanServices.ViewLap_BukuBesarMultiTahun(iddata, p_daritahun, p_sampaitahun, pbulan, p_sampaibulan, darikode, sampaikode, userid, isakun_neraca);

                    // If you use EPPlus in a noncommercial context
                    // according to the Polyform Noncommercial license:
                    // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using ExcelPackage package = new();
                    //Here goes the ExcelPackage code etc

                    DataTable dt = DSGL.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        var wsDt = package.Workbook.Worksheets.Add("General Ledger");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].LoadFromDataTable(dt, true);
                        wsDt.DeleteColumn(1, 2);//delete column1 dan 2
                        wsDt.Cells[2, 5, dt.Rows.Count + 1, 5].Style.Numberformat.Format = "dd-MMM-yyyy";
                        wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                        wsDt.Cells[2, 8, dt.Rows.Count + 1, 8].Style.Numberformat.Format = fullNumberFormat;
                        //subtotal debet
                        wsDt.Cells[dt.Rows.Count + 2, 7, dt.Rows.Count + 2, 7].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(2, 7, dt.Rows.Count + 1, 7).Address);
                        wsDt.Cells[dt.Rows.Count + 2, 7, dt.Rows.Count + 2, 7].Style.Numberformat.Format = "#,##0.00";
                        wsDt.Cells[dt.Rows.Count + 2, 7, dt.Rows.Count + 2, 7].Style.Font.Bold = true;
                        // 
                        //subtotal kredit
                        wsDt.Cells[dt.Rows.Count + 2, 8, dt.Rows.Count + 2, 8].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(2, 8, dt.Rows.Count + 1, 8).Address);
                        wsDt.Cells[dt.Rows.Count + 2, 8, dt.Rows.Count + 2, 8].Style.Numberformat.Format = "#,##0.00";
                        wsDt.Cells[dt.Rows.Count + 2, 8, dt.Rows.Count + 2, 8].Style.Font.Bold = true;
                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        string file = tempPath + "BukuBesar.xlsx";
                        File.WriteAllBytes(file, bin);

                        //These lines will open it in Excel
                        ProcessStartInfo pi = new(file)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(pi);
                    }
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        DataTable konsolidasi;
        private void ExportNeracaKonsolidasi(int pbulan, int p_daritahun,string periode)
        {
            try
            {
                konsolidasi = LaporanServices.ViewLap_NeracaKonsolidasi(p_daritahun, CompanyInfo.IDPT, pbulan, LoginInfo.userID);

                konsolidasi.Columns.Add("TOTAL", typeof(double));

                int columnCount = konsolidasi.Columns.Count;


                //menghitung total transaksi setiap lokasi dinamis
                foreach (DataRow row in konsolidasi.Rows)
                {
                    double summaryValue = 0.0;

                    for (int i = 4; i < columnCount - 1; i++)
                    {
                        string value2;
                        string value = row[i].ToString();
                        if (string.IsNullOrEmpty(value))
                        {
                            value2 = "0";
                        }
                        else
                        {
                            value2 = value;
                        }

                        summaryValue += Convert.ToDouble(value2);

                    }
                    //Locate the specific column in which you want to update the summary
                    DataColumn summaryColumn = konsolidasi.Columns["TOTAL"];

                    //Update the value of that column for the identified row with the calculated summary value
                    row[summaryColumn] = summaryValue;
                }

                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filename = string.Empty;
                using (ExcelPackage package = new ())
                {

                    if (konsolidasi.Rows.Count > 0)
                    {
                        // add a new worksheet to the Excel package
                        var worksheet1 = package.Workbook.Worksheets.Add("PivotNeraca");
                        var worksheet2 = package.Workbook.Worksheets.Add("Neraca Konsolidasi");


                        //Add the headers
                        worksheet2.Cells[1, 1].Value = CompanyInfo.NAMAPT;
                        worksheet2.Cells[2, 1].Value = "NERACA KONSOLIDASI";
                        worksheet2.Cells[3, 1].Value = "Periode :" + periode;

                        var dataRange = worksheet2.Cells["A5"].LoadFromDataTable(konsolidasi, PrintHeaders: true);


                        // number formats
                        string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "[Red](#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;
                        worksheet2.Cells[6, 3, dataRange.End.Row, konsolidasi.Columns.Count].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();
                        worksheet1.Cells[1, 1].Value = CompanyInfo.NAMAPT;
                        worksheet1.Cells[2, 1].Value = "NERACA KONSOLIDASI";
                        worksheet1.Cells[3, 1].Value = "Periode :" + periode; ;

                        // create pivot table in Sheet1
                        var pivotTable = worksheet1.PivotTables.Add(worksheet1.Cells["A5"], dataRange, "Perlokasi");

                        pivotTable.RowFields.Add(pivotTable.Fields["KATEGORI"]);
                        pivotTable.RowFields.Add(pivotTable.Fields["KELOMPOK"]);
                        pivotTable.RowFields.Add(pivotTable.Fields["REKENING"]);

                        ExcelPivotTableDataField dataField;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKSARANG"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKPBUN"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKJAKARTA"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKPKS"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKINTI"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["KSKPUSAT"]);
                        dataField.Format = fullNumberFormat;
                        dataField = pivotTable.DataFields.Add(pivotTable.Fields["TOTAL"]);
                        dataField.Format = fullNumberFormat;

                        //We want the datafields to appear in columns
                        pivotTable.DataOnRows = false;

                        // Set ShowGrandTotalsRows and ShowGrandTotalsColumns to false
                        pivotTable.RowGrandTotals = false;
                        pivotTable.ColumnGrandTotals = false;


                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        filename = tempPath + "\\Neraca_Konsolidasi.xlsx";
                        File.WriteAllBytes(@filename, bin);

                    }
                }
                ProcessStartInfo psi = new(@filename)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        string kelompok;
        private void searchLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            GridView view = searchLookUpEdit1View;
            int rowhandle = view.FocusedRowHandle;
            //string jenis = ;
            posisi = view.GetRowCellValue(rowhandle, "POSISI").ToString();
            kelompok = view.GetRowCellValue(rowhandle, "GRP").ToString();
            searchLookUpEdit2.EditValue = searchLookUpEdit1.EditValue;
        }

        private void cmbbulan2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            int pbulan = cmbbulan.SelectedIndex + 1;
            int ptahun = Convert.ToInt32(daritahun.Value);
            var bulan = "Periode : " + cmbbulan.Text + "-" + daritahun.Value.ToString();
            periode = pbulan.ToString("00") + "/" + ptahun.ToString();

            iddata =CompanyInfo.IDDATA;
            var iskebun = CompanyInfo.JENIS_AKUNTING;
            var userid = LoginInfo.userID;
            //get data for report
            DSNeraca = LaporanServices.ViewLap_Neraca(iddata, pbulan, ptahun, userid);
            //DSNeraca.WriteXmlSchema("Neraca.xsd");


            BalanceSheetIndukSkontro neraca = new BalanceSheetIndukSkontro
            {
                DataSource = DSNeraca
            };
            foreach (var subreport in neraca.AllControls<XRSubreport>())
            {
                subreport.BeforePrint += subreport_BeforePrint;
            }


            neraca.Parameters["PBULAN"].Value = pbulan;
            neraca.Parameters["PTAHUN"].Value = ptahun;
            neraca.Parameters["BULAN"].Value = bulan;
            neraca.Parameters["PERIODE"].Value = periode;
            neraca.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
            neraca.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
            neraca.Parameters["USERID"].Value = userid;
            neraca.RequestParameters = true;
            ReportPrintTool tool = new ReportPrintTool(neraca);
            tool.ShowPreview();
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
                lblcompany.Text = "Laporan Laba / Rugi";
                cmbbulan.Enabled = true;
                panel1.Visible = false;
                cmbbulan2.SelectedIndex = -1;
                cmbbulan2.Enabled = false;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 1)
            {
                lblcompany.Text = "Laporan Neraca";
                cmbbulan.Enabled = true;
                panel1.Visible = false;
                cmbbulan2.SelectedIndex = -1;
                cmbbulan2.Enabled = false;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 2)
            {
                lblcompany.Text = "Laporan Neraca (Skontro)";
                cmbbulan.Enabled = true;
                panel1.Visible = false;
                cmbbulan2.SelectedIndex = -1;
                cmbbulan2.Enabled = false;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = false;
            }
            if (radioGroup1.SelectedIndex == 3)
            {
                lblcompany.Text = "Laporan Neraca Saldo";
                cmbbulan.SelectedIndex = 0;
                cmbbulan2.SelectedIndex = 5;
                cmbbulan.Enabled = false;
                cmbbulan2.Enabled = false;
                panel1.Visible = false;
                cmbbulan.SelectedIndex = 0;
                cmbbulan2.SelectedIndex = 11;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 4)
            {
                lblcompany.Text = "Laporan Neraca (Semester 2)";
                cmbbulan.SelectedIndex = 6;
                cmbbulan2.SelectedIndex = 11;
                cmbbulan.Enabled = false;
                cmbbulan2.Enabled = false;
                panel1.Visible = false;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = false;
            }
            if (radioGroup1.SelectedIndex == 5)
            {
                lblcompany.Text = "Laporan Neraca Konsolidasi";
                cmbbulan.Enabled = true;
                panel1.Visible = false;
                cmbbulan2.SelectedIndex = -1;
                cmbbulan2.Enabled = false;
                sampaitahun.Value = 0;
                sampaitahun.Enabled = false;
                sbexport.Enabled = true;
            }
            if (radioGroup1.SelectedIndex == 6)
            {
                lblcompany.Text = "Laporan Buku Besar";
                cmbbulan.Enabled = true;
                cmbbulan2.Enabled = true;
                cmbbulan.SelectedIndex = 0;
                cmbbulan2.SelectedIndex = 11;
                panel1.Visible = true;
                daritahun.Properties.MinValue = Acct.TahunMin;
                daritahun.Properties.MaxValue = Acct.TahunMax;
                daritahun.Value = Acct.TahunMax;

                sampaitahun.Properties.MinValue = Acct.TahunMin;
                sampaitahun.Properties.MaxValue = Acct.TahunMax;
                sampaitahun.Value = Acct.TahunMax;
                sampaitahun.Enabled = true;
                sbexport.Enabled = true;
            }

            sbexport.Enabled = sbexport.Enabled && AuthorizationService.CanExportReports();
        }
        private SoundPlayer Player = new SoundPlayer();
       // private FileInfo memoryStreamObject;

        private void sbcetak_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewReports))
            {
                return;
            }

            try
            {
                int pbulan = cmbbulan.SelectedIndex + 1;
                int p_sampaibulan = cmbbulan2.SelectedIndex + 1;
                int p_daritahun = Convert.ToInt32(daritahun.Value);
                int p_sampaitahun = Convert.ToInt32(sampaitahun.Value);
                var lastDayOfMonth = DateTime.DaysInMonth(p_daritahun, pbulan);
                var bulan = "Periode : " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                var bulanneraca =  lastDayOfMonth+" " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                var periode = pbulan.ToString("00") + "/" + p_daritahun.ToString();

                var iddata =CompanyInfo.IDDATA;
                var jenis = CompanyInfo.JENIS_AKUNTING;
                var userid = LoginInfo.userID;

                
               SplashScreenManager.ShowForm(typeof(WaitForm_Load));
                Cursor.Current = Cursors.WaitCursor;

                var adacoa = AccountServices.CekCOAExist(iddata, p_daritahun);
                if (adacoa == 1) //1 tidak ada coa 
                {
                    XtraMessageBox.Show("Daftar Perkiraan belum tersedia", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (radioGroup1.SelectedIndex == 0)
                {
                    //bool akses = LevelAksesServices.CetakExport(34, LoginInfo.userID);
                    //if (akses == false)
                    //{
                    //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    //    this.Player.Play();
                    //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}

                    //cek record jurnal exist ?
                    var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    if (record == 0) 
                    { 
                        XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);                         
                        return; 
                    }

                    //generate + get data in a single round-trip (Laba Rugi V2)
                    DSLabaRugi = LaporanServices.ViewLap_LabaRugi_V2(iddata, pbulan, p_daritahun, userid, CompanyInfo.JENIS_AKUNTING);

                    Income_statement2 laporan = new Income_statement2
                    {
                        DataSource = DSLabaRugi
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = p_daritahun;
                    laporan.Parameters["BULAN"].Value = bulan;
                    laporan.Parameters["PERIODE"].Value = periode;
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new ReportPrintTool(laporan);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 1)
                {
                    //bool akses = LevelAksesServices.CetakExport(33, LoginInfo.userID);
                    //if (akses == false)
                    //{
                    //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    //    this.Player.Play();
                    //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    ////cek record jurnal exist ?

                    //var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    //if (record == 0) { XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_Neraca(iddata, pbulan, p_daritahun, userid);
                    //DSNeraca.WriteXmlSchema("Neraca.xsd");


                    BalanceSheet laporan = new()
                    {
                        DataSource = DSNeraca
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = p_daritahun;
                    laporan.Parameters["BULAN"].Value = bulanneraca;
                    laporan.Parameters["PERIODE"].Value = periode;
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.Parameters["USERID"].Value = userid;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new(laporan);
                    tool.ShowPreview();
                    

                }
                if (radioGroup1.SelectedIndex == 2)
                {
                    //bool akses = LevelAksesServices.CetakExport(33, LoginInfo.userID);
                    //if (akses == false)
                    //{
                    //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    //    this.Player.Play();
                    //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    ////cek record jurnal exist ?

                    //var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    //if (record == 0) { XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_Neraca(iddata, pbulan, p_daritahun, userid);
                    //DSNeraca.WriteXmlSchema("Neraca.xsd");


                    BalanceSheetIndukSkontro neraca = new BalanceSheetIndukSkontro
                    {
                        DataSource = DSNeraca
                    };
                    foreach (var subreport in neraca.AllControls<XRSubreport>())
                    {
                        subreport.BeforePrint += subreport_BeforePrint;
                    }


                    neraca.Parameters["PBULAN"].Value = pbulan;
                    neraca.Parameters["PTAHUN"].Value = p_daritahun;
                    neraca.Parameters["BULAN"].Value = bulanneraca;
                    neraca.Parameters["PERIODE"].Value = periode;
                    neraca.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    neraca.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    neraca.Parameters["USERID"].Value = userid;
                    neraca.RequestParameters = true;
                    ReportPrintTool tool = new ReportPrintTool(neraca);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 3)
                {
                    XtraMessageBox.Show("Module Belum Aktif", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //bool akses = LevelAksesServices.CetakExport(33, LoginInfo.userID);
                    //if (akses == false)
                    //{
                    //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    //    this.Player.Play();
                    //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    ////get data for report
                    //var neracasaldo = LaporanServices.NeracaSaldoTahun(iddata, p_daritahun);
                    //// DSNeraca.WriteXmlSchema("NeracaH1.xsd");


                    //NeracaSaldoTahun laporan = new NeracaSaldoTahun
                    //{
                    //    DataSource = neracasaldo
                    //};

                    ////laporan.Parameters["PBULAN"].Value = pbulan;
                    ////laporan.Parameters["PTAHUN"].Value = p_daritahun;
                    ////laporan.Parameters["BULAN"].Value = "Tahun : " + p_daritahun;
                    ////laporan.Parameters["PERIODE"].Value = periode;
                    ////laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    ////laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    ////laporan.Parameters["USERID"].Value = userid;
                    //laporan.RequestParameters = true;
                    //ReportPrintTool tool = new ReportPrintTool(laporan);
                    //tool.ShowPreview();


                }
                if (radioGroup1.SelectedIndex == 4)
                {
                    //get data for report
                    DSNeraca = LaporanServices.ViewLap_NeracaHalfYear(iddata, p_daritahun, userid, 2);
                   // DSNeraca.WriteXmlSchema("NeracaH2.xsd");


                    BalanceSheetHalf2 laporan = new BalanceSheetHalf2
                    {
                        DataSource = DSNeraca
                    };

                    laporan.Parameters["PBULAN"].Value = pbulan;
                    laporan.Parameters["PTAHUN"].Value = p_daritahun;
                    laporan.Parameters["BULAN"].Value = "Tahun : " + p_daritahun;
                    laporan.Parameters["PERIODE"].Value = periode;
                    laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                    laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                    laporan.Parameters["USERID"].Value = userid;
                    laporan.RequestParameters = true;
                    ReportPrintTool tool = new ReportPrintTool(laporan);
                    tool.ShowPreview();
                }
                if (radioGroup1.SelectedIndex == 5)
                {
                    Acct.p_bulan = pbulan;
                    Acct.p_tahun = p_daritahun;
                    Acct.p_periode = "Periode :"+pbulan.ToString("0#") + "/" + p_daritahun.ToString();
                    FrmNeracaKonsolidasi f = new();
                    f.ShowDialog();

                }
                if (radioGroup1.SelectedIndex == 6)
                {
                    //bool akses = LevelAksesServices.CetakExport(31, LoginInfo.userID);
                    //if (akses == false)
                    //{
                    //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    //    this.Player.Play();
                    //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    if (searchLookUpEdit1.EditValue==null || searchLookUpEdit2.EditValue == null)
                    {
                        XtraMessageBox.Show("Silahkan pilih kode Akun", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    //cek record jurnal exist ?
                    //var record = JurnalServices.CekRecordJurnalExist(iddata, periode);
                    //if (record == 0)
                    //{
                    //    XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}
                    string rangebulan;

                    if (daritahun.Value > sampaitahun.Value)
                    {
                        XtraMessageBox.Show("Pilihan tahun salah", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (daritahun.Value == sampaitahun.Value)
                    {
                        if (cmbbulan.SelectedIndex > cmbbulan2.SelectedIndex)
                        {
                            XtraMessageBox.Show("Pilihan bulan salah", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                    }

                    if (daritahun.Value == sampaitahun.Value)
                    {
                        if (cmbbulan.SelectedIndex == cmbbulan2.SelectedIndex)
                        {
                            rangebulan = "Periode : " + cmbbulan.Text + "-" + daritahun.Value.ToString();
                        }
                        else
                        {                           
                           rangebulan = "Periode : " + cmbbulan.Text + " s/d " + cmbbulan2.Text + " " + daritahun.Value.ToString();
                        }
                    }
                    else
                    {
                        rangebulan = "Periode : " + cmbbulan.Text + " "+ daritahun.Value.ToString()+" s/d " + cmbbulan2.Text + " " + sampaitahun.Value.ToString();

                    }
                    
                   
                    var darikode = searchLookUpEdit1.EditValue.ToString();
                    var sampaikode = searchLookUpEdit2.EditValue.ToString();

                    string[] myArray_neraca = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };

                    bool isneraca = myArray_neraca.Contains(kelompok);
                    var isakun_neraca = string.Empty;
                    if (isneraca)
                    {
                        isakun_neraca = "NERACA";
                    }
                    else
                    {
                        isakun_neraca = "NON NERACA";
                    }


                    //get data for report
                    //DSGL = LaporanServices.ViewLap_BukuBesar(iddata,ptahun,pbulan,p_sampaibulan,darikode,sampaikode,userid,"NERACA");
                    DSGL = LaporanServices.ViewLap_BukuBesarMultiTahun(iddata, p_daritahun, p_sampaitahun, pbulan, p_sampaibulan, darikode, sampaikode, userid, isakun_neraca);
                   // DSGL.WriteXmlSchema("GeneralLedger.xsd");
                  

                    if (posisi == "D")
                    {
                        GeneralLedgerD2 laporan = new()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = p_daritahun;
                        laporan.Parameters["BULAN"].Value = rangebulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new(laporan);
                        tool.ShowPreview();
                    }
                    else
                    {
                        GeneralLedgerK2 laporan = new()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = p_daritahun;
                        laporan.Parameters["BULAN"].Value = rangebulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new(laporan);
                        tool.ShowPreview();
                    }
                    

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

       
    }

}
