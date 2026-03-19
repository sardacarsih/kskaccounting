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
            cardPanel = new Panel();
            lblversi = new DevExpress.XtraEditors.LabelControl();
            footerLayout = new TableLayoutPanel();
            simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            Login = new DevExpress.XtraEditors.SimpleButton();
            txtpwd = new DevExpress.XtraEditors.TextEdit();
            txtuserid = new DevExpress.XtraEditors.TextEdit();
            lblPassword = new DevExpress.XtraEditors.LabelControl();
            lblUserId = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).BeginInit();
            backgroundPanel.SuspendLayout();
            cardPanel.SuspendLayout();
            footerLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtpwd.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtuserid.Properties).BeginInit();
            SuspendLayout();
            // 
            // backgroundPanel
            // 
            backgroundPanel.BackColor = Color.FromArgb(13, 30, 52);
            backgroundPanel.BackgroundImage = Properties.Resources.gl;
            backgroundPanel.BackgroundImageLayout = ImageLayout.Zoom;
            backgroundPanel.Controls.Add(cardPanel);
            backgroundPanel.Dock = DockStyle.Fill;
            backgroundPanel.Location = new Point(0, 0);
            backgroundPanel.Name = "backgroundPanel";
            backgroundPanel.Padding = new Padding(24);
            backgroundPanel.Size = new Size(1280, 720);
            backgroundPanel.TabIndex = 0;
            // 
            // cardPanel
            // 
            cardPanel.BackColor = Color.FromArgb(245, 249, 255);
            cardPanel.BorderStyle = BorderStyle.FixedSingle;
            cardPanel.Controls.Add(lblversi);
            cardPanel.Controls.Add(footerLayout);
            cardPanel.Controls.Add(txtpwd);
            cardPanel.Controls.Add(txtuserid);
            cardPanel.Controls.Add(lblPassword);
            cardPanel.Controls.Add(lblUserId);
            cardPanel.Controls.Add(lblSubtitle);
            cardPanel.Controls.Add(lblTitle);
            cardPanel.Location = new Point(380, 171);
            cardPanel.Name = "cardPanel";
            cardPanel.Padding = new Padding(30, 28, 30, 22);
            cardPanel.Size = new Size(520, 378);
            cardPanel.TabIndex = 0;
            // 
            // lblversi
            // 
            lblversi.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblversi.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            lblversi.Appearance.ForeColor = Color.FromArgb(74, 85, 104);
            lblversi.Appearance.Options.UseFont = true;
            lblversi.Appearance.Options.UseForeColor = true;
            lblversi.Location = new Point(30, 340);
            lblversi.Name = "lblversi";
            lblversi.Size = new Size(42, 15);
            lblversi.TabIndex = 7;
            lblversi.TabStop = false;
            lblversi.Text = "Version";
            // 
            // footerLayout
            // 
            footerLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            footerLayout.ColumnCount = 2;
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            footerLayout.Controls.Add(simpleButton1, 0, 0);
            footerLayout.Controls.Add(Login, 1, 0);
            footerLayout.Location = new Point(30, 273);
            footerLayout.Name = "footerLayout";
            footerLayout.RowCount = 1;
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            footerLayout.Size = new Size(458, 44);
            footerLayout.TabIndex = 2;
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
            simpleButton1.Size = new Size(223, 38);
            simpleButton1.TabIndex = 1;
            simpleButton1.Text = "Cancel";
            simpleButton1.ToolTip = "Cancel";
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
            Login.Location = new Point(232, 3);
            Login.Name = "Login";
            Login.Size = new Size(223, 38);
            Login.TabIndex = 0;
            Login.Text = "Sign In";
            Login.ToolTip = "Login";
            Login.Click += Login_Click;
            // 
            // txtpwd
            // 
            txtpwd.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtpwd.Location = new Point(30, 225);
            txtpwd.Name = "txtpwd";
            txtpwd.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtpwd.Properties.Appearance.Options.UseFont = true;
            txtpwd.Properties.Name = "textEdit1";
            txtpwd.Properties.UseSystemPasswordChar = true;
            txtpwd.Size = new Size(458, 28);
            txtpwd.TabIndex = 1;
            txtpwd.ToolTip = "Password";
            txtpwd.KeyDown += txtpwd_KeyDown;
            // 
            // txtuserid
            // 
            txtuserid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtuserid.Location = new Point(30, 169);
            txtuserid.Name = "txtuserid";
            txtuserid.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtuserid.Properties.Appearance.Options.UseFont = true;
            txtuserid.Properties.CharacterCasing = CharacterCasing.Lower;
            txtuserid.Properties.Name = "textEdit1";
            txtuserid.Size = new Size(458, 28);
            txtuserid.TabIndex = 0;
            txtuserid.ToolTip = "UserID";
            txtuserid.KeyDown += txtuserid_KeyDown;
            // 
            // lblPassword
            // 
            lblPassword.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblPassword.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            lblPassword.Appearance.Options.UseFont = true;
            lblPassword.Appearance.Options.UseForeColor = true;
            lblPassword.Location = new Point(30, 145);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(62, 17);
            lblPassword.TabIndex = 5;
            lblPassword.TabStop = false;
            lblPassword.Text = "Password";
            // 
            // lblUserId
            // 
            lblUserId.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            lblUserId.Appearance.ForeColor = Color.FromArgb(30, 41, 59);
            lblUserId.Appearance.Options.UseFont = true;
            lblUserId.Appearance.Options.UseForeColor = true;
            lblUserId.Location = new Point(30, 114);
            lblUserId.Name = "lblUserId";
            lblUserId.Size = new Size(47, 17);
            lblUserId.TabIndex = 4;
            lblUserId.TabStop = false;
            lblUserId.Text = "User ID";
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(71, 85, 105);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(30, 72);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(229, 17);
            lblSubtitle.TabIndex = 6;
            lblSubtitle.TabStop = false;
            lblSubtitle.Text = "Use your account credentials to continue.";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Appearance.Options.UseForeColor = true;
            lblTitle.Location = new Point(30, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(183, 32);
            lblTitle.TabIndex = 3;
            lblTitle.TabStop = false;
            lblTitle.Text = "Welcome Back";
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
            Text = "Login";
            Load += Frmlogin_Load;
            Resize += Frmlogin_Resize;
            ((System.ComponentModel.ISupportInitialize)behaviorManager1).EndInit();
            backgroundPanel.ResumeLayout(false);
            cardPanel.ResumeLayout(false);
            cardPanel.PerformLayout();
            footerLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)txtpwd.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtuserid.Properties).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel backgroundPanel;
        private Panel cardPanel;
        private TableLayoutPanel footerLayout;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.LabelControl lblUserId;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.SimpleButton Login;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit txtuserid;
        private DevExpress.XtraEditors.TextEdit txtpwd;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.LabelControl lblversi;
    }
}
