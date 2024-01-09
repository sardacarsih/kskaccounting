namespace Accounting.Form
{
    partial class FrmReportParamKebun
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
            this.sbcetak = new DevExpress.XtraEditors.SimpleButton();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.sbexport = new DevExpress.XtraEditors.SimpleButton();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.radioGroup1 = new DevExpress.XtraEditors.RadioGroup();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblcompany
            // 
            this.lblcompany.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblcompany.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblcompany.Location = new System.Drawing.Point(2, 4);
            this.lblcompany.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblcompany.Name = "lblcompany";
            this.lblcompany.Size = new System.Drawing.Size(414, 25);
            this.lblcompany.TabIndex = 2;
            this.lblcompany.Text = "Laporan Kebun";
            this.lblcompany.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sbcetak
            // 
            this.sbcetak.Location = new System.Drawing.Point(311, 172);
            this.sbcetak.Name = "sbcetak";
            this.sbcetak.Size = new System.Drawing.Size(79, 29);
            this.sbcetak.TabIndex = 9;
            this.sbcetak.Text = "Preview";
            this.sbcetak.Click += new System.EventHandler(this.sbcetak_Click);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(331, 136);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(58, 20);
            this.setahun.TabIndex = 8;
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(227, 136);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(99, 20);
            this.cmbbulan.TabIndex = 7;
            // 
            // sbexport
            // 
            this.sbexport.Location = new System.Drawing.Point(227, 172);
            this.sbexport.Name = "sbexport";
            this.sbexport.Size = new System.Drawing.Size(79, 29);
            this.sbexport.TabIndex = 9;
            this.sbexport.Text = "Export";
            this.sbexport.Click += new System.EventHandler(this.sbexport_Click);
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.radioGroup1);
            this.groupControl1.Location = new System.Drawing.Point(3, 18);
            this.groupControl1.Margin = new System.Windows.Forms.Padding(2);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(219, 185);
            this.groupControl1.TabIndex = 13;
            this.groupControl1.Text = "Jenis Laporan";
            // 
            // radioGroup1
            // 
            this.radioGroup1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioGroup1.Location = new System.Drawing.Point(2, 23);
            this.radioGroup1.Name = "radioGroup1";
            this.radioGroup1.Properties.Columns = 1;
            this.radioGroup1.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Biaya Rekap TM INTI per Divisi", true, null, ""),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Rincian Pekerjaan TBM"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Rincian Pekerjaan Pemeliharaan (TM)"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Rincian Pekerjaan Panen (TM)")});
            this.radioGroup1.Size = new System.Drawing.Size(215, 160);
            this.radioGroup1.TabIndex = 11;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.groupControl1);
            this.groupControl2.Controls.Add(this.sbexport);
            this.groupControl2.Controls.Add(this.sbcetak);
            this.groupControl2.Controls.Add(this.cmbbulan);
            this.groupControl2.Controls.Add(this.setahun);
            this.groupControl2.Location = new System.Drawing.Point(5, 37);
            this.groupControl2.Margin = new System.Windows.Forms.Padding(2);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(411, 217);
            this.groupControl2.TabIndex = 14;
            // 
            // FrmReportParamKebun
            // 
            this.Appearance.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Appearance.ForeColor = System.Drawing.Color.Purple;
            this.Appearance.Options.UseBackColor = true;
            this.Appearance.Options.UseForeColor = true;
            this.Appearance.Options.UseTextOptions = true;
            this.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(428, 265);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.lblcompany);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Glow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.LookAndFeel.SkinName = "DevExpress Dark Style";
            this.LookAndFeel.UseWindowsXPTheme = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmReportParamKebun";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Laporan Kebun";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.Load += new System.EventHandler(this.FrmReportParamEstate_Load);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblcompany;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.SimpleButton sbcetak;
        private DevExpress.XtraEditors.SimpleButton sbexport;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.RadioGroup radioGroup1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
    }
}