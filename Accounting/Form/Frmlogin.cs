using Accounting.BusinessLayer;
using Accounting.Models.Login;
using Accounting.Properties;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class Frmlogin : DevExpress.XtraEditors.XtraForm
    {
        private const int ShellMinWidth = 980;
        private const int ShellMaxWidth = 1760;
        private const int ShellMinHeight = 560;
        private const int CardMinWidth = 460;
        private const int CardMaxWidth = 560;
        private const int CardMinHeight = 500;
        private const int CardMaxHeight = 500;

        private readonly SoundPlayer Player = new();
        private bool isAuthenticating;

        private readonly record struct LayoutPreset(
            int SafeXMin,
            int SafeXMax,
            int SafeYMin,
            int SafeYMax,
            int SingleColumnThreshold,
            float BrandPercent,
            int CardMinW,
            int CardMaxW,
            int CardMinH,
            int CardMaxH,
            int CardYOffset,
            float TitleFontSize,
            float SubtitleFontSize);

        private readonly record struct CardLayoutSnapshot(
            int ContentWidth,
            int TitleHeight,
            int SubtitleHeight,
            int ErrorHeight,
            int UserLabelHeight,
            int PasswordLabelHeight,
            int InputHeight,
            int PasswordLayoutHeight,
            int OptionsLayoutHeight,
            int FooterLayoutHeight,
            int VerticalGapSmall,
            int VerticalGapMedium,
            int FooterInfoGap,
            int ToggleButtonWidth,
            int VersionWidth,
            int InfoHeight,
            int RequiredHeight);

        public Frmlogin()
        {
            InitializeComponent();
            ConfigureLoginBackground();
            MinimumSize = new Size(1080, 680);
            formHostPanel.AutoScroll = true;
            cardPanel.MinimumSize = new Size(CardMinWidth, CardMinHeight);

            txtuserid.EditValueChanged += Input_EditValueChanged;
            txtpwd.EditValueChanged += Input_EditValueChanged;
            linkForgotPassword.AutoEllipsis = true;
            linkForgotPassword.TextAlign = ContentAlignment.MiddleRight;
            chkRememberMe.AutoEllipsis = true;
            chkRememberMe.Visible = false;
            lblTitle.AutoSizeMode = LabelAutoSizeMode.Vertical;
            lblSubtitle.AutoSizeMode = LabelAutoSizeMode.Vertical;
            lblError.AutoSizeMode = LabelAutoSizeMode.Vertical;
            lblSupport.AutoSizeMode = LabelAutoSizeMode.None;
            lblSupport.Appearance.Options.UseTextOptions = true;
            lblSupport.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblSupport.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.EllipsisCharacter;
            lblSupport.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
        }

        private void Frmlogin_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.FromControl(this).WorkingArea;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            lblversi.Text = $"Versi {fvi.FileVersion ?? Application.ProductVersion}";

            ApplyResponsiveLayout();
            ClearValidationState();
        }

        private void Frmlogin_Resize(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ConfigureLoginBackground()
        {
            backgroundPanel.BackColor = Color.FromArgb(228, 235, 243);

            string customBackgroundPath = Path.Combine(Application.StartupPath, "Resources", "login-wireguard-bg.png");
            if (File.Exists(customBackgroundPath))
            {
                backgroundPanel.BackgroundImage = Image.FromFile(customBackgroundPath);
            }
            else
            {
                backgroundPanel.BackgroundImage = Resources.cloud_storage_6621829_1920;
            }

            backgroundPanel.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void ApplyResponsiveLayout()
        {
            if (backgroundPanel == null || shellPanel == null || formHostPanel == null)
            {
                return;
            }

            LayoutPreset preset = GetLayoutPreset(backgroundPanel.ClientSize.Width, backgroundPanel.ClientSize.Height);
            int safeX = Math.Clamp(backgroundPanel.ClientSize.Width / 20, preset.SafeXMin, preset.SafeXMax);
            int safeY = Math.Clamp(backgroundPanel.ClientSize.Height / 18, preset.SafeYMin, preset.SafeYMax);
            backgroundPanel.Padding = new Padding(safeX, safeY, safeX, safeY);

            int availableWidth = Math.Max(360, backgroundPanel.ClientSize.Width - (safeX * 2));
            int availableHeight = Math.Max(320, backgroundPanel.ClientSize.Height - (safeY * 2));

            int shellWidth = availableWidth < ShellMinWidth
                ? availableWidth
                : Math.Clamp(availableWidth, ShellMinWidth, ShellMaxWidth);

            int shellHeight = availableHeight < ShellMinHeight
                ? availableHeight
                : Math.Clamp(availableHeight, ShellMinHeight, backgroundPanel.ClientSize.Height);

            shellPanel.Size = new Size(shellWidth, shellHeight);
            shellPanel.Location = new Point(
                (backgroundPanel.ClientSize.Width - shellPanel.Width) / 2,
                (backgroundPanel.ClientSize.Height - shellPanel.Height) / 2);

            bool singleColumnMode = shellWidth < preset.SingleColumnThreshold;
            ConfigureLayoutMode(singleColumnMode, preset.BrandPercent);
            ApplyAdaptiveTypography(preset);
            ResizeCard(singleColumnMode, safeX, preset);
            LayoutCardContent();
            ApplyRoundedCorners(cardPanel, 18);
            ApplyRoundedCorners(brandPanel, 24);
            ApplyRoundedCorners(logoPanel, 14);
        }

        private void ConfigureLayoutMode(bool singleColumnMode, float brandPercent)
        {
            if (singleColumnMode)
            {
                if (mainLayout.ColumnCount != 1)
                {
                    mainLayout.SuspendLayout();
                    mainLayout.ColumnStyles.Clear();
                    mainLayout.ColumnCount = 1;
                    mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    mainLayout.SetColumn(formHostPanel, 0);
                    mainLayout.ResumeLayout();
                }

                brandPanel.Visible = false;
                formHostPanel.Margin = Padding.Empty;
                return;
            }

            if (mainLayout.ColumnCount != 2)
            {
                mainLayout.SuspendLayout();
                mainLayout.ColumnStyles.Clear();
                mainLayout.ColumnCount = 2;
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, brandPercent));
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F - brandPercent));
                mainLayout.SetColumn(brandPanel, 0);
                mainLayout.SetColumn(formHostPanel, 1);
                mainLayout.ResumeLayout();
            }
            else
            {
                mainLayout.ColumnStyles[0].SizeType = SizeType.Percent;
                mainLayout.ColumnStyles[0].Width = brandPercent;
                mainLayout.ColumnStyles[1].SizeType = SizeType.Percent;
                mainLayout.ColumnStyles[1].Width = 100F - brandPercent;
            }

            brandPanel.Visible = true;
            formHostPanel.Margin = new Padding(14, 0, 0, 0);
        }

        private void ResizeCard(bool singleColumnMode, int safeX, LayoutPreset preset)
        {
            int maxCardWidth = singleColumnMode ? Math.Max(600, preset.CardMaxW) : preset.CardMaxW;
            int formWidth = Math.Max(360, formHostPanel.ClientSize.Width);
            int formHeight = Math.Max(320, formHostPanel.ClientSize.Height);
            int effectiveMinWidth = Math.Max(CardMinWidth, preset.CardMinW);
            int effectiveMaxWidth = Math.Max(effectiveMinWidth, maxCardWidth);
            int cardWidth = Math.Clamp(formWidth - Math.Max(32, safeX / 2), effectiveMinWidth, effectiveMaxWidth);

            int effectiveMinHeight = Math.Max(CardMinHeight, preset.CardMinH);
            int preferredMaxHeight = Math.Max(effectiveMinHeight, Math.Max(CardMaxHeight, preset.CardMaxH));
            int preferredHeight = Math.Clamp((int)(formHeight * 0.74), effectiveMinHeight, preferredMaxHeight);
            int requiredHeight = Math.Max(effectiveMinHeight, CreateCardLayoutSnapshot(cardWidth).RequiredHeight);
            int cardHeight = Math.Max(preferredHeight, requiredHeight);

            cardPanel.Size = new Size(cardWidth, cardHeight);
            int yOffset = singleColumnMode ? -6 : preset.CardYOffset;
            int topInset = ScaleByDpi(8);
            int cardY = cardHeight <= formHeight
                ? Math.Max(topInset, ((formHeight - cardPanel.Height) / 2) + yOffset)
                : topInset;
            cardPanel.Location = new Point((formWidth - cardPanel.Width) / 2, cardY);
            formHostPanel.AutoScrollMinSize = new Size(
                cardPanel.Right + ScaleByDpi(8),
                cardPanel.Bottom + ScaleByDpi(8));
        }

        private static LayoutPreset GetLayoutPreset(int width, int height)
        {
            if (width >= 2560)
            {
                return new LayoutPreset(52, 148, 34, 96, 1260, 43F, 450, 580, 440, 510, -12, 19F, 10.5F);
            }

            if (width >= 1920)
            {
                return new LayoutPreset(40, 124, 28, 90, 1220, 44F, 440, 570, 438, 504, -12, 18.5F, 10.25F);
            }

            if (width >= 1600)
            {
                return new LayoutPreset(32, 106, 24, 82, 1180, 45F, 432, 560, 434, 500, -10, 18F, 10F);
            }

            if (width >= 1366 || (width >= 1280 && height >= 720))
            {
                return new LayoutPreset(24, 84, 20, 72, 1120, 46F, 420, 548, 430, 492, -8, 17.5F, 10F);
            }

            return new LayoutPreset(20, 64, 16, 60, 1080, 47F, 400, 536, 420, 486, -6, 17F, 9.75F);
        }

        private void ApplyAdaptiveTypography(LayoutPreset preset)
        {
            lblTitle.Appearance.Font = new Font("Segoe UI", preset.TitleFontSize, FontStyle.Bold, GraphicsUnit.Point);
            lblSubtitle.Appearance.Font = new Font("Segoe UI", preset.SubtitleFontSize, FontStyle.Regular, GraphicsUnit.Point);
            lblBrandTitle.Appearance.Font = new Font("Segoe UI", preset.TitleFontSize + 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblBrandTagline.Appearance.Font = new Font("Segoe UI Semibold", preset.SubtitleFontSize + 1.75F, FontStyle.Regular, GraphicsUnit.Point);
            lblBrandDesc.Appearance.Font = new Font("Segoe UI", preset.SubtitleFontSize + 0.5F, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void LayoutCardContent()
        {
            cardPanel.SuspendLayout();

            int left = cardPanel.Padding.Left;
            int right = cardPanel.Padding.Right;
            int top = cardPanel.Padding.Top;
            int bottom = cardPanel.Padding.Bottom;
            CardLayoutSnapshot snapshot = CreateCardLayoutSnapshot(cardPanel.ClientSize.Width);
            int y = top + 4;

            lblTitle.SetBounds(left, y, snapshot.ContentWidth, snapshot.TitleHeight);
            y += snapshot.TitleHeight + ScaleByDpi(8);

            lblSubtitle.SetBounds(left, y, snapshot.ContentWidth, snapshot.SubtitleHeight);
            y += snapshot.SubtitleHeight + ScaleByDpi(12);

            lblError.SetBounds(left, y, snapshot.ContentWidth, Math.Max(snapshot.ErrorHeight, 1));
            y += lblError.Visible ? snapshot.ErrorHeight + ScaleByDpi(10) : ScaleByDpi(8);

            lblUserId.SetBounds(left, y, snapshot.ContentWidth, snapshot.UserLabelHeight);
            y += lblUserId.Height + 4;

            txtuserid.SetBounds(left, y, snapshot.ContentWidth, snapshot.InputHeight);
            y += txtuserid.Height + snapshot.VerticalGapSmall;

            lblPassword.SetBounds(left, y, snapshot.ContentWidth, snapshot.PasswordLabelHeight);
            y += lblPassword.Height + 4;

            passwordLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
            passwordLayout.ColumnStyles[1].Width = snapshot.ToggleButtonWidth;
            passwordLayout.SetBounds(left, y, snapshot.ContentWidth, snapshot.PasswordLayoutHeight);
            y += passwordLayout.Height + snapshot.VerticalGapSmall;

            optionsLayout.ColumnStyles[0].Width = 45F;
            optionsLayout.ColumnStyles[1].Width = 55F;
            optionsLayout.SetBounds(left, y, snapshot.ContentWidth, snapshot.OptionsLayoutHeight);
            y += optionsLayout.Height + snapshot.VerticalGapMedium;

            int minInfoY = y + snapshot.FooterLayoutHeight + snapshot.FooterInfoGap;
            int preferredInfoY = cardPanel.ClientSize.Height - bottom - snapshot.InfoHeight;
            int infoY = Math.Max(minInfoY, preferredInfoY);
            int footerY = infoY - snapshot.FooterInfoGap - snapshot.FooterLayoutHeight;

            footerLayout.SetBounds(left, footerY, snapshot.ContentWidth, snapshot.FooterLayoutHeight);

            int footerTextGap = ScaleByDpi(16);
            int versionWidth = Math.Min(snapshot.VersionWidth, snapshot.ContentWidth);
            int supportX = Math.Min(left + versionWidth + footerTextGap, left + snapshot.ContentWidth);
            int supportWidth = Math.Max(0, (left + snapshot.ContentWidth) - supportX);
            lblversi.SetBounds(left, infoY, versionWidth, snapshot.InfoHeight);
            lblSupport.SetBounds(supportX, infoY, supportWidth, snapshot.InfoHeight);

            if (cardPanel.ClientSize.Height - top - bottom < 350)
            {
                btnTogglePassword.Text = "Lihat";
                txtpwd.Properties.UseSystemPasswordChar = true;
            }

            cardPanel.ResumeLayout();
        }

        private CardLayoutSnapshot CreateCardLayoutSnapshot(int cardWidth)
        {
            int left = cardPanel.Padding.Left;
            int right = cardPanel.Padding.Right;
            int top = cardPanel.Padding.Top;
            int bottom = cardPanel.Padding.Bottom;
            int contentWidth = Math.Max(320, cardWidth - left - right);
            int inputHeight = ScaleByDpi(34);
            int passwordLayoutHeight = ScaleByDpi(40);
            int optionsLayoutHeight = ScaleByDpi(30);
            int footerLayoutHeight = ScaleByDpi(52);
            int verticalGapSmall = ScaleByDpi(6);
            int verticalGapMedium = ScaleByDpi(10);
            int footerInfoGap = ScaleByDpi(8);
            int minPasswordInputWidth = ScaleByDpi(180);
            int toggleButtonWidth = Math.Max(
                ScaleByDpi(96),
                MeasureTextWidth(btnTogglePassword.Text, btnTogglePassword.Font) + ScaleByDpi(28));
            toggleButtonWidth = Math.Min(toggleButtonWidth, Math.Max(ScaleByDpi(96), contentWidth - minPasswordInputWidth));

            int titleHeight = MeasureTextHeight(lblTitle.Text, lblTitle.Appearance.Font, contentWidth, true);
            int subtitleHeight = MeasureTextHeight(lblSubtitle.Text, lblSubtitle.Appearance.Font, contentWidth, true);
            int errorHeight = lblError.Visible
                ? MeasureTextHeight(lblError.Text, lblError.Appearance.Font, contentWidth, true)
                : 0;
            int userLabelHeight = MeasureTextHeight(lblUserId.Text, lblUserId.Appearance.Font, contentWidth, false);
            int passwordLabelHeight = MeasureTextHeight(lblPassword.Text, lblPassword.Appearance.Font, contentWidth, false);
            int infoHeight = Math.Max(
                MeasureTextHeight(lblversi.Text, lblversi.Appearance.Font, contentWidth, false),
                MeasureTextHeight(lblSupport.Text, lblSupport.Appearance.Font, contentWidth, false));
            int versionWidth = MeasureTextWidth(lblversi.Text, lblversi.Appearance.Font) + ScaleByDpi(6);

            int requiredHeight =
                top + 4 +
                titleHeight + ScaleByDpi(8) +
                subtitleHeight + ScaleByDpi(12) +
                (lblError.Visible ? errorHeight + ScaleByDpi(10) : ScaleByDpi(8)) +
                userLabelHeight + ScaleByDpi(4) +
                inputHeight + verticalGapSmall +
                passwordLabelHeight + ScaleByDpi(4) +
                passwordLayoutHeight + verticalGapSmall +
                optionsLayoutHeight + verticalGapMedium +
                footerLayoutHeight + footerInfoGap +
                infoHeight + bottom;

            return new CardLayoutSnapshot(
                contentWidth,
                titleHeight,
                subtitleHeight,
                errorHeight,
                userLabelHeight,
                passwordLabelHeight,
                inputHeight,
                passwordLayoutHeight,
                optionsLayoutHeight,
                footerLayoutHeight,
                verticalGapSmall,
                verticalGapMedium,
                footerInfoGap,
                toggleButtonWidth,
                versionWidth,
                infoHeight,
                requiredHeight);
        }

        private static int MeasureTextHeight(string text, Font font, int width, bool allowWrap)
        {
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.TextBoxControl;
            flags |= allowWrap ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine;
            Size measured = TextRenderer.MeasureText(text ?? string.Empty, font, new Size(width, int.MaxValue), flags);
            return Math.Max(font.Height, measured.Height);
        }

        private static int MeasureTextWidth(string text, Font font)
        {
            Size measured = TextRenderer.MeasureText(
                text ?? string.Empty,
                font,
                new Size(int.MaxValue, font.Height),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            return Math.Max(0, measured.Width);
        }

        private int ScaleByDpi(int logicalPixels)
        {
            float scale = DeviceDpi <= 0 ? 1F : DeviceDpi / 96F;
            return Math.Max(logicalPixels, (int)Math.Round(logicalPixels * scale));
        }

        private static void ApplyRoundedCorners(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            GraphicsPath path = new();
            int diameter = radius * 2;
            Rectangle bounds = new(0, 0, control.Width, control.Height);

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            control.Region?.Dispose();
            control.Region = new Region(path);
            path.Dispose();
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

            IOverlaySplashScreenHandle? handle = null;

            try
            {
                ClearValidationState();
                SetBusyState(true);
                handle = SplashScreenManager.ShowOverlayForm(this);

                string password = txtpwd.Text;
                LoginAuthResult result = UserManager_Services.AuthenticateForModule(userId, password, LoginInfo.MODULE);
                HandleAuthResult(result);
            }
            catch (Exception ex)
            {
                SetValidationError("Terjadi kesalahan sistem saat proses masuk.");
                XtraMessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void HandleAuthResult(LoginAuthResult result)
        {
            switch (result.Status)
            {
                case LoginAuthStatus.Success:
                    ClearValidationState();
                    CompleteLogin(result.Users, result.RequiresPasswordChange);
                    break;

                case LoginAuthStatus.UserNotFound:
                case LoginAuthStatus.InvalidPassword:
                    HandleInvalidCredentials();
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

                case LoginAuthStatus.LockedOut:
                    HandleLockedOut(result.LockoutUntilUtc);
                    break;

                default:
                    SetValidationError("Proses login tidak dapat diselesaikan.");
                    XtraMessageBox.Show("Proses login tidak dapat diselesaikan.", "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void CompleteLogin(System.Collections.Generic.List<LOGIN_USERS_DTO> users, bool requiresPasswordChange)
        {
            if (users.Count == 0)
            {
                HandleNoLocationAccess();
                return;
            }

            LOGIN_USERS_DTO user = users[0];
            AppSession.StartAuthenticatedUser(user.USERID, user.LEVEL_USER, LoginInfo.MODULE, requiresPasswordChange);

            if (requiresPasswordChange && !EnsureRequiredPasswordChange())
            {
                SetValidationError("Password sementara harus diganti sebelum melanjutkan.", txtpwd);
                txtpwd.SelectAll();
                txtpwd.Focus();
                return;
            }

            if (users.Count > 1)
            {
                using FrmLokasi lokasiDataForm = new();
                Hide();
                lokasiDataForm.ShowDialog();
                return;
            }

            AppSession.ApplyLocationContext(user);
            UserManager_Services.RecordLocationSelection(user.USERID, LoginInfo.MODULE, user.IDDATA);
            Hide();
            new MainView().Show();
        }

        private bool IsInputValid()
        {
            ClearValidationState();

            if (string.IsNullOrWhiteSpace(txtuserid.Text))
            {
                PlaySound("userid.wav");
                SetValidationError("User ID wajib diisi.", txtuserid);
                txtuserid.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtpwd.Text))
            {
                SetValidationError("Kata sandi wajib diisi.", txtpwd);
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
            chkRememberMe.Enabled = !busy;
            btnTogglePassword.Enabled = !busy;
            linkForgotPassword.Enabled = !busy;
            Login.Text = busy ? "Memproses..." : "Masuk";
            UseWaitCursor = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private void PlaySound(string soundFileName)
        {
            Player.SoundLocation = Path.Combine(Environment.CurrentDirectory, "wav", soundFileName);
            Player.Play();
        }

        private bool EnsureRequiredPasswordChange()
        {
            using FrmChangePass changePasswordForm = new()
            {
                RequireCurrentPassword = false,
                ForcePasswordChange = true,
                StartPosition = FormStartPosition.CenterParent
            };

            DialogResult dialogResult = changePasswordForm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                return true;
            }

            XtraMessageBox.Show(
                "Password sementara wajib diganti sebelum masuk ke aplikasi.",
                "Password Wajib Diganti",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private void HandleInvalidCredentials()
        {
            PlaySound("sandi_salah.wav");
            SetValidationError("User ID atau password tidak valid.", txtpwd);
            XtraMessageBox.Show("User ID atau password tidak valid.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtpwd.SelectAll();
            txtpwd.Focus();
        }

        private void HandleInactiveUser()
        {
            PlaySound("maaf_noakses.wav");
            SetValidationError("Akun tidak aktif. Hubungi administrator.", txtuserid);
            XtraMessageBox.Show("Akun Anda tidak aktif. Hubungi administrator.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void HandleNoModuleAccess()
        {
            PlaySound("maaf_noakses.wav");
            SetValidationError($"Tidak ada akses ke modul {LoginInfo.MODULE}.", txtuserid);
            XtraMessageBox.Show($"Kredensial valid, tetapi akses modul '{LoginInfo.MODULE}' belum diberikan.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void HandleNoLocationAccess()
        {
            PlaySound("iddata.wav");
            SetValidationError("Akun ini belum memiliki akses lokasi.", txtuserid);
            XtraMessageBox.Show("Kredensial valid, tetapi akses lokasi (IDDATA) belum diberikan.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void HandleLockedOut(DateTimeOffset? lockoutUntilUtc)
        {
            PlaySound("hub_manager.wav");
            TimeSpan remaining = lockoutUntilUtc.HasValue
                ? lockoutUntilUtc.Value - DateTimeOffset.UtcNow
                : TimeSpan.FromMinutes(15);
            int waitMinutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
            SetValidationError($"Akun terkunci sementara. Coba lagi dalam {waitMinutes} menit.", txtuserid);
            XtraMessageBox.Show($"Akun terkunci sementara. Coba lagi dalam {waitMinutes} menit.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtuserid.Focus();
        }

        private void SimpleButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            bool reveal = txtpwd.Properties.UseSystemPasswordChar;
            txtpwd.Properties.UseSystemPasswordChar = !reveal;
            btnTogglePassword.Text = reveal ? "Sembunyikan" : "Lihat";
            ApplyResponsiveLayout();
            txtpwd.Focus();
            txtpwd.SelectionStart = txtpwd.Text.Length;
        }

        private void linkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            XtraMessageBox.Show("Silakan hubungi administrator sistem untuk reset kata sandi.", "Lupa Kata Sandi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Input_EditValueChanged(object? sender, EventArgs e)
        {
            ClearValidationState();
        }

        private void SetValidationError(string message, TextEdit? target = null)
        {
            bool layoutChanged = lblError.Visible != !string.IsNullOrWhiteSpace(message) || !string.Equals(lblError.Text, message, StringComparison.Ordinal);
            lblError.Text = message;
            lblError.Visible = !string.IsNullOrWhiteSpace(message);

            MarkFieldInvalid(txtuserid, false);
            MarkFieldInvalid(txtpwd, false);

            if (target != null)
            {
                MarkFieldInvalid(target, true);
            }

            if (layoutChanged && IsHandleCreated)
            {
                ApplyResponsiveLayout();
            }
        }

        private void ClearValidationState()
        {
            bool layoutChanged = lblError.Visible || !string.Equals(lblError.Text, " ", StringComparison.Ordinal);
            lblError.Visible = false;
            lblError.Text = " ";
            MarkFieldInvalid(txtuserid, false);
            MarkFieldInvalid(txtpwd, false);

            if (layoutChanged && IsHandleCreated)
            {
                ApplyResponsiveLayout();
            }
        }

        private static void MarkFieldInvalid(TextEdit editor, bool invalid)
        {
            editor.Properties.Appearance.Options.UseBackColor = true;
            editor.Properties.Appearance.BackColor = invalid ? Color.FromArgb(254, 242, 242) : Color.White;
        }

        private void SetUserInformation(LOGIN_USERS_DTO user)
        {
            AppSession.ApplyLocationContext(user);
        }
    }
}
