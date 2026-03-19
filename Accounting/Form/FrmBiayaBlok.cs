
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Accounting.Utilities;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.Form
{
    public partial class FrmBiayaBlok : DevExpress.XtraEditors.XtraForm
    {
        public FrmBiayaBlok()
        {
            InitializeComponent();
            PeriodeHelper.LoadMonthNamesIntoComboBox(cmbbulan1);
            PeriodeHelper.LoadMonthNamesIntoComboBox(cmbbulan2);
            PeriodeHelper.LoadTahunintoSpinEdit(setahun1);
            PeriodeHelper.LoadTahunintoSpinEdit(setahun2);
        }
        private readonly OracleConnection conn = new(LoginInfo.OracleConnString);
        private void FrmBiayaBlok_Load(object sender, EventArgs e)
        {
            setahun1.Properties.MinValue = Acct.TahunMin;
            setahun1.Properties.MaxValue = Acct.TahunMax;

            setahun2.Properties.MinValue = Acct.TahunMin;
            setahun2.Properties.MaxValue = Acct.TahunMax;

            setahun2.EditValue = Acct.TahunMax;
            setahun1.EditValue = Acct.TahunMax - 5;

        }

        private DataTable Load_BiayaBlok(string p_IDDATA, int dari, int sampai, string p_isfrom)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.REKAP_BIAYA_BLOK", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
                _command.Parameters.Add(":p_dari", OracleDbType.Int16).Value = dari;
                _command.Parameters.Add(":p_sampai", OracleDbType.Int16).Value = sampai;
                _command.Parameters.Add(":p_isfrom", OracleDbType.Varchar2, 20).Value = p_isfrom;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        private async void btnproses_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            try
            {
                handle = SplashScreenManager.ShowOverlayForm(this);

                // Load journal details asynchronously
                var journalDetails = await AlokasiBiayaBlok.GetJournalDetailsAsync(dariperiode, sampaiperiode, CompanyInfo.IDDATA);

                // Bind the result to the pivot grid
                gridControl1.DataSource = journalDetails;
                gridView1.BestFitColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (handle != null)
                    SplashScreenManager.CloseOverlayForm(handle);
            }
        }


        private void sbexport_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            try
            {
                handle = SplashScreenManager.ShowOverlayForm(this);

                var pivotExportOptions = new DevExpress.XtraPivotGrid.PivotXlsxExportOptions
                {
                    ExportType = DevExpress.Export.ExportType.WYSIWYG
                };

                string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "pivot.xlsx");
                gridControl1.ExportToXlsx(exportPath, pivotExportOptions);

                ProcessStartInfo pi = new ProcessStartInfo(exportPath)
                {
                    UseShellExecute = true
                };
                Process.Start(pi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (handle != null)
                {
                    SplashScreenManager.CloseOverlayForm(handle);
                }
            }
        }


        private void cmbbulan1_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeperiode();
        }

        private int dariperiode, sampaiperiode;

        private void changeperiode()
        {
            // Validate and get first period
            if (!TryGetPeriod(cmbbulan1.SelectedIndex, setahun1.EditValue, out dariperiode))
            {
                // Handle invalid first period (e.g., show error message)
                return;
            }

            // Validate and get second period
            if (!TryGetPeriod(cmbbulan2.SelectedIndex, setahun2.EditValue, out sampaiperiode))
            {
                // Handle invalid second period
                return;
            }

            // Optional: Ensure dariperiode <= sampaiperiode
            if (dariperiode > sampaiperiode)
            {
                // Handle invalid period range
            }
        }

        private bool TryGetPeriod(int monthIndex, object yearValue, out int period)
        {
            period = 0;

            // Validate month selection
            if (monthIndex < 0 || monthIndex > 11) // Months 0-11 correspond to 1-12
            {
                //MessageBox.Show("Please select a valid month");
                return false;
            }

            // Validate year value
            if (yearValue == null || !int.TryParse(yearValue.ToString(), out int year) || year < 1000 || year > 9999)
            {
               // MessageBox.Show("Please enter a valid 4-digit year");
                return false;
            }

            // Create period (YYYYMM format)
            int month = monthIndex + 1;
            period = year * 100 + month;
            return true;
        }

        private void setahun1_EditValueChanged(object sender, EventArgs e)
        {
            changeperiode();
        }

        private void cmbbulan2_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeperiode();
        }

        private void setahun2_EditValueChanged(object sender, EventArgs e)
        {
            changeperiode();
        }
    }
}