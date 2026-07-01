using Accounting.BusinessLayer;
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
                string periode = $"12/{tahun:0000}";
                string bulan = $"{cmbbulan.Text} - {tahun:0000}";

                // Pastikan COA tahun yang ditutup sudah tersedia (sejajar dengan tutup bulan).
                if (AccountServices.CekCOAExist(CompanyInfo.IDDATA, tahun) == 1)
                {
                    XtraMessageBox.Show("Daftar Perkiraan Belum tersedia", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var errcoaCheck = ToolsServices.Analisa_kesalahan_COA(CompanyInfo.IDDATA, tahun);
                if (errcoaCheck.Rows.Count > 0)
                {
                    COAError errorCoa = new()
                    {
                        Myperiode = periode,
                        ibulan = 12,
                        itahun = tahun
                    };
                    errorCoa.ShowDialog();
                    return;
                }

                ClosingYearRequest request = new(
                    CompanyInfo.IDDATA,
                    tahun,
                    LoginInfo.userID,
                    CompanyInfo.JENIS_AKUNTING,
                    checkEditjurnalclosing.Checked);
                ClosingYearResult result = ClosingYearServices.CloseYear(request);

                if (result.Status == ClosingYearStatus.LockedPeriod)
                {
                    PlaySound("akhir_tahun_kunci.wav");
                    XtraMessageBox.Show("Proses Closing diBatalkan...!!!\n" + result.Message, "Error Closing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (result.Status == ClosingYearStatus.NotBalanced)
                {
                    PlaySound("neraca_error.wav");
                    BalancedError be = new()
                    {
                        Myperiode = periode,
                        ibulan = 12,
                        itahun = tahun
                    };
                    be.ShowDialog();
                    return;
                }

                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);

                watch.Stop();
                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

                PlaySound("akhir_tahun.wav");
                XtraMessageBox.Show("Proses Tutup Tahun Selesai\n\n" +
                    "Periode Akuntansi : " + bulan +
                    "\nLokasi Data : " + CompanyInfo.IDDATA +
                    "\nLaba / Rugi : " + result.LabaRugi.ToString("#,##0.00") +
                    "\nCOA Tahun Berikutnya : " + result.CoaAction +
                    "\n" + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlaySound(string fileName)
        {
            player.SoundLocation = Environment.CurrentDirectory + "\\wav\\" + fileName;
            player.Play();
        }
    }
}
