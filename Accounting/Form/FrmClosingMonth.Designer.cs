
namespace Accounting.Form
{
    partial class FrmClosingMonth
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
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.lbldata = new DevExpress.XtraEditors.LabelControl();
            this.lblwilayah = new DevExpress.XtraEditors.LabelControl();
            this.lblnamapt = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.checkEditjurnalclosing = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditjurnalclosing.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(20, 173);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(45, 17);
            this.labelControl3.TabIndex = 9;
            this.labelControl3.Text = "Periode";
            // 
            // labelControl1
            // 
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.labelControl1.Location = new System.Drawing.Point(21, 8);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(461, 13);
            this.labelControl1.TabIndex = 9;
            this.labelControl1.Text = "Proses ini diperlukan untuk menghitung ulang Saldo Akhir dan membuat jurnal closi" +
    "ng otomatis";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(214, 237);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(2);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 12;
            this.simpleButton1.Text = "Proses";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(308, 170);
            this.setahun.Margin = new System.Windows.Forms.Padding(2);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(85, 20);
            this.setahun.TabIndex = 11;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(150, 170);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(2);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(139, 20);
            this.cmbbulan.TabIndex = 10;
            this.cmbbulan.SelectedIndexChanged += new System.EventHandler(this.cmbbulan_SelectedIndexChanged);
            // 
            // labelControl4
            // 
            this.labelControl4.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl4.Appearance.Options.UseFont = true;
            this.labelControl4.Location = new System.Drawing.Point(20, 99);
            this.labelControl4.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(67, 17);
            this.labelControl4.TabIndex = 9;
            this.labelControl4.Text = "Lokasi Data";
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl5.Appearance.Options.UseFont = true;
            this.labelControl5.Location = new System.Drawing.Point(20, 136);
            this.labelControl5.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(45, 17);
            this.labelControl5.TabIndex = 9;
            this.labelControl5.Text = "Wilayah";
            // 
            // lbldata
            // 
            this.lbldata.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbldata.Appearance.Options.UseFont = true;
            this.lbldata.Location = new System.Drawing.Point(150, 96);
            this.lbldata.Margin = new System.Windows.Forms.Padding(2);
            this.lbldata.Name = "lbldata";
            this.lbldata.Size = new System.Drawing.Size(139, 21);
            this.lbldata.TabIndex = 9;
            this.lbldata.Text = "Nama Perusahaan";
            // 
            // lblwilayah
            // 
            this.lblwilayah.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblwilayah.Appearance.Options.UseFont = true;
            this.lblwilayah.Location = new System.Drawing.Point(150, 133);
            this.lblwilayah.Margin = new System.Windows.Forms.Padding(2);
            this.lblwilayah.Name = "lblwilayah";
            this.lblwilayah.Size = new System.Drawing.Size(139, 21);
            this.lblwilayah.TabIndex = 9;
            this.lblwilayah.Text = "Nama Perusahaan";
            // 
            // lblnamapt
            // 
            this.lblnamapt.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblnamapt.Appearance.Options.UseFont = true;
            this.lblnamapt.Location = new System.Drawing.Point(150, 59);
            this.lblnamapt.Margin = new System.Windows.Forms.Padding(2);
            this.lblnamapt.Name = "lblnamapt";
            this.lblnamapt.Size = new System.Drawing.Size(139, 21);
            this.lblnamapt.TabIndex = 13;
            this.lblnamapt.Text = "Nama Perusahaan";
            // 
            // labelControl7
            // 
            this.labelControl7.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl7.Appearance.Options.UseFont = true;
            this.labelControl7.Location = new System.Drawing.Point(20, 62);
            this.labelControl7.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(106, 17);
            this.labelControl7.TabIndex = 14;
            this.labelControl7.Text = "Nama Perusahaan";
            // 
            // checkEditjurnalclosing
            // 
            this.checkEditjurnalclosing.Location = new System.Drawing.Point(152, 204);
            this.checkEditjurnalclosing.Name = "checkEditjurnalclosing";
            this.checkEditjurnalclosing.Properties.Caption = "Buat Jurnal Closing Otomatis";
            this.checkEditjurnalclosing.Size = new System.Drawing.Size(241, 20);
            this.checkEditjurnalclosing.TabIndex = 15;
            // 
            // FrmClosingMonth
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(514, 295);
            this.Controls.Add(this.checkEditjurnalclosing);
            this.Controls.Add(this.lblnamapt);
            this.Controls.Add(this.labelControl7);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.cmbbulan);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.lblwilayah);
            this.Controls.Add(this.lbldata);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.labelControl1);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmClosingMonth";
            this.Text = "Tutup Buku Bulanan";
            this.Load += new System.EventHandler(this.FrmClosingMonth_Load);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditjurnalclosing.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl lbldata;
        private DevExpress.XtraEditors.LabelControl lblwilayah;
        private DevExpress.XtraEditors.LabelControl lblnamapt;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.CheckEdit checkEditjurnalclosing;
    }
}