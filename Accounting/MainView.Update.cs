using Accounting.Form;
using Accounting.Utilities.Update;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using Serilog;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting
{
    public partial class MainView
    {
        private readonly SemaphoreSlim _updateCheckLock = new(1, 1);
        private readonly System.Windows.Forms.Timer _updateTimer = new();
        private UpdateService _updateService;
        private UpdateCheckResult _availableUpdate;
        private BarButtonItem _updateTersediaButton;
        private BarButtonItem _cekUpdateButton;
        private bool _updateDialogOpen;
        private bool _closingForUpdate;

        private void ConfigureUpdateMenu()
        {
            if (_updateTersediaButton == null)
            {
                _updateTersediaButton = new BarButtonItem(ribbonControl1.Manager, "Update Tersedia")
                {
                    Name = "bbiUpdateTersedia",
                    Alignment = BarItemLinkAlignment.Right,
                    PaintStyle = BarItemPaintStyle.Caption,
                    Visibility = BarItemVisibility.Never
                };
                _updateTersediaButton.ItemAppearance.Normal.ForeColor = Color.White;
                _updateTersediaButton.ItemAppearance.Normal.BackColor = Color.ForestGreen;
                _updateTersediaButton.ItemAppearance.Normal.Font = new Font(_updateTersediaButton.ItemAppearance.Normal.Font, FontStyle.Bold);
                _updateTersediaButton.ItemAppearance.Normal.Options.UseForeColor = true;
                _updateTersediaButton.ItemAppearance.Normal.Options.UseBackColor = true;
                _updateTersediaButton.ItemAppearance.Normal.Options.UseFont = true;
                _updateTersediaButton.ItemClick += (_, _) => ShowUpdateDialog();
                ribbonControl1.Items.Add(_updateTersediaButton);
                ribbonStatusBar1.ItemLinks.Add(_updateTersediaButton);
            }

            if (_cekUpdateButton == null)
            {
                _cekUpdateButton = new BarButtonItem(ribbonControl1.Manager, "Cek Update")
                {
                    Name = "bbiCekUpdate"
                };
                _cekUpdateButton.ImageOptions.Image = bbcheckupdate.ImageOptions.Image;
                _cekUpdateButton.ImageOptions.LargeImage = bbcheckupdate.ImageOptions.LargeImage;
                _cekUpdateButton.ItemClick += async (_, _) => await CheckForUpdateAsync(silent: false);
                ribbonControl1.Items.Add(_cekUpdateButton);
                ribbonPageGroup21.ItemLinks.Add(_cekUpdateButton);
            }
        }

        private void StartUpdateChecks()
        {
            try
            {
                ConfigureUpdateMenu();

                UpdateOptions options = UpdateOptionsLoader.Load();
                _updateService = new UpdateService(options);
                if (!_updateService.IsEnabled)
                {
                    return;
                }

                _updateTimer.Interval = (int)TimeSpan.FromMinutes(options.CheckIntervalMinutes).TotalMilliseconds;
                _updateTimer.Tick += async (_, _) => await CheckForUpdateAsync(silent: true);
                _updateTimer.Start();
                _ = CheckForUpdateAsync(silent: true);
            }
            catch (Exception ex)
            {
                // The update check must never stop the application from starting.
                Log.Error(ex, "Fitur update gagal diinisialisasi");
            }
        }

        private async Task CheckForUpdateAsync(bool silent)
        {
            _updateService ??= new UpdateService(UpdateOptionsLoader.Load());
            if (!_updateService.IsEnabled)
            {
                if (!silent)
                {
                    XtraMessageBox.Show(
                        this,
                        "Server update belum dikonfigurasi.\nIsi section \"Update\" pada Utilities\\config.json atau key Update:ManifestUrl pada App.config.",
                        "Cek Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return;
            }

            if (!await _updateCheckLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                UpdateCheckResult result = await _updateService.CheckAsync(CancellationToken.None);
                if (IsDisposed || _closingForUpdate)
                {
                    return;
                }

                switch (result.Status)
                {
                    case UpdateCheckStatus.Available:
                        _availableUpdate = result;
                        _updateTersediaButton.Caption = $"Update Tersedia (v{result.LatestVersion})";
                        _updateTersediaButton.Visibility = BarItemVisibility.Always;

                        // Optional updates only surface as the green status-bar button until clicked.
                        if (result.IsMandatory || !silent)
                        {
                            ShowUpdateDialog();
                        }

                        break;

                    case UpdateCheckStatus.UpToDate:
                        _availableUpdate = null;
                        _updateTersediaButton.Visibility = BarItemVisibility.Never;
                        if (!silent)
                        {
                            XtraMessageBox.Show(
                                this,
                                $"Anda sudah menggunakan versi terbaru ({result.CurrentVersion}).",
                                "Cek Update",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }

                        break;

                    case UpdateCheckStatus.Failed when !silent:
                        XtraMessageBox.Show(
                            this,
                            $"Gagal memeriksa update:\n{result.Error}",
                            "Cek Update",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Cek update gagal");
            }
            finally
            {
                _updateCheckLock.Release();
            }
        }

        private void ShowUpdateDialog()
        {
            if (_availableUpdate is null || _updateService is null || _updateDialogOpen)
            {
                return;
            }

            DialogResult result;
            _updateDialogOpen = true;
            try
            {
                using var dialog = new FrmUpdateTersedia(_updateService, _availableUpdate);
                result = dialog.ShowDialog(this);
            }
            finally
            {
                _updateDialogOpen = false;
            }

            if (result is DialogResult.OK or DialogResult.Abort)
            {
                // OK: Accounting.Updater is waiting for this process to exit. Abort: mandatory update declined.
                _closingForUpdate = true;
                Close();
            }
        }

        private void StopUpdateChecks()
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
        }
    }
}
