
namespace Accounting.Form
{
    partial class SplashScreen_Start
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cardPanel = new DevExpress.XtraEditors.PanelControl();
            this.labelEnvironment = new DevExpress.XtraEditors.LabelControl();
            this.labelVersion = new DevExpress.XtraEditors.LabelControl();
            this.progressBarControl = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this.labelStatus = new DevExpress.XtraEditors.LabelControl();
            this.labelTagline = new DevExpress.XtraEditors.LabelControl();
            this.labelAppName = new DevExpress.XtraEditors.LabelControl();
            this.peLogo = new DevExpress.XtraEditors.PictureEdit();
            this.labelCopyright = new DevExpress.XtraEditors.LabelControl();
            this.fadeTimer = new System.Windows.Forms.Timer(this.components);
            this.pulseTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.cardPanel)).BeginInit();
            this.cardPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.peLogo.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // cardPanel
            // 
            this.cardPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(26)))), ((int)(((byte)(42)))));
            this.cardPanel.Appearance.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(17)))), ((int)(((byte)(29)))));
            this.cardPanel.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(78)))), ((int)(((byte)(120)))));
            this.cardPanel.Appearance.Options.UseBackColor = true;
            this.cardPanel.Appearance.Options.UseBorderColor = true;
            this.cardPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.cardPanel.Controls.Add(this.labelEnvironment);
            this.cardPanel.Controls.Add(this.labelVersion);
            this.cardPanel.Controls.Add(this.progressBarControl);
            this.cardPanel.Controls.Add(this.labelStatus);
            this.cardPanel.Controls.Add(this.labelTagline);
            this.cardPanel.Controls.Add(this.labelAppName);
            this.cardPanel.Controls.Add(this.peLogo);
            this.cardPanel.Controls.Add(this.labelCopyright);
            this.cardPanel.Location = new System.Drawing.Point(197, 89);
            this.cardPanel.Name = "cardPanel";
            this.cardPanel.Padding = new System.Windows.Forms.Padding(32, 28, 32, 26);
            this.cardPanel.Size = new System.Drawing.Size(726, 470);
            this.cardPanel.TabIndex = 0;
            // 
            // labelEnvironment
            // 
            this.labelEnvironment.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.labelEnvironment.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(159)))), ((int)(((byte)(178)))), ((int)(((byte)(214)))));
            this.labelEnvironment.Appearance.Options.UseFont = true;
            this.labelEnvironment.Appearance.Options.UseForeColor = true;
            this.labelEnvironment.Location = new System.Drawing.Point(627, 438);
            this.labelEnvironment.Name = "labelEnvironment";
            this.labelEnvironment.Size = new System.Drawing.Size(77, 15);
            this.labelEnvironment.TabIndex = 7;
            this.labelEnvironment.Text = "Environment: ";
            // 
            // labelVersion
            // 
            this.labelVersion.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelVersion.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(159)))), ((int)(((byte)(178)))), ((int)(((byte)(214)))));
            this.labelVersion.Appearance.Options.UseFont = true;
            this.labelVersion.Appearance.Options.UseForeColor = true;
            this.labelVersion.Location = new System.Drawing.Point(34, 438);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(47, 15);
            this.labelVersion.TabIndex = 6;
            this.labelVersion.Text = "Version ";
            // 
            // progressBarControl
            // 
            this.progressBarControl.EditValue = 0;
            this.progressBarControl.Location = new System.Drawing.Point(116, 334);
            this.progressBarControl.Name = "progressBarControl";
            this.progressBarControl.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(39)))), ((int)(((byte)(63)))));
            this.progressBarControl.Properties.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(161)))), ((int)(((byte)(255)))));
            this.progressBarControl.Properties.Appearance.Options.UseBackColor = true;
            this.progressBarControl.Properties.Appearance.Options.UseForeColor = true;
            this.progressBarControl.Properties.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(128)))), ((int)(((byte)(242)))));
            this.progressBarControl.Properties.LookAndFeel.SkinName = "Office 2019 Colorful";
            this.progressBarControl.Properties.LookAndFeel.UseDefaultLookAndFeel = false;
            this.progressBarControl.Properties.MarqueeAnimationSpeed = 34;
            this.progressBarControl.Properties.ShowTitle = true;
            this.progressBarControl.Properties.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(168)))), ((int)(((byte)(255)))));
            this.progressBarControl.Size = new System.Drawing.Size(492, 14);
            this.progressBarControl.TabIndex = 4;
            // 
            // labelStatus
            // 
            this.labelStatus.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.labelStatus.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(219)))), ((int)(((byte)(245)))));
            this.labelStatus.Appearance.Options.UseFont = true;
            this.labelStatus.Appearance.Options.UseForeColor = true;
            this.labelStatus.Appearance.Options.UseTextOptions = true;
            this.labelStatus.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelStatus.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelStatus.Location = new System.Drawing.Point(114, 304);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(496, 23);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Initializing secure startup sequence...";
            // 
            // labelTagline
            // 
            this.labelTagline.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelTagline.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(203)))), ((int)(((byte)(233)))));
            this.labelTagline.Appearance.Options.UseFont = true;
            this.labelTagline.Appearance.Options.UseForeColor = true;
            this.labelTagline.Appearance.Options.UseTextOptions = true;
            this.labelTagline.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelTagline.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelTagline.Location = new System.Drawing.Point(86, 219);
            this.labelTagline.Name = "labelTagline";
            this.labelTagline.Size = new System.Drawing.Size(552, 40);
            this.labelTagline.TabIndex = 2;
            this.labelTagline.Text = "Reliable financial operations platform";
            // 
            // labelAppName
            // 
            this.labelAppName.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppName.Appearance.ForeColor = System.Drawing.Color.White;
            this.labelAppName.Appearance.Options.UseFont = true;
            this.labelAppName.Appearance.Options.UseForeColor = true;
            this.labelAppName.Appearance.Options.UseTextOptions = true;
            this.labelAppName.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelAppName.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelAppName.Location = new System.Drawing.Point(84, 152);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(556, 58);
            this.labelAppName.TabIndex = 1;
            this.labelAppName.Text = "GL Accounting Suite";
            // 
            // peLogo
            // 
            this.peLogo.EditValue = global::Accounting.Properties.Resources.the_it_dept_logo_200px_300x155;
            this.peLogo.Location = new System.Drawing.Point(273, 43);
            this.peLogo.Name = "peLogo";
            this.peLogo.Properties.AllowFocused = false;
            this.peLogo.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.peLogo.Properties.Appearance.Options.UseBackColor = true;
            this.peLogo.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.peLogo.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.peLogo.Properties.ShowMenu = false;
            this.peLogo.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.peLogo.Size = new System.Drawing.Size(178, 88);
            this.peLogo.TabIndex = 0;
            // 
            // labelCopyright
            // 
            this.labelCopyright.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelCopyright.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(159)))), ((int)(((byte)(178)))), ((int)(((byte)(214)))));
            this.labelCopyright.Appearance.Options.UseFont = true;
            this.labelCopyright.Appearance.Options.UseForeColor = true;
            this.labelCopyright.Appearance.Options.UseTextOptions = true;
            this.labelCopyright.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelCopyright.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelCopyright.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.labelCopyright.Location = new System.Drawing.Point(116, 376);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(492, 39);
            this.labelCopyright.TabIndex = 5;
            this.labelCopyright.Text = "Copyright";
            // 
            // fadeTimer
            // 
            this.fadeTimer.Interval = 20;
            this.fadeTimer.Tick += new System.EventHandler(this.fadeTimer_Tick);
            // 
            // pulseTimer
            // 
            this.pulseTimer.Interval = 52;
            this.pulseTimer.Tick += new System.EventHandler(this.pulseTimer_Tick);
            // 
            // SplashScreen_Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(7)))), ((int)(((byte)(14)))), ((int)(((byte)(25)))));
            this.ClientSize = new System.Drawing.Size(1120, 648);
            this.Controls.Add(this.cardPanel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "SplashScreen_Start";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Text = "SplashScreen_Start";
            this.Load += new System.EventHandler(this.SplashScreen_Start_Load);
            this.Resize += new System.EventHandler(this.SplashScreen_Start_Resize);
            this.Shown += new System.EventHandler(this.SplashScreen_Start_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.cardPanel)).EndInit();
            this.cardPanel.ResumeLayout(false);
            this.cardPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.peLogo.Properties)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.PanelControl cardPanel;
        private DevExpress.XtraEditors.LabelControl labelEnvironment;
        private DevExpress.XtraEditors.LabelControl labelVersion;
        private DevExpress.XtraEditors.MarqueeProgressBarControl progressBarControl;
        private DevExpress.XtraEditors.LabelControl labelCopyright;
        private DevExpress.XtraEditors.LabelControl labelStatus;
        private DevExpress.XtraEditors.LabelControl labelTagline;
        private DevExpress.XtraEditors.LabelControl labelAppName;
        private DevExpress.XtraEditors.PictureEdit peLogo;
        private System.Windows.Forms.Timer fadeTimer;
        private System.Windows.Forms.Timer pulseTimer;
    }
}
