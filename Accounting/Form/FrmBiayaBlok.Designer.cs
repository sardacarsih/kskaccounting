
namespace Accounting.Form
{
    partial class FrmBiayaBlok
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBiayaBlok));
            sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            cmbbulan2 = new DevExpress.XtraEditors.ComboBoxEdit();
            cmbbulan1 = new DevExpress.XtraEditors.ComboBoxEdit();
            sbexport = new DevExpress.XtraEditors.SimpleButton();
            btnproses = new DevExpress.XtraEditors.SimpleButton();
            setahun2 = new DevExpress.XtraEditors.SpinEdit();
            setahun1 = new DevExpress.XtraEditors.SpinEdit();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            panelControl1 = new DevExpress.XtraEditors.PanelControl();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbbulan2.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulan1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)setahun2.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)setahun1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).BeginInit();
            panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            SuspendLayout();
            // 
            // sidePanel1
            // 
            sidePanel1.Controls.Add(cmbbulan2);
            sidePanel1.Controls.Add(cmbbulan1);
            sidePanel1.Controls.Add(sbexport);
            sidePanel1.Controls.Add(btnproses);
            sidePanel1.Controls.Add(setahun2);
            sidePanel1.Controls.Add(setahun1);
            sidePanel1.Controls.Add(labelControl1);
            sidePanel1.Controls.Add(labelControl3);
            sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            sidePanel1.Location = new System.Drawing.Point(0, 0);
            sidePanel1.Margin = new System.Windows.Forms.Padding(2);
            sidePanel1.Name = "sidePanel1";
            sidePanel1.Size = new System.Drawing.Size(750, 48);
            sidePanel1.TabIndex = 1;
            sidePanel1.Text = "sidePanel1";
            // 
            // cmbbulan2
            // 
            cmbbulan2.Location = new System.Drawing.Point(282, 3);
            cmbbulan2.Name = "cmbbulan2";
            cmbbulan2.Properties.AdvancedModeOptions.Label = "Bulan";
            cmbbulan2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbbulan2.Properties.UseAdvancedMode = DevExpress.Utils.DefaultBoolean.True;
            cmbbulan2.Size = new System.Drawing.Size(100, 42);
            cmbbulan2.TabIndex = 7;
            cmbbulan2.SelectedIndexChanged += cmbbulan2_SelectedIndexChanged;
            // 
            // cmbbulan1
            // 
            cmbbulan1.Location = new System.Drawing.Point(45, 3);
            cmbbulan1.Name = "cmbbulan1";
            cmbbulan1.Properties.AdvancedModeOptions.Label = "Bulan";
            cmbbulan1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            cmbbulan1.Properties.UseAdvancedMode = DevExpress.Utils.DefaultBoolean.True;
            cmbbulan1.Size = new System.Drawing.Size(100, 42);
            cmbbulan1.TabIndex = 7;
            cmbbulan1.SelectedIndexChanged += cmbbulan1_SelectedIndexChanged;
            // 
            // sbexport
            // 
            sbexport.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("sbexport.ImageOptions.Image");
            sbexport.Location = new System.Drawing.Point(617, 6);
            sbexport.Margin = new System.Windows.Forms.Padding(2);
            sbexport.Name = "sbexport";
            sbexport.Size = new System.Drawing.Size(97, 20);
            sbexport.TabIndex = 6;
            sbexport.Text = "Export";
            sbexport.Click += sbexport_Click;
            // 
            // btnproses
            // 
            btnproses.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("btnproses.ImageOptions.Image");
            btnproses.Location = new System.Drawing.Point(516, 6);
            btnproses.Margin = new System.Windows.Forms.Padding(2);
            btnproses.Name = "btnproses";
            btnproses.Size = new System.Drawing.Size(97, 20);
            btnproses.TabIndex = 6;
            btnproses.Text = "Proses";
            btnproses.Click += btnproses_Click;
            // 
            // setahun2
            // 
            setahun2.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            setahun2.Location = new System.Drawing.Point(392, 3);
            setahun2.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            setahun2.Name = "setahun2";
            setahun2.Properties.AdvancedModeOptions.Label = "Tahun";
            setahun2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            setahun2.Properties.DisplayFormat.FormatString = "d";
            setahun2.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun2.Properties.EditFormat.FormatString = "d";
            setahun2.Properties.MaskSettings.Set("mask", "d");
            setahun2.Properties.UseAdvancedMode = DevExpress.Utils.DefaultBoolean.True;
            setahun2.Size = new System.Drawing.Size(67, 42);
            setahun2.TabIndex = 5;
            setahun2.EditValueChanged += setahun2_EditValueChanged;
            // 
            // setahun1
            // 
            setahun1.EditValue = new decimal(new int[] { 0, 0, 0, 0 });
            setahun1.Location = new System.Drawing.Point(150, 3);
            setahun1.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            setahun1.Name = "setahun1";
            setahun1.Properties.AdvancedModeOptions.Label = "Tahun";
            setahun1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            setahun1.Properties.DisplayFormat.FormatString = "d";
            setahun1.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            setahun1.Properties.EditFormat.FormatString = "d";
            setahun1.Properties.MaskSettings.Set("mask", "d");
            setahun1.Properties.UseAdvancedMode = DevExpress.Utils.DefaultBoolean.True;
            setahun1.Size = new System.Drawing.Size(67, 42);
            setahun1.TabIndex = 5;
            setahun1.EditValueChanged += setahun1_EditValueChanged;
            // 
            // labelControl1
            // 
            labelControl1.Location = new System.Drawing.Point(229, 12);
            labelControl1.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new System.Drawing.Size(43, 17);
            labelControl1.TabIndex = 4;
            labelControl1.Text = "Sampai";
            // 
            // labelControl3
            // 
            labelControl3.Location = new System.Drawing.Point(4, 15);
            labelControl3.Margin = new System.Windows.Forms.Padding(425, 462, 425, 462);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new System.Drawing.Size(24, 17);
            labelControl3.TabIndex = 4;
            labelControl3.Text = "Dari";
            // 
            // panelControl1
            // 
            panelControl1.Controls.Add(gridControl1);
            panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            panelControl1.Location = new System.Drawing.Point(0, 48);
            panelControl1.Margin = new System.Windows.Forms.Padding(2);
            panelControl1.Name = "panelControl1";
            panelControl1.Size = new System.Drawing.Size(750, 344);
            panelControl1.TabIndex = 2;
            // 
            // gridControl1
            // 
            gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            gridControl1.Location = new System.Drawing.Point(2, 2);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new System.Drawing.Size(746, 340);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // FrmBiayaBlok
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(750, 392);
            Controls.Add(panelControl1);
            Controls.Add(sidePanel1);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "FrmBiayaBlok";
            Text = "Analisa Biaya Blok";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Load += FrmBiayaBlok_Load;
            sidePanel1.ResumeLayout(false);
            sidePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)cmbbulan2.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)cmbbulan1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)setahun2.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)setahun1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)panelControl1).EndInit();
            panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnproses;
        private DevExpress.XtraEditors.SpinEdit setahun2;
        private DevExpress.XtraEditors.SpinEdit setahun1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton sbexport;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan2;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
    }
}