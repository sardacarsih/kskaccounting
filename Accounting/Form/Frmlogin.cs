using DevExpress.XtraSplashScreen;
using System.Media;
using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;
using Accounting.Services;
using Accounting.BusinessLayer;
using System.Drawing;
using Accounting.Models.Login;
using System.Collections.Concurrent;

namespace Accounting.Form
{
    public partial class Frmlogin : DevExpress.XtraEditors.XtraForm
    {
        private const int CardMargin = 24;
        private static readonly Size CardMinSize = new(400, 320);
        private static readonly Size CardMaxSize = new(560, 400);
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly ConcurrentDictionary<string, LoginAttemptState> FailedLoginAttempts = new(StringComparer.OrdinalIgnoreCase);

        private readonly SoundPlayer Player = new();
        private bool isAuthenticating;

        private sealed class LoginAttemptState
        {
            public int FailedCount { get; init; }
            public DateTimeOffset? LockoutUntilUtc { get; init; }
        }

        public Frmlogin()
        {
            InitializeComponent();
        }

        private void Frmlogin_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.FromControl(this).WorkingArea;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            lblversi.Text = $"Version {fvi.FileVersion ?? Application.ProductVersion}";

            ApplyResponsiveLayout();
        }

        private void Frmlogin_Resize(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            if (backgroundPanel == null || cardPanel == null)
            {
                return;
            }

            int width = Math.Max(320, backgroundPanel.ClientSize.Width - (CardMargin * 2));
            int height = Math.Max(260, backgroundPanel.ClientSize.Height - (CardMargin * 2));

            int targetWidth = Math.Clamp((int)(width * 0.4), CardMinSize.Width, CardMaxSize.Width);
            int targetHeight = Math.Clamp((int)(height * 0.62), CardMinSize.Height, CardMaxSize.Height);

            if (width < CardMinSize.Width)
            {
                targetWidth = width;
            }

            if (height < CardMinSize.Height)
            {
                targetHeight = height;
            }

            cardPanel.Size = new Size(targetWidth, targetHeight);
            cardPanel.Location = new Point(
                (backgroundPanel.ClientSize.Width - cardPanel.Width) / 2,
                (backgroundPanel.ClientSize.Height - cardPanel.Height) / 2);
        }

        private void txtuserid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtpwd.Focus();
            }
        }

        private void txtpwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (string.IsNullOrWhiteSpace(txtuserid.Text.Trim()))
                {
                    txtuserid.Focus();
                }
                else if (!string.IsNullOrWhiteSpace(txtpwd.Text.Trim()))
                {
                    Login.PerformClick();
                }
            }
        }

        private void Login_Click(object sender, EventArgs e)
        {
            if (isAuthenticating)
            {
                return;
            }

            if (!IsInputValid())
            {
                return;
            }

            string userId = txtuserid.Text.Trim();

            if (IsUserLockedOut(userId, out TimeSpan lockoutRemaining))
            {
                PlaySound("hub_manager.wav");
                int waitMinutes = Math.Max(1, (int)Math.Ceiling(lockoutRemaining.TotalMinutes));
                XtraMessageBox.Show($"UserID ini terkunci sementara. Coba lagi dalam {waitMinutes} menit.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            IOverlaySplashScreenHandle? handle = null;

            try
            {
                SetBusyState(true);
                handle = SplashScreenManager.ShowOverlayForm(this);

                string password = txtpwd.Text.Trim();

                // === RESET PASSWORD SEMENTARA — HAPUS SETELAH SELESAI ===
                UserManager_Services.ResetPassword("dharyadi", "dev");
                XtraMessageBox.Show("Password untuk 'dharyadi' sudah di-reset ke 'dev'.", "Reset Password");
                // === END RESET ===

                LoginAuthResult result = UserManager_Services.AuthenticateForModule(userId, password, LoginInfo.MODULE);
                HandleAuthResult(result, userId);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (handle != null)
                {
                    SplashScreenManager.CloseOverlayForm(handle);
                }

                SetBusyState(false);
            }
        }

        private void HandleAuthResult(LoginAuthResult result, string userId)
        {
            switch (result.Status)
            {
                case LoginAuthStatus.Success:
                    ResetFailedAttempts(userId);
                    CompleteLogin(result.Users);
                    break;

                case LoginAuthStatus.UserNotFound:
                    HandleUserNotFound();
                    break;

                case LoginAuthStatus.InvalidPassword:
                    HandleInvalidPassword(userId);
                    break;

                case LoginAuthStatus.InactiveUser:
                    HandleInactiveUser();
                    break;

                case LoginAuthStatus.NoModuleAccess:
                    HandleNoModuleAccess();
                    break;

                case LoginAuthStatus.NoLocationAccess:
                    HandleNoLocationAccess();
                    break;

                default:
                    XtraMessageBox.Show("Proses login tidak dapat diselesaikan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void CompleteLogin(System.Collections.Generic.List<LOGIN_USERS_DTO> users)
        {
            if (users.Count == 0)
            {
                HandleNoLocationAccess();
                return;
            }

            LOGIN_USERS_DTO user = users[0];

            if (users.Count > 1)
            {
                SetLoginInfo(user);
                using FrmLokasi lokasiDataForm = new();
                Hide();
                lokasiDataForm.ShowDialog();
                return;
            }

            SetUserInformation(user);
            Hide();
            new MainView().Show();
        }

        private bool IsInputValid()
        {
            if (string.IsNullOrWhiteSpace(txtuserid.Text))
            {
                PlaySound("userid.wav");
                XtraMessageBox.Show("UserID belum diisi.", "Konfirmasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtuserid.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtpwd.Text))
            {
                txtpwd.Focus();
                return false;
            }

            return true;
        }

        private void SetBusyState(bool busy)
        {
            isAuthenticating = busy;
            Login.Enabled = !busy;
            simpleButton1.Enabled = !busy;
            txtuserid.Enabled = !busy;
            txtpwd.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void PlaySound(string soundFileName)
        {
            Player.SoundLocation = Path.Combine(Environment.CurrentDirectory, "wav", soundFileName);
            Player.Play();
        }

        private void SetLoginInfo(LOGIN_USERS_DTO user)
        {
            LoginInfo.role = user.LEVEL_USER;
            LoginInfo.userID = user.USERID;
        }

        private void HandleInvalidPassword(string userId)
        {
            (int remainingAttempts, TimeSpan? lockoutRemaining) = RegisterFailedAttempt(userId);

            if (lockoutRemaining.HasValue)
            {
                PlaySound("hub_manager.wav");
                int waitMinutes = Math.Max(1, (int)Math.Ceiling(lockoutRemaining.Value.TotalMinutes));
                XtraMessageBox.Show($"Password salah. UserID terkunci selama {waitMinutes} menit.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                PlaySound("sandi_salah.wav");
                XtraMessageBox.Show($"Password salah. Sisa kesempatan: {remainingAttempts}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtpwd.SelectAll();
            txtpwd.Focus();
        }

        private void HandleUserNotFound()
        {
            PlaySound("nore_userid.wav");
            XtraMessageBox.Show("UserID tidak ditemukan pada master user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtuserid.SelectAll();
            txtuserid.Focus();
        }

        private void HandleInactiveUser()
        {
            PlaySound("maaf_noakses.wav");
            XtraMessageBox.Show("Akun Anda tidak aktif. Hubungi administrator.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void HandleNoModuleAccess()
        {
            PlaySound("maaf_noakses.wav");
            XtraMessageBox.Show($"Kredensial valid, tetapi akses modul '{LoginInfo.MODULE}' belum diberikan.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void HandleNoLocationAccess()
        {
            PlaySound("iddata.wav");
            XtraMessageBox.Show("Kredensial valid, tetapi akses lokasi (IDDATA) belum diberikan.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private static bool IsUserLockedOut(string userId, out TimeSpan remainingLockout)
        {
            remainingLockout = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if (!FailedLoginAttempts.TryGetValue(userId, out LoginAttemptState? state) || state.LockoutUntilUtc == null)
            {
                return false;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (state.LockoutUntilUtc <= now)
            {
                FailedLoginAttempts.TryRemove(userId, out _);
                return false;
            }

            remainingLockout = state.LockoutUntilUtc.Value - now;
            return true;
        }

        private static (int RemainingAttempts, TimeSpan? LockoutRemaining) RegisterFailedAttempt(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (MaxFailedAttempts, null);
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            LoginAttemptState state = FailedLoginAttempts.AddOrUpdate(
                userId,
                _ => new LoginAttemptState { FailedCount = 1 },
                (_, current) =>
                {
                    if (current.LockoutUntilUtc.HasValue && current.LockoutUntilUtc > now)
                    {
                        return current;
                    }

                    int nextFailedCount = 1;
                    if (!current.LockoutUntilUtc.HasValue)
                    {
                        nextFailedCount = current.FailedCount + 1;
                    }

                    if (nextFailedCount >= MaxFailedAttempts)
                    {
                        return new LoginAttemptState
                        {
                            FailedCount = MaxFailedAttempts,
                            LockoutUntilUtc = now.Add(LockoutDuration)
                        };
                    }

                    return new LoginAttemptState { FailedCount = nextFailedCount };
                });

            if (state.LockoutUntilUtc.HasValue && state.LockoutUntilUtc > now)
            {
                return (0, state.LockoutUntilUtc.Value - now);
            }

            return (Math.Max(0, MaxFailedAttempts - state.FailedCount), null);
        }

        private static void ResetFailedAttempts(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            FailedLoginAttempts.TryRemove(userId, out _);
        }

        private void SimpleButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetUserInformation(LOGIN_USERS_DTO user)
        {
            LoginInfo.role = user.LEVEL_USER;
            LoginInfo.userID = user.USERID;
            CompanyInfo.ESTATE = user.ESTATE;
            CompanyInfo.IDDATA = user.IDDATA;
            CompanyInfo.NAMAPT = user.NAMAPT;
            CompanyInfo.WILAYAH = user.WILAYAH;
            CompanyInfo.JENIS_AKUNTING = user.JENIS_AKUNTANSI;
            Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
            Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
            Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
            Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);
        }
    }
}
