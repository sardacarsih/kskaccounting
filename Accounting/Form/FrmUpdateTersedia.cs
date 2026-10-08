using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Accounting.Utilities.Update;
using Serilog;

namespace Accounting.Form
{
    /// <summary>
    /// Dialog "Update Tersedia": versi baru + catatan rilis, lalu unduh, verifikasi, dan jalankan Accounting.Updater.
    /// DialogResult.OK = updater sudah berjalan (pemanggil wajib menutup aplikasi);
    /// DialogResult.Abort = update wajib ditolak (pemanggil menutup aplikasi);
    /// DialogResult.Cancel = ditunda.
    /// </summary>
    public sealed class FrmUpdateTersedia : XtraForm
    {
        private readonly LabelControl lblJudul = new();
        private readonly LabelControl lblInfo = new();
        private readonly MemoEdit memoNotes = new();
        private readonly ProgressBarControl progress = new();
        private readonly LabelControl lblStatus = new();
        private readonly SimpleButton btnUpdate = new() { Text = "Update Sekarang" };
        private readonly SimpleButton btnNanti = new();

        private readonly UpdateService service;
        private readonly UpdateCheckResult update;
        private CancellationTokenSource? downloadCts;

        internal FrmUpdateTersedia(UpdateService service, UpdateCheckResult update)
        {
            this.service = service;
            this.update = update;

            Text = "Update Tersedia";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(12);
            AcceptButton = btnUpdate;

            BuildLayout();
            FormClosing += FrmUpdateTersedia_FormClosing;
            Shown += (_, _) =>
            {
                // Fokus awal di catatan rilis membuat seluruh teks tampak terblok; arahkan ke tombol Update.
                memoNotes.DeselectAll();
                ActiveControl = btnUpdate;
            };
        }

        private void BuildLayout()
        {
            lblJudul.Text = $"Versi baru {update.LatestVersion} tersedia";
            lblJudul.Appearance.Font = new Font(lblJudul.Appearance.Font.FontFamily, 12f, FontStyle.Bold);

            string tanggal = string.IsNullOrWhiteSpace(update.Manifest?.ReleaseDate) ? "-" : update.Manifest!.ReleaseDate!;
            lblInfo.Text = $"Versi Anda: {update.CurrentVersion}    Tanggal rilis: {tanggal}";
            if (update.IsMandatory)
            {
                lblInfo.Text += Environment.NewLine + "Update ini WAJIB dipasang sebelum aplikasi dapat digunakan.";
                lblInfo.Appearance.ForeColor = Color.Firebrick;
            }

            memoNotes.Properties.ReadOnly = true;
            memoNotes.TabStop = false;
            memoNotes.Properties.ScrollBars = ScrollBars.Vertical;
            memoNotes.Text = string.IsNullOrWhiteSpace(update.Manifest?.Notes)
                ? "(Tidak ada catatan rilis)"
                : update.Manifest!.Notes!.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            memoNotes.Size = new Size(520, 220);

            progress.Size = new Size(520, 18);
            progress.Properties.Maximum = 100;
            progress.Properties.ShowTitle = true;
            progress.Visible = false;

            lblStatus.Text = "Aplikasi akan ditutup dan dibuka kembali setelah update terpasang.";
            lblStatus.Appearance.ForeColor = Color.DimGray;
            if (!UpdateService.CanWriteAppDirectory())
            {
                lblStatus.Text = "Akun Windows Anda tidak punya izin mengubah folder aplikasi." + Environment.NewLine +
                    "Update akan meminta izin administrator (UAC). Hubungi IT bila Anda tidak punya password administrator.";
                lblStatus.Appearance.ForeColor = Color.Firebrick;
            }

            btnNanti.Text = update.IsMandatory ? "Keluar" : "Nanti";
            btnUpdate.Click += BtnUpdate_Click;
            btnNanti.Click += (_, _) =>
            {
                DialogResult = update.IsMandatory ? DialogResult.Abort : DialogResult.Cancel;
                Close();
            };

            var buttons = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = new Padding(0, 8, 0, 0)
            };
            btnUpdate.Size = new Size(170, 30);
            btnNanti.Size = new Size(100, 30);
            buttons.Controls.AddRange(new Control[] { btnNanti, btnUpdate });

            var table = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Fill };
            table.Controls.Add(lblJudul);
            table.Controls.Add(lblInfo);
            table.Controls.Add(new LabelControl { Text = "Catatan rilis:", Margin = new Padding(3, 10, 3, 3) });
            table.Controls.Add(memoNotes);
            table.Controls.Add(progress);
            table.Controls.Add(lblStatus);
            table.Controls.Add(buttons);
            Controls.Add(table);
        }

        private async void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (XtraMessageBox.Show(
                    this,
                    "Aplikasi akan ditutup untuk memasang update.\nPastikan semua pekerjaan sudah disimpan.\n\nLanjutkan?",
                    "Update Aplikasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // Checked again on click: the folder rights may have been fixed while the dialog was open.
            if (!UpdateService.CanWriteAppDirectory() && !ConfirmAdministratorAvailable())
            {
                return;
            }

            SetBusy(true);
            downloadCts = new CancellationTokenSource();
            try
            {
                var reporter = new Progress<double>(value => progress.Position = (int)Math.Round(value * 100));
                lblStatus.Text = "Mengunduh paket update...";
                string zipPath = await service.DownloadAsync(update, reporter, downloadCts.Token);

                lblStatus.Text = "Paket terverifikasi. Menjalankan installer...";
                UpdateService.LaunchInstaller(zipPath, update.LatestVersion!);

                DialogResult = DialogResult.OK;
                downloadCts.Dispose();
                downloadCts = null;
                Close();
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Unduhan dibatalkan.";
                SetBusy(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Update ke versi {Version} gagal", update.LatestVersion);
                lblStatus.Text = "Update gagal.";
                SetBusy(false);
                XtraMessageBox.Show(this, $"Update gagal:\n{ex.Message}", "Update Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                downloadCts?.Dispose();
                downloadCts = null;
            }
        }

        /// <summary>
        /// A standard user cannot pass the UAC prompt Accounting.Updater raises for a read-only install folder,
        /// so don't download ~100 MB unless someone with an administrator password is at hand.
        /// </summary>
        private bool ConfirmAdministratorAvailable()
        {
            Log.Warning("Folder aplikasi {Folder} tidak bisa ditulis oleh {User}; update butuh UAC.",
                UpdateService.AppDirectory, Environment.UserName);

            return XtraMessageBox.Show(
                this,
                "Akun Windows Anda tidak punya izin mengubah folder aplikasi:\n" +
                UpdateService.AppDirectory + "\n\n" +
                "Update hanya bisa dipasang bila administrator mengisi password pada jendela UAC yang akan muncul.\n" +
                "Jika tidak ada, pilih No dan hubungi IT agar folder ini diberi izin Modify untuk grup Users " +
                "atau update dipasang oleh administrator.\n\n" +
                "Lanjutkan unduh dan pasang update?",
                "Izin Administrator Diperlukan",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void SetBusy(bool busy)
        {
            btnUpdate.Enabled = !busy;
            btnNanti.Enabled = !busy;
            progress.Visible = busy || progress.Position > 0;
            if (busy)
            {
                progress.Position = 0;
            }

            UseWaitCursor = busy;
        }

        private void FrmUpdateTersedia_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (downloadCts is not null)
            {
                // Closing mid-download would leave the user unsure whether the update ran.
                e.Cancel = true;
                return;
            }

            if (update.IsMandatory && DialogResult != DialogResult.OK)
            {
                DialogResult = DialogResult.Abort;
            }
        }
    }
}
