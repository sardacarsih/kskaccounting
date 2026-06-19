using Accounting.BusinessLayer;
using Accounting.CoaImport.Domain;
using Accounting.CoaImport.Infrastructure;
using Accounting.CoaImport.Presentation;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmImportCOA : DevExpress.XtraEditors.XtraForm
    {
        private FrmImportCoaViewModel viewModel;

        public FrmImportCOA()
        {
            InitializeComponent();
        }

        private void FrmImportCOA_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanImportCoa))
            {
                Close();
                return;
            }

            int tahun = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 6, 4));
            setahun.Value = tahun;

            viewModel = CoaImportModuleFactory.CreateViewModel(
                LoginInfo.OracleConnString,
                CompanyInfo.IDDATA,
                LoginInfo.userID,
                CompanyInfo.JENIS_AKUNTING,
                tahun);
        }

        private void sbbrowse_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanImportCoa))
            {
                return;
            }

            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {
                gridControl1.DataSource = null;
                cboSheet.Text = "";
                using OpenFileDialog ofd = new() { Filter = "Excel Workbook|*.xlsx| Excel 97-2003 Workbook|*.xls" };
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                txtPath.Text = ofd.FileName;
                viewModel.LoadWorkbook(ofd.FileName);

                cboSheet.Properties.Items.Clear();
                foreach (string sheet in viewModel.Sheets)
                {
                    cboSheet.Properties.Items.Add(sheet);
                }

                if (viewModel.Sheets.Count > 0)
                {
                    cboSheet.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                SBImport.Enabled = false;
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SBImport_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanImportCoa))
            {
                return;
            }

            int tahun = (int)setahun.Value;
            if (XtraMessageBox.Show("Lanjutkan Proses Import Daftar Perkiraan ? " +
                "\n\nTahun : " + tahun + " " +
                "\nLokasi Data :" + CompanyInfo.IDDATA,
                "Confirm Proses",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            bool isSuccess = false;
            SetImportControlsEnabled(false);
            ShowImportProgress(new CoaImportProgress(0, "Menyiapkan import COA...", 0, viewModel.Rows.Count));
            try
            {
                viewModel.Year = tahun;
                viewModel.Mode = xeparsial.Checked ? CoaImportMode.Partial : CoaImportMode.Full;
                Progress<CoaImportProgress> progress = new(ShowImportProgress);

                CoaImportResult result = await Task.Run(() => viewModel.Import(progress));
                if (!result.IsSuccess)
                {
                    XtraMessageBox.Show(BuildImportErrorMessage(result), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!xeparsial.Checked)
                {
                    Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
                    Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
                    Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
                    Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);
                }

                TimeSpan timeSpan = result.Elapsed;
                string waktuproses = string.Format(
                    "Waktu Proses : {0}h {1}m {2}s {3}ms",
                    timeSpan.Hours,
                    timeSpan.Minutes,
                    timeSpan.Seconds,
                    timeSpan.Milliseconds);
                SetImportControlsEnabled(true);
                SBImport.Enabled = false;
                isSuccess = true;
                XtraMessageBox.Show("Import Chart Of Account Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetImportProgress();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!isSuccess)
                {
                    SetImportControlsEnabled(true);
                    SBImport.Enabled = viewModel.CanImport;
                }
            }
        }

        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (viewModel == null || cboSheet.SelectedItem == null)
                {
                    return;
                }

                viewModel.PreviewSheet(cboSheet.SelectedItem.ToString());
                gridControl1.DataSource = viewModel.Rows;
                SBImport.Enabled = viewModel.CanImport;
                lblrecord.Text = viewModel.StatusText;

                if (viewModel.LastIssues.Count > 0)
                {
                    XtraMessageBox.Show(
                        "Format File Excel Daftar Perkiraan Salah\n" + BuildIssueMessage(viewModel.LastIssues),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (viewModel.Rows.Count > 0)
                {
                    gridView1.Columns[7].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridView1.Columns[7].DisplayFormat.FormatString = "n2";
                    gridView1.BestFitColumns();
                }
            }
            catch (SystemException ex)
            {
                SBImport.Enabled = false;
                XtraMessageBox.Show("Format File Excel Daftar Perkiraan Salah\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string BuildImportErrorMessage(CoaImportResult result)
        {
            if (result.Issues.Count == 0)
            {
                return result.Message;
            }

            return result.Message + Environment.NewLine + BuildIssueMessage(result.Issues);
        }

        private static string BuildIssueMessage(IReadOnlyList<CoaImportValidationIssue> issues)
        {
            return string.Join(
                Environment.NewLine,
                issues.Select(issue => issue.Value is null ? issue.Message : $"{issue.Message} ({issue.Value})"));
        }

        private void ShowImportProgress(CoaImportProgress progress)
        {
            progressImport.Visible = true;
            lblImportProgress.Visible = true;

            if (progress.UseRowCount && progress.TotalRows > 0)
            {
                progressImport.Properties.Maximum = progress.TotalRows;
                progressImport.Position = Math.Clamp(progress.ProcessedRows, 0, progress.TotalRows);
                lblImportProgress.Text = $"{progress.Stage} {progress.ProcessedRows:N0} / {progress.TotalRows:N0}";
                return;
            }

            progressImport.Properties.Maximum = 100;
            progressImport.Position = Math.Clamp(progress.Percent, 0, 100);
            lblImportProgress.Text = progress.Stage;
        }

        private void ResetImportProgress()
        {
            progressImport.Properties.Maximum = 100;
            progressImport.Position = 0;
            progressImport.Visible = false;
            lblImportProgress.Visible = false;
            lblImportProgress.Text = string.Empty;
        }

        private void SetImportControlsEnabled(bool enabled)
        {
            sbbrowse.Enabled = enabled;
            cboSheet.Enabled = enabled;
            setahun.Enabled = enabled;
            xeparsial.Enabled = enabled;
            SBImport.Enabled = enabled && viewModel?.CanImport == true;
        }
    }
}
