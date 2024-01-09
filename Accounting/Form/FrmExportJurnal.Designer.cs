
namespace Accounting.Form
{
    partial class FrmExportJurnal
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
            this.btnexport = new DevExpress.XtraEditors.SimpleButton();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.NoJurnal = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Tanggal = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Kode = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Rekening = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Debet = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Kredit = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Keterangan = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Posted = new DevExpress.XtraGrid.Columns.GridColumn();
            this.Periode = new DevExpress.XtraGrid.Columns.GridColumn();
            this.DID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.RowNo = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnexport
            // 
            this.btnexport.ImageOptions.Image = global::Accounting.Properties.Resources.exporttoxls_16x16;
            this.btnexport.Location = new System.Drawing.Point(228, 5);
            this.btnexport.Margin = new System.Windows.Forms.Padding(2);
            this.btnexport.Name = "btnexport";
            this.btnexport.Size = new System.Drawing.Size(75, 21);
            this.btnexport.TabIndex = 10;
            this.btnexport.Text = "Export";
            this.btnexport.Click += new System.EventHandler(this.btnexport_Click);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(169, 8);
            this.setahun.Margin = new System.Windows.Forms.Padding(2);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(55, 20);
            this.setahun.TabIndex = 9;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(57, 8);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(2);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(100, 20);
            this.cmbbulan.TabIndex = 8;
            this.cmbbulan.SelectedIndexChanged += new System.EventHandler(this.cmbbulan_SelectedIndexChanged);
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(10, 12);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(36, 13);
            this.labelControl3.TabIndex = 7;
            this.labelControl3.Text = "Periode";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Location = new System.Drawing.Point(10, 29);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(803, 363);
            this.gridControl1.TabIndex = 11;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.NoJurnal,
            this.Tanggal,
            this.Kode,
            this.Rekening,
            this.Debet,
            this.Kredit,
            this.Keterangan,
            this.Posted,
            this.Periode,
            this.DID});
            this.gridView1.DetailHeight = 217;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsFind.AlwaysVisible = true;
            this.gridView1.OptionsFind.ShowFindButton = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.NoJurnal, DevExpress.Data.ColumnSortOrder.Ascending),
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.Tanggal, DevExpress.Data.ColumnSortOrder.Ascending)});
            // 
            // NoJurnal
            // 
            this.NoJurnal.Caption = "NoJurnal";
            this.NoJurnal.FieldName = "NoJurnal";
            this.NoJurnal.Name = "NoJurnal";
            this.NoJurnal.OptionsColumn.FixedWidth = true;
            this.NoJurnal.Visible = true;
            this.NoJurnal.VisibleIndex = 0;
            this.NoJurnal.Width = 80;
            // 
            // Tanggal
            // 
            this.Tanggal.Caption = "Tanggal";
            this.Tanggal.FieldName = "Tanggal";
            this.Tanggal.Name = "Tanggal";
            this.Tanggal.OptionsColumn.FixedWidth = true;
            this.Tanggal.Visible = true;
            this.Tanggal.VisibleIndex = 1;
            this.Tanggal.Width = 67;
            // 
            // Kode
            // 
            this.Kode.Caption = "Kode";
            this.Kode.FieldName = "Kode";
            this.Kode.Name = "Kode";
            this.Kode.OptionsColumn.FixedWidth = true;
            this.Kode.Visible = true;
            this.Kode.VisibleIndex = 2;
            this.Kode.Width = 80;
            // 
            // Rekening
            // 
            this.Rekening.Caption = "Rekening";
            this.Rekening.FieldName = "Rekening";
            this.Rekening.Name = "Rekening";
            this.Rekening.Visible = true;
            this.Rekening.VisibleIndex = 3;
            // 
            // Debet
            // 
            this.Debet.Caption = "Debet";
            this.Debet.DisplayFormat.FormatString = "{0:n2}";
            this.Debet.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.Debet.FieldName = "Debet";
            this.Debet.Name = "Debet";
            this.Debet.OptionsColumn.FixedWidth = true;
            this.Debet.Visible = true;
            this.Debet.VisibleIndex = 4;
            this.Debet.Width = 87;
            // 
            // Kredit
            // 
            this.Kredit.Caption = "Kredit";
            this.Kredit.DisplayFormat.FormatString = "{0:n2}";
            this.Kredit.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.Kredit.FieldName = "Kredit";
            this.Kredit.Name = "Kredit";
            this.Kredit.OptionsColumn.FixedWidth = true;
            this.Kredit.Visible = true;
            this.Kredit.VisibleIndex = 5;
            this.Kredit.Width = 87;
            // 
            // Keterangan
            // 
            this.Keterangan.Caption = "Keterangan";
            this.Keterangan.FieldName = "Keterangan";
            this.Keterangan.Name = "Keterangan";
            this.Keterangan.Visible = true;
            this.Keterangan.VisibleIndex = 6;
            // 
            // Posted
            // 
            this.Posted.Caption = "Posted";
            this.Posted.FieldName = "Posted";
            this.Posted.Name = "Posted";
            this.Posted.OptionsColumn.FixedWidth = true;
            this.Posted.Visible = true;
            this.Posted.VisibleIndex = 7;
            this.Posted.Width = 40;
            // 
            // Periode
            // 
            this.Periode.Caption = "Periode";
            this.Periode.FieldName = "Periode";
            this.Periode.Name = "Periode";
            this.Periode.OptionsColumn.FixedWidth = true;
            this.Periode.Visible = true;
            this.Periode.VisibleIndex = 8;
            this.Periode.Width = 47;
            // 
            // DID
            // 
            this.DID.Caption = "DID";
            this.DID.FieldName = "DID";
            this.DID.Name = "DID";
            // 
            // RowNo
            // 
            this.RowNo.Name = "RowNo";
            // 
            // FrmExportJurnal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 400);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.btnexport);
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.cmbbulan);
            this.Controls.Add(this.labelControl3);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmExportJurnal";
            this.Text = "Export Jurnal";
            this.Load += new System.EventHandler(this.FrmExportJurnal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnexport;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn NoJurnal;
        private DevExpress.XtraGrid.Columns.GridColumn Tanggal;
        private DevExpress.XtraGrid.Columns.GridColumn RowNo;
        private DevExpress.XtraGrid.Columns.GridColumn Kode;
        private DevExpress.XtraGrid.Columns.GridColumn Rekening;
        private DevExpress.XtraGrid.Columns.GridColumn Debet;
        private DevExpress.XtraGrid.Columns.GridColumn Kredit;
        private DevExpress.XtraGrid.Columns.GridColumn Keterangan;
        private DevExpress.XtraGrid.Columns.GridColumn Posted;
        private DevExpress.XtraGrid.Columns.GridColumn Periode;
        private DevExpress.XtraGrid.Columns.GridColumn DID;
    }
}