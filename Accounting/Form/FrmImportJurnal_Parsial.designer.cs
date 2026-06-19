
namespace Accounting.Form
{
    partial class FrmImportJurnal_Parsial
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
            this.txtPath = new DevExpress.XtraEditors.LabelControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.lblrecord = new DevExpress.XtraEditors.LabelControl();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.SBImport = new DevExpress.XtraEditors.SimpleButton();
            this.sbbrowse = new DevExpress.XtraEditors.SimpleButton();
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
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(463, 21);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(72, 21);
            this.txtPath.TabIndex = 1;
            this.txtPath.Text = "Lokasi File";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.Location = new System.Drawing.Point(12, 74);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1245, 551);
            this.gridControl1.TabIndex = 2;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 367;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsFind.AlwaysVisible = true;
            this.gridView1.OptionsFind.ShowFindButton = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // lblrecord
            // 
            this.lblrecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblrecord.Location = new System.Drawing.Point(1148, 24);
            this.lblrecord.Name = "lblrecord";
            this.lblrecord.Size = new System.Drawing.Size(68, 21);
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
            this.setahun.Location = new System.Drawing.Point(240, 14);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(83, 28);
            this.setahun.TabIndex = 8;
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(84, 14);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(150, 28);
            this.cmbbulan.TabIndex = 7;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(12, 21);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(53, 21);
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "Periode";
            // 
            // SBImport
            // 
            this.SBImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SBImport.Enabled = false;
            this.SBImport.ImageOptions.Image = global::Accounting.Properties.Resources.editdatasource_16x16;
            this.SBImport.Location = new System.Drawing.Point(1030, 9);
            this.SBImport.Name = "SBImport";
            this.SBImport.Size = new System.Drawing.Size(112, 36);
            this.SBImport.TabIndex = 3;
            this.SBImport.Text = "Import";
            this.SBImport.Click += new System.EventHandler(this.SBImport_Click);
            // 
            // sbbrowse
            // 
            this.sbbrowse.ImageOptions.Image = global::Accounting.Properties.Resources.open2_16x16;
            this.sbbrowse.Location = new System.Drawing.Point(329, 13);
            this.sbbrowse.Name = "sbbrowse";
            this.sbbrowse.Size = new System.Drawing.Size(128, 36);
            this.sbbrowse.TabIndex = 0;
            this.sbbrowse.Text = "Browse File";
            this.sbbrowse.Click += new System.EventHandler(this.sbbrowse_Click);
            // 
            // cboSheet
            // 
            this.cboSheet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSheet.Location = new System.Drawing.Point(874, 17);
            this.cboSheet.Name = "cboSheet";
            this.cboSheet.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboSheet.Size = new System.Drawing.Size(150, 28);
            this.cboSheet.TabIndex = 11;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.cboSheet_SelectedIndexChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Location = new System.Drawing.Point(822, 24);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(39, 21);
            this.labelControl1.TabIndex = 10;
            this.labelControl1.Text = "Sheet";
            // 
            // progressImport
            // 
            this.progressImport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressImport.Location = new System.Drawing.Point(12, 52);
            this.progressImport.Name = "progressImport";
            this.progressImport.Properties.Maximum = 100;
            this.progressImport.Properties.Minimum = 0;
            this.progressImport.Properties.PercentView = false;
            this.progressImport.Properties.ShowTitle = true;
            this.progressImport.Size = new System.Drawing.Size(1245, 16);
            this.progressImport.TabIndex = 12;
            this.progressImport.Visible = false;
            // 
            // lblImportProgress
            // 
            this.lblImportProgress.Location = new System.Drawing.Point(12, 35);
            this.lblImportProgress.Name = "lblImportProgress";
            this.lblImportProgress.Size = new System.Drawing.Size(0, 13);
            this.lblImportProgress.TabIndex = 13;
            this.lblImportProgress.Visible = false;
            // 
            // FrmImportJurnal_Parsial
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1269, 637);
            this.Controls.Add(this.progressImport);
            this.Controls.Add(this.lblImportProgress);
            this.Controls.Add(this.cboSheet);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.setahun);
            this.Controls.Add(this.cmbbulan);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.SBImport);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.lblrecord);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.sbbrowse);
            this.Name = "FrmImportJurnal_Parsial";
            this.Text = "Import Jurnal";
            this.Load += new System.EventHandler(this.FrmImportJurnal_Parsial_Load);
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
        private DevExpress.XtraEditors.LabelControl txtPath;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.LabelControl lblrecord;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton SBImport;
        private DevExpress.XtraEditors.SimpleButton sbbrowse;
        private DevExpress.XtraEditors.ComboBoxEdit cboSheet;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.ProgressBarControl progressImport;
        private DevExpress.XtraEditors.LabelControl lblImportProgress;
    }
}