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
        private const int ShellMinHeight = 480;
        private const int TargetCardWidth = 440;
        private const int TargetCardHeight = 480;

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
            MinimumSize = new Size(1024, 600);
            formHostPanel.AutoScroll = true;
            cardPanel.MinimumSize = new Size(400, 420);

            // Double-buffer the container panels so the responsive relayout repaints
            // smoothly instead of flickering panel-by-panel on startup and resize.
            EnableDoubleBuffering(backgroundPanel);
            EnableDoubleBuffering(shellPanel);
            EnableDoubleBuffering(mainLayout);
            EnableDoubleBuffering(formHostPanel);
            EnableDoubleBuffering(cardPanel);
            EnableDoubleBuffering(passwordLayout);
            EnableDoubleBuffering(optionsLayout);
            EnableDoubleBuffering(footerLayout);

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

        // The form's DoubleBuffered flag does not propagate to child controls, so the
        // startup relayout repaints each container panel independently, producing the
        // visible flicker/pop-in. Enable double buffering on each panel directly via
        // its protected DoubleBuffered property.
        private static void EnableDoubleBuffering(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(control, true);
        }

        private void Frmlogin_Load(object sender, EventArgs e)
        {
            // For 1280x720 screens, WorkingArea may be ~1280x680 (taskbar).
            // Ensure the form fills the working area without overflow.
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;

            // Apply the final bounds without letting the resize trigger its own layout
            // pass; we run ApplyResponsiveLayout exactly once below so the form lays out
            // and paints in a single pass before it first becomes visible.
            Resize -= Frmlogin_Resize;
            Bounds = workArea;
            Resize += Frmlogin_Resize;

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
            backgroundPanel.BackColor = Color.FromArgb(16, 29, 48);

            string customBackgroundPath = Path.Combine(Application.StartupPath, "Resources", "login-wireguard-bg.png");
            if (File.Exists(customBackgroundPath))
            {
                backgroundPanel.BackgroundImage = Image.FromFile(customBackgroundPath);
            }
            else
            {
                backgroundPanel.BackgroundImage = null;
            }

            backgroundPanel.BackgroundImageLayout = ImageLayout.Stretch;

            // Overlay mode: make intermediary panels transparent
            // so the background image is fully visible behind the login card
            shellPanel.BackColor = Color.Transparent;
            shellPanel.Dock = DockStyle.Fill;
            mainLayout.BackColor = Color.Transparent;
            formHostPanel.BackColor = Color.Transparent;
        }

        private void ApplyResponsiveLayout()
        {
            if (backgroundPanel == null || shellPanel == null || formHostPanel == null)
            {
                return;
            }

            // Guard: during InitializeComponent → ResumeLayout, the form may not
            // have its final dimensions yet, producing invalid layout calculations.
            if (backgroundPanel.ClientSize.Width < 100 || backgroundPanel.ClientSize.Height < 100)
            {
                return;
            }

            // Overlay mode: no padding on background, shell fills via Dock
            backgroundPanel.Padding = Padding.Empty;

            LayoutPreset preset = GetLayoutPreset(backgroundPanel.ClientSize.Width, backgroundPanel.ClientSize.Height);

            // Coalesce all child repositioning in this pass into a single layout cycle.
            SuspendLayout();
            try
            {
                // Always single column with brand hidden (branding is in the background image)
                ConfigureLayoutMode(true, 0F);
                ApplyAdaptiveTypography(preset);

                int safeX = Math.Clamp(backgroundPanel.ClientSize.Width / 20, preset.SafeXMin, preset.SafeXMax);
                ResizeCard(true, safeX, preset);
                LayoutCardContent();
                ApplyRoundedCorners(cardPanel, 18);
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        private void ConfigureLayoutMode(bool singleColumnMode, float brandPercent)
        {
            // Overlay mode: always single column, brand panel permanently hidden
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
        }

        private void ResizeCard(bool singleColumnMode, int safeX, LayoutPreset preset)
        {
            int formWidth = Math.Max(360, formHostPanel.ClientSize.Width);
            int formHeight = Math.Max(320, formHostPanel.ClientSize.Height);

            // 1. Scale ideal dimensions based on system DPI
            int cardWidth = ScaleByDpi(TargetCardWidth);

            // 1a. Use compact padding when vertical space is tight (e.g. 1280x720)
            bool compactMode = formHeight < ScaleByDpi(660);
            if (compactMode)
            {
                cardPanel.Padding = new Padding(28, 20, 28, 18);
            }
            else
            {
                cardPanel.Padding = new Padding(34, 30, 34, 24);
            }

            int requiredHeight = CreateCardLayoutSnapshot(cardWidth).RequiredHeight;
            int cardHeight = Math.Max(ScaleByDpi(TargetCardHeight), requiredHeight);

            // 2. Adjust gracefully if form size is smaller than target card dimensions
            if (cardWidth > formWidth - ScaleByDpi(32))
            {
                cardWidth = formWidth - ScaleByDpi(32);
            }
            if (cardHeight > formHeight - ScaleByDpi(16))
            {
                cardHeight = formHeight - ScaleByDpi(16);
            }

            cardPanel.Size = new Size(cardWidth, cardHeight);
            int topInset = ScaleByDpi(8);
            
            // Lower the card vertically to avoid overlapping background elements (logo/icons)
            int yOffset = ScaleByDpi(70);
            int cardY = topInset;
            if (cardHeight <= formHeight)
            {
                int calculatedY = (formHeight - cardPanel.Height) / 2 + yOffset;
                int maxVal = formHeight - cardPanel.Height - ScaleByDpi(20);
                cardY = topInset <= maxVal ? Math.Clamp(calculatedY, topInset, maxVal) : topInset;
            }

            // 3. Position card on the right side with a comfortable margin.
            //    Use a smaller margin on compact (1280x720-class) screens.
            int rightMargin = formWidth <= 1300 ? ScaleByDpi(40) : ScaleByDpi(64);
            int cardX = Math.Max(ScaleByDpi(16), formWidth - cardPanel.Width - rightMargin);

            cardPanel.Location = new Point(cardX - 100, cardY + 50);
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
                return new LayoutPreset(24, 84, 16, 56, 1120, 46F, 410, 540, 400, 470, -6, 17F, 9.75F);
            }

            return new LayoutPreset(20, 64, 12, 48, 1080, 47F, 380, 520, 380, 460, -4, 16.5F, 9.5F);
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
                
                // Force immediate layout resolution to ensure correct coordinates for the overlay window
                this.PerformLayout();
                this.Update();

                OverlayWindowOptions options = new OverlayWindowOptions
                {
                    CustomPainter = new CustomOverlayPainter()
                };
                handle = SplashScreenManager.ShowOverlayForm(cardPanel, options);

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

        private class CustomOverlayPainter : OverlayWindowPainterBase
        {
            protected override void Draw(OverlayWindowCustomDrawContext context)
            {
                // Disable default rendering so we don't paint the whole form
                context.Handled = true;

                var cache = context.DrawArgs.Cache;
                var bounds = context.DrawArgs.Bounds;

                // Fill only the cardPanel bounds with a premium semi-transparent dark shade
                using (var brush = new SolidBrush(Color.FromArgb(100, 16, 29, 48)))
                {
                    cache.FillRectangle(brush, bounds);
                }

                // Draw the default animation spinner inside the cardPanel bounds
                context.DrawImage();
            }
        }
    }
}
