
namespace Accounting.Form
{
    partial class FrmAkunEF
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAkunEF));
            DevExpress.XtraGrid.GridFormatRule gridFormatRule1 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule2 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleValue formatConditionRuleValue1 = new DevExpress.XtraEditors.FormatConditionRuleValue();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule3 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleValue formatConditionRuleValue2 = new DevExpress.XtraEditors.FormatConditionRuleValue();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule4 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleValue formatConditionRuleValue3 = new DevExpress.XtraEditors.FormatConditionRuleValue();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule5 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleValue formatConditionRuleValue4 = new DevExpress.XtraEditors.FormatConditionRuleValue();
            this.SALDOAKHIR = new DevExpress.XtraGrid.Columns.GridColumn();
            this.SALDOAWAL = new DevExpress.XtraGrid.Columns.GridColumn();
            this.MUTASI = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ISAKTIF = new DevExpress.XtraGrid.Columns.GridColumn();
            this.sbadd = new DevExpress.XtraEditors.SimpleButton();
            this.sbubah = new DevExpress.XtraEditors.SimpleButton();
            this.sbhapus = new DevExpress.XtraEditors.SimpleButton();
            this.sbexport = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.lookUpEdit1 = new DevExpress.XtraEditors.LookUpEdit();
            this.sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cedetail = new DevExpress.XtraEditors.CheckEdit();
            this.cegroup = new DevExpress.XtraEditors.CheckEdit();
            this.cetbm = new DevExpress.XtraEditors.CheckEdit();
            this.cetm = new DevExpress.XtraEditors.CheckEdit();
            this.CEMUTASI = new DevExpress.XtraEditors.CheckEdit();
            this.NilaiSaldo = new DevExpress.XtraEditors.CheckEdit();
            this.AkunLabaRugi = new DevExpress.XtraEditors.CheckEdit();
            this.AkunNeraca = new DevExpress.XtraEditors.CheckEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.ID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KODE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NAMA = new DevExpress.XtraGrid.Columns.GridColumn();
            this.GRP = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LVL = new DevExpress.XtraGrid.Columns.GridColumn();
            this.INDUK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.GD = new DevExpress.XtraGrid.Columns.GridColumn();
            this.POSISI = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AWALTAHUN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DEBET = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KREDIT = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DIVISI = new DevExpress.XtraGrid.Columns.GridColumn();
            this.BLOK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TAHUNTANAM = new DevExpress.XtraGrid.Columns.GridColumn();
            this.sbexpadvanced = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdit1.Properties)).BeginInit();
            this.sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cedetail.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cegroup.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cetbm.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cetm.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CEMUTASI.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NilaiSaldo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AkunLabaRugi.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AkunNeraca.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // SALDOAKHIR
            // 
            this.SALDOAKHIR.Caption = "Saldo Akhir";
            this.SALDOAKHIR.DisplayFormat.FormatString = "{0:n2}";
            this.SALDOAKHIR.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.SALDOAKHIR.FieldName = "SALDOAKHIR";
            this.SALDOAKHIR.MinWidth = 16;
            this.SALDOAKHIR.Name = "SALDOAKHIR";
            this.SALDOAKHIR.Visible = true;
            this.SALDOAKHIR.VisibleIndex = 10;
            this.SALDOAKHIR.Width = 150;
            // 
            // SALDOAWAL
            // 
            this.SALDOAWAL.Caption = "Saldo Awal";
            this.SALDOAWAL.DisplayFormat.FormatString = "{0:n2}";
            this.SALDOAWAL.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.SALDOAWAL.FieldName = "SALDOAWAL";
            this.SALDOAWAL.MinWidth = 16;
            this.SALDOAWAL.Name = "SALDOAWAL";
            this.SALDOAWAL.Visible = true;
            this.SALDOAWAL.VisibleIndex = 7;
            this.SALDOAWAL.Width = 150;
            // 
            // MUTASI
            // 
            this.MUTASI.Caption = "Mutasi";
            this.MUTASI.DisplayFormat.FormatString = "n2";
            this.MUTASI.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.MUTASI.FieldName = "MUTASI";
            this.MUTASI.MinWidth = 18;
            this.MUTASI.Name = "MUTASI";
            this.MUTASI.UnboundDataType = typeof(decimal);
            this.MUTASI.UnboundExpression = "Iif([POSISI] = \'D\', [DEBET] - [KREDIT], [KREDIT] - [DEBET])";
            this.MUTASI.Width = 38;
            // 
            // ISAKTIF
            // 
            this.ISAKTIF.Caption = "AKTIF";
            this.ISAKTIF.FieldName = "ISAKTIF";
            this.ISAKTIF.MinWidth = 10;
            this.ISAKTIF.Name = "ISAKTIF";
            this.ISAKTIF.OptionsColumn.FixedWidth = true;
            this.ISAKTIF.Width = 40;
            // 
            // sbadd
            // 
            this.sbadd.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sbadd.Appearance.Options.UseFont = true;
            this.sbadd.ImageOptions.Image = global::Accounting.Properties.Resources.add_16x16;
            this.sbadd.Location = new System.Drawing.Point(269, 9);
            this.sbadd.Margin = new System.Windows.Forms.Padding(0);
            this.sbadd.Name = "sbadd";
            this.sbadd.Size = new System.Drawing.Size(69, 31);
            this.sbadd.TabIndex = 0;
            this.sbadd.Text = "Baru";
            this.sbadd.Click += new System.EventHandler(this.sbadd_Click);
            // 
            // sbubah
            // 
            this.sbubah.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sbubah.Appearance.Options.UseFont = true;
            this.sbubah.ImageOptions.Image = global::Accounting.Properties.Resources.updatefield_16x16;
            this.sbubah.Location = new System.Drawing.Point(343, 9);
            this.sbubah.Margin = new System.Windows.Forms.Padding(1208, 1338, 1208, 1338);
            this.sbubah.Name = "sbubah";
            this.sbubah.Size = new System.Drawing.Size(69, 31);
            this.sbubah.TabIndex = 1;
            this.sbubah.Text = "Ubah";
            this.sbubah.Click += new System.EventHandler(this.sbubah_Click);
            // 
            // sbhapus
            // 
            this.sbhapus.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sbhapus.Appearance.Options.UseFont = true;
            this.sbhapus.ImageOptions.Image = global::Accounting.Properties.Resources.removepivotfield_16x16;
            this.sbhapus.Location = new System.Drawing.Point(420, 9);
            this.sbhapus.Margin = new System.Windows.Forms.Padding(1208, 1338, 1208, 1338);
            this.sbhapus.Name = "sbhapus";
            this.sbhapus.Size = new System.Drawing.Size(69, 31);
            this.sbhapus.TabIndex = 2;
            this.sbhapus.Text = "Hapus";
            this.sbhapus.Click += new System.EventHandler(this.sbhapus_Click);
            // 
            // sbexport
            // 
            this.sbexport.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sbexport.Appearance.Options.UseFont = true;
            this.sbexport.ImageOptions.Image = global::Accounting.Properties.Resources.exporttoxlsx_16x16;
            this.sbexport.Location = new System.Drawing.Point(501, 9);
            this.sbexport.Margin = new System.Windows.Forms.Padding(1208, 1338, 1208, 1338);
            this.sbexport.Name = "sbexport";
            this.sbexport.Size = new System.Drawing.Size(69, 31);
            this.sbexport.TabIndex = 2;
            this.sbexport.Text = "Export";
            this.sbexport.Click += new System.EventHandler(this.sbexport_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Location = new System.Drawing.Point(1132, 18);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(69, 21);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Tipe Akun";
            // 
            // lookUpEdit1
            // 
            this.lookUpEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lookUpEdit1.Location = new System.Drawing.Point(1207, 14);
            this.lookUpEdit1.Margin = new System.Windows.Forms.Padding(4);
            this.lookUpEdit1.Name = "lookUpEdit1";
            this.lookUpEdit1.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lookUpEdit1.Properties.Appearance.Options.UseFont = true;
            this.lookUpEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpEdit1.Properties.PopupWidthMode = DevExpress.XtraEditors.PopupWidthMode.ContentWidth;
            this.lookUpEdit1.Size = new System.Drawing.Size(169, 28);
            this.lookUpEdit1.TabIndex = 0;
            this.lookUpEdit1.EditValueChanged += new System.EventHandler(this.lookUpEdit1_EditValueChanged);
            // 
            // sidePanel1
            // 
            this.sidePanel1.Controls.Add(this.sbexpadvanced);
            this.sidePanel1.Controls.Add(this.sbexport);
            this.sidePanel1.Controls.Add(this.sbubah);
            this.sidePanel1.Controls.Add(this.setahun);
            this.sidePanel1.Controls.Add(this.sbhapus);
            this.sidePanel1.Controls.Add(this.cmbbulan);
            this.sidePanel1.Controls.Add(this.sbadd);
            this.sidePanel1.Controls.Add(this.cedetail);
            this.sidePanel1.Controls.Add(this.cegroup);
            this.sidePanel1.Controls.Add(this.cetbm);
            this.sidePanel1.Controls.Add(this.cetm);
            this.sidePanel1.Controls.Add(this.CEMUTASI);
            this.sidePanel1.Controls.Add(this.NilaiSaldo);
            this.sidePanel1.Controls.Add(this.AkunLabaRugi);
            this.sidePanel1.Controls.Add(this.AkunNeraca);
            this.sidePanel1.Controls.Add(this.lookUpEdit1);
            this.sidePanel1.Controls.Add(this.labelControl3);
            this.sidePanel1.Controls.Add(this.labelControl1);
            this.sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.sidePanel1.Location = new System.Drawing.Point(0, 0);
            this.sidePanel1.Margin = new System.Windows.Forms.Padding(4);
            this.sidePanel1.Name = "sidePanel1";
            this.sidePanel1.Size = new System.Drawing.Size(1384, 60);
            this.sidePanel1.TabIndex = 0;
            this.sidePanel1.Text = "sidePanel1";
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(197, 10);
            this.setahun.Margin = new System.Windows.Forms.Padding(4);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.setahun.Properties.Appearance.Options.UseFont = true;
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(63, 28);
            this.setahun.TabIndex = 5;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(81, 10);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(4);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmbbulan.Properties.Appearance.Options.UseFont = true;
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(108, 28);
            this.cmbbulan.TabIndex = 4;
            this.cmbbulan.SelectedIndexChanged += new System.EventHandler(this.cmbbulan_SelectedIndexChanged);
            // 
            // cedetail
            // 
            this.cedetail.Location = new System.Drawing.Point(1033, 29);
            this.cedetail.Margin = new System.Windows.Forms.Padding(4);
            this.cedetail.Name = "cedetail";
            this.cedetail.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cedetail.Properties.Appearance.Options.UseFont = true;
            this.cedetail.Properties.Caption = "Detail";
            this.cedetail.Size = new System.Drawing.Size(66, 25);
            this.cedetail.TabIndex = 3;
            this.cedetail.CheckedChanged += new System.EventHandler(this.cedetail_CheckedChanged);
            // 
            // cegroup
            // 
            this.cegroup.Location = new System.Drawing.Point(1033, 7);
            this.cegroup.Margin = new System.Windows.Forms.Padding(4);
            this.cegroup.Name = "cegroup";
            this.cegroup.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cegroup.Properties.Appearance.Options.UseFont = true;
            this.cegroup.Properties.Caption = "Group";
            this.cegroup.Size = new System.Drawing.Size(66, 25);
            this.cegroup.TabIndex = 3;
            this.cegroup.CheckedChanged += new System.EventHandler(this.cegroup_CheckedChanged);
            // 
            // cetbm
            // 
            this.cetbm.Location = new System.Drawing.Point(852, 6);
            this.cetbm.Margin = new System.Windows.Forms.Padding(4);
            this.cetbm.Name = "cetbm";
            this.cetbm.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cetbm.Properties.Appearance.Options.UseFont = true;
            this.cetbm.Properties.Caption = "TBM";
            this.cetbm.Size = new System.Drawing.Size(66, 25);
            this.cetbm.TabIndex = 3;
            this.cetbm.CheckedChanged += new System.EventHandler(this.cetbm_CheckedChanged);
            this.cetbm.Click += new System.EventHandler(this.cetbm_Click);
            // 
            // cetm
            // 
            this.cetm.Location = new System.Drawing.Point(853, 29);
            this.cetm.Margin = new System.Windows.Forms.Padding(4);
            this.cetm.Name = "cetm";
            this.cetm.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cetm.Properties.Appearance.Options.UseFont = true;
            this.cetm.Properties.Caption = "TM";
            this.cetm.Size = new System.Drawing.Size(65, 25);
            this.cetm.TabIndex = 3;
            this.cetm.CheckedChanged += new System.EventHandler(this.cetm_CheckedChanged);
            this.cetm.Click += new System.EventHandler(this.cetm_Click);
            // 
            // CEMUTASI
            // 
            this.CEMUTASI.Location = new System.Drawing.Point(919, 7);
            this.CEMUTASI.Margin = new System.Windows.Forms.Padding(4);
            this.CEMUTASI.Name = "CEMUTASI";
            this.CEMUTASI.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.CEMUTASI.Properties.Appearance.Options.UseFont = true;
            this.CEMUTASI.Properties.Caption = "Mutasi <>0";
            this.CEMUTASI.Size = new System.Drawing.Size(116, 25);
            this.CEMUTASI.TabIndex = 3;
            this.CEMUTASI.CheckedChanged += new System.EventHandler(this.CEMUTASI_CheckedChanged);
            // 
            // NilaiSaldo
            // 
            this.NilaiSaldo.Location = new System.Drawing.Point(919, 29);
            this.NilaiSaldo.Margin = new System.Windows.Forms.Padding(4);
            this.NilaiSaldo.Name = "NilaiSaldo";
            this.NilaiSaldo.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.NilaiSaldo.Properties.Appearance.Options.UseFont = true;
            this.NilaiSaldo.Properties.Caption = "Saldo <>0";
            this.NilaiSaldo.Size = new System.Drawing.Size(134, 25);
            this.NilaiSaldo.TabIndex = 3;
            this.NilaiSaldo.CheckedChanged += new System.EventHandler(this.NilaiSaldo_CheckedChanged);
            // 
            // AkunLabaRugi
            // 
            this.AkunLabaRugi.Location = new System.Drawing.Point(718, 29);
            this.AkunLabaRugi.Margin = new System.Windows.Forms.Padding(4);
            this.AkunLabaRugi.Name = "AkunLabaRugi";
            this.AkunLabaRugi.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AkunLabaRugi.Properties.Appearance.Options.UseFont = true;
            this.AkunLabaRugi.Properties.Caption = "Akun Laba Rugi";
            this.AkunLabaRugi.Size = new System.Drawing.Size(134, 25);
            this.AkunLabaRugi.TabIndex = 3;
            this.AkunLabaRugi.CheckedChanged += new System.EventHandler(this.AkunLabaRugi_CheckedChanged);
            // 
            // AkunNeraca
            // 
            this.AkunNeraca.Location = new System.Drawing.Point(718, 4);
            this.AkunNeraca.Margin = new System.Windows.Forms.Padding(4);
            this.AkunNeraca.Name = "AkunNeraca";
            this.AkunNeraca.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AkunNeraca.Properties.Appearance.Options.UseFont = true;
            this.AkunNeraca.Properties.Caption = "Akun Neraca";
            this.AkunNeraca.Size = new System.Drawing.Size(134, 25);
            this.AkunNeraca.TabIndex = 3;
            this.AkunNeraca.CheckedChanged += new System.EventHandler(this.AkunNeraca_CheckedChanged);
            // 
            // labelControl3
            // 
            this.labelControl3.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelControl3.Appearance.Options.UseFont = true;
            this.labelControl3.Location = new System.Drawing.Point(8, 17);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(53, 21);
            this.labelControl3.TabIndex = 2;
            this.labelControl3.Text = "Periode";
            // 
            // imageCollection1
            // 
            this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
            this.imageCollection1.Images.SetKeyName(0, "boreport2_16x16.png");
            this.imageCollection1.Images.SetKeyName(1, "refreshallpivottable_16x16.png");
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.gridControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 60);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1384, 558);
            this.panelControl1.TabIndex = 5;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(987, 445, 987, 445);
            this.gridControl1.Location = new System.Drawing.Point(2, 2);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(1846, 1893, 1846, 1893);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1380, 554);
            this.gridControl1.TabIndex = 3;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Appearance.ColumnFilterButtonActive.Options.UseFont = true;
            this.gridView1.Appearance.CustomizationFormHint.Options.UseFont = true;
            this.gridView1.Appearance.DetailTip.Options.UseFont = true;
            this.gridView1.Appearance.Empty.Options.UseFont = true;
            this.gridView1.Appearance.EvenRow.Options.UseFont = true;
            this.gridView1.Appearance.FilterCloseButton.Options.UseFont = true;
            this.gridView1.Appearance.FilterPanel.Options.UseFont = true;
            this.gridView1.Appearance.FixedLine.Options.UseFont = true;
            this.gridView1.Appearance.FocusedCell.Options.UseFont = true;
            this.gridView1.Appearance.FocusedRow.Options.UseFont = true;
            this.gridView1.Appearance.FooterPanel.Options.UseFont = true;
            this.gridView1.Appearance.GroupButton.Options.UseFont = true;
            this.gridView1.Appearance.GroupFooter.Options.UseFont = true;
            this.gridView1.Appearance.GroupPanel.Options.UseFont = true;
            this.gridView1.Appearance.GroupRow.Options.UseFont = true;
            this.gridView1.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridView1.Appearance.HideSelectionRow.Options.UseFont = true;
            this.gridView1.Appearance.HorzLine.Options.UseFont = true;
            this.gridView1.Appearance.HotTrackedRow.Options.UseFont = true;
            this.gridView1.Appearance.OddRow.Options.UseFont = true;
            this.gridView1.Appearance.Preview.Options.UseFont = true;
            this.gridView1.Appearance.RowSeparator.Options.UseFont = true;
            this.gridView1.Appearance.SelectedRow.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.gridView1.Appearance.SelectedRow.Options.UseFont = true;
            this.gridView1.Appearance.TopNewRow.Options.UseFont = true;
            this.gridView1.Appearance.VertLine.Options.UseFont = true;
            this.gridView1.Appearance.ViewCaption.Options.UseFont = true;
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.ID,
            this.KODE,
            this.NAMA,
            this.GRP,
            this.LVL,
            this.INDUK,
            this.GD,
            this.POSISI,
            this.ISAKTIF,
            this.AWALTAHUN,
            this.SALDOAWAL,
            this.DEBET,
            this.KREDIT,
            this.MUTASI,
            this.SALDOAKHIR,
            this.DIVISI,
            this.BLOK,
            this.TAHUNTANAM});
            this.gridView1.DetailHeight = 20000;
            gridFormatRule1.Name = "Format0";
            gridFormatRule1.Rule = null;
            gridFormatRule2.Column = this.SALDOAKHIR;
            gridFormatRule2.Name = "Minus_Sakhir";
            formatConditionRuleValue1.Condition = DevExpress.XtraEditors.FormatCondition.Less;
            formatConditionRuleValue1.PredefinedName = "Red Bold Text";
            formatConditionRuleValue1.Value1 = 0D;
            gridFormatRule2.Rule = formatConditionRuleValue1;
            gridFormatRule3.Column = this.SALDOAWAL;
            gridFormatRule3.Name = "Minus_Sawal";
            formatConditionRuleValue2.Condition = DevExpress.XtraEditors.FormatCondition.Less;
            formatConditionRuleValue2.PredefinedName = "Red Bold Text";
            formatConditionRuleValue2.Value1 = 0D;
            gridFormatRule3.Rule = formatConditionRuleValue2;
            gridFormatRule4.Column = this.MUTASI;
            gridFormatRule4.Name = "Minus_Mutasi";
            formatConditionRuleValue3.Condition = DevExpress.XtraEditors.FormatCondition.Less;
            formatConditionRuleValue3.PredefinedName = "Red Bold Text";
            formatConditionRuleValue3.Value1 = "0";
            gridFormatRule4.Rule = formatConditionRuleValue3;
            gridFormatRule5.ApplyToRow = true;
            gridFormatRule5.Column = this.ISAKTIF;
            gridFormatRule5.Name = "akunnonaktif";
            formatConditionRuleValue4.Appearance.BackColor = System.Drawing.Color.Red;
            formatConditionRuleValue4.Appearance.Options.UseBackColor = true;
            formatConditionRuleValue4.Condition = DevExpress.XtraEditors.FormatCondition.Equal;
            formatConditionRuleValue4.Value1 = 'T';
            gridFormatRule5.Rule = formatConditionRuleValue4;
            this.gridView1.FormatRules.Add(gridFormatRule1);
            this.gridView1.FormatRules.Add(gridFormatRule2);
            this.gridView1.FormatRules.Add(gridFormatRule3);
            this.gridView1.FormatRules.Add(gridFormatRule4);
            this.gridView1.FormatRules.Add(gridFormatRule5);
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsClipboard.CopyColumnHeaders = DevExpress.Utils.DefaultBoolean.False;
            this.gridView1.OptionsFind.ShowFindButton = false;
            this.gridView1.OptionsView.FilterCriteriaDisplayStyle = DevExpress.XtraEditors.FilterCriteriaDisplayStyle.Visual;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.KODE, DevExpress.Data.ColumnSortOrder.Ascending),
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.ID, DevExpress.Data.ColumnSortOrder.Ascending)});
            this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView1_PopupMenuShowing);
            this.gridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridView1_KeyDown);
            // 
            // ID
            // 
            this.ID.Caption = "ID";
            this.ID.FieldName = "ID";
            this.ID.MinWidth = 889;
            this.ID.Name = "ID";
            this.ID.Width = 6453;
            // 
            // KODE
            // 
            this.KODE.Caption = "Account";
            this.KODE.FieldName = "KODEACC";
            this.KODE.MinWidth = 16;
            this.KODE.Name = "KODE";
            this.KODE.OptionsColumn.FixedWidth = true;
            this.KODE.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.BeginsWith;
            this.KODE.Visible = true;
            this.KODE.VisibleIndex = 0;
            this.KODE.Width = 110;
            // 
            // NAMA
            // 
            this.NAMA.Caption = "Nama Perkiraan";
            this.NAMA.FieldName = "NAMAACC";
            this.NAMA.MinWidth = 22;
            this.NAMA.Name = "NAMA";
            this.NAMA.OptionsFilter.AutoFilterCondition = DevExpress.XtraGrid.Columns.AutoFilterCondition.Contains;
            this.NAMA.Visible = true;
            this.NAMA.VisibleIndex = 1;
            this.NAMA.Width = 267;
            // 
            // GRP
            // 
            this.GRP.Caption = "Jenis";
            this.GRP.FieldName = "GRP";
            this.GRP.MinWidth = 10;
            this.GRP.Name = "GRP";
            this.GRP.Visible = true;
            this.GRP.VisibleIndex = 2;
            this.GRP.Width = 42;
            // 
            // LVL
            // 
            this.LVL.Caption = "Level";
            this.LVL.FieldName = "LVL";
            this.LVL.MinWidth = 45;
            this.LVL.Name = "LVL";
            this.LVL.Visible = true;
            this.LVL.VisibleIndex = 3;
            this.LVL.Width = 45;
            // 
            // INDUK
            // 
            this.INDUK.Caption = "Induk";
            this.INDUK.FieldName = "INDUK";
            this.INDUK.MinWidth = 16;
            this.INDUK.Name = "INDUK";
            this.INDUK.Visible = true;
            this.INDUK.VisibleIndex = 4;
            this.INDUK.Width = 82;
            // 
            // GD
            // 
            this.GD.Caption = "Gen";
            this.GD.FieldName = "GD";
            this.GD.MinWidth = 11;
            this.GD.Name = "GD";
            this.GD.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.GD.Visible = true;
            this.GD.VisibleIndex = 5;
            this.GD.Width = 29;
            // 
            // POSISI
            // 
            this.POSISI.Caption = "Saldo Normal";
            this.POSISI.FieldName = "POSISI";
            this.POSISI.MinWidth = 11;
            this.POSISI.Name = "POSISI";
            this.POSISI.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.POSISI.Visible = true;
            this.POSISI.VisibleIndex = 6;
            this.POSISI.Width = 29;
            // 
            // AWALTAHUN
            // 
            this.AWALTAHUN.Caption = "Awal Tahun";
            this.AWALTAHUN.DisplayFormat.FormatString = "n2";
            this.AWALTAHUN.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.AWALTAHUN.FieldName = "AWALTAHUN";
            this.AWALTAHUN.MinWidth = 18;
            this.AWALTAHUN.Name = "AWALTAHUN";
            this.AWALTAHUN.Width = 18;
            // 
            // DEBET
            // 
            this.DEBET.Caption = "Debet";
            this.DEBET.DisplayFormat.FormatString = "{0:n2}";
            this.DEBET.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.DEBET.FieldName = "DEBET";
            this.DEBET.MinWidth = 11;
            this.DEBET.Name = "DEBET";
            this.DEBET.Visible = true;
            this.DEBET.VisibleIndex = 8;
            this.DEBET.Width = 128;
            // 
            // KREDIT
            // 
            this.KREDIT.Caption = "Kredit";
            this.KREDIT.DisplayFormat.FormatString = "{0:n2}";
            this.KREDIT.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.KREDIT.FieldName = "KREDIT";
            this.KREDIT.MinWidth = 16;
            this.KREDIT.Name = "KREDIT";
            this.KREDIT.Visible = true;
            this.KREDIT.VisibleIndex = 9;
            this.KREDIT.Width = 128;
            // 
            // DIVISI
            // 
            this.DIVISI.Caption = "Divisi";
            this.DIVISI.FieldName = "DIVISI";
            this.DIVISI.MinWidth = 18;
            this.DIVISI.Name = "DIVISI";
            this.DIVISI.Width = 67;
            // 
            // BLOK
            // 
            this.BLOK.Caption = "Blok";
            this.BLOK.FieldName = "BLOK";
            this.BLOK.MinWidth = 18;
            this.BLOK.Name = "BLOK";
            this.BLOK.Width = 67;
            // 
            // TAHUNTANAM
            // 
            this.TAHUNTANAM.Caption = "TahunTanam";
            this.TAHUNTANAM.FieldName = "TAHUNTANAM";
            this.TAHUNTANAM.MinWidth = 18;
            this.TAHUNTANAM.Name = "TAHUNTANAM";
            this.TAHUNTANAM.Width = 67;
            // 
            // sbexpadvanced
            // 
            this.sbexpadvanced.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sbexpadvanced.Appearance.Options.UseFont = true;
            this.sbexpadvanced.ImageOptions.Image = global::Accounting.Properties.Resources.exporttoxlsx_16x16;
            this.sbexpadvanced.Location = new System.Drawing.Point(577, 9);
            this.sbexpadvanced.Margin = new System.Windows.Forms.Padding(1208, 1338, 1208, 1338);
            this.sbexpadvanced.Name = "sbexpadvanced";
            this.sbexpadvanced.Size = new System.Drawing.Size(124, 31);
            this.sbexpadvanced.TabIndex = 2;
            this.sbexpadvanced.Text = "Exp. Advance";
            this.sbexpadvanced.Click += new System.EventHandler(this.sbexport_Click);
            // 
            // FrmAkunEF
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1384, 618);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.sidePanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Glow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmAkunEF";
            this.Text = "Chart Of Account";
            this.Load += new System.EventHandler(this.FrmAkunEF_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdit1.Properties)).EndInit();
            this.sidePanel1.ResumeLayout(false);
            this.sidePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cedetail.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cegroup.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cetbm.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cetm.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CEMUTASI.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NilaiSaldo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AkunLabaRugi.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AkunNeraca.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SimpleButton sbadd;
        private DevExpress.XtraEditors.SimpleButton sbubah;
        private DevExpress.XtraEditors.SimpleButton sbhapus;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LookUpEdit lookUpEdit1;
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.SimpleButton sbexport;
        private DevExpress.XtraEditors.CheckEdit AkunLabaRugi;
        private DevExpress.XtraEditors.CheckEdit AkunNeraca;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraEditors.CheckEdit NilaiSaldo;
        private DevExpress.XtraEditors.CheckEdit cetbm;
        private DevExpress.XtraEditors.CheckEdit cetm;
        private DevExpress.XtraEditors.CheckEdit CEMUTASI;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn ID;
        private DevExpress.XtraGrid.Columns.GridColumn KODE;
        private DevExpress.XtraGrid.Columns.GridColumn INDUK;
        private DevExpress.XtraGrid.Columns.GridColumn NAMA;
        private DevExpress.XtraGrid.Columns.GridColumn GRP;
        private DevExpress.XtraGrid.Columns.GridColumn LVL;
        private DevExpress.XtraGrid.Columns.GridColumn GD;
        private DevExpress.XtraGrid.Columns.GridColumn POSISI;
        private DevExpress.XtraGrid.Columns.GridColumn ISAKTIF;
        private DevExpress.XtraGrid.Columns.GridColumn AWALTAHUN;
        private DevExpress.XtraGrid.Columns.GridColumn SALDOAWAL;
        private DevExpress.XtraGrid.Columns.GridColumn DEBET;
        private DevExpress.XtraGrid.Columns.GridColumn KREDIT;
        private DevExpress.XtraGrid.Columns.GridColumn MUTASI;
        private DevExpress.XtraGrid.Columns.GridColumn SALDOAKHIR;
        private DevExpress.XtraGrid.Columns.GridColumn DIVISI;
        private DevExpress.XtraGrid.Columns.GridColumn BLOK;
        private DevExpress.XtraGrid.Columns.GridColumn TAHUNTANAM;
        private DevExpress.XtraEditors.CheckEdit cegroup;
        private DevExpress.XtraEditors.CheckEdit cedetail;
        private DevExpress.XtraEditors.SimpleButton sbexpadvanced;
    }
}