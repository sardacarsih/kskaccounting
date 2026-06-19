using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    partial class Frmlogin
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
            components = new System.ComponentModel.Container();
            behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(components);
            backgroundPanel = new Panel();
            shellPanel = new Panel();
            mainLayout = new TableLayoutPanel();
            brandPanel = new Panel();
            lblBrandDesc = new DevExpress.XtraEditors.LabelControl();
            lblBrandTagline = new DevExpress.XtraEditors.LabelControl();
            lblBrandTitle = new DevExpress.XtraEditors.LabelControl();
            logoPanel = new Panel();
            lblLogo = new DevExpress.XtraEditors.LabelControl();
            formHostPanel = new Panel();
            cardPanel = new Panel();
            lblSupport = new DevExpress.XtraEditors.LabelControl();
            lblversi = new DevExpress.XtraEditors.LabelControl();
            optionsLayout = new TableLayoutPanel();
            linkForgotPassword = new LinkLabel();
            chkRememberMe = new CheckBox();
            passwordLayout = new TableLayoutPanel();
            btnTogglePassword = new DevExpress.XtraEditors.SimpleButton();
            txtpwd = new DevExpress.XtraEditors.TextEdit();
            footerLayout = new TableLayoutPanel();
            simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            Login = new DevExpress.XtraEditors.SimpleButton();
            txtuserid = new DevExpress.XtraEditors.TextEdit();
            lblError = new DevExpress.XtraEditors.LabelControl();
            lblPassword = new DevExpress.XtraEditors.LabelControl();
            lblUserId = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).BeginInit();
            backgroundPanel.SuspendLayout();
            shellPanel.SuspendLayout();
            mainLayout.SuspendLayout();
            brandPanel.SuspendLayout();
            logoPanel.SuspendLayout();
            formHostPanel.SuspendLayout();
            cardPanel.SuspendLayout();
            optionsLayout.SuspendLayout();
            passwordLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtpwd.Properties).BeginInit();
            footerLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtuserid.Properties).BeginInit();
            SuspendLayout();
            // 
            // backgroundPanel
            // 
            backgroundPanel.BackColor = Color.FromArgb(16, 29, 48);
            backgroundPanel.Controls.Add(shellPanel);
            backgroundPanel.Dock = DockStyle.Fill;
            backgroundPanel.Location = new Point(0, 0);
            backgroundPanel.Name = "backgroundPanel";
            backgroundPanel.Padding = new Padding(32, 24, 32, 24);
            backgroundPanel.Size = new Size(1280, 720);
            backgroundPanel.TabIndex = 0;
            // 
            // shellPanel
            // 
            shellPanel.Controls.Add(mainLayout);
            shellPanel.Location = new Point(80, 48);
            shellPanel.Name = "shellPanel";
            shellPanel.Size = new Size(1120, 624);
            shellPanel.TabIndex = 0;
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            mainLayout.Controls.Add(brandPanel, 0, 0);
            mainLayout.Controls.Add(formHostPanel, 1, 0);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.Size = new Size(1120, 624);
            mainLayout.TabIndex = 0;
            // 
            // brandPanel
            // 
            brandPanel.BackColor = Color.FromArgb(23, 40, 66);
            brandPanel.Controls.Add(lblBrandDesc);
            brandPanel.Controls.Add(lblBrandTagline);
            brandPanel.Controls.Add(lblBrandTitle);
            brandPanel.Controls.Add(logoPanel);
            brandPanel.Dock = DockStyle.Fill;
            brandPanel.Location = new Point(0, 0);
            brandPanel.Margin = new Padding(0, 0, 14, 0);
            brandPanel.Name = "brandPanel";
            brandPanel.Padding = new Padding(44, 46, 44, 46);
            brandPanel.Size = new Size(501, 624);
            brandPanel.TabIndex = 0;
            // 
            // lblBrandDesc
            // 
            lblBrandDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblBrandDesc.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblBrandDesc.Appearance.ForeColor = Color.FromArgb(191, 211, 233);
            lblBrandDesc.Appearance.Options.UseFont = true;
            lblBrandDesc.Appearance.Options.UseForeColor = true;
            lblBrandDesc.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            lblBrandDesc.Location = new Point(44, 236);
            lblBrandDesc.Name = "lblBrandDesc";
            lblBrandDesc.Size = new Size(413, 57);
            lblBrandDesc.TabIndex = 3;
            lblBrandDesc.TabStop = false;
            lblBrandDesc.Text = "Akses operasional akuntansi, kontrol periode, dan alur pelaporan dengan autentikasi enterprise yang aman.";
            // 
            // lblBrandTagline
            // 
            lblBrandTagline.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblBrandTagline.Appearance.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblBrandTagline.Appearance.ForeColor = Color.FromArgb(213, 227, 243);
            lblBrandTagline.Appearance.Options.UseFont = true;
            lblBrandTagline.Appearance.Options.UseForeColor = true;
            lblBrandTagline.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            lblBrandTagline.Location = new Point(44, 187);
            lblBrandTagline.Name = "lblBrandTagline";
            lblBrandTagline.Size = new Size(413, 42);
            lblBrandTagline.TabIndex = 2;
            lblBrandTagline.TabStop = false;
            lblBrandTagline.Text = "Platform ERP terintegrasi untuk perkebunan, manufaktur, dan akuntansi multi-lokasi.";
            // 
            // lblBrandTitle
            // 
            lblBrandTitle.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Point);
            lblBrandTitle.Appearance.ForeColor = Color.White;
            lblBrandTitle.Appearance.Options.UseFont = true;
            lblBrandTitle.Appearance.Options.UseForeColor = true;
            lblBrandTitle.Location = new Point(44, 128);
            lblBrandTitle.Name = "lblBrandTitle";
            lblBrandTitle.Size = new Size(190, 50);
            lblBrandTitle.TabIndex = 1;
            lblBrandTitle.TabStop = false;
            lblBrandTitle.Text = "GL System";
            // 
            // logoPanel
            // 
            logoPanel.BackColor = Color.FromArgb(35, 58, 88);
            logoPanel.Controls.Add(lblLogo);
            logoPanel.Location = new Point(44, 46);
            logoPanel.Name = "logoPanel";
            logoPanel.Size = new Size(84, 64);
            logoPanel.TabIndex = 0;
            // 
            // lblLogo
            // 
            lblLogo.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
            lblLogo.Appearance.ForeColor = Color.White;
            lblLogo.Appearance.Options.UseFont = true;
            lblLogo.Appearance.Options.UseForeColor = true;
            lblLogo.Appearance.Options.UseTextOptions = true;
            lblLogo.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblLogo.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblLogo.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblLogo.Dock = DockStyle.Fill;
            lblLogo.Location = new Point(0, 0);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(84, 64);
            lblLogo.TabIndex = 0;
            lblLogo.TabStop = false;
            lblLogo.Text = "GL";
            // 
            // formHostPanel
            // 
            formHostPanel.BackColor = Color.Transparent;
            formHostPanel.Controls.Add(cardPanel);
            formHostPanel.Dock = DockStyle.Fill;
            formHostPanel.Location = new Point(529, 0);
            formHostPanel.Margin = new Padding(14, 0, 0, 0);
            formHostPanel.Name = "formHostPanel";
            formHostPanel.Size = new Size(591, 624);
            formHostPanel.TabIndex = 1;
            // 
            // cardPanel
            // 
            cardPanel.BackColor = Color.FromArgb(245, 249, 255);
            cardPanel.Controls.Add(lblSupport);
            cardPanel.Controls.Add(lblversi);
            cardPanel.Controls.Add(optionsLayout);
            cardPanel.Controls.Add(passwordLayout);
            cardPanel.Controls.Add(footerLayout);
            cardPanel.Controls.Add(txtuserid);
            cardPanel.Controls.Add(lblError);
            cardPanel.Controls.Add(lblPassword);
            cardPanel.Controls.Add(lblUserId);
            cardPanel.Controls.Add(lblSubtitle);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Location = new Point(42, 78);
            cardPanel.Name = "cardPanel";
            cardPanel.Padding = new Padding(28, 24, 28, 20);
            cardPanel.Size = new Size(506, 468);
            cardPanel.TabIndex = 0;
            // 
            // lblSupport
            // 
            lblSupport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblSupport.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblSupport.Appearance.ForeColor = Color.FromArgb(91, 103, 119);
            lblSupport.Appearance.Options.UseFont = true;
            lblSupport.Appearance.Options.UseForeColor = true;
            lblSupport.Location = new Point(345, 435);
            lblSupport.Name = "lblSupport";
            lblSupport.Size = new Size(125, 15);
            lblSupport.TabIndex = 10;
            lblSupport.TabStop = false;
            lblSupport.Text = "support@glsystem.local";
            // 
            // lblversi
            // 
            lblversi.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblversi.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblversi.Appearance.ForeColor = Color.FromArgb(91, 103, 119);
            lblversi.Appearance.Options.UseFont = true;
            lblversi.Appearance.Options.UseForeColor = true;
            lblversi.Location = new Point(34, 435);
            lblversi.Name = "lblversi";
            lblversi.Size = new Size(42, 15);
            lblversi.TabIndex = 9;
            lblversi.TabStop = false;
            lblversi.Text = "Versi";
            // 
            // optionsLayout
            // 
            optionsLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optionsLayout.ColumnCount = 2;
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionsLayout.Controls.Add(linkForgotPassword, 1, 0);
            optionsLayout.Controls.Add(chkRememberMe, 0, 0);
            optionsLayout.Location = new Point(34, 297);
            optionsLayout.Name = "optionsLayout";
            optionsLayout.RowCount = 1;
            optionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            optionsLayout.Size = new Size(438, 28);
            optionsLayout.TabIndex = 5;
            // 
            // linkForgotPassword
            // 
            linkForgotPassword.ActiveLinkColor = Color.FromArgb(30, 64, 175);
            linkForgotPassword.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            linkForgotPassword.AutoSize = true;
            linkForgotPassword.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            linkForgotPassword.LinkBehavior = LinkBehavior.HoverUnderline;
            linkForgotPassword.LinkColor = Color.FromArgb(30, 64, 175);
            linkForgotPassword.Location = new Point(322, 4);
            linkForgotPassword.Margin = new Padding(3, 4, 3, 0);
            linkForgotPassword.Name = "linkForgotPassword";
            linkForgotPassword.Size = new Size(113, 17);
            linkForgotPassword.TabIndex = 1;
            linkForgotPassword.TabStop = true;
            linkForgotPassword.Text = "Lupa kata sandi?";
            linkForgotPassword.VisitedLinkColor = Color.FromArgb(30, 64, 175);
            linkForgotPassword.LinkClicked += linkForgotPassword_LinkClicked;
            // 
            // chkRememberMe
            // 
            chkRememberMe.AutoSize = true;
            chkRememberMe.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            chkRememberMe.ForeColor = Color.FromArgb(45, 55, 72);
            chkRememberMe.Location = new Point(3, 3);
            chkRememberMe.Name = "chkRememberMe";
            chkRememberMe.Size = new Size(106, 21);
            chkRememberMe.TabIndex = 0;
            chkRememberMe.Text = "Ingat saya";
            chkRememberMe.UseVisualStyleBackColor = true;
            // 
            // passwordLayout
            // 
            passwordLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            passwordLayout.ColumnCount = 2;
            passwordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            passwordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            passwordLayout.Controls.Add(btnTogglePassword, 1, 0);
            passwordLayout.Controls.Add(txtpwd, 0, 0);
            passwordLayout.Location = new Point(34, 255);
            passwordLayout.Name = "passwordLayout";
            passwordLayout.RowCount = 1;
            passwordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            passwordLayout.Size = new Size(438, 36);
            passwordLayout.TabIndex = 4;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.Appearance.BackColor = Color.FromArgb(226, 232, 240);
            btnTogglePassword.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            btnTogglePassword.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            btnTogglePassword.Appearance.Options.UseBackColor = true;
            btnTogglePassword.Appearance.Options.UseFont = true;
            btnTogglePassword.Appearance.Options.UseForeColor = true;
            btnTogglePassword.Dock = DockStyle.Fill;
            btnTogglePassword.Location = new Point(353, 3);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(82, 30);
            btnTogglePassword.TabIndex = 1;
            btnTogglePassword.Text = "Lihat";
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // txtpwd
            // 
            txtpwd.Dock = DockStyle.Fill;
            txtpwd.Location = new Point(3, 3);
            txtpwd.Name = "txtpwd";
            txtpwd.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtpwd.Properties.Appearance.Options.UseFont = true;
            txtpwd.Properties.AutoHeight = false;
            txtpwd.Properties.Name = "textEdit1";
            txtpwd.Properties.UseSystemPasswordChar = true;
            txtpwd.Size = new Size(344, 30);
            txtpwd.TabIndex = 0;
            txtpwd.ToolTip = "Kata sandi";
            txtpwd.KeyDown += txtpwd_KeyDown;
            // 
            // footerLayout
            // 
            footerLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            footerLayout.ColumnCount = 2;
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footerLayout.Controls.Add(simpleButton1, 0, 0);
            footerLayout.Controls.Add(Login, 1, 0);
            footerLayout.Location = new Point(34, 339);
            footerLayout.Name = "footerLayout";
            footerLayout.RowCount = 1;
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            footerLayout.Size = new Size(438, 46);
            footerLayout.TabIndex = 6;
            // 
            // simpleButton1
            // 
            simpleButton1.Appearance.BackColor = Color.FromArgb(226, 232, 240);
            simpleButton1.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            simpleButton1.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            simpleButton1.Appearance.Options.UseBackColor = true;
            simpleButton1.Appearance.Options.UseFont = true;
            simpleButton1.Appearance.Options.UseForeColor = true;
            simpleButton1.Dock = DockStyle.Fill;
            simpleButton1.Location = new Point(3, 3);
            simpleButton1.Name = "simpleButton1";
            simpleButton1.Size = new Size(213, 40);
            simpleButton1.TabIndex = 0;
            simpleButton1.Text = "Batal";
            simpleButton1.ToolTip = "Batal";
            simpleButton1.Click += SimpleButton1_Click;
            // 
            // Login
            // 
            Login.Appearance.BackColor = Color.FromArgb(30, 64, 175);
            Login.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            Login.Appearance.ForeColor = Color.White;
            Login.Appearance.Options.UseBackColor = true;
            Login.Appearance.Options.UseFont = true;
            Login.Appearance.Options.UseForeColor = true;
            Login.Dock = DockStyle.Fill;
            Login.Location = new Point(222, 3);
            Login.Name = "Login";
            Login.Size = new Size(213, 40);
            Login.TabIndex = 1;
            Login.Text = "Masuk";
            Login.ToolTip = "Masuk";
            Login.Click += Login_Click;
            // 
            // txtuserid
            // 
            txtuserid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtuserid.Location = new Point(34, 206);
            txtuserid.Name = "txtuserid";
            txtuserid.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtuserid.Properties.Appearance.Options.UseFont = true;
            txtuserid.Properties.AutoHeight = false;
            txtuserid.Properties.CharacterCasing = CharacterCasing.Lower;
            txtuserid.Properties.Name = "textEdit1";
            txtuserid.Size = new Size(438, 32);
            txtuserid.TabIndex = 3;
            txtuserid.ToolTip = "User ID";
            txtuserid.KeyDown += txtuserid_KeyDown;
            // 
            // lblError
            // 
            lblError.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            lblError.Appearance.ForeColor = Color.FromArgb(185, 28, 28);
            lblError.Appearance.Options.UseFont = true;
            lblError.Appearance.Options.UseForeColor = true;
            lblError.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            lblError.Location = new Point(34, 148);
            lblError.Name = "lblError";
            lblError.Size = new Size(438, 17);
            lblError.TabIndex = 8;
            lblError.TabStop = false;
            lblError.Text = " ";
            lblError.Visible = false;
            // 
            // lblPassword
            // 
            lblPassword.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblPassword.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            lblPassword.Appearance.Options.UseFont = true;
            lblPassword.Appearance.Options.UseForeColor = true;
            lblPassword.Location = new Point(34, 234);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(62, 17);
            lblPassword.TabIndex = 7;
            lblPassword.TabStop = false;
            lblPassword.Text = "Kata Sandi";
            // 
            // lblUserId
            // 
            lblUserId.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblUserId.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            lblUserId.Appearance.Options.UseFont = true;
            lblUserId.Appearance.Options.UseForeColor = true;
            lblUserId.Location = new Point(34, 183);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(107, 17);
            lblUserId.TabIndex = 6;
            lblUserId.TabStop = false;
            lblUserId.Text = "User ID / Email";
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(34, 102);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(365, 17);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.TabStop = false;
            lblSubtitle.Text = "Masuk menggunakan kredensial akun enterprise Anda.";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(34, 58);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(194, 32);
            lblTitle.TabIndex = 0;
            lblTitle.TabStop = false;
            lblTitle.Text = "Selamat Datang";
            // 
            // Frmlogin
            // 
            AcceptButton = Login;
            Appearance.Options.UseFont = true;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = simpleButton1;
            ClientSize = new Size(1280, 720);
            Controls.Add(backgroundPanel);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Frmlogin";
            Text = "Masuk";
            Load += Frmlogin_Load;
            Resize += Frmlogin_Resize;
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).EndInit();
            backgroundPanel.ResumeLayout(false);
            shellPanel.ResumeLayout(false);
            mainLayout.ResumeLayout(false);
            brandPanel.ResumeLayout(false);
            brandPanel.PerformLayout();
            logoPanel.ResumeLayout(false);
            formHostPanel.ResumeLayout(false);
            cardPanel.ResumeLayout(false);
            cardPanel.PerformLayout();
            optionsLayout.ResumeLayout(false);
            optionsLayout.PerformLayout();
            passwordLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtpwd.Properties).EndInit();
            footerLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtuserid.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel backgroundPanel;
        private Panel shellPanel;
        private TableLayoutPanel mainLayout;
        private Panel brandPanel;
        private Panel formHostPanel;
        private Panel cardPanel;
        private Panel logoPanel;
        private TableLayoutPanel passwordLayout;
        private TableLayoutPanel optionsLayout;
        private TableLayoutPanel footerLayout;
        private DevExpress.XtraEditors.LabelControl lblLogo;
        private DevExpress.XtraEditors.LabelControl lblBrandTitle;
        private DevExpress.XtraEditors.LabelControl lblBrandTagline;
        private DevExpress.XtraEditors.LabelControl lblBrandDesc;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.LabelControl lblUserId;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.SimpleButton Login;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.SimpleButton btnTogglePassword;
        private DevExpress.XtraEditors.TextEdit txtuserid;
        private DevExpress.XtraEditors.TextEdit txtpwd;
        private DevExpress.XtraEditors.LabelControl lblError;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.LabelControl lblversi;
        private DevExpress.XtraEditors.LabelControl lblSupport;
        private CheckBox chkRememberMe;
        private LinkLabel linkForgotPassword;
    }
}
