
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPeriode));
            setahun = new DevExpress.XtraEditors.SpinEdit();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            KUNCI = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemToggleSwitch1 = new DevExpress.XtraEditors.Repository.RepositoryItemToggleSwitch();
            PERIODE = new DevExpress.XtraGrid.Columns.GridColumn();
            BULAN = new DevExpress.XtraGrid.Columns.GridColumn();
            IDPERIODE = new DevExpress.XtraGrid.Columns.GridColumn();
            repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            repositoryItemCheckedComboBoxEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
            simpleButton2 = new DevExpress.XtraEditors.SimpleButton();
            imageCollection1 = new DevExpress.Utils.ImageCollection(components);
            ((System.ComponentModel.ISupportInitialize)setahun.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemToggleSwitch1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckEdit1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckedComboBoxEdit1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imageCollection1).BeginInit();
            SuspendLayout();
            // 
            // setahun
            // 
            setahun.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            setahun.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            setahun.Location = new System.Drawing.Point(55, 4);
            setahun.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            setahun.Name = "setahun";
            setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            setahun.Properties.DisplayFormat.FormatString = "d";
            setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun.Properties.EditFormat.FormatString = "d";
            setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun.Properties.MaskSettings.Set("mask", "d");
            setahun.Size = new System.Drawing.Size(74, 24);
            setahun.TabIndex = 4;
            setahun.EditValueChanged += setahun_EditValueChanged;
            // 
            // labelControl1
            // 
            labelControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            labelControl1.Location = new System.Drawing.Point(9, 7);
            labelControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new System.Drawing.Size(35, 17);
            labelControl1.TabIndex = 4;
            labelControl1.Text = "Tahun";
            // 
            // gridControl1
            // 
            gridControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            gridControl1.Location = new System.Drawing.Point(9, 41);
            gridControl1.MainView = gridView1;
            gridControl1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            gridControl1.Name = "gridControl1";
            gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { repositoryItemCheckEdit1, repositoryItemCheckedComboBoxEdit1, repositoryItemToggleSwitch1 });
            gridControl1.Size = new System.Drawing.Size(472, 486);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { KUNCI, PERIODE, BULAN, IDPERIODE });
            gridView1.DetailHeight = 284;
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsEditForm.PopupEditFormWidth = 622;
            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.PopupMenuShowing += gridView1_PopupMenuShowing;
            gridView1.RowUpdated += gridView1_RowUpdated;
            // 
            // KUNCI
            // 
            KUNCI.Caption = "KUNCI";
            KUNCI.ColumnEdit = repositoryItemToggleSwitch1;
            KUNCI.FieldName = "KUNCI";
            KUNCI.MinWidth = 23;
            KUNCI.Name = "KUNCI";
            KUNCI.Visible = true;
            KUNCI.VisibleIndex = 1;
            KUNCI.Width = 87;
            // 
            // repositoryItemToggleSwitch1
            // 
            repositoryItemToggleSwitch1.AutoHeight = false;
            repositoryItemToggleSwitch1.Name = "repositoryItemToggleSwitch1";
            repositoryItemToggleSwitch1.OffText = "Off";
            repositoryItemToggleSwitch1.OnText = "On";
            repositoryItemToggleSwitch1.ValueOff = "T";
            repositoryItemToggleSwitch1.ValueOn = "Y";
            // 
            // PERIODE
            // 
            PERIODE.Caption = "PERIODE";
            PERIODE.FieldName = "PERIODE";
            PERIODE.MinWidth = 23;
            PERIODE.Name = "PERIODE";
            PERIODE.Visible = true;
            PERIODE.VisibleIndex = 2;
            PERIODE.Width = 87;
            // 
            // BULAN
            // 
            BULAN.Caption = "BULAN";
            BULAN.FieldName = "BULAN";
            BULAN.MinWidth = 23;
            BULAN.Name = "BULAN";
            BULAN.OptionsColumn.AllowEdit = false;
            BULAN.Visible = true;
            BULAN.VisibleIndex = 0;
            BULAN.Width = 87;
            // 
            // IDPERIODE
            // 
            IDPERIODE.Caption = "IDPERIODE";
            IDPERIODE.FieldName = "IDPERIODE";
            IDPERIODE.MinWidth = 23;
            IDPERIODE.Name = "IDPERIODE";
            IDPERIODE.Width = 87;
            // 
            // repositoryItemCheckEdit1
            // 
            repositoryItemCheckEdit1.AutoHeight = false;
            repositoryItemCheckEdit1.CheckBoxOptions.Style = DevExpress.XtraEditors.Controls.CheckBoxStyle.CheckBox;
            repositoryItemCheckEdit1.DisplayValueChecked = "1";
            repositoryItemCheckEdit1.DisplayValueUnchecked = "0";
            repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            // 
            // repositoryItemCheckedComboBoxEdit1
            // 
            repositoryItemCheckedComboBoxEdit1.AutoHeight = false;
            repositoryItemCheckedComboBoxEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            repositoryItemCheckedComboBoxEdit1.Name = "repositoryItemCheckedComboBoxEdit1";
            // 
            // simpleButton2
            // 
            simpleButton2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            simpleButton2.Location = new System.Drawing.Point(377, 0);
            simpleButton2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            simpleButton2.Name = "simpleButton2";
            simpleButton2.Size = new System.Drawing.Size(55, 27);
            simpleButton2.TabIndex = 3;
            simpleButton2.Text = "Tutup";
            simpleButton2.Click += simpleButton2_Click;
            // 
            // imageCollection1
            // 
            imageCollection1.ImageStream = (DevExpress.Utils.ImageCollectionStreamer)resources.GetObject("imageCollection1.ImageStream");
            imageCollection1.Images.SetKeyName(0, "cancel_32x32.png");
            // 
            // FrmPeriode
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(491, 537);
            ControlBox = false;
            Controls.Add(simpleButton2);
            Controls.Add(gridControl1);
            Controls.Add(labelControl1);
            Controls.Add(setahun);
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            MaximizeBox = true;
            MinimizeBox = true;
            MinimumSize = new System.Drawing.Size(560, 520);
            Name = "FrmPeriode";
            Text = "Periode Akuntansi";
            Load += FrmPeriode_Load;
            Resize += FrmPeriode_Resize;
            ((System.ComponentModel.ISupportInitialize)setahun.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemToggleSwitch1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckEdit1).EndInit();
            ((System.ComponentModel.ISupportInitialize)repositoryItemCheckedComboBoxEdit1).EndInit();
            ((System.ComponentModel.ISupportInitialize)imageCollection1).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
