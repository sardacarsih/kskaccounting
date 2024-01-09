
namespace Accounting
{
    partial class FrmChangePass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmChangePass));
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.lbluserid = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.txtnama = new DevExpress.XtraEditors.TextEdit();
            this.txtdept = new DevExpress.XtraEditors.TextEdit();
            this.txtjab = new DevExpress.XtraEditors.TextEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.txtoldpass = new DevExpress.XtraEditors.TextEdit();
            this.txtnewpass = new DevExpress.XtraEditors.TextEdit();
            this.txtpassconf = new DevExpress.XtraEditors.TextEdit();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.lblrole = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.txtnama.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdept.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtjab.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtoldpass.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnewpass.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtpassconf.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(19, 173);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(110, 19);
            this.labelControl3.TabIndex = 9;
            this.labelControl3.Text = "Password Lama";
            // 
            // simpleButton1
            // 
            this.simpleButton1.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton1.ImageOptions.Image")));
            this.simpleButton1.Location = new System.Drawing.Point(176, 265);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(196, 40);
            this.simpleButton1.TabIndex = 6;
            this.simpleButton1.Text = "Update";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(19, 80);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(41, 19);
            this.labelControl2.TabIndex = 9;
            this.labelControl2.Text = "Nama";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(19, 108);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(86, 19);
            this.labelControl4.TabIndex = 9;
            this.labelControl4.Text = "Departemen";
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(19, 136);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(54, 19);
            this.labelControl5.TabIndex = 9;
            this.labelControl5.Text = "Jabatan";
            // 
            // lbluserid
            // 
            this.lbluserid.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbluserid.Appearance.Options.UseFont = true;
            this.lbluserid.Location = new System.Drawing.Point(176, 3);
            this.lbluserid.Name = "lbluserid";
            this.lbluserid.Size = new System.Drawing.Size(56, 19);
            this.lbluserid.TabIndex = 9;
            this.lbluserid.Text = "UserID";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(19, 3);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(54, 19);
            this.labelControl1.TabIndex = 9;
            this.labelControl1.Text = "User ID";
            // 
            // txtnama
            // 
            this.txtnama.Location = new System.Drawing.Point(176, 77);
            this.txtnama.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtnama.Name = "txtnama";
            this.txtnama.Size = new System.Drawing.Size(196, 26);
            this.txtnama.TabIndex = 0;
            this.txtnama.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnama_KeyDown);
            // 
            // txtdept
            // 
            this.txtdept.Location = new System.Drawing.Point(176, 105);
            this.txtdept.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtdept.Name = "txtdept";
            this.txtdept.Size = new System.Drawing.Size(196, 26);
            this.txtdept.TabIndex = 1;
            this.txtdept.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtdept_KeyDown);
            // 
            // txtjab
            // 
            this.txtjab.Location = new System.Drawing.Point(176, 133);
            this.txtjab.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtjab.Name = "txtjab";
            this.txtjab.Size = new System.Drawing.Size(196, 26);
            this.txtjab.TabIndex = 2;
            this.txtjab.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtjab_KeyDown);
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(19, 200);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(104, 19);
            this.labelControl6.TabIndex = 9;
            this.labelControl6.Text = "Password Baru";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(19, 227);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(147, 19);
            this.labelControl7.TabIndex = 9;
            this.labelControl7.Text = "Konfirmasi Password";
            // 
            // txtoldpass
            // 
            this.txtoldpass.Location = new System.Drawing.Point(176, 170);
            this.txtoldpass.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtoldpass.Name = "txtoldpass";
            this.txtoldpass.Properties.PasswordChar = '*';
            this.txtoldpass.Size = new System.Drawing.Size(196, 26);
            this.txtoldpass.TabIndex = 3;
            this.txtoldpass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtoldpass_KeyDown);
            // 
            // txtnewpass
            // 
            this.txtnewpass.Location = new System.Drawing.Point(176, 197);
            this.txtnewpass.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtnewpass.Name = "txtnewpass";
            this.txtnewpass.Properties.PasswordChar = '*';
            this.txtnewpass.Size = new System.Drawing.Size(196, 26);
            this.txtnewpass.TabIndex = 4;
            this.txtnewpass.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnewpass_KeyDown);
            // 
            // txtpassconf
            // 
            this.txtpassconf.Location = new System.Drawing.Point(176, 224);
            this.txtpassconf.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtpassconf.Name = "txtpassconf";
            this.txtpassconf.Properties.PasswordChar = '*';
            this.txtpassconf.Size = new System.Drawing.Size(196, 26);
            this.txtpassconf.TabIndex = 5;
            this.txtpassconf.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtpassconf_KeyDown);
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(19, 28);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(31, 19);
            this.labelControl8.TabIndex = 9;
            this.labelControl8.Text = "Role";
            // 
            // lblrole
            // 
            this.lblrole.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblrole.Appearance.Options.UseFont = true;
            this.lblrole.Location = new System.Drawing.Point(176, 28);
            this.lblrole.Name = "lblrole";
            this.lblrole.Size = new System.Drawing.Size(74, 19);
            this.lblrole.TabIndex = 9;
            this.lblrole.Text = "RoleUser";
            // 
            // FrmChangePass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 312);
            this.Controls.Add(this.txtpassconf);
            this.Controls.Add(this.txtnewpass);
            this.Controls.Add(this.txtoldpass);
            this.Controls.Add(this.txtjab);
            this.Controls.Add(this.txtdept);
            this.Controls.Add(this.txtnama);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.lblrole);
            this.Controls.Add(this.lbluserid);
            this.Controls.Add(this.labelControl8);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl7);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.labelControl3);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmChangePass";
            this.Text = "Ganti Password";
            this.Load += new System.EventHandler(this.FrmChangePass_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtnama.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdept.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtjab.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtoldpass.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnewpass.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtpassconf.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl lbluserid;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit txtnama;
        private DevExpress.XtraEditors.TextEdit txtdept;
        private DevExpress.XtraEditors.TextEdit txtjab;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.TextEdit txtoldpass;
        private DevExpress.XtraEditors.TextEdit txtnewpass;
        private DevExpress.XtraEditors.TextEdit txtpassconf;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.LabelControl lblrole;
    }
}