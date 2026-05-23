using DevExpress.XtraSplashScreen;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class SplashScreen_Start : SplashScreen
    {
        private const int DesignWidth = 1366;
        private const int DesignHeight = 768;
        private float pulsePhase;

        public SplashScreen_Start()
        {
            InitializeComponent();
            ConfigureStartupMetadata();
            ApplyResponsiveLayout();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            if (cmd is SplashScreenCommand command && command == SplashScreenCommand.UpdateStatus && arg is string statusText && !string.IsNullOrWhiteSpace(statusText))
            {
                labelStatus.Text = statusText;
            }

            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
            UpdateStatus
        }

        private void ConfigureStartupMetadata()
        {
            string version = Application.ProductVersion;
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                {
                    version = versionInfo.FileVersion;
                }
            }
            catch
            {
            }

            string environment = "PROD";
#if DEBUG
            environment = "DEV";
#endif
            labelVersion.Text = $"Version {version}";
            labelEnvironment.Text = $"Environment: {environment}";
            labelCopyright.Text = $"Copyright (c) Graha Fajar 2021-{DateTime.Now.Year}";
        }

        private void SplashScreen_Start_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            ApplyResponsiveLayout();
        }

        private void SplashScreen_Start_Shown(object sender, EventArgs e)
        {
            fadeTimer.Start();
            pulseTimer.Start();
        }

        private void SplashScreen_Start_Resize(object sender, EventArgs e)
        {
            ApplyResponsiveLayout();
            Invalidate();
        }

        private void ApplyResponsiveLayout()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                return;
            }

            int minMarginX = Math.Max(20, ClientSize.Width / 26);
            int minMarginY = Math.Max(18, ClientSize.Height / 22);
            int maxCardWidth = Math.Min(980, ClientSize.Width - (minMarginX * 2));
            int maxCardHeight = Math.Min(560, ClientSize.Height - (minMarginY * 2));

            int preferredCardWidth = (int)(ClientSize.Width * 0.54f);
            int preferredCardHeight = (int)(ClientSize.Height * 0.70f);
            int cardWidth = Math.Clamp(preferredCardWidth, 520, Math.Max(520, maxCardWidth));
            int cardHeight = Math.Clamp(preferredCardHeight, 360, Math.Max(360, maxCardHeight));

            cardPanel.Size = new Size(cardWidth, cardHeight);
            cardPanel.Location = new Point(
                (ClientSize.Width - cardPanel.Width) / 2,
                (ClientSize.Height - cardPanel.Height) / 2);

            float layoutScale = Math.Min(ClientSize.Width / (float)DesignWidth, ClientSize.Height / (float)DesignHeight);
            float cardScale = Math.Clamp(layoutScale, 0.85f, 1.65f);

            int sidePadding = (int)Math.Round(32 * cardScale);
            int topPadding = (int)Math.Round(28 * cardScale);
            int bottomPadding = (int)Math.Round(24 * cardScale);
            cardPanel.Padding = new Padding(sidePadding, topPadding, sidePadding, bottomPadding);

            int contentWidth = Math.Max(320, cardPanel.ClientSize.Width - (cardPanel.Padding.Left + cardPanel.Padding.Right));
            int y = cardPanel.Padding.Top;

            int logoWidth = Math.Clamp((int)(176 * cardScale), 130, Math.Max(130, contentWidth - 140));
            int logoHeight = Math.Clamp((int)(86 * cardScale), 62, 160);
            peLogo.Size = new Size(logoWidth, logoHeight);
            peLogo.Location = new Point((cardPanel.ClientSize.Width - peLogo.Width) / 2, y);
            y += peLogo.Height + Math.Clamp((int)(16 * cardScale), 10, 26);

            labelAppName.Appearance.Font = new Font("Segoe UI Semibold", Math.Clamp(22f * cardScale, 18f, 38f), FontStyle.Bold);
            labelAppName.Size = new Size(contentWidth, Math.Clamp((int)(56 * cardScale), 42, 90));
            labelAppName.Location = new Point(cardPanel.Padding.Left, y);
            y += labelAppName.Height + Math.Clamp((int)(8 * cardScale), 6, 12);

            labelTagline.Appearance.Font = new Font("Segoe UI", Math.Clamp(10f * cardScale, 9f, 14f), FontStyle.Regular);
            labelTagline.Size = new Size(contentWidth, Math.Clamp((int)(36 * cardScale), 24, 52));
            labelTagline.Location = new Point(cardPanel.Padding.Left, y);
            y += labelTagline.Height + Math.Clamp((int)(20 * cardScale), 12, 30);

            labelStatus.Appearance.Font = new Font("Segoe UI", Math.Clamp(9f * cardScale, 8f, 12f), FontStyle.Regular);
            labelStatus.Size = new Size(contentWidth, Math.Clamp((int)(24 * cardScale), 20, 32));
            labelStatus.Location = new Point(cardPanel.Padding.Left, y);
            y += labelStatus.Height + Math.Clamp((int)(6 * cardScale), 4, 10);

            progressBarControl.Size = new Size(contentWidth, Math.Clamp((int)(14 * cardScale), 10, 18));
            progressBarControl.Location = new Point(cardPanel.Padding.Left, y);
            y += progressBarControl.Height + Math.Clamp((int)(14 * cardScale), 10, 20);

            labelCopyright.Appearance.Font = new Font("Segoe UI", Math.Clamp(8.5f * cardScale, 8f, 11f), FontStyle.Regular);
            labelCopyright.Size = new Size(contentWidth, Math.Clamp((int)(28 * cardScale), 20, 38));
            labelCopyright.Location = new Point(cardPanel.Padding.Left, y);

            int footerY = cardPanel.ClientSize.Height - cardPanel.Padding.Bottom - Math.Max(labelVersion.Height, labelEnvironment.Height);
            labelVersion.Appearance.Font = new Font("Segoe UI", Math.Clamp(8.5f * cardScale, 8f, 11f), FontStyle.Regular);
            labelVersion.Location = new Point(cardPanel.Padding.Left, footerY);

            labelEnvironment.Appearance.Font = new Font("Segoe UI Semibold", Math.Clamp(8.5f * cardScale, 8f, 11f), FontStyle.Regular);
            labelEnvironment.Location = new Point(
                cardPanel.ClientSize.Width - cardPanel.Padding.Right - labelEnvironment.Width,
                footerY);
        }

        private void fadeTimer_Tick(object sender, EventArgs e)
        {
            if (Opacity >= 1)
            {
                fadeTimer.Stop();
                return;
            }

            Opacity = Math.Min(1, Opacity + 0.08);
        }

        private void pulseTimer_Tick(object sender, EventArgs e)
        {
            pulsePhase += 0.18f;
            if (pulsePhase >= (float)(Math.PI * 2))
            {
                pulsePhase = 0f;
            }

            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle bounds = ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                base.OnPaintBackground(e);
                return;
            }

            using (LinearGradientBrush backgroundBrush = new LinearGradientBrush(
                bounds,
                Color.FromArgb(8, 15, 30),
                Color.FromArgb(16, 36, 63),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(backgroundBrush, bounds);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float pulse = 0.5f + (0.5f * (float)Math.Sin(pulsePhase));

            int glowAlpha = (int)(28 + (pulse * 26));
            int glowSize = Math.Max(220, (int)(Math.Min(bounds.Width, bounds.Height) * 0.42f));
            Rectangle glow = new Rectangle(
                (bounds.Width - glowSize) / 2,
                (bounds.Height - glowSize) / 2 - (int)(22 * pulse),
                glowSize,
                glowSize);

            using (GraphicsPath glowPath = new GraphicsPath())
            {
                glowPath.AddEllipse(glow);
                using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                {
                    glowBrush.CenterColor = Color.FromArgb(glowAlpha, 95, 165, 255);
                    glowBrush.SurroundColors = new[] { Color.FromArgb(0, 95, 165, 255) };
                    e.Graphics.FillEllipse(glowBrush, glow);
                }
            }
        }

    }
}
