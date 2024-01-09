
namespace Accounting.Form
{
    partial class FrmMasterDivisi
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
            this.btnsimpan = new DevExpress.XtraEditors.SimpleButton();
            this.btnhapus = new DevExpress.XtraEditors.SimpleButton();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.ESTATEID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AKTIF = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.btnbaru = new DevExpress.XtraEditors.SimpleButton();
            this.txtnamadiv = new DevExpress.XtraEditors.TextEdit();
            this.txtkodediv = new DevExpress.XtraEditors.TextEdit();
            this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamadiv.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkodediv.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // nAMALabel
            // 
            this.nAMALabel.AutoSize = true;
            this.nAMALabel.Location = new System.Drawing.Point(5, 44);
            this.nAMALabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nAMALabel.Name = "nAMALabel";
            this.nAMALabel.Size = new System.Drawing.Size(70, 13);
            this.nAMALabel.TabIndex = 38;
            this.nAMALabel.Text = "NAMA DIVISI";
            // 
            // uSERIDLabel
            // 
            this.uSERIDLabel.AutoSize = true;
            this.uSERIDLabel.Location = new System.Drawing.Point(5, 24);
            this.uSERIDLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.uSERIDLabel.Name = "uSERIDLabel";
            this.uSERIDLabel.Size = new System.Drawing.Size(68, 13);
            this.uSERIDLabel.TabIndex = 39;
            this.uSERIDLabel.Text = "KODE DIVISI";
            // 
            // btnsimpan
            // 
            this.btnsimpan.Location = new System.Drawing.Point(77, 63);
            this.btnsimpan.Margin = new System.Windows.Forms.Padding(2);
            this.btnsimpan.Name = "btnsimpan";
            this.btnsimpan.Size = new System.Drawing.Size(64, 23);
            this.btnsimpan.TabIndex = 0;
            this.btnsimpan.Text = "Simpan";
            this.btnsimpan.Click += new System.EventHandler(this.btnsimpan_Click);
            // 
            // btnhapus
            // 
            this.btnhapus.Enabled = false;
            this.btnhapus.Location = new System.Drawing.Point(145, 63);
            this.btnhapus.Margin = new System.Windows.Forms.Padding(2);
            this.btnhapus.Name = "btnhapus";
            this.btnhapus.Size = new System.Drawing.Size(64, 23);
            this.btnhapus.TabIndex = 1;
            this.btnhapus.Text = "Hapus";
            this.btnhapus.Click += new System.EventHandler(this.btnhapus_Click);
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Location = new System.Drawing.Point(229, 8);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemToggleSwitch1});
            this.gridControl1.Size = new System.Drawing.Size(760, 458);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.ESTATEID,
            this.gridColumn2,
            this.gridColumn3,
            this.gridColumn4,
            this.gridColumn5,
            this.AKTIF,
            this.gridColumn6,
            this.gridColumn7});
            this.gridView1.DetailHeight = 217;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.GroupCount = 1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsView.ShowFooter = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.ESTATEID, DevExpress.Data.ColumnSortOrder.Ascending)});
            this.gridView1.DoubleClick += new System.EventHandler(this.gridView1_DoubleClick);
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "DIVISIID";
            this.gridColumn1.FieldName = "DIVISIID";
            this.gridColumn1.MinWidth = 13;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Width = 50;
            // 
            // ESTATEID
            // 
            this.ESTATEID.Caption = "ESTATE";
            this.ESTATEID.FieldName = "ESTATEID";
            this.ESTATEID.Name = "ESTATEID";
            this.ESTATEID.Visible = true;
            this.ESTATEID.VisibleIndex = 4;
            this.ESTATEID.Width = 50;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "KODE";
            this.gridColumn2.FieldName = "KODE";
            this.gridColumn2.MinWidth = 13;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 0;
            this.gridColumn2.Width = 50;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "DIVISI";
            this.gridColumn3.FieldName = "DIVISI";
            this.gridColumn3.MinWidth = 13;
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 1;
            this.gridColumn3.Width = 50;
            // 
            // gridColumn4
            // 
            this.gridColumn4.Caption = "LUAS_TBM";
            this.gridColumn4.FieldName = "LUASTBM";
            this.gridColumn4.MinWidth = 13;
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "LUASTBM", "{0:N2}")});
            this.gridColumn4.Visible = true;
            this.gridColumn4.VisibleIndex = 2;
            this.gridColumn4.Width = 50;
            // 
            // gridColumn5
            // 
            this.gridColumn5.Caption = "LUAS_TM";
            this.gridColumn5.FieldName = "LUASTM";
            this.gridColumn5.MinWidth = 13;
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.Summary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
            new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Sum, "LUASTM", "{0:N2}")});
            this.gridColumn5.Visible = true;
            this.gridColumn5.VisibleIndex = 3;
            this.gridColumn5.Width = 50;
            // 
            // AKTIF
            // 
            this.AKTIF.Caption = "AKTIF";
            this.AKTIF.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.AKTIF.FieldName = "AKTIF";
            this.AKTIF.Name = "AKTIF";
            this.AKTIF.Visible = true;
            this.AKTIF.VisibleIndex = 4;
            this.AKTIF.Width = 50;
            // 
            // repositoryItemToggleSwitch1
            // 
            this.repositoryItemToggleSwitch1.AutoHeight = false;
            this.repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
            this.repositoryItemToggleSwitch1.OffText = "T";
            this.repositoryItemToggleSwitch1.OnText = "Y";
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.btnhapus);
            this.groupControl2.Controls.Add(this.btnsimpan);
            this.groupControl2.Controls.Add(this.btnbaru);
            this.groupControl2.Controls.Add(this.nAMALabel);
            this.groupControl2.Controls.Add(this.txtnamadiv);
            this.groupControl2.Controls.Add(this.uSERIDLabel);
            this.groupControl2.Controls.Add(this.txtkodediv);
            this.groupControl2.Location = new System.Drawing.Point(8, 8);
            this.groupControl2.Margin = new System.Windows.Forms.Padding(2);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(217, 90);
            this.groupControl2.TabIndex = 0;
            this.groupControl2.Text = "Input Divisi";
            // 
            // btnbaru
            // 
            this.btnbaru.Location = new System.Drawing.Point(9, 63);
            this.btnbaru.Margin = new System.Windows.Forms.Padding(2);
            this.btnbaru.Name = "btnbaru";
            this.btnbaru.Size = new System.Drawing.Size(64, 23);
            this.btnbaru.TabIndex = 0;
            this.btnbaru.Text = "Baru";
            this.btnbaru.Click += new System.EventHandler(this.btnbaru_Click);
            // 
            // txtnamadiv
            // 
            this.txtnamadiv.Location = new System.Drawing.Point(91, 41);
            this.txtnamadiv.Margin = new System.Windows.Forms.Padding(2);
            this.txtnamadiv.Name = "txtnamadiv";
            this.txtnamadiv.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtnamadiv.Size = new System.Drawing.Size(113, 20);
            this.txtnamadiv.TabIndex = 1;
            this.txtnamadiv.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtnamadiv_KeyDown);
            // 
            // txtkodediv
            // 
            this.txtkodediv.Location = new System.Drawing.Point(91, 20);
            this.txtkodediv.Margin = new System.Windows.Forms.Padding(2);
            this.txtkodediv.Name = "txtkodediv";
            this.txtkodediv.Properties.MaskSettings.Set("MaskManagerType", typeof(DevExpress.Data.Mask.SimpleMaskManager));
            this.txtkodediv.Properties.MaskSettings.Set("MaskManagerSignature", "ignoreMaskBlank=True");
            this.txtkodediv.Properties.MaskSettings.Set("mask", "00");
            this.txtkodediv.Size = new System.Drawing.Size(45, 20);
            this.txtkodediv.TabIndex = 0;
            this.txtkodediv.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtkodediv_KeyDown);
            // 
            // gridColumn6
            // 
            this.gridColumn6.Caption = "TRANSIT PANEN";
            this.gridColumn6.FieldName = "TRANSIT_PANEN";
            this.gridColumn6.Name = "gridColumn6";
            this.gridColumn6.Visible = true;
            this.gridColumn6.VisibleIndex = 5;
            // 
            // gridColumn7
            // 
            this.gridColumn7.Caption = "TRANSIT RAWAT";
            this.gridColumn7.FieldName = "TRANSIT_RAWAT";
            this.gridColumn7.Name = "gridColumn7";
            this.gridColumn7.Visible = true;
            this.gridColumn7.VisibleIndex = 6;
            // 
            // FrmMasterDivisi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 473);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.gridControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmMasterDivisi";
            this.Text = "Divisi";
            this.Load += new System.EventHandler(this.FrmMasterDivisi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtnamadiv.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtkodediv.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SimpleButton btnsimpan;
        private DevExpress.XtraEditors.SimpleButton btnhapus;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.TextEdit txtnamadiv;
        private DevExpress.XtraEditors.TextEdit txtkodediv;
        private DevExpress.XtraEditors.SimpleButton btnbaru;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private System.Windows.Forms.Label nAMALabel;
        private System.Windows.Forms.Label uSERIDLabel;
        private DevExpress.XtraGrid.Columns.GridColumn ESTATEID;
        private DevExpress.XtraGrid.Columns.GridColumn AKTIF;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
    }
}