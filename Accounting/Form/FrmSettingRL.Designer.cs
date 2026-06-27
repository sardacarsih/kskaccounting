
namespace Accounting.Form
{
    partial class FrmSettingRL
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
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.KELOMPOK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.LVL = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KODE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.GRP = new DevExpress.XtraGrid.Columns.GridColumn();
            this.TAMPILKAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.checkEdit1 = new DevExpress.XtraEditors.ToggleSwitch();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.NO = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(12, 12);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemToggleSwitch1});
            this.gridControl1.Size = new System.Drawing.Size(588, 435);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.Click += new System.EventHandler(this.gridControl1_Click);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.KELOMPOK,
            this.LVL,
            this.KODE,
            this.GRP,
            this.TAMPILKAN,
            this.NO});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView1_RowUpdated);
            // 
            // KELOMPOK
            // 
            this.KELOMPOK.Caption = "KELOMPOK";
            this.KELOMPOK.FieldName = "KELOMPOK";
            this.KELOMPOK.MinWidth = 30;
            this.KELOMPOK.Name = "KELOMPOK";
            this.KELOMPOK.OptionsColumn.AllowEdit = false;
            this.KELOMPOK.Visible = true;
            this.KELOMPOK.VisibleIndex = 1;
            this.KELOMPOK.Width = 172;
            // 
            // LVL
            // 
            this.LVL.Caption = "LVL";
            this.LVL.FieldName = "LVL";
            this.LVL.MinWidth = 30;
            this.LVL.Name = "LVL";
            this.LVL.Visible = true;
            this.LVL.VisibleIndex = 2;
            this.LVL.Width = 172;
            // 
            // KODE
            // 
            this.KODE.Caption = "KODE";
            this.KODE.FieldName = "KODE";
            this.KODE.MinWidth = 30;
            this.KODE.Name = "KODE";
            this.KODE.Width = 112;
            // 
            // GRP
            // 
            this.GRP.Caption = "GRP";
            this.GRP.FieldName = "GRP";
            this.GRP.MinWidth = 30;
            this.GRP.Name = "GRP";
            this.GRP.Width = 112;
            // 
            // TAMPILKAN
            // 
            this.TAMPILKAN.Caption = "TAMPILKAN";
            this.TAMPILKAN.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.TAMPILKAN.FieldName = "TAMPILKAN";
            this.TAMPILKAN.MinWidth = 30;
            this.TAMPILKAN.Name = "TAMPILKAN";
            this.TAMPILKAN.Visible = true;
            this.TAMPILKAN.VisibleIndex = 3;
            this.TAMPILKAN.Width = 176;
            // 
            // repositoryItemToggleSwitch1
            // 
            this.repositoryItemToggleSwitch1.AutoHeight = false;
            this.repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
            this.repositoryItemToggleSwitch1.OffText = "Off";
            this.repositoryItemToggleSwitch1.OnText = "On";
            this.repositoryItemToggleSwitch1.ValueOff = "T";
            this.repositoryItemToggleSwitch1.ValueOn = "Y";
            // 
            // gridControl2
            // 
            this.gridControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl2.Location = new System.Drawing.Point(606, 12);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(559, 619);
            this.gridControl2.TabIndex = 0;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            this.gridView2.OptionsBehavior.Editable = false;
            this.gridView2.OptionsView.ShowGroupPanel = false;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 474);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(121, 21);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Tampilkan Nilai 0";
            // 
            // checkEdit1
            // 
            this.checkEdit1.Location = new System.Drawing.Point(158, 463);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkEdit1.Properties.Appearance.Options.UseFont = true;
            this.checkEdit1.Properties.OffText = "Tidak";
            this.checkEdit1.Properties.OnText = "Ya";
            this.checkEdit1.Size = new System.Drawing.Size(249, 32);
            this.checkEdit1.TabIndex = 13;
            this.checkEdit1.EditValueChanged += new System.EventHandler(this.checkEdit1_EditValueChanged);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(488, 461);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(112, 34);
            this.simpleButton1.TabIndex = 14;
            this.simpleButton1.Text = "Reset";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // NO
            // 
            this.NO.Caption = "NO";
            this.NO.FieldName = "URUT";
            this.NO.Name = "NO";
            this.NO.Visible = true;
            this.NO.VisibleIndex = 0;
            this.NO.Width = 50;
            // 
            // FrmSettingRL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1174, 643);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.checkEdit1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.gridControl2);
            this.Controls.Add(this.gridControl1);
            this.Name = "FrmSettingRL";
            this.Text = "Pengaturan Laporan";
            this.Load += new System.EventHandler(this.FrmSettingRL_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.Columns.GridColumn KELOMPOK;
        private DevExpress.XtraGrid.Columns.GridColumn LVL;
        private DevExpress.XtraGrid.Columns.GridColumn KODE;
        private DevExpress.XtraGrid.Columns.GridColumn GRP;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ToggleSwitch checkEdit1;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraGrid.Columns.GridColumn TAMPILKAN;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
        private DevExpress.XtraGrid.Columns.GridColumn NO;
    }
}