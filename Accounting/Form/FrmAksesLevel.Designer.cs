
namespace Accounting.Form
{
    partial class FrmAksesLevel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAksesLevel));
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.BUKA = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.BARU = new DevExpress.XtraGrid.Columns.GridColumn();
            this.SIMPAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.UBAH = new DevExpress.XtraGrid.Columns.GridColumn();
            this.KETERANGAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AKSESID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.CETAK = new DevExpress.XtraGrid.Columns.GridColumn();
            this.HAPUS = new DevExpress.XtraGrid.Columns.GridColumn();
            this.AKSIID = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemCheckedComboBoxEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            this.lookUpEditLevel = new DevExpress.XtraEditors.LookUpEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.TXTNEWROLE = new DevExpress.XtraEditors.TextEdit();
            this.btnduplikatnewrole = new DevExpress.XtraEditors.SimpleButton();
            this.btnhapus = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditLevel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTNEWROLE.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 8);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(36, 19);
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "Level";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.Location = new System.Drawing.Point(12, 38);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit1,
            this.repositoryItemCheckedComboBoxEdit1,
            this.repositoryItemToggleSwitch1});
            this.gridControl1.Size = new System.Drawing.Size(1009, 438);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.BUKA,
            this.BARU,
            this.SIMPAN,
            this.UBAH,
            this.KETERANGAN,
            this.AKSESID,
            this.CETAK,
            this.HAPUS,
            this.AKSIID});
            this.gridView1.DetailHeight = 317;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView1_RowUpdated);
            // 
            // BUKA
            // 
            this.BUKA.AppearanceHeader.Options.UseTextOptions = true;
            this.BUKA.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.BUKA.Caption = "BUKA";
            this.BUKA.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.BUKA.FieldName = "BUKA";
            this.BUKA.MinWidth = 30;
            this.BUKA.Name = "BUKA";
            this.BUKA.Visible = true;
            this.BUKA.VisibleIndex = 2;
            this.BUKA.Width = 56;
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
            // BARU
            // 
            this.BARU.AppearanceHeader.Options.UseTextOptions = true;
            this.BARU.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.BARU.Caption = "BARU";
            this.BARU.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.BARU.FieldName = "BARU";
            this.BARU.MinWidth = 30;
            this.BARU.Name = "BARU";
            this.BARU.Visible = true;
            this.BARU.VisibleIndex = 3;
            this.BARU.Width = 56;
            // 
            // SIMPAN
            // 
            this.SIMPAN.AppearanceHeader.Options.UseTextOptions = true;
            this.SIMPAN.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.SIMPAN.Caption = "SIMPAN";
            this.SIMPAN.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.SIMPAN.FieldName = "SIMPAN";
            this.SIMPAN.MinWidth = 30;
            this.SIMPAN.Name = "SIMPAN";
            this.SIMPAN.Visible = true;
            this.SIMPAN.VisibleIndex = 4;
            this.SIMPAN.Width = 56;
            // 
            // UBAH
            // 
            this.UBAH.AppearanceHeader.Options.UseTextOptions = true;
            this.UBAH.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.UBAH.Caption = "UBAH";
            this.UBAH.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.UBAH.FieldName = "UBAH";
            this.UBAH.MinWidth = 30;
            this.UBAH.Name = "UBAH";
            this.UBAH.Visible = true;
            this.UBAH.VisibleIndex = 5;
            this.UBAH.Width = 57;
            // 
            // KETERANGAN
            // 
            this.KETERANGAN.Caption = "KETERANGAN";
            this.KETERANGAN.FieldName = "KETERANGAN";
            this.KETERANGAN.MinWidth = 350;
            this.KETERANGAN.Name = "KETERANGAN";
            this.KETERANGAN.OptionsColumn.AllowEdit = false;
            this.KETERANGAN.OptionsColumn.FixedWidth = true;
            this.KETERANGAN.Visible = true;
            this.KETERANGAN.VisibleIndex = 1;
            this.KETERANGAN.Width = 600;
            // 
            // AKSESID
            // 
            this.AKSESID.Caption = "AKSESID";
            this.AKSESID.FieldName = "AKSESID";
            this.AKSESID.MinWidth = 30;
            this.AKSESID.Name = "AKSESID";
            this.AKSESID.Width = 63;
            // 
            // CETAK
            // 
            this.CETAK.AppearanceHeader.Options.UseTextOptions = true;
            this.CETAK.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.CETAK.Caption = "CETAK";
            this.CETAK.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.CETAK.FieldName = "CETAK";
            this.CETAK.MinWidth = 30;
            this.CETAK.Name = "CETAK";
            this.CETAK.Visible = true;
            this.CETAK.VisibleIndex = 7;
            this.CETAK.Width = 52;
            // 
            // HAPUS
            // 
            this.HAPUS.AppearanceHeader.Options.UseTextOptions = true;
            this.HAPUS.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.HAPUS.Caption = "HAPUS";
            this.HAPUS.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.HAPUS.FieldName = "HAPUS";
            this.HAPUS.MinWidth = 30;
            this.HAPUS.Name = "HAPUS";
            this.HAPUS.Visible = true;
            this.HAPUS.VisibleIndex = 6;
            this.HAPUS.Width = 35;
            // 
            // AKSIID
            // 
            this.AKSIID.Caption = "AKSIID";
            this.AKSIID.FieldName = "AKSIID";
            this.AKSIID.MinWidth = 30;
            this.AKSIID.Name = "AKSIID";
            this.AKSIID.OptionsColumn.FixedWidth = true;
            this.AKSIID.Visible = true;
            this.AKSIID.VisibleIndex = 0;
            this.AKSIID.Width = 61;
            // 
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.CheckBox;
            this.repositoryItemCheckEdit1.DisplayValueChecked = "1";
            this.repositoryItemCheckEdit1.DisplayValueUnchecked = "0";
            this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            // 
            // repositoryItemCheckedComboBoxEdit1
            // 
            this.repositoryItemCheckedComboBoxEdit1.AutoHeight = false;
            this.repositoryItemCheckedComboBoxEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemCheckedComboBoxEdit1.Name = "repositoryItemCheckedComboBoxEdit1";
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(937, 5);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(82, 31);
            this.simpleButton2.TabIndex = 3;
            this.simpleButton2.Text = "Tutup";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
          
            // 
            // lookUpEditLevel
            // 
            this.lookUpEditLevel.Location = new System.Drawing.Point(57, 3);
            this.lookUpEditLevel.Name = "lookUpEditLevel";
            this.lookUpEditLevel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpEditLevel.Size = new System.Drawing.Size(172, 26);
            this.lookUpEditLevel.TabIndex = 5;
            this.lookUpEditLevel.EditValueChanged += new System.EventHandler(this.lookUpEditLevel_EditValueChanged);
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(341, 9);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(157, 19);
            this.labelControl2.TabIndex = 4;
            this.labelControl2.Text = "Duplikat ke Level Baru";
            // 
            // TXTNEWROLE
            // 
            this.TXTNEWROLE.Location = new System.Drawing.Point(504, 6);
            this.TXTNEWROLE.Name = "TXTNEWROLE";
            this.TXTNEWROLE.Size = new System.Drawing.Size(150, 26);
            this.TXTNEWROLE.TabIndex = 6;
            // 
            // btnduplikatnewrole
            // 
            this.btnduplikatnewrole.ImageOptions.Image = global::Accounting.Properties.Resources.copy_16x16;
            this.btnduplikatnewrole.Location = new System.Drawing.Point(235, 2);
            this.btnduplikatnewrole.Name = "btnduplikatnewrole";
            this.btnduplikatnewrole.Size = new System.Drawing.Size(100, 31);
            this.btnduplikatnewrole.TabIndex = 3;
            this.btnduplikatnewrole.Text = "Duplikat";
            this.btnduplikatnewrole.Click += new System.EventHandler(this.btnduplikatnewrole_Click);
            // 
            // btnhapus
            // 
            this.btnhapus.ImageOptions.Image = global::Accounting.Properties.Resources.removepivotfield_16x161;
            this.btnhapus.Location = new System.Drawing.Point(839, 5);
            this.btnhapus.Name = "btnhapus";
            this.btnhapus.Size = new System.Drawing.Size(82, 31);
            this.btnhapus.TabIndex = 3;
            this.btnhapus.Text = "Hapus";
            this.btnhapus.Click += new System.EventHandler(this.btnhapus_Click);
            // 
            // FrmAksesLevel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1033, 487);
            this.ControlBox = false;
            this.Controls.Add(this.TXTNEWROLE);
            this.Controls.Add(this.lookUpEditLevel);
            this.Controls.Add(this.btnhapus);
            this.Controls.Add(this.btnduplikatnewrole);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Name = "FrmAksesLevel";
            this.Text = "Akses Level";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmAksesLevel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditLevel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTNEWROLE.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraGrid.Columns.GridColumn BUKA;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
        private DevExpress.XtraGrid.Columns.GridColumn BARU;
        private DevExpress.XtraGrid.Columns.GridColumn SIMPAN;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraGrid.Columns.GridColumn UBAH;
        private DevExpress.XtraEditors.LookUpEdit lookUpEditLevel;
        private DevExpress.XtraGrid.Columns.GridColumn KETERANGAN;
        private DevExpress.XtraGrid.Columns.GridColumn AKSESID;
        private DevExpress.XtraGrid.Columns.GridColumn CETAK;
        private DevExpress.XtraGrid.Columns.GridColumn HAPUS;
        private DevExpress.XtraGrid.Columns.GridColumn AKSIID;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit TXTNEWROLE;
        private DevExpress.XtraEditors.SimpleButton btnduplikatnewrole;
        private DevExpress.XtraEditors.SimpleButton btnhapus;
    }
}