
namespace Accounting
{
    partial class FrmAkunAdd
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
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.sbtutup = new DevExpress.XtraEditors.SimpleButton();
            this.checkEditnonaktif = new DevExpress.XtraEditors.CheckEdit();
            this.checkEditbagianakun = new DevExpress.XtraEditors.CheckEdit();
            this.txtnamaakun = new DevExpress.XtraEditors.TextEdit();
            this.txtnoakungroup = new DevExpress.XtraEditors.TextEdit();
            this.lookUpEditbagiandari = new DevExpress.XtraEditors.LookUpEdit();
            this.lookUpEdikat = new DevExpress.XtraEditors.LookUpEdit();
            this.gd = new DevExpress.XtraEditors.RadioGroup();
            this.txtkepalaakun = new DevExpress.XtraEditors.TextEdit();
            this.txtlvel = new DevExpress.XtraEditors.TextEdit();
            this.lblinduk = new DevExpress.XtraEditors.LabelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.rgsisi = new DevExpress.XtraEditors.RadioGroup();
            this.lbliddata = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.txtnoakundetail = new DevExpress.XtraEditors.TextEdit();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditnonaktif.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditbagianakun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamaakun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnoakungroup.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditbagiandari.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdikat.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gd.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkepalaakun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtlvel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgsisi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnoakundetail.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(33, 35);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(47, 13);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Tipe Akun";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(33, 169);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(51, 13);
            this.labelControl2.TabIndex = 1;
            this.labelControl2.Text = "Kode Akun";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(33, 196);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(54, 13);
            this.labelControl3.TabIndex = 1;
            this.labelControl3.Text = "Nama Akun";
            // 
            // sbtutup
            // 
            this.sbtutup.Location = new System.Drawing.Point(201, 227);
            this.sbtutup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sbtutup.Name = "sbtutup";
            this.sbtutup.Size = new System.Drawing.Size(75, 30);
            this.sbtutup.TabIndex = 10;
            this.sbtutup.Text = "Tutup";
            this.sbtutup.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // checkEditnonaktif
            // 
            this.checkEditnonaktif.Location = new System.Drawing.Point(366, 195);
            this.checkEditnonaktif.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkEditnonaktif.Name = "checkEditnonaktif";
            this.checkEditnonaktif.Properties.Caption = "Non Aktif";
            this.checkEditnonaktif.Size = new System.Drawing.Size(100, 20);
            this.checkEditnonaktif.TabIndex = 9;
            // 
            // checkEditbagianakun
            // 
            this.checkEditbagianakun.Location = new System.Drawing.Point(111, 60);
            this.checkEditbagianakun.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkEditbagianakun.Name = "checkEditbagianakun";
            this.checkEditbagianakun.Properties.Caption = "Bagian Dari Akun";
            this.checkEditbagianakun.Size = new System.Drawing.Size(239, 20);
            this.checkEditbagianakun.TabIndex = 1;
            this.checkEditbagianakun.CheckStateChanged += new System.EventHandler(this.checkEditbagianakun_CheckStateChanged);
            this.checkEditbagianakun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.checkEditbagianakun_KeyDown);
            // 
            // txtnamaakun
            // 
            this.txtnamaakun.Location = new System.Drawing.Point(111, 195);
            this.txtnamaakun.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtnamaakun.Name = "txtnamaakun";
            this.txtnamaakun.Size = new System.Drawing.Size(241, 20);
            this.txtnamaakun.TabIndex = 6;
            this.txtnamaakun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnamaakun_KeyDown);
            // 
            // txtnoakungroup
            // 
            this.txtnoakungroup.Location = new System.Drawing.Point(144, 168);
            this.txtnoakungroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtnoakungroup.Name = "txtnoakungroup";
            this.txtnoakungroup.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtnoakungroup.Properties.MaskSettings.Set("mask", "00000.");
            this.txtnoakungroup.Properties.MaskSettings.Set("ignoreMaskBlank", false);
            this.txtnoakungroup.Properties.UseMaskAsDisplayFormat = true;
            this.txtnoakungroup.Size = new System.Drawing.Size(46, 20);
            this.txtnoakungroup.TabIndex = 4;
            this.txtnoakungroup.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnoakungroup_KeyDown);
            // 
            // lookUpEditbagiandari
            // 
            this.lookUpEditbagiandari.Location = new System.Drawing.Point(111, 87);
            this.lookUpEditbagiandari.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lookUpEditbagiandari.Name = "lookUpEditbagiandari";
            this.lookUpEditbagiandari.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            this.lookUpEditbagiandari.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpEditbagiandari.Properties.CaseSensitiveSearch = true;
            this.lookUpEditbagiandari.Size = new System.Drawing.Size(124, 20);
            this.lookUpEditbagiandari.TabIndex = 1;
            this.lookUpEditbagiandari.EditValueChanged += new System.EventHandler(this.lookUpEditbagiandari_EditValueChanged);
            this.lookUpEditbagiandari.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lookUpEditbagiandari_KeyDown);
            // 
            // lookUpEdikat
            // 
            this.lookUpEdikat.Location = new System.Drawing.Point(111, 33);
            this.lookUpEdikat.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lookUpEdikat.Name = "lookUpEdikat";
            this.lookUpEdikat.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpEdikat.Properties.DisplayMember = "TIPEID";
            this.lookUpEdikat.Properties.ValueMember = "TIPEID";
            this.lookUpEdikat.Size = new System.Drawing.Size(239, 20);
            this.lookUpEdikat.TabIndex = 0;
            this.lookUpEdikat.EditValueChanged += new System.EventHandler(this.lookUpEdikat_EditValueChanged);
            this.lookUpEdikat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lookUpEdikat_KeyDown);
            // 
            // gd
            // 
            this.gd.Enabled = false;
            this.gd.Location = new System.Drawing.Point(111, 135);
            this.gd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gd.Name = "gd";
            this.gd.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Group"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Detail")});
            this.gd.Size = new System.Drawing.Size(124, 22);
            this.gd.TabIndex = 2;
            this.gd.SelectedIndexChanged += new System.EventHandler(this.gd_SelectedIndexChanged);
            this.gd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gd_KeyDown);
            // 
            // txtkepalaakun
            // 
            this.txtkepalaakun.Location = new System.Drawing.Point(111, 168);
            this.txtkepalaakun.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtkepalaakun.Name = "txtkepalaakun";
            this.txtkepalaakun.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtkepalaakun.Properties.MaskSettings.Set("mask", "00.");
            this.txtkepalaakun.Size = new System.Drawing.Size(29, 20);
            this.txtkepalaakun.TabIndex = 3;
            this.txtkepalaakun.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkepalaakun_KeyDown);
            // 
            // txtlvel
            // 
            this.txtlvel.EditValue = "1";
            this.txtlvel.Enabled = false;
            this.txtlvel.Location = new System.Drawing.Point(269, 168);
            this.txtlvel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtlvel.Name = "txtlvel";
            this.txtlvel.Size = new System.Drawing.Size(21, 20);
            this.txtlvel.TabIndex = 11;
            // 
            // lblinduk
            // 
            this.lblinduk.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblinduk.Appearance.ForeColor = System.Drawing.Color.Blue;
            this.lblinduk.Appearance.Options.UseFont = true;
            this.lblinduk.Appearance.Options.UseForeColor = true;
            this.lblinduk.Location = new System.Drawing.Point(111, 108);
            this.lblinduk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lblinduk.Name = "lblinduk";
            this.lblinduk.Size = new System.Drawing.Size(68, 15);
            this.lblinduk.TabIndex = 1;
            this.lblinduk.Text = "Nama Induk";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(241, 169);
            this.labelControl7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(25, 13);
            this.labelControl7.TabIndex = 10;
            this.labelControl7.Text = "Level";
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(295, 147);
            this.labelControl8.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(62, 13);
            this.labelControl8.TabIndex = 1;
            this.labelControl8.Text = "Saldo Normal";
            // 
            // rgsisi
            // 
            this.rgsisi.Enabled = false;
            this.rgsisi.Location = new System.Drawing.Point(295, 163);
            this.rgsisi.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rgsisi.Name = "rgsisi";
            this.rgsisi.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Debet"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Kredit")});
            this.rgsisi.Properties.ReadOnly = true;
            this.rgsisi.Size = new System.Drawing.Size(124, 22);
            this.rgsisi.TabIndex = 12;
            // 
            // lbliddata
            // 
            this.lbliddata.Location = new System.Drawing.Point(181, 11);
            this.lbliddata.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lbliddata.Name = "lbliddata";
            this.lbliddata.Size = new System.Drawing.Size(30, 13);
            this.lbliddata.TabIndex = 1;
            this.lbliddata.Text = "iddata";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(111, 227);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 30);
            this.simpleButton1.TabIndex = 9;
            this.simpleButton1.Text = "Simpan";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // txtnoakundetail
            // 
            this.txtnoakundetail.Location = new System.Drawing.Point(194, 168);
            this.txtnoakundetail.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtnoakundetail.Name = "txtnoakundetail";
            this.txtnoakundetail.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtnoakundetail.Properties.MaskSettings.Set("mask", "000");
            this.txtnoakundetail.Properties.MaskSettings.Set("ignoreMaskBlank", false);
            this.txtnoakundetail.Properties.UseMaskAsDisplayFormat = true;
            this.txtnoakundetail.Size = new System.Drawing.Size(46, 20);
            this.txtnoakundetail.TabIndex = 5;
            this.txtnoakundetail.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnoakundetail_KeyDown);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(111, 9);
            this.setahun.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Size = new System.Drawing.Size(67, 20);
            this.setahun.TabIndex = 13;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(33, 11);
            this.labelControl6.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(30, 13);
            this.labelControl6.TabIndex = 1;
            this.labelControl6.Text = "Tahun";
            // 
            // FrmAkunAdd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 262);
            this.ControlBox = false;
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.rgsisi);
            this.Controls.Add(this.gd);
            this.Controls.Add(this.sbtutup);
            this.Controls.Add(this.checkEditnonaktif);
            this.Controls.Add(this.checkEditbagianakun);
            this.Controls.Add(this.txtnamaakun);
            this.Controls.Add(this.txtlvel);
            this.Controls.Add(this.txtkepalaakun);
            this.Controls.Add(this.txtnoakundetail);
            this.Controls.Add(this.txtnoakungroup);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.lbliddata);
            this.Controls.Add(this.labelControl8);
            this.Controls.Add(this.labelControl7);
            this.Controls.Add(this.lblinduk);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.lookUpEditbagiandari);
            this.Controls.Add(this.lookUpEdikat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmAkunAdd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Akun";
            this.Load += new System.EventHandler(this.FrmAkunAddEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.checkEditnonaktif.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditbagianakun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamaakun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnoakungroup.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditbagiandari.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdikat.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gd.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkepalaakun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtlvel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rgsisi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnoakundetail.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LookUpEdit lookUpEdikat;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit txtnoakungroup;
        private DevExpress.XtraEditors.CheckEdit checkEditbagianakun;
        private DevExpress.XtraEditors.LookUpEdit lookUpEditbagiandari;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.CheckEdit checkEditnonaktif;
        private DevExpress.XtraEditors.TextEdit txtnamaakun;
        private DevExpress.XtraEditors.SimpleButton sbtutup;
        private DevExpress.XtraEditors.RadioGroup gd;
        private DevExpress.XtraEditors.TextEdit txtkepalaakun;
        private DevExpress.XtraEditors.TextEdit txtlvel;
        private DevExpress.XtraEditors.LabelControl lblinduk;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private DevExpress.XtraEditors.RadioGroup rgsisi;
        private DevExpress.XtraEditors.LabelControl lbliddata;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit txtnoakundetail;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.LabelControl labelControl6;
    }
}