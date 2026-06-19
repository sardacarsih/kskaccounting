using Accounting.JurnalImport.Domain;
using Accounting.JurnalImport.Infrastructure;
using Accounting.JurnalImport.Presentation;
using Accounting.Model;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmImportJurnal_Parsial : XtraForm
    {
        private readonly SoundPlayer Player = new();
        private FrmImportJurnalViewModel? viewModel;
        private int pbulan;
        private int ptahun;

        public FrmImportJurnal_Parsial()
        {
            InitializeComponent();
        }

        private void FrmImportJurnal_Parsial_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationService.EnsureCanImportJurnal();
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new MethodInvoker(Close));
                return;
            }

            try
            {
                viewModel = JurnalImportModuleFactory.CreateExcelViewModel(CompanyInfo.IDDATA, LoginInfo.userID);
                viewModel.Mode = JurnalImportMode.AddOnly;
                cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
                pbulan = int.Parse(Acct.PeriodeMax.ToString()[^2..]);
                cmbbulan.SelectedIndex = pbulan - 1;
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sbbrowse_Click(object sender, EventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            try
            {
                gridControl1.DataSource = null;
                cboSheet.Text = string.Empty;
                using OpenFileDialog ofd = new() { Filter = "Excel Workbook|*.xlsx| Excel 97-2003 Workbook|*.xls" };
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                viewModel.LoadWorkbook(ofd.FileName);
                txtPath.Text = ofd.FileName;
                cboSheet.Properties.Items.Clear();
                cboSheet.Properties.Items.AddRange(viewModel.Sheets.ToList());
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

        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (viewModel == null || cboSheet.SelectedItem == null)
            {
                return;
            }

            try
            {
                viewModel.PreviewSheet(cboSheet.SelectedItem.ToString() ?? string.Empty);
                gridControl1.DataSource = viewModel.Rows;
                SBImport.Enabled = viewModel.CanImport;
                lblrecord.Text = viewModel.StatusText;
                ConfigureGrid();

                if (viewModel.LastIssues.Count > 0)
                {
                    XtraMessageBox.Show(BuildIssueMessage(viewModel.LastIssues), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SystemException ex)
            {
                SBImport.Enabled = false;
                XtraMessageBox.Show("Format Jurnal File Excel Salah\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SBImport_Click(object sender, EventArgs e)
        {
            if (viewModel == null)
            {
                return;
            }

            try
            {
                AuthorizationService.EnsureCanImportJurnal();
                pbulan = cmbbulan.SelectedIndex + 1;
                ptahun = Convert.ToInt32(setahun.Value);
                string periodeDisplay = cmbbulan.Text + " - " + setahun.Value;

                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal ? " +
                    "\n\nPeriode : " + periodeDisplay + " " +
                    "\nLokasi Data :" + CompanyInfo.IDDATA,
                    "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                viewModel.Month = pbulan;
                viewModel.Year = ptahun;
                viewModel.CoaYear = ptahun;
                viewModel.Source = JurnalImportSource.Excel;

                SetImportControlsEnabled(false);
                ShowImportProgress(new JurnalImportProgress(0, "Menyiapkan import jurnal...", 0, viewModel.Rows.Count));
                Progress<JurnalImportProgress> progress = new(ShowImportProgress);
                JurnalImportResult result = await Task.Run(() => viewModel.Import(progress));

                if (!result.IsSuccess)
                {
                    PlayFailureSound(result);
                    XtraMessageBox.Show(BuildResultMessage(result), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                SBImport.Enabled = false;
                if (result.HasRecalculationWarning)
                {
                    XtraMessageBox.Show(BuildRecalculationWarningMessage(result), "Import Jurnal Selesai Dengan Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetImportProgress();
                    return;
                }

                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
                Player.Play();
                XtraMessageBox.Show("Import Jurnal Selesai \n " + FormatElapsed(result.Elapsed), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ResetImportProgress();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    BuildUnexpectedImportErrorMessage(ex, cmbbulan.Text, setahun.Value, cboSheet.Text, lblrecord.Text),
                    "Import Jurnal Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetImportControlsEnabled(true);
                SBImport.Enabled = viewModel.CanImport;
            }
        }

        private void ConfigureGrid()
        {
            if (gridView1.Columns.Count == 0)
            {
                return;
            }

            if (gridView1.Columns["Tanggal"] != null)
            {
                gridView1.Columns["Tanggal"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gridView1.Columns["Tanggal"].DisplayFormat.FormatString = "dd-MMM-yyyy";
            }

            foreach (string fieldName in new[] { "Debet", "Kredit" })
            {
                if (gridView1.Columns[fieldName] != null)
                {
                    gridView1.Columns[fieldName].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    gridView1.Columns[fieldName].DisplayFormat.FormatString = "n2";
                }
            }

            gridView1.BestFitColumns();
        }

        private void PlayFailureSound(JurnalImportResult result)
        {
            string sound = result.StatusCode switch
            {
                0 => "jurnal_bedaperiode.wav",
                1 => "jurnal_imbang.wav",
                _ when result.Issues.Any(issue => issue.Code is "PERIOD_LOCKED") => "periode_dikunci.wav",
                _ when result.Issues.Any(issue => issue.Code is "PERIOD_MISMATCH") => "periode_beda.wav",
                _ when result.Issues.Any(issue => issue.Code is "NOJURNAL_EXISTS" or "DUPLICATE_JOURNAL_DIFFERENT_DATE") => "jurnal_duplikasi.wav",
                _ when result.Issues.Any(issue => issue.Code is "ACCOUNT_NOT_FOUND") => "jurnal_daftarperk.wav",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(sound))
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\" + sound;
                Player.Play();
            }
        }

        private static string BuildResultMessage(JurnalImportResult result)
        {
            if (result.BalanceIssues.Count > 0)
            {
                return result.Message + Environment.NewLine + BuildBalanceIssueMessage(result.BalanceIssues);
            }

            if (result.Issues.Count == 0)
            {
                return result.Message;
            }

            return BuildIssueMessage(result.Issues);
        }

        private static string BuildIssueMessage(System.Collections.Generic.IReadOnlyList<JurnalImportValidationIssue> issues)
        {
            JurnalImportValidationIssue first = issues[0];
            string values = string.Join(Environment.NewLine, issues.Select(issue => issue.Value).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct());
            return string.IsNullOrWhiteSpace(values) ? first.Message : first.Message + Environment.NewLine + values;
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            return $"Waktu Proses : {elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s {elapsed.Milliseconds}ms";
        }

        private static string BuildBalanceIssueMessage(System.Collections.Generic.IReadOnlyList<JurnalImportBalanceIssue> issues)
        {
            return string.Join(
                Environment.NewLine,
                issues.Select(issue => $"{issue.NoJurnal} | {issue.Tanggal:dd-MMM-yyyy} | Debet {issue.Debet:n2} | Kredit {issue.Kredit:n2} | Selisih {issue.Selisih:n2}"));
        }

        private static string BuildRecalculationWarningMessage(JurnalImportResult result)
        {
            return "Import Jurnal Selesai" +
                Environment.NewLine + FormatElapsed(result.Elapsed) +
                Environment.NewLine + Environment.NewLine +
                result.RecalculationWarning;
        }

        private void ShowImportProgress(JurnalImportProgress progress)
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
            cmbbulan.Enabled = enabled;
            setahun.Enabled = enabled;
            SBImport.Enabled = enabled && viewModel?.CanImport == true;
        }

        private static string BuildUnexpectedImportErrorMessage(Exception ex, string monthName, object year, string sheetName, string recordText)
        {
            string technicalMessage = GetFullExceptionMessage(ex);
            if (technicalMessage.Contains("ORA-01008", StringComparison.OrdinalIgnoreCase))
            {
                return "Import jurnal gagal saat menyimpan data ke database." +
                    Environment.NewLine + Environment.NewLine +
                    "Data belum berhasil diimport dan perubahan sudah dibatalkan otomatis." +
                    Environment.NewLine +
                    "Ini bukan kesalahan format Excel. Masalah ada pada parameter simpan database aplikasi." +
                    Environment.NewLine +
                    "Tidak perlu import ulang berkali-kali sebelum masalah ini diperbaiki." +
                    Environment.NewLine + Environment.NewLine +
                    "Detail proses:" +
                    Environment.NewLine + $"Mode: Add jurnal baru (tanpa hapus jurnal existing)" +
                    Environment.NewLine + $"Periode: {monthName} {year}" +
                    Environment.NewLine + $"Sheet: {sheetName}" +
                    Environment.NewLine + $"Jumlah data: {recordText}" +
                    Environment.NewLine + Environment.NewLine +
                    "Informasi teknis untuk IT:" +
                    Environment.NewLine +
                    "Oracle ORA-01008 berarti ada parameter SQL yang belum terisi saat aplikasi menyimpan jurnal. " +
                    "Mohon screenshot pesan ini dan hubungi IT/support aplikasi." +
                    Environment.NewLine + technicalMessage;
            }

            return "Import jurnal gagal." +
                Environment.NewLine + Environment.NewLine +
                "Data belum berhasil diimport." +
                Environment.NewLine + Environment.NewLine +
                "Detail proses:" +
                Environment.NewLine + $"Mode: Add jurnal baru (tanpa hapus jurnal existing)" +
                Environment.NewLine + $"Periode: {monthName} {year}" +
                Environment.NewLine + $"Sheet: {sheetName}" +
                Environment.NewLine + $"Jumlah data: {recordText}" +
                Environment.NewLine + Environment.NewLine +
                "Informasi teknis:" +
                Environment.NewLine + technicalMessage;
        }

        private static string GetFullExceptionMessage(Exception ex)
        {
            return ex.InnerException == null
                ? ex.Message
                : ex.Message + Environment.NewLine + ex.InnerException.Message;
        }
    }
}
