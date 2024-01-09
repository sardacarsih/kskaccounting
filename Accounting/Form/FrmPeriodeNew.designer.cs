namespace Accounting.Form
{
    partial class FrmPeriodeNew
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
            this.lblcompany = new System.Windows.Forms.Label();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.sbcetak = new DevExpress.XtraEditors.SimpleButton();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.lblwilayah = new DevExpress.XtraEditors.LabelControl();
            this.lbldata = new DevExpress.XtraEditors.LabelControl();
            this.lblpt = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lblcompany
            // 
            this.lblcompany.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblcompany.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblcompany.Location = new System.Drawing.Point(5, 9);
            this.lblcompany.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblcompany.Name = "lblcompany";
            this.lblcompany.Size = new System.Drawing.Size(460, 55);
            this.lblcompany.TabIndex = 2;
            this.lblcompany.Text = "Periode Akuntansi";
            this.lblcompany.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(12, 179);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(95, 19);
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "Mulai Periode";
            // 
            // sbcetak
            // 
            this.sbcetak.ImageOptions.Image = global::Accounting.Properties.Resources.savepagesetup_16x16;
            this.sbcetak.Location = new System.Drawing.Point(173, 233);
            this.sbcetak.Name = "sbcetak";
            this.sbcetak.Size = new System.Drawing.Size(112, 45);
            this.sbcetak.TabIndex = 9;
            this.sbcetak.Text = "Simpan";
            this.sbcetak.Click += new System.EventHandler(this.sbcetak_Click);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(313, 176);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(83, 26);
            this.setahun.TabIndex = 8;
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(157, 176);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(150, 26);
            this.cmbbulan.TabIndex = 7;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(12, 133);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(55, 19);
            this.labelControl5.TabIndex = 10;
            this.labelControl5.Text = "Wilayah";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(12, 109);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(81, 19);
            this.labelControl4.TabIndex = 11;
            this.labelControl4.Text = "Lokasi Data";
            // 
            // lblwilayah
            // 
            this.lblwilayah.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblwilayah.Appearance.Options.UseFont = true;
            this.lblwilayah.Location = new System.Drawing.Point(158, 133);
            this.lblwilayah.Name = "lblwilayah";
            this.lblwilayah.Size = new System.Drawing.Size(139, 21);
            this.lblwilayah.TabIndex = 12;
            this.lblwilayah.Text = "Nama Perusahaan";
            // 
            // lbldata
            // 
            this.lbldata.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbldata.Appearance.Options.UseFont = true;
            this.lbldata.Location = new System.Drawing.Point(158, 109);
            this.lbldata.Name = "lbldata";
            this.lbldata.Size = new System.Drawing.Size(139, 21);
            this.lbldata.TabIndex = 13;
            this.lbldata.Text = "Nama Perusahaan";
            // 
            // lblpt
            // 
            this.lblpt.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblpt.Appearance.Options.UseFont = true;
            this.lblpt.Location = new System.Drawing.Point(158, 84);
            this.lblpt.Name = "lblpt";
            this.lblpt.Size = new System.Drawing.Size(139, 21);
            this.lblpt.TabIndex = 14;
            this.lblpt.Text = "Nama Perusahaan";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 84);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(127, 19);
            this.labelControl2.TabIndex = 15;
            this.labelControl2.Text = "Nama Perusahaan";
            // 
            // FrmPeriodeNew
            // 
            this.Appearance.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Appearance.ForeColor = System.Drawing.Color.Purple;
            this.Appearance.Options.UseBackColor = true;
            this.Appearance.Options.UseForeColor = true;
            this.Appearance.Options.UseTextOptions = true;
            this.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 290);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.lblwilayah);
            this.Controls.Add(this.lbldata);
            this.Controls.Add(this.lblpt);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.sbcetak);
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.cmbbulan);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.lblcompany);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Glow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.LookAndFeel.SkinName = "DevExpress Dark Style";
            this.LookAndFeel.UseWindowsXPTheme = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmPeriodeNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Periode Akuntansi";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.Load += new System.EventHandler(this.FrmPeriodeNew_Load);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblcompany;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton sbcetak;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl lblwilayah;
        private DevExpress.XtraEditors.LabelControl lbldata;
        private DevExpress.XtraEditors.LabelControl lblpt;
        private DevExpress.XtraEditors.LabelControl labelControl2;
    }
}