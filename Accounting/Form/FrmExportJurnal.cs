using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmExportJurnal : DevExpress.XtraEditors.XtraForm
    {
        IQueryable<JurnalDetailDTO> JurnalDetail ;
        public FrmExportJurnal()
        {
            InitializeComponent();
        }
        private void FrmExportJurnal_Load(object sender, EventArgs e)
        {
            try
            {
                // Mengisi ComboBox dengan nama bulan untuk cmbbulandari
                cmbbulandari.Properties.Items.AddRange(new[]
                {
            "Januari", "Februari", "Maret", "April", "Mei",
            "Juni", "Juli", "Agustus", "September", "Oktober",
            "Nopember", "Desember"
        });

                // Mengisi ComboBox dengan nama bulan untuk cmbbulansampai (sama dengan cmbbulandari)
                cmbbulansampai.Properties.Items.AddRange(new[]
                {
            "Januari", "Februari", "Maret", "April", "Mei",
            "Juni", "Juli", "Agustus", "September", "Oktober",
            "Nopember", "Desember"
        });

                // Menentukan bulan pertama yang dipilih untuk cmbbulandari dan cmbbulansampai
                cmbbulandari.SelectedIndex = 0;
                cmbbulansampai.SelectedIndex = 0;

                // Mengatur nilai minimum dan maksimum untuk tahun
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;  // Menetapkan tahun maksimum sebagai nilai default
            }
            catch (SystemException ex)
            {
                // Menampilkan pesan error jika terjadi pengecualian
                XtraMessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
           
                Load_Jurnal_Periode();
        }

        private void Load_Jurnal_Periode()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(gridControl1);
                handle.QueueFocus(IntPtr.Zero);

                if (cmbbulandari.SelectedIndex == -1 || cmbbulansampai.SelectedIndex == -1 || setahun.Value == null)
                    return;

                var bulan1 = cmbbulandari.SelectedIndex + 1;
                var bulan2 = cmbbulansampai.SelectedIndex + 1;
                var tahun = Convert.ToInt16(setahun.Value);

                if (bulan2 < bulan1)
                {
                    XtraMessageBox.Show("Bulan akhir harus lebih besar atau sama dengan bulan awal.", "Kesalahan Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                JurnalDetail = JurnalServices
    .GetJurnalDetails_DapperAsQueryable(CompanyInfo.IDDATA, tahun, bulan1, bulan2)
    .OrderBy(j => j.Tanggal)
    .ThenBy(j => j.NoJurnal);

                gridControl1.DataSource = JurnalDetail.ToList();


                // Terapkan sorting di GridView
                gridView1.BeginSort();
                gridView1.ClearSorting(); // atau gridView1.SortInfo.Clear();
                gridView1.Columns["Tanggal"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                gridView1.Columns["NoJurnal"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
                gridView1.EndSort();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Periode();
        }

        private void btnexport_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            //try
            //{
                    // If you use EPPlus in a noncommercial context
                    // according to the Polyform Noncommercial license:
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    string filename = string.Empty;
                    using (ExcelPackage package = new ())
                    {

                        if (JurnalDetail.Count() > 0)
                        {
                            var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                            //Load the datatable and set the number formats...
                            wsDt.Cells["A1"].LoadFromCollection(JurnalDetail, true);

                            //wsDt.Cells["A1"].LoadFromCollection(mydata);
                            wsDt.DeleteColumn(1, 2);

                            //Add the headers
                            wsDt.Cells[1, 1].Value = "NoJurnal";
                            wsDt.Cells[1, 2].Value = "Tanggal";
                            wsDt.Cells[1, 3].Value = "RowNo";
                            wsDt.Cells[1, 4].Value = "Kode";
                            wsDt.Cells[1, 5].Value = "Rekening";
                            wsDt.Cells[1, 6].Value = "Debet";
                            wsDt.Cells[1, 7].Value = "Kredit";
                            wsDt.Cells[1, 8].Value = "Keterangan";
                            wsDt.Cells[1, 9].Value = "Posted";
                            wsDt.Cells[1, 10].Value = "Periode";

                            wsDt.Cells[2, 2, JurnalDetail.Count() + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                            //=IF(A2<>A1,1,C1+1)
                            wsDt.Cells[2, 3, JurnalDetail.Count() + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, JurnalDetail.Count() + 1, 3).Address);

                            // number formats
                            string positiveFormat = "#,##0.00_)";
                            string negativeFormat = "(#,##0.00)";
                            string zeroFormat = "-_)";
                            string numberFormat = positiveFormat + ";" + negativeFormat;
                            string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                            wsDt.Cells[2, 6, JurnalDetail.Count() + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                            //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                            // 
                            wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                            //package.Save();
                            // package.Dispose();

                            Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        filename = tempPath +CompanyInfo.IDDATA + "Jurnal.xlsx";
                            File.WriteAllBytes(@filename, bin);

                        }
                    }
                ProcessStartInfo psi = new(@filename)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            //}
            //catch (Exception ex)
            //{
                
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //}
    
        }
    }
}