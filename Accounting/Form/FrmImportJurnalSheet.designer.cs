namespace Accounting.Form
{
    partial class FrmImportJurnalSheet
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
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.sbbrowse = new DevExpress.XtraEditors.SimpleButton();
            this.txtPath = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.cboSheet = new DevExpress.XtraEditors.ComboBoxEdit();
            this.SBImport = new DevExpress.XtraEditors.SimpleButton();
            this.lblrecord = new DevExpress.XtraEditors.LabelControl();
            this.lblImportProgress = new DevExpress.XtraEditors.LabelControl();
            this.progressImport = new DevExpress.XtraEditors.ProgressBarControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.filePathToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSheet.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressImport.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            //
            // cmbbulan
            //
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.TabIndex = 0;
            //
            // setahun
            //
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.TabIndex = 1;
            //
            // sbbrowse
            //
            this.sbbrowse.Name = "sbbrowse";
            this.sbbrowse.TabIndex = 2;
            this.sbbrowse.Click += new System.EventHandler(this.sbbrowse_Click);
            //
            // txtPath
            //
            this.txtPath.Name = "txtPath";
            this.txtPath.TabIndex = 3;
            //
            // labelControl1
            //
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.TabIndex = 4;
            //
            // cboSheet
            //
            this.cboSheet.Name = "cboSheet";
            this.cboSheet.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboSheet.TabIndex = 5;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.cboSheet_SelectedIndexChanged);
            //
            // SBImport
            //
            this.SBImport.Name = "SBImport";
            this.SBImport.TabIndex = 6;
            this.SBImport.Click += new System.EventHandler(this.SBImport_Click);
            //
            // lblrecord
            //
            this.lblrecord.Name = "lblrecord";
            this.lblrecord.TabIndex = 7;
            //
            // lblImportProgress
            //
            this.lblImportProgress.Name = "lblImportProgress";
            this.lblImportProgress.TabIndex = 8;
            //
            // progressImport
            //
            this.progressImport.Name = "progressImport";
            this.progressImport.TabIndex = 9;
            //
            // gridControl1
            //
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.TabIndex = 10;
            //
            // gridView1
            //
            this.gridView1.DetailHeight = 367;
            this.gridView1.Name = "gridView1";
            //
            // FrmImportJurnalSheet
            //
            JurnalImportFormLayout.Apply(
                this,
                this.cmbbulan,
                this.setahun,
                this.sbbrowse,
                this.txtPath,
                this.labelControl1,
                this.cboSheet,
                this.SBImport,
                this.lblrecord,
                this.lblImportProgress,
                this.progressImport,
                this.gridControl1,
                this.gridView1);
            this.Name = "FrmImportJurnalSheet";
            this.Text = "Import Jurnal Periode";
            this.Load += new System.EventHandler(this.FrmImportJurnalSheet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSheet.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressImport.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.SimpleButton sbbrowse;
        private DevExpress.XtraEditors.LabelControl txtPath;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ComboBoxEdit cboSheet;
        private DevExpress.XtraEditors.SimpleButton SBImport;
        private DevExpress.XtraEditors.LabelControl lblrecord;
        private DevExpress.XtraEditors.LabelControl lblImportProgress;
        private DevExpress.XtraEditors.ProgressBarControl progressImport;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.ToolTip filePathToolTip;
    }
}
