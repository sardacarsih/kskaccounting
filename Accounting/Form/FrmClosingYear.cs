using Accounting.ClosingYear;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Diagnostics;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmClosingYear : DevExpress.XtraEditors.XtraForm
    {
        private readonly SoundPlayer player = new();
        private readonly ClosingYearWorkflow closingYearWorkflow = new(new ClosingYearWorkflowDependencies());

        public FrmClosingYear()
        {
            InitializeComponent();
        }

        private void FrmClosingYear_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Desember" });
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            cmbbulan.SelectedIndex = 0;
            lblpt.Text = CompanyInfo.NAMAPT;
            lbldata.Text = CompanyInfo.IDDATA;
            lblwilayah.Text = CompanyInfo.WILAYAH;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {
                Stopwatch watch = new();
                watch.Start();

                int tahun = Convert.ToInt32(setahun.Value);
                ClosingYearRequest request = new(
                    CompanyInfo.IDDATA,
                    tahun,
                    LoginInfo.userID,
                    CompanyInfo.JENIS_AKUNTING,
                    checkEditjurnalclosing.Checked);
                ClosingYearWorkflowResult workflowResult = closingYearWorkflow.Execute(request);

                if (!HandleWorkflowResult(workflowResult, watch))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool HandleWorkflowResult(ClosingYearWorkflowResult workflowResult, Stopwatch watch)
        {
            switch (workflowResult.Status)
            {
                case ClosingYearWorkflowStatus.MissingCoa:
                    XtraMessageBox.Show(workflowResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                case ClosingYearWorkflowStatus.CoaValidationErrors:
                    ShowCoaError(workflowResult);
                    return false;
                case ClosingYearWorkflowStatus.LockedPeriod:
                    PlaySound("akhir_tahun_kunci.wav");
                    XtraMessageBox.Show("Proses Closing diBatalkan...!!!\n" + workflowResult.Message, "Error Closing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                case ClosingYearWorkflowStatus.NotBalanced:
                    PlaySound("neraca_error.wav");
                    ShowBalancedError(workflowResult);
                    return false;
                case ClosingYearWorkflowStatus.Success:
                    ShowSuccess(workflowResult, watch);
                    return true;
                default:
                    throw new InvalidOperationException($"Status tutup tahun tidak dikenal: {workflowResult.Status}");
            }
        }

        private static void ShowCoaError(ClosingYearWorkflowResult workflowResult)
        {
            COAError errorCoa = new()
            {
                Myperiode = workflowResult.Period,
                ibulan = workflowResult.Month,
                itahun = workflowResult.Year
            };
            errorCoa.ShowDialog();
        }

        private static void ShowBalancedError(ClosingYearWorkflowResult workflowResult)
        {
            ClosingYearResult result = workflowResult.ClosingResult
                ?? throw new InvalidOperationException("Hasil tutup tahun kosong.");

            XtraMessageBox.Show(
                "Neraca belum balance.\n\n" +
                "Periode Akuntansi : " + workflowResult.Period +
                "\nSelisih : " + result.Selisih.ToString("#,##0.00"),
                "Error Closing",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void ShowSuccess(ClosingYearWorkflowResult workflowResult, Stopwatch watch)
        {
            ClosingYearResult result = workflowResult.ClosingResult
                ?? throw new InvalidOperationException("Hasil tutup tahun kosong.");

            watch.Stop();
            TimeSpan timeSpan = watch.Elapsed;
            string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            PlaySound("akhir_tahun.wav");
            XtraMessageBox.Show("Proses Tutup Tahun Selesai\n\n" +
                "Periode Akuntansi : " + workflowResult.MonthDisplay +
                "\nLokasi Data : " + CompanyInfo.IDDATA +
                "\nLaba / Rugi : " + result.LabaRugi.ToString("#,##0.00") +
                "\nCOA Tahun Berikutnya : " + result.CoaAction +
                "\n" + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PlaySound(string fileName)
        {
            player.SoundLocation = Environment.CurrentDirectory + "\\wav\\" + fileName;
            player.Play();
        }
    }
}
