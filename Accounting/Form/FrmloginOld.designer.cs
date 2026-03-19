namespace Accounting.Form
{
    partial class FrmloginOld
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
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            this.txtpwd = new DevExpress.XtraEditors.TextEdit();
            this.txtuserid = new DevExpress.XtraEditors.TextEdit();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.Login = new DevExpress.XtraEditors.SimpleButton();
            this.lblversi = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtpwd.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtuserid.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // txtpwd
            // 
            this.txtpwd.Location = new System.Drawing.Point(176, 155);
            this.txtpwd.Margin = new System.Windows.Forms.Padding(2);
            this.txtpwd.Name = "txtpwd";
            this.txtpwd.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtpwd.Properties.Appearance.Options.UseFont = true;
            this.txtpwd.Properties.Name = "textEdit1";
            this.txtpwd.Properties.UseSystemPasswordChar = true;
            this.txtpwd.Size = new System.Drawing.Size(92, 28);
            this.txtpwd.TabIndex = 1;
            this.txtpwd.ToolTip = "Password";
            this.txtpwd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtpwd_KeyDown);
            // 
            // txtuserid
            // 
            this.txtuserid.Location = new System.Drawing.Point(73, 155);
            this.txtuserid.Margin = new System.Windows.Forms.Padding(2);
            this.txtuserid.Name = "txtuserid";
            this.txtuserid.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtuserid.Properties.Appearance.Options.UseFont = true;
            this.txtuserid.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            this.txtuserid.Properties.Name = "textEdit1";
            this.txtuserid.Size = new System.Drawing.Size(92, 28);
            this.txtuserid.TabIndex = 0;
            this.txtuserid.ToolTip = "UserID";
            this.txtuserid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtuserid_KeyDown);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(20, 151);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.simpleButton1.Size = new System.Drawing.Size(33, 27);
            this.simpleButton1.TabIndex = 3;
            this.simpleButton1.ToolTip = "Cancel";
            this.simpleButton1.Click += new System.EventHandler(this.SimpleButton1_Click);
            // 
            // Login
            // 
            this.Login.Location = new System.Drawing.Point(281, 151);
            this.Login.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Login.Name = "Login";
            this.Login.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.Login.Size = new System.Drawing.Size(36, 27);
            this.Login.TabIndex = 2;
            this.Login.ToolTip = "Login";
            this.Login.Click += new System.EventHandler(this.Login_Click);
            // 
            // lblversi
            // 
            this.lblversi.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblversi.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblversi.Appearance.Options.UseFont = true;
            this.lblversi.Appearance.Options.UseForeColor = true;
            this.lblversi.Location = new System.Drawing.Point(237, 10);
            this.lblversi.Name = "lblversi";
            this.lblversi.Size = new System.Drawing.Size(42, 15);
            this.lblversi.TabIndex = 4;
            this.lblversi.Text = "Version";
            // 
            // Frmlogin
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImageLayoutStore = System.Windows.Forms.ImageLayout.Stretch;
            this.BackgroundImageStore = global::Accounting.Properties.Resources.gl2021;
            this.ClientSize = new System.Drawing.Size(339, 191);
            this.Controls.Add(this.lblversi);
            this.Controls.Add(this.txtpwd);
            this.Controls.Add(this.txtuserid);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.Login);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(10, 50);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Frmlogin";
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Frmlogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtpwd.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtuserid.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.SimpleButton Login;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit txtuserid;
        private DevExpress.XtraEditors.TextEdit txtpwd;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.LabelControl lblversi;
    }
}