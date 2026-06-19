
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
            this.progressImport = new DevExpress.XtraEditors.ProgressBarControl();
            this.lblImportProgress = new DevExpress.XtraEditors.LabelControl();
            this.sbbrowse = new DevExpress.XtraEditors.SimpleButton();
            this.txtPath = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.SBImport = new DevExpress.XtraEditors.SimpleButton();
            this.lblrecord = new DevExpress.XtraEditors.LabelControl();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.cboSheet = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSheet.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressImport.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // sbbrowse
            // 
            this.sbbrowse.ImageOptions.Image = global::Accounting.Properties.Resources.open2_16x16;
            this.sbbrowse.Location = new System.Drawing.Point(227, 7);
            this.sbbrowse.Margin = new System.Windows.Forms.Padding(2);
            this.sbbrowse.Name = "sbbrowse";
            this.sbbrowse.Size = new System.Drawing.Size(89, 20);
            this.sbbrowse.TabIndex = 0;
            this.sbbrowse.Text = "Browse File";
            this.sbbrowse.Click += new System.EventHandler(this.sbbrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(329, 14);
            this.txtPath.Margin = new System.Windows.Forms.Padding(2);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(48, 13);
            this.txtPath.TabIndex = 1;
            this.txtPath.Text = "Lokasi File";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Location = new System.Drawing.Point(8, 74);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(2);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(830, 313);
            this.gridControl1.TabIndex = 2;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 227;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // SBImport
            // 
            this.SBImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SBImport.Enabled = false;
            this.SBImport.ImageOptions.Image = global::Accounting.Properties.Resources.newdatasource_16x16;
            this.SBImport.Location = new System.Drawing.Point(695, 7);
            this.SBImport.Margin = new System.Windows.Forms.Padding(2);
            this.SBImport.Name = "SBImport";
            this.SBImport.Size = new System.Drawing.Size(75, 20);
            this.SBImport.TabIndex = 3;
            this.SBImport.Text = "Import";
            this.SBImport.Click += new System.EventHandler(this.SBImport_Click);
            // 
            // lblrecord
            // 
            this.lblrecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblrecord.Location = new System.Drawing.Point(777, 14);
            this.lblrecord.Margin = new System.Windows.Forms.Padding(2);
            this.lblrecord.Name = "lblrecord";
            this.lblrecord.Size = new System.Drawing.Size(47, 13);
            this.lblrecord.TabIndex = 1;
            this.lblrecord.Text = "JlhRecord";
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(160, 7);
            this.setahun.Margin = new System.Windows.Forms.Padding(2);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Size = new System.Drawing.Size(55, 20);
            this.setahun.TabIndex = 8;
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(56, 7);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(2);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(100, 20);
            this.cmbbulan.TabIndex = 7;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(9, 14);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(36, 13);
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "Periode";
            // 
            // cboSheet
            // 
            this.cboSheet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSheet.Location = new System.Drawing.Point(583, 7);
            this.cboSheet.Margin = new System.Windows.Forms.Padding(2);
            this.cboSheet.Name = "cboSheet";
            this.cboSheet.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboSheet.Size = new System.Drawing.Size(100, 20);
            this.cboSheet.TabIndex = 9;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.cboSheet_SelectedIndexChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Location = new System.Drawing.Point(549, 14);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(2);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(28, 13);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Text = "Sheet";
            // 
            // progressImport
            // 
            this.progressImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressImport.Location = new System.Drawing.Point(8, 52);
            this.progressImport.Name = "progressImport";
            this.progressImport.Properties.Maximum = 100;
            this.progressImport.Properties.Minimum = 0;
            this.progressImport.Properties.PercentView = false;
            this.progressImport.Properties.ShowTitle = true;
            this.progressImport.Size = new System.Drawing.Size(830, 16);
            this.progressImport.TabIndex = 12;
            this.progressImport.Visible = false;
            // 
            // lblImportProgress
            // 
            this.lblImportProgress.Location = new System.Drawing.Point(8, 35);
            this.lblImportProgress.Name = "lblImportProgress";
            this.lblImportProgress.Size = new System.Drawing.Size(0, 13);
            this.lblImportProgress.TabIndex = 13;
            this.lblImportProgress.Visible = false;
            // 
            // FrmImportJurnalSheet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 394);
            this.Controls.Add(this.progressImport);
            this.Controls.Add(this.lblImportProgress);
            this.Controls.Add(this.cboSheet);
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.cmbbulan);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.SBImport);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.lblrecord);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.sbbrowse);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmImportJurnalSheet";
            this.Text = "Import Jurnal Periode";
            this.Load += new System.EventHandler(this.FrmImportJurnalSheet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSheet.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressImport.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton sbbrowse;
        private DevExpress.XtraEditors.LabelControl txtPath;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SimpleButton SBImport;
        private DevExpress.XtraEditors.LabelControl lblrecord;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.ComboBoxEdit cboSheet;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ProgressBarControl progressImport;
        private DevExpress.XtraEditors.LabelControl lblImportProgress;
    }
}