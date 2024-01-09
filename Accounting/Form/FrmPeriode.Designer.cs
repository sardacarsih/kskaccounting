
namespace Accounting.Form
{
    partial class FrmPeriode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPeriode));
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.KUNCI = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            this.PERIODE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.BULAN = new DevExpress.XtraGrid.Columns.GridColumn();
            this.IDPERIODE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.repositoryItemCheckedComboBoxEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
            this.simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            this.SuspendLayout();
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(71, 5);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(94, 26);
            this.setahun.TabIndex = 4;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 8);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(45, 19);
            this.labelControl1.TabIndex = 4;
            this.labelControl1.Text = "Tahun";
            // 
            // gridControl1
            // 
            this.gridControl1.Location = new System.Drawing.Point(12, 46);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit1,
            this.repositoryItemCheckedComboBoxEdit1,
            this.repositoryItemToggleSwitch1});
            this.gridControl1.Size = new System.Drawing.Size(607, 544);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.KUNCI,
            this.PERIODE,
            this.BULAN,
            this.IDPERIODE});
            this.gridView1.DetailHeight = 317;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridView1_PopupMenuShowing);
            this.gridView1.RowUpdated += new DevExpress.XtraGrid.Views.Base.RowObjectEventHandler(this.gridView1_RowUpdated);
            // 
            // KUNCI
            // 
            this.KUNCI.Caption = "KUNCI";
            this.KUNCI.ColumnEdit = this.repositoryItemToggleSwitch1;
            this.KUNCI.FieldName = "KUNCI";
            this.KUNCI.MinWidth = 30;
            this.KUNCI.Name = "KUNCI";
            this.KUNCI.Visible = true;
            this.KUNCI.VisibleIndex = 1;
            this.KUNCI.Width = 112;
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
            // PERIODE
            // 
            this.PERIODE.Caption = "PERIODE";
            this.PERIODE.FieldName = "PERIODE";
            this.PERIODE.MinWidth = 30;
            this.PERIODE.Name = "PERIODE";
            this.PERIODE.Visible = true;
            this.PERIODE.VisibleIndex = 2;
            this.PERIODE.Width = 112;
            // 
            // BULAN
            // 
            this.BULAN.Caption = "BULAN";
            this.BULAN.FieldName = "BULAN";
            this.BULAN.MinWidth = 30;
            this.BULAN.Name = "BULAN";
            this.BULAN.OptionsColumn.AllowEdit = false;
            this.BULAN.Visible = true;
            this.BULAN.VisibleIndex = 0;
            this.BULAN.Width = 112;
            // 
            // IDPERIODE
            // 
            this.IDPERIODE.Caption = "IDPERIODE";
            this.IDPERIODE.FieldName = "IDPERIODE";
            this.IDPERIODE.MinWidth = 30;
            this.IDPERIODE.Name = "IDPERIODE";
            this.IDPERIODE.Width = 112;
            // 
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.CheckBox;
            this.repositoryItemCheckEdit1.DisplayValueChecked = "1";
            this.repositoryItemCheckEdit1.DisplayValueUnchecked = "0";
            this.repositoryItemCheckEdit1.ImageOptions.ImageChecked = ((System.Drawing.Image)(resources.GetObject("repositoryItemCheckEdit1.ImageOptions.ImageChecked")));
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
            this.simpleButton2.Location = new System.Drawing.Point(485, 0);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(71, 31);
            this.simpleButton2.TabIndex = 3;
            this.simpleButton2.Text = "Tutup";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // imageCollection1
         
            // FrmPeriode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 601);
            this.ControlBox = false;
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.setahun);
            this.Name = "FrmPeriode";
            this.Text = "Periode Akuntansi";
            this.Load += new System.EventHandler(this.FrmPeriode_Load);
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemToggleSwitch1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckedComboBoxEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SimpleButton simpleButton2;
        private DevExpress.XtraGrid.Columns.GridColumn KUNCI;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit repositoryItemCheckedComboBoxEdit1;
        private DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch repositoryItemToggleSwitch1;
        private DevExpress.XtraGrid.Columns.GridColumn PERIODE;
        private DevExpress.XtraGrid.Columns.GridColumn BULAN;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraGrid.Columns.GridColumn IDPERIODE;
    }
}