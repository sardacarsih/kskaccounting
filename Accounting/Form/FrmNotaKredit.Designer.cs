
namespace Accounting.Form
{
    partial class FrmNotaKredit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmNotaKredit));
            DevExpress.XtraGrid.GridFormatRule gridFormatRule1 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleValue formatConditionRuleValue1 = new DevExpress.XtraEditors.FormatConditionRuleValue();
            this.sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            this.btnexport = new DevExpress.XtraEditors.SimpleButton();
            this.sbtandai = new DevExpress.XtraEditors.SimpleButton();
            this.btncetak = new DevExpress.XtraEditors.SimpleButton();
            this.setahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.imageCollection1 = new DevExpress.Utils.ImageCollection(this.components);
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.KODE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.NAMAACC = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridViewND = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewND)).BeginInit();
            this.SuspendLayout();
            // 
            // sidePanel1
            // 
            this.sidePanel1.Controls.Add(this.btnexport);
            this.sidePanel1.Controls.Add(this.sbtandai);
            this.sidePanel1.Controls.Add(this.btncetak);
            this.sidePanel1.Controls.Add(this.setahun);
            this.sidePanel1.Controls.Add(this.cmbbulan);
            this.sidePanel1.Controls.Add(this.labelControl3);
            this.sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.sidePanel1.Location = new System.Drawing.Point(0, 0);
            this.sidePanel1.Margin = new System.Windows.Forms.Padding(2);
            this.sidePanel1.Name = "sidePanel1";
            this.sidePanel1.Size = new System.Drawing.Size(1468, 58);
            this.sidePanel1.TabIndex = 0;
            this.sidePanel1.Text = "sidePanel1";
            // 
            // btnexport
            // 
            this.btnexport.Enabled = false;
            this.btnexport.Location = new System.Drawing.Point(698, 7);
            this.btnexport.Margin = new System.Windows.Forms.Padding(4);
            this.btnexport.Name = "btnexport";
            this.btnexport.Size = new System.Drawing.Size(144, 45);
            this.btnexport.TabIndex = 0;
            this.btnexport.Text = "Export";
            this.btnexport.Click += new System.EventHandler(this.btnexport_Click);
            // 
            // sbtandai
            // 
            this.sbtandai.Enabled = false;
            this.sbtandai.Location = new System.Drawing.Point(850, 7);
            this.sbtandai.Margin = new System.Windows.Forms.Padding(4);
            this.sbtandai.Name = "sbtandai";
            this.sbtandai.Size = new System.Drawing.Size(189, 45);
            this.sbtandai.TabIndex = 0;
            this.sbtandai.Text = "Update Status NK";
            this.sbtandai.Click += new System.EventHandler(this.sbtandai_Click);
            // 
            // btncetak
            // 
            this.btncetak.Enabled = false;
            this.btncetak.Location = new System.Drawing.Point(546, 7);
            this.btncetak.Margin = new System.Windows.Forms.Padding(4);
            this.btncetak.Name = "btncetak";
            this.btncetak.Size = new System.Drawing.Size(144, 45);
            this.btncetak.TabIndex = 0;
            this.btncetak.Text = "Preview";
            this.btncetak.Click += new System.EventHandler(this.btncetak_Click);
            // 
            // setahun
            // 
            this.setahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.setahun.Location = new System.Drawing.Point(308, 18);
            this.setahun.Margin = new System.Windows.Forms.Padding(4);
            this.setahun.Name = "setahun";
            this.setahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.setahun.Properties.DisplayFormat.FormatString = "d";
            this.setahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.EditFormat.FormatString = "d";
            this.setahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.setahun.Properties.MaskSettings.Set("mask", "d");
            this.setahun.Size = new System.Drawing.Size(96, 20);
            this.setahun.TabIndex = 0;
            this.setahun.EditValueChanged += new System.EventHandler(this.setahun_EditValueChanged);
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(114, 18);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(4);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(186, 20);
            this.cmbbulan.TabIndex = 0;
            this.cmbbulan.SelectedIndexChanged += new System.EventHandler(this.cmbbulan_SelectedIndexChanged);
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(8, 20);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(36, 13);
            this.labelControl3.TabIndex = 0;
            this.labelControl3.Text = "Periode";
            // 
            // imageCollection1
            // 
            this.imageCollection1.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("imageCollection1.ImageStream")));
            // 
            // gridControl2
            // 
            this.gridControl2.Dock = System.Windows.Forms.DockStyle.Left;
            this.gridControl2.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(13, 8, 13, 8);
            this.gridControl2.Location = new System.Drawing.Point(0, 58);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Margin = new System.Windows.Forms.Padding(30, 31, 30, 31);
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(546, 552);
            this.gridControl2.TabIndex = 0;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            this.gridControl2.Click += new System.EventHandler(this.gridControl2_Click);
            this.gridControl2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gridControl2_KeyUp);
            // 
            // gridView2
            // 
            this.gridView2.Appearance.FocusedCell.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.gridView2.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridView2.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.gridView2.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridView2.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.KODE,
            this.NAMAACC});
            this.gridView2.DetailHeight = 1140;
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            this.gridView2.OptionsBehavior.Editable = false;
            this.gridView2.OptionsView.ShowGroupPanel = false;
            this.gridView2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridControl2_KeyUp);
            // 
            // KODE
            // 
            this.KODE.Caption = "KODE";
            this.KODE.FieldName = "KODE";
            this.KODE.MinWidth = 22;
            this.KODE.Name = "KODE";
            this.KODE.Visible = true;
            this.KODE.VisibleIndex = 0;
            this.KODE.Width = 124;
            // 
            // NAMAACC
            // 
            this.NAMAACC.Caption = "PERKIRAAN";
            this.NAMAACC.FieldName = "PERKIRAAN";
            this.NAMAACC.MinWidth = 22;
            this.NAMAACC.Name = "NAMAACC";
            this.NAMAACC.Visible = true;
            this.NAMAACC.VisibleIndex = 1;
            this.NAMAACC.Width = 402;
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.EmbeddedNavigator.Margin = new System.Windows.Forms.Padding(103, 66, 103, 66);
            this.gridControl1.Location = new System.Drawing.Point(546, 58);
            this.gridControl1.MainView = this.gridViewND;
            this.gridControl1.Margin = new System.Windows.Forms.Padding(243, 253, 243, 253);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(922, 552);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewND});
            // 
            // gridViewND
            // 
            this.gridViewND.Appearance.FocusedCell.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.gridViewND.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridViewND.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.gridViewND.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridViewND.DetailHeight = 19000;
            gridFormatRule1.ApplyToRow = true;
            gridFormatRule1.Name = "Format0";
            formatConditionRuleValue1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            formatConditionRuleValue1.Appearance.Options.UseBackColor = true;
            formatConditionRuleValue1.Condition = DevExpress.XtraEditors.FormatCondition.Equal;
            formatConditionRuleValue1.Value1 = "False";
            gridFormatRule1.Rule = formatConditionRuleValue1;
            this.gridViewND.FormatRules.Add(gridFormatRule1);
            this.gridViewND.GridControl = this.gridControl1;
            this.gridViewND.Name = "gridViewND";
            this.gridViewND.OptionsBehavior.Editable = false;
            this.gridViewND.OptionsFilter.AllowFilterEditor = false;
            this.gridViewND.OptionsFind.AlwaysVisible = true;
            this.gridViewND.OptionsFind.ShowFindButton = false;
            this.gridViewND.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.gridViewND.OptionsView.ShowFooter = true;
            this.gridViewND.OptionsView.ShowGroupPanel = false;
            this.gridViewND.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(this.gridViewND_RowStyle);
            // 
            // FrmNotaKredit
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1468, 610);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.gridControl2);
            this.Controls.Add(this.sidePanel1);
            this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmNotaKredit";
            this.Text = "Nota Kredit";
            this.Load += new System.EventHandler(this.FrmNotaKredit_Load);
            this.sidePanel1.ResumeLayout(false);
            this.sidePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.setahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageCollection1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewND)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.SpinEdit setahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraEditors.SimpleButton btncetak;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewND;
        private DevExpress.XtraGrid.Columns.GridColumn KODE;
        private DevExpress.XtraGrid.Columns.GridColumn NAMAACC;
        private DevExpress.XtraEditors.SimpleButton btnexport;
        private DevExpress.XtraEditors.SimpleButton sbtandai;
    }
}