
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
            btnexport = new DevExpress.XtraEditors.SimpleButton();
            setahun = new DevExpress.XtraEditors.SpinEdit();
            cmbbulandari = new DevExpress.XtraEditors.ComboBoxEdit();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            NoJurnal = new DevExpress.XtraGrid.Columns.GridColumn();
            Tanggal = new DevExpress.XtraGrid.Columns.GridColumn();
            Kode = new DevExpress.XtraGrid.Columns.GridColumn();
            Rekening = new DevExpress.XtraGrid.Columns.GridColumn();
            Debet = new DevExpress.XtraGrid.Columns.GridColumn();
            Kredit = new DevExpress.XtraGrid.Columns.GridColumn();
            Keterangan = new DevExpress.XtraGrid.Columns.GridColumn();
            Posted = new DevExpress.XtraGrid.Columns.GridColumn();
            Periode = new DevExpress.XtraGrid.Columns.GridColumn();
            DID = new DevExpress.XtraGrid.Columns.GridColumn();
            RowNo = new DevExpress.XtraGrid.Columns.GridColumn();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            cmbbulansampai = new DevExpress.XtraEditors.ComboBoxEdit();
            ((System.ComponentModel.ISupportInitialize)setahun.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulandari.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulansampai.Properties).BeginInit();
            SuspendLayout();
            // 
            // btnexport
            // 
            btnexport.ImageOptions.Image = Properties.Resources.exporttoxls_16x16;
            btnexport.Location = new System.Drawing.Point(369, 2);
            btnexport.Margin = new System.Windows.Forms.Padding(2);
            btnexport.Name = "btnexport";
            btnexport.Size = new System.Drawing.Size(75, 21);
            btnexport.TabIndex = 10;
            btnexport.Text = "Export";
            btnexport.Click += btnexport_Click;
            // 
            // setahun
            // 
            setahun.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            setahun.Location = new System.Drawing.Point(310, 3);
            setahun.Margin = new System.Windows.Forms.Padding(2);
            setahun.Name = "setahun";
            setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            setahun.Properties.DisplayFormat.FormatString = "d";
            setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun.Properties.EditFormat.FormatString = "d";
            setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun.Properties.MaskSettings.Set("mask", "d");
            setahun.Size = new System.Drawing.Size(55, 20);
            setahun.TabIndex = 9;
            setahun.EditValueChanged += setahun_EditValueChanged;
            // 
            // cmbbulandari
            // 
            cmbbulandari.Location = new System.Drawing.Point(57, 3);
            cmbbulandari.Margin = new System.Windows.Forms.Padding(2);
            cmbbulandari.Name = "cmbbulandari";
            cmbbulandari.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbbulandari.Properties.ImmediatePopup = true;
            cmbbulandari.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbbulandari.Size = new System.Drawing.Size(100, 20);
            cmbbulandari.TabIndex = 8;
            cmbbulandari.SelectedIndexChanged += cmbbulan_SelectedIndexChanged;
            // 
            // labelControl3
            // 
            labelControl3.Location = new System.Drawing.Point(10, 10);
            labelControl3.Margin = new System.Windows.Forms.Padding(2);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new System.Drawing.Size(36, 13);
            labelControl3.TabIndex = 7;
            labelControl3.Text = "Periode";
            // 
            // gridControl1
            // 
            gridControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            gridControl1.Location = new System.Drawing.Point(10, 29);
            gridControl1.MainView = gridView1;
            gridControl1.Margin = new System.Windows.Forms.Padding(2);
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new System.Drawing.Size(803, 363);
            gridControl1.TabIndex = 11;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { NoJurnal, Tanggal, Kode, Rekening, Debet, Kredit, Keterangan, Posted, Periode, DID });
            gridView1.DetailHeight = 217;
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsFind.AlwaysVisible = true;
            gridView1.OptionsFind.ShowFindButton = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] { new DevExpress.XtraGrid.Columns.GridColumnSortInfo(NoJurnal, DevExpress.Data.ColumnSortOrder.Ascending), new DevExpress.XtraGrid.Columns.GridColumnSortInfo(Tanggal, DevExpress.Data.ColumnSortOrder.Ascending) });
            // 
            // NoJurnal
            // 
            NoJurnal.Caption = "NoJurnal";
            NoJurnal.FieldName = "NoJurnal";
            NoJurnal.Name = "NoJurnal";
            NoJurnal.OptionsColumn.FixedWidth = true;
            NoJurnal.Visible = true;
            NoJurnal.VisibleIndex = 0;
            NoJurnal.Width = 80;
            // 
            // Tanggal
            // 
            Tanggal.Caption = "Tanggal";
            Tanggal.FieldName = "Tanggal";
            Tanggal.Name = "Tanggal";
            Tanggal.OptionsColumn.FixedWidth = true;
            Tanggal.Visible = true;
            Tanggal.VisibleIndex = 1;
            Tanggal.Width = 67;
            // 
            // Kode
            // 
            Kode.Caption = "Kode";
            Kode.FieldName = "Kode";
            Kode.Name = "Kode";
            Kode.OptionsColumn.FixedWidth = true;
            Kode.Visible = true;
            Kode.VisibleIndex = 2;
            Kode.Width = 120;
            // 
            // Rekening
            // 
            Rekening.Caption = "Rekening";
            Rekening.FieldName = "Rekening";
            Rekening.Name = "Rekening";
            Rekening.Visible = true;
            Rekening.VisibleIndex = 3;
            // 
            // Debet
            // 
            Debet.Caption = "Debet";
            Debet.DisplayFormat.FormatString = "{0:n2}";
            Debet.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            Debet.FieldName = "Debet";
            Debet.Name = "Debet";
            Debet.OptionsColumn.FixedWidth = true;
            Debet.Visible = true;
            Debet.VisibleIndex = 4;
            Debet.Width = 87;
            // 
            // Kredit
            // 
            Kredit.Caption = "Kredit";
            Kredit.DisplayFormat.FormatString = "{0:n2}";
            Kredit.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            Kredit.FieldName = "Kredit";
            Kredit.Name = "Kredit";
            Kredit.OptionsColumn.FixedWidth = true;
            Kredit.Visible = true;
            Kredit.VisibleIndex = 5;
            Kredit.Width = 87;
            // 
            // Keterangan
            // 
            Keterangan.Caption = "Keterangan";
            Keterangan.FieldName = "Keterangan";
            Keterangan.Name = "Keterangan";
            Keterangan.Visible = true;
            Keterangan.VisibleIndex = 6;
            // 
            // Posted
            // 
            Posted.Caption = "Posted";
            Posted.FieldName = "Posted";
            Posted.Name = "Posted";
            Posted.OptionsColumn.FixedWidth = true;
            Posted.Visible = true;
            Posted.VisibleIndex = 7;
            Posted.Width = 40;
            // 
            // Periode
            // 
            Periode.Caption = "Periode";
            Periode.FieldName = "Periode";
            Periode.Name = "Periode";
            Periode.OptionsColumn.FixedWidth = true;
            Periode.Visible = true;
            Periode.VisibleIndex = 8;
            Periode.Width = 47;
            // 
            // DID
            // 
            DID.Caption = "DID";
            DID.FieldName = "DID";
            DID.Name = "DID";
            // 
            // RowNo
            // 
            RowNo.Name = "RowNo";
            // 
            // labelControl1
            // 
            labelControl1.Location = new System.Drawing.Point(165, 10);
            labelControl1.Margin = new System.Windows.Forms.Padding(2);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new System.Drawing.Size(34, 13);
            labelControl1.TabIndex = 7;
            labelControl1.Text = "Sampai";
            // 
            // cmbbulansampai
            // 
            cmbbulansampai.Location = new System.Drawing.Point(203, 3);
            cmbbulansampai.Margin = new System.Windows.Forms.Padding(2);
            cmbbulansampai.Name = "cmbbulansampai";
            cmbbulansampai.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbbulansampai.Properties.ImmediatePopup = true;
            cmbbulansampai.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            cmbbulansampai.Size = new System.Drawing.Size(100, 20);
            cmbbulansampai.TabIndex = 8;
            cmbbulansampai.SelectedIndexChanged += cmbbulan_SelectedIndexChanged;
            // 
            // FrmExportJurnal
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(821, 400);
            Controls.Add(gridControl1);
            Controls.Add(btnexport);
            Controls.Add(setahun);
            Controls.Add(cmbbulansampai);
            Controls.Add(labelControl1);
            Controls.Add(cmbbulandari);
            Controls.Add(labelControl3);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "FrmExportJurnal";
            Text = "Export Jurnal";
            Load += FrmExportJurnal_Load;
            ((System.ComponentModel.ISupportInitialize)setahun.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulandari.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulansampai.Properties).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnexport;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulandari;
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
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulansampai;
    }
}