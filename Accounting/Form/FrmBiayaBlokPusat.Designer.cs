
namespace Accounting.Form
{
    partial class FrmBiayaBlokPusat
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
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding1 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding2 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding3 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding4 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding5 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding6 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding7 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding8 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            DevExpress.XtraPivotGrid.DataSourceColumnBinding dataSourceColumnBinding9 = new DevExpress.XtraPivotGrid.DataSourceColumnBinding();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBiayaBlokPusat));
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.KAT = new DevExpress.XtraPivotGrid.PivotGridField();
            this.PEKERJAAN = new DevExpress.XtraPivotGrid.PivotGridField();
            this.LOKASI = new DevExpress.XtraPivotGrid.PivotGridField();
            this.TAHUN = new DevExpress.XtraPivotGrid.PivotGridField();
            this.BULAN = new DevExpress.XtraPivotGrid.PivotGridField();
            this.pivotGridField1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.ISSTATUS = new DevExpress.XtraPivotGrid.PivotGridField();
            this.ISDIVISI = new DevExpress.XtraPivotGrid.PivotGridField();
            this.ISBLOK = new DevExpress.XtraPivotGrid.PivotGridField();
            this.sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            this.sbexport = new DevExpress.XtraEditors.SimpleButton();
            this.btnproses = new DevExpress.XtraEditors.SimpleButton();
            this.setahun2 = new DevExpress.XtraEditors.SpinEdit();
            this.setahun1 = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            this.sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Fields.AddRange(new DevExpress.XtraPivotGrid.PivotGridField[] {
            this.KAT,
            this.PEKERJAAN,
            this.LOKASI,
            this.TAHUN,
            this.BULAN,
            this.pivotGridField1,
            this.ISSTATUS,
            this.ISDIVISI,
            this.ISBLOK});
            this.pivotGridControl1.Location = new System.Drawing.Point(2, 2);
            this.pivotGridControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.OptionsData.DataProcessingEngine = DevExpress.XtraPivotGrid.PivotDataProcessingEngine.Optimized;
            this.pivotGridControl1.OptionsDataField.RowHeaderWidth = 67;
            this.pivotGridControl1.OptionsView.RowTreeOffset = 14;
            this.pivotGridControl1.OptionsView.RowTreeWidth = 67;
            this.pivotGridControl1.Size = new System.Drawing.Size(823, 384);
            this.pivotGridControl1.TabIndex = 0;
            // 
            // KAT
            // 
            this.KAT.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.KAT.AreaIndex = 2;
            dataSourceColumnBinding1.ColumnName = "KATEGORI";
            this.KAT.DataBinding = dataSourceColumnBinding1;
            this.KAT.MinWidth = 13;
            this.KAT.Name = "KAT";
            this.KAT.Width = 67;
            // 
            // PEKERJAAN
            // 
            this.PEKERJAAN.AreaIndex = 0;
            dataSourceColumnBinding2.ColumnName = "PERKIRAAN";
            this.PEKERJAAN.DataBinding = dataSourceColumnBinding2;
            this.PEKERJAAN.MinWidth = 13;
            this.PEKERJAAN.Name = "PEKERJAAN";
            this.PEKERJAAN.Width = 67;
            // 
            // LOKASI
            // 
            this.LOKASI.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.LOKASI.AreaIndex = 0;
            dataSourceColumnBinding3.ColumnName = "IDDATA";
            this.LOKASI.DataBinding = dataSourceColumnBinding3;
            this.LOKASI.MinWidth = 13;
            this.LOKASI.Name = "LOKASI";
            this.LOKASI.Width = 67;
            // 
            // TAHUN
            // 
            this.TAHUN.Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
            this.TAHUN.AreaIndex = 0;
            dataSourceColumnBinding4.ColumnName = "GLYEAR";
            this.TAHUN.DataBinding = dataSourceColumnBinding4;
            this.TAHUN.MinWidth = 13;
            this.TAHUN.Name = "TAHUN";
            this.TAHUN.Width = 67;
            // 
            // BULAN
            // 
            this.BULAN.Area = DevExpress.XtraPivotGrid.PivotArea.ColumnArea;
            this.BULAN.AreaIndex = 1;
            dataSourceColumnBinding5.ColumnName = "GLMONTH";
            this.BULAN.DataBinding = dataSourceColumnBinding5;
            this.BULAN.MinWidth = 13;
            this.BULAN.Name = "BULAN";
            this.BULAN.Width = 67;
            // 
            // pivotGridField1
            // 
            this.pivotGridField1.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.pivotGridField1.AreaIndex = 0;
            this.pivotGridField1.Caption = "JUMLAH";
            this.pivotGridField1.CellFormat.FormatString = "N2";
            this.pivotGridField1.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            dataSourceColumnBinding6.ColumnName = "BULANINI";
            this.pivotGridField1.DataBinding = dataSourceColumnBinding6;
            this.pivotGridField1.MinWidth = 13;
            this.pivotGridField1.Name = "pivotGridField1";
            this.pivotGridField1.TotalCellFormat.FormatString = "N2";
            this.pivotGridField1.TotalCellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.pivotGridField1.TotalValueFormat.FormatString = "N2";
            this.pivotGridField1.TotalValueFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.pivotGridField1.ValueFormat.FormatString = "N2";
            this.pivotGridField1.ValueFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.pivotGridField1.Width = 67;
            // 
            // ISSTATUS
            // 
            this.ISSTATUS.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.ISSTATUS.AreaIndex = 1;
            this.ISSTATUS.Caption = "STATUS";
            dataSourceColumnBinding7.ColumnName = "STATUS";
            this.ISSTATUS.DataBinding = dataSourceColumnBinding7;
            this.ISSTATUS.MinWidth = 13;
            this.ISSTATUS.Name = "ISSTATUS";
            this.ISSTATUS.Width = 67;
            // 
            // ISDIVISI
            // 
            this.ISDIVISI.Area = DevExpress.XtraPivotGrid.PivotArea.RowArea;
            this.ISDIVISI.AreaIndex = 3;
            this.ISDIVISI.Caption = "DIVISI";
            dataSourceColumnBinding8.ColumnName = "DIVISI";
            this.ISDIVISI.DataBinding = dataSourceColumnBinding8;
            this.ISDIVISI.MinWidth = 13;
            this.ISDIVISI.Name = "ISDIVISI";
            this.ISDIVISI.Width = 67;
            // 
            // ISBLOK
            // 
            this.ISBLOK.AreaIndex = 1;
            this.ISBLOK.Caption = "BLOK";
            dataSourceColumnBinding9.ColumnName = "NAMABLOK";
            this.ISBLOK.DataBinding = dataSourceColumnBinding9;
            this.ISBLOK.MinWidth = 13;
            this.ISBLOK.Name = "ISBLOK";
            this.ISBLOK.Width = 67;
            // 
            // sidePanel1
            // 
            this.sidePanel1.Controls.Add(this.sbexport);
            this.sidePanel1.Controls.Add(this.btnproses);
            this.sidePanel1.Controls.Add(this.setahun2);
            this.sidePanel1.Controls.Add(this.setahun1);
            this.sidePanel1.Controls.Add(this.labelControl1);
            this.sidePanel1.Controls.Add(this.labelControl3);
            this.sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.sidePanel1.Location = new System.Drawing.Point(0, 0);
            this.sidePanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sidePanel1.Name = "sidePanel1";
            this.sidePanel1.Size = new System.Drawing.Size(827, 41);
            this.sidePanel1.TabIndex = 1;
            this.sidePanel1.Text = "sidePanel1";
            // 
            // sbexport
            // 
            this.sbexport.Enabled = false;
            this.sbexport.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("sbexport.ImageOptions.Image")));
            this.sbexport.Location = new System.Drawing.Point(339, 11);
            this.sbexport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sbexport.Name = "sbexport";
            this.sbexport.Size = new System.Drawing.Size(77, 20);
            this.sbexport.TabIndex = 6;
            this.sbexport.Text = "Export";
            this.sbexport.Click += new System.EventHandler(this.sbexport_Click);
            // 
            // btnproses
            // 
            this.btnproses.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnproses.ImageOptions.Image")));
            this.btnproses.Location = new System.Drawing.Point(258, 11);
            this.btnproses.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnproses.Name = "btnproses";
            this.btnproses.Size = new System.Drawing.Size(77, 20);
            this.btnproses.TabIndex = 6;
            this.btnproses.Text = "Proses";
            this.btnproses.Click += new System.EventHandler(this.btnproses_Click);
            // 
            // setahun2
            // 
            this.setahun2.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun2.Location = new System.Drawing.Point(164, 11);
            this.setahun2.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            this.setahun2.Name = "setahun2";
            this.setahun2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun2.Properties.DisplayFormat.FormatString = "d";
            this.setahun2.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun2.Properties.EditFormat.FormatString = "d";
            this.setahun2.Properties.MaskSettings.Set("mask", "d");
            this.setahun2.Size = new System.Drawing.Size(73, 20);
            this.setahun2.TabIndex = 5;
            // 
            // setahun1
            // 
            this.setahun1.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun1.Location = new System.Drawing.Point(37, 11);
            this.setahun1.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            this.setahun1.Name = "setahun1";
            this.setahun1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun1.Properties.DisplayFormat.FormatString = "d";
            this.setahun1.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun1.Properties.EditFormat.FormatString = "d";
            this.setahun1.Properties.MaskSettings.Set("mask", "d");
            this.setahun1.Size = new System.Drawing.Size(73, 20);
            this.setahun1.TabIndex = 5;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(123, 18);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(34, 13);
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "Sampai";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(11, 18);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(19, 13);
            this.labelControl3.TabIndex = 4;
            this.labelControl3.Text = "Dari";
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.pivotGridControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 41);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(827, 388);
            this.panelControl1.TabIndex = 2;
            // 
            // FrmBiayaBlokPusat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 429);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.sidePanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FrmBiayaBlokPusat";
            this.Text = "Analisa Biaya";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmBiayaBlokPusat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            this.sidePanel1.ResumeLayout(false);
            this.sidePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraPivotGrid.PivotGridField KAT;
        private DevExpress.XtraPivotGrid.PivotGridField PEKERJAAN;
        private DevExpress.XtraPivotGrid.PivotGridField LOKASI;
        private DevExpress.XtraPivotGrid.PivotGridField TAHUN;
        private DevExpress.XtraPivotGrid.PivotGridField BULAN;
        private DevExpress.XtraEditors.SimpleButton btnproses;
        private DevExpress.XtraEditors.SpinEdit setahun2;
        private DevExpress.XtraEditors.SpinEdit setahun1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton sbexport;
        private DevExpress.XtraPivotGrid.PivotGridField pivotGridField1;
        private DevExpress.XtraPivotGrid.PivotGridField ISSTATUS;
        private DevExpress.XtraPivotGrid.PivotGridField ISDIVISI;
        private DevExpress.XtraPivotGrid.PivotGridField ISBLOK;
    }
}