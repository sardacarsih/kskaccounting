using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmExportJurnal : DevExpress.XtraEditors.XtraForm
    {
        IQueryable<JurnalDetailDTO> JurnalDetail = null;
        public FrmExportJurnal()
        {
            InitializeComponent();
        }
        private void FrmExportJurnal_Load(object sender, EventArgs e)
        {
            try
            {
                cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
               
                cmbbulan.SelectedIndex = 0;
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var bulan = cmbbulan.SelectedIndex + 1;
                var tahun = Convert.ToInt16(setahun.Value);
                string p_periode = bulan.ToString("0#") + "/" + tahun.ToString();
                JurnalDetail = JurnalServices.GetJurnalDetails_Dapper(CompanyInfo.INIT, p_periode);

                gridControl1.DataSource = JurnalDetail;

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
            try
            {
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
                        filename = tempPath + CompanyInfo.INIT + "Jurnal.xlsx";
                            File.WriteAllBytes(@filename, bin);

                        }
                    }
                ProcessStartInfo psi = new(@filename)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
    
        }
    }
}