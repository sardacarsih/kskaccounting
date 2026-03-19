
namespace Accounting.Form
{
    partial class FrmMasterAkun
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
            this.nAMALabel = new System.Windows.Forms.Label();
            this.uSERIDLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnsimpan = new DevExpress.XtraEditors.SimpleButton();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.radioGroup = new DevExpress.XtraEditors.RadioGroup();
            this.leinduk = new DevExpress.XtraEditors.LookUpEdit();
            this.LEKELOMPOK = new DevExpress.XtraEditors.LookUpEdit();
            this.BTNIMPORT = new DevExpress.XtraEditors.SimpleButton();
            this.BTNEXPORT = new DevExpress.XtraEditors.SimpleButton();
            this.btnbaru = new DevExpress.XtraEditors.SimpleButton();
            this.txtnama = new DevExpress.XtraEditors.TextEdit();
            this.txtdg = new DevExpress.XtraEditors.TextEdit();
            this.txtlvl = new DevExpress.XtraEditors.TextEdit();
            this.txtjenis = new DevExpress.XtraEditors.TextEdit();
            this.txtkodeakhir = new DevExpress.XtraEditors.TextEdit();
            this.txtnamainduk = new DevExpress.XtraEditors.TextEdit();
            this.txtkode = new DevExpress.XtraEditors.TextEdit();
            this.radioGroup1 = new DevExpress.XtraEditors.RadioGroup();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leinduk.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LEKELOMPOK.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnama.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdg.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtlvl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtjenis.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkodeakhir.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamainduk.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // nAMALabel
            // 
            this.nAMALabel.AutoSize = true;
            this.nAMALabel.Location = new System.Drawing.Point(10, 173);
            this.nAMALabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nAMALabel.Name = "nAMALabel";
            this.nAMALabel.Size = new System.Drawing.Size(66, 13);
            this.nAMALabel.TabIndex = 38;
            this.nAMALabel.Text = "NAMA AKUN";
            // 
            // uSERIDLabel
            // 
            this.uSERIDLabel.AutoSize = true;
            this.uSERIDLabel.Location = new System.Drawing.Point(10, 146);
            this.uSERIDLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.uSERIDLabel.Name = "uSERIDLabel";
            this.uSERIDLabel.Size = new System.Drawing.Size(64, 13);
            this.uSERIDLabel.TabIndex = 39;
            this.uSERIDLabel.Text = "KODE AKUN";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "KELOMPOK";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 93);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 39;
            this.label3.Text = "INDUK";
            // 
            // btnsimpan
            // 
            this.btnsimpan.Location = new System.Drawing.Point(149, 204);
            this.btnsimpan.Margin = new System.Windows.Forms.Padding(2);
            this.btnsimpan.Name = "btnsimpan";
            this.btnsimpan.Size = new System.Drawing.Size(64, 23);
            this.btnsimpan.TabIndex = 0;
            this.btnsimpan.Text = "Simpan";
            this.btnsimpan.Click += new System.EventHandler(this.btnsimpan_Click);
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Location = new System.Drawing.Point(374, 35);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1066, 594);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 217;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsFind.AlwaysVisible = true;
            this.gridView1.OptionsFind.ShowFindButton = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            //
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.radioGroup);
            this.groupControl2.Controls.Add(this.leinduk);
            this.groupControl2.Controls.Add(this.LEKELOMPOK);
            this.groupControl2.Controls.Add(this.BTNIMPORT);
            this.groupControl2.Controls.Add(this.BTNEXPORT);
            this.groupControl2.Controls.Add(this.btnsimpan);
            this.groupControl2.Controls.Add(this.btnbaru);
            this.groupControl2.Controls.Add(this.nAMALabel);
            this.groupControl2.Controls.Add(this.txtnama);
            this.groupControl2.Controls.Add(this.label3);
            this.groupControl2.Controls.Add(this.label1);
            this.groupControl2.Controls.Add(this.uSERIDLabel);
            this.groupControl2.Controls.Add(this.txtdg);
            this.groupControl2.Controls.Add(this.txtlvl);
            this.groupControl2.Controls.Add(this.txtjenis);
            this.groupControl2.Controls.Add(this.txtkodeakhir);
            this.groupControl2.Controls.Add(this.txtnamainduk);
            this.groupControl2.Controls.Add(this.txtkode);
            this.groupControl2.Location = new System.Drawing.Point(8, 8);
            this.groupControl2.Margin = new System.Windows.Forms.Padding(2);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(362, 247);
            this.groupControl2.TabIndex = 0;
            this.groupControl2.Text = "Input Master Akun Blok";
            // 
            // radioGroup
            // 
            this.radioGroup.Location = new System.Drawing.Point(82, 57);
            this.radioGroup.Margin = new System.Windows.Forms.Padding(2);
            this.radioGroup.Name = "radioGroup";
            this.radioGroup.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Group"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Detail")});
            this.radioGroup.Size = new System.Drawing.Size(171, 23);
            this.radioGroup.TabIndex = 2;
            this.radioGroup.SelectedIndexChanged += new System.EventHandler(this.radioGroup_SelectedIndexChanged);
            // 
            // leinduk
            // 
            this.leinduk.Location = new System.Drawing.Point(82, 90);
            this.leinduk.Margin = new System.Windows.Forms.Padding(2);
            this.leinduk.Name = "leinduk";
            this.leinduk.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            this.leinduk.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.leinduk.Properties.PopupFormMinSize = new System.Drawing.Size(533, 186);
            this.leinduk.Size = new System.Drawing.Size(171, 20);
            this.leinduk.TabIndex = 41;
            // 
            // LEKELOMPOK
            // 
            this.LEKELOMPOK.Location = new System.Drawing.Point(82, 27);
            this.LEKELOMPOK.Margin = new System.Windows.Forms.Padding(2);
            this.LEKELOMPOK.Name = "LEKELOMPOK";
            this.LEKELOMPOK.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.LEKELOMPOK.Size = new System.Drawing.Size(171, 20);
            this.LEKELOMPOK.TabIndex = 41;
            this.LEKELOMPOK.EditValueChanged += new System.EventHandler(this.LEKELOMPOK_EditValueChanged);
            // 
            // BTNIMPORT
            // 
            this.BTNIMPORT.Enabled = false;
            this.BTNIMPORT.Location = new System.Drawing.Point(13, 204);
            this.BTNIMPORT.Margin = new System.Windows.Forms.Padding(2);
            this.BTNIMPORT.Name = "BTNIMPORT";
            this.BTNIMPORT.Size = new System.Drawing.Size(64, 23);
            this.BTNIMPORT.TabIndex = 1;
            this.BTNIMPORT.Text = "Import";
            // 
            // BTNEXPORT
            // 
            this.BTNEXPORT.Enabled = false;
            this.BTNEXPORT.Location = new System.Drawing.Point(285, 204);
            this.BTNEXPORT.Margin = new System.Windows.Forms.Padding(2);
            this.BTNEXPORT.Name = "BTNEXPORT";
            this.BTNEXPORT.Size = new System.Drawing.Size(64, 23);
            this.BTNEXPORT.TabIndex = 1;
            this.BTNEXPORT.Text = "Export";
            //
            // btnbaru
            // 
            this.btnbaru.Location = new System.Drawing.Point(81, 204);
            this.btnbaru.Margin = new System.Windows.Forms.Padding(2);
            this.btnbaru.Name = "btnbaru";
            this.btnbaru.Size = new System.Drawing.Size(64, 23);
            this.btnbaru.TabIndex = 0;
            this.btnbaru.Text = "Baru";
            this.btnbaru.Click += new System.EventHandler(this.btnbaru_Click);
            // 
            // txtnama
            // 
            this.txtnama.Location = new System.Drawing.Point(80, 171);
            this.txtnama.Margin = new System.Windows.Forms.Padding(2);
            this.txtnama.Name = "txtnama";
            this.txtnama.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtnama.Size = new System.Drawing.Size(173, 20);
            this.txtnama.TabIndex = 1;
            this.txtnama.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnamablok_KeyDown);
            // 
            // txtdg
            // 
            this.txtdg.EditValue = "D";
            this.txtdg.Location = new System.Drawing.Point(228, 144);
            this.txtdg.Margin = new System.Windows.Forms.Padding(2);
            this.txtdg.Name = "txtdg";
            this.txtdg.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtdg.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtdg.Properties.MaskSettings.Set("mask", "000");
            this.txtdg.Properties.ReadOnly = true;
            this.txtdg.Size = new System.Drawing.Size(23, 20);
            this.txtdg.TabIndex = 0;
            this.txtdg.ToolTip = "G/D";
            this.txtdg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // txtlvl
            // 
            this.txtlvl.Location = new System.Drawing.Point(201, 144);
            this.txtlvl.Margin = new System.Windows.Forms.Padding(2);
            this.txtlvl.Name = "txtlvl";
            this.txtlvl.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtlvl.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtlvl.Properties.MaskSettings.Set("mask", "000");
            this.txtlvl.Properties.ReadOnly = true;
            this.txtlvl.Size = new System.Drawing.Size(23, 20);
            this.txtlvl.TabIndex = 0;
            this.txtlvl.ToolTip = "LEVEL";
            this.txtlvl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // txtjenis
            // 
            this.txtjenis.Location = new System.Drawing.Point(175, 144);
            this.txtjenis.Margin = new System.Windows.Forms.Padding(2);
            this.txtjenis.Name = "txtjenis";
            this.txtjenis.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtjenis.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtjenis.Properties.MaskSettings.Set("mask", "000");
            this.txtjenis.Properties.ReadOnly = true;
            this.txtjenis.Size = new System.Drawing.Size(23, 20);
            this.txtjenis.TabIndex = 0;
            this.txtjenis.ToolTip = "JENIS";
            this.txtjenis.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // txtkodeakhir
            // 
            this.txtkodeakhir.Location = new System.Drawing.Point(140, 144);
            this.txtkodeakhir.Margin = new System.Windows.Forms.Padding(2);
            this.txtkodeakhir.Name = "txtkodeakhir";
            this.txtkodeakhir.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtkodeakhir.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtkodeakhir.Properties.MaskSettings.Set("mask", "000");
            this.txtkodeakhir.Properties.MaxLength = 12;
            this.txtkodeakhir.Size = new System.Drawing.Size(31, 20);
            this.txtkodeakhir.TabIndex = 0;
            this.txtkodeakhir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // txtnamainduk
            // 
            this.txtnamainduk.Location = new System.Drawing.Point(80, 120);
            this.txtnamainduk.Margin = new System.Windows.Forms.Padding(2);
            this.txtnamainduk.Name = "txtnamainduk";
            this.txtnamainduk.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtnamainduk.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtnamainduk.Properties.MaskSettings.Set("mask", "000");
            this.txtnamainduk.Properties.ReadOnly = true;
            this.txtnamainduk.Size = new System.Drawing.Size(173, 20);
            this.txtnamainduk.TabIndex = 0;
            this.txtnamainduk.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // txtkode
            // 
            this.txtkode.Location = new System.Drawing.Point(80, 144);
            this.txtkode.Margin = new System.Windows.Forms.Padding(2);
            this.txtkode.Name = "txtkode";
            this.txtkode.Properties.MaxLength = 12;
            this.txtkode.Properties.ReadOnly = true;
            this.txtkode.Size = new System.Drawing.Size(56, 20);
            this.txtkode.TabIndex = 0;
            this.txtkode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodeblok_KeyDown);
            // 
            // radioGroup1
            // 
            this.radioGroup1.Location = new System.Drawing.Point(374, 8);
            this.radioGroup1.Margin = new System.Windows.Forms.Padding(2);
            this.radioGroup1.Name = "radioGroup1";
            this.radioGroup1.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "TBM"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "TM PANEN"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "TM PERAWATAN")});
            this.radioGroup1.Size = new System.Drawing.Size(403, 23);
            this.radioGroup1.TabIndex = 2;
            this.radioGroup1.SelectedIndexChanged += new System.EventHandler(this.radioGroup1_SelectedIndexChanged);
            // 
            // FrmMasterAkun
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1451, 636);
            this.Controls.Add(this.radioGroup1);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.gridControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmMasterAkun";
            this.Text = "Master Akun Blok";
            this.Load += new System.EventHandler(this.FrmMasterAkun_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leinduk.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LEKELOMPOK.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnama.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdg.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtlvl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtjenis.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkodeakhir.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamainduk.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SimpleButton btnsimpan;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.TextEdit txtnama;
        private DevExpress.XtraEditors.TextEdit txtkode;
        private DevExpress.XtraEditors.SimpleButton btnbaru;
        private DevExpress.XtraEditors.RadioGroup radioGroup1;
        private DevExpress.XtraEditors.LookUpEdit LEKELOMPOK;
        private DevExpress.XtraEditors.SimpleButton BTNIMPORT;
        private DevExpress.XtraEditors.SimpleButton BTNEXPORT;
        private DevExpress.XtraEditors.RadioGroup radioGroup;
        private DevExpress.XtraEditors.TextEdit txtkodeakhir;
        private DevExpress.XtraEditors.LookUpEdit leinduk;
        private DevExpress.XtraEditors.TextEdit txtjenis;
        private DevExpress.XtraEditors.TextEdit txtlvl;
        private DevExpress.XtraEditors.TextEdit txtnamainduk;
        private DevExpress.XtraEditors.TextEdit txtdg;
        private System.Windows.Forms.Label nAMALabel;
        private System.Windows.Forms.Label uSERIDLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
    }
}