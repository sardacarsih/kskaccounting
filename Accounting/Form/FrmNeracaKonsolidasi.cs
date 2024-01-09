using Accounting.BusinessLayer;
using DevExpress.DataAccess.Sql;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmNeracaKonsolidasi : XtraForm
    {
        public FrmNeracaKonsolidasi()
        {
            InitializeComponent();
        }
        DataTable konsolidasi;
        private void FrmNeracaKonsolidasi_Load(object sender, EventArgs e)
        {
            try
            {
                konsolidasi = LaporanServices.ViewLap_NeracaKonsolidasi(Acct.p_tahun, CompanyInfo.IDPT, Acct.p_bulan, LoginInfo.userID);

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
                gridControl1.DataSource = konsolidasi;

                //Format dinamis sesuai kolom lokasi
                for (int i = 4; i < columnCount; i++)
                {
                    System.Data.DataColumn column = konsolidasi.Columns[i];
                    gridView1.Columns[i].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridView1.Columns[i].DisplayFormat.FormatString = "n2";
                    gridView1.Columns[i].Summary.Clear();
                    gridView1.Columns[i].Summary.Add(DevExpress.Data.SummaryItemType.Sum, column.ColumnName, "{0:N2}");


                }
                gridView1.Columns[0].Visible = false;
                gridView1.BestFitColumns();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                //hapus menu jika ada
                e.Menu.Items.Clear();

                DXMenuItem ExportXLS = CreateMenuItemExport(view, rowHandle);

                ExportXLS.BeginGroup = true;

                e.Menu.Items.Add(ExportXLS);

            }
        }

        private DXMenuItem CreateMenuItemExport(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Export to Excel", new EventHandler(OnDetailExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnDetailExportClick(object? sender, EventArgs e)
        {
            try
            {
               
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filename = string.Empty;
                using (ExcelPackage package = new ExcelPackage())
                {

                    if (konsolidasi.Rows.Count > 0)
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Neraca Konsolidasi");

                        //Load the datatable and set the number formats...
                        worksheet.Cells["A5"].LoadFromDataTable(konsolidasi, PrintHeaders: true);

                        //wsDt.Cells["A1"].LoadFromCollection(mydata);
                        //wsDt.DeleteColumn(2);

                        //Add the headers
                        worksheet.Cells[1, 1].Value = CompanyInfo.NAMAPT;
                        worksheet.Cells[2, 1].Value = "NERACA KONSOLIDASI";
                        worksheet.Cells[3, 1].Value = Acct.p_periode;
                        // wsDt.Cells[4, 3].Value = "WILAYAH PEMBUKUAN";

                        // wsDt.Cells[2, 2, dt.Rows.Count+ 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //=IF(A2<>A1,1,C1+1)
                        // wsDt.Cells[2, 3, dt.Rows.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, dt.Rows.Count + 1, 3).Address);


                            // number formats
                            string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "[Red](#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;
                            worksheet.Cells[6, 3, konsolidasi.Rows.Count + 5, konsolidasi.Columns.Count].Style.Numberformat.Format = fullNumberFormat;
                            //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                            // 
                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        filename = tempPath + "\\Neraca_Konsolidasi.xlsx";
                        File.WriteAllBytes(@filename, bin);

                    }
                }
                ProcessStartInfo psi = new (@filename);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}