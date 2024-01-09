namespace Accounting.Form
{
    partial class FrmReportParam
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
            this.lblcompany = new System.Windows.Forms.Label();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.sbcetak = new DevExpress.XtraEditors.SimpleButton();
            this.daritahun = new DevExpress.XtraEditors.SpinEdit();
            this.cmbbulan = new DevExpress.XtraEditors.ComboBoxEdit();
            this.radioGroup1 = new DevExpress.XtraEditors.RadioGroup();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.searchLookUpEdit2 = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn8 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.searchLookUpEdit1 = new DevExpress.XtraEditors.SearchLookUpEdit();
            this.searchLookUpEdit1View = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.cmbbulan2 = new DevExpress.XtraEditors.ComboBoxEdit();
            this.sbexport = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.sampaitahun = new DevExpress.XtraEditors.SpinEdit();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(this.daritahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1View)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sampaitahun.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblcompany
            // 
            this.lblcompany.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblcompany.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblcompany.Location = new System.Drawing.Point(14, 42);
            this.lblcompany.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblcompany.Name = "lblcompany";
            this.lblcompany.Size = new System.Drawing.Size(453, 25);
            this.lblcompany.TabIndex = 2;
            this.lblcompany.Text = "Laporan Keuangan";
            this.lblcompany.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(235, 179);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(19, 13);
            this.labelControl3.TabIndex = 6;
            this.labelControl3.Text = "Dari";
            // 
            // sbcetak
            // 
            this.sbcetak.Location = new System.Drawing.Point(375, 244);
            this.sbcetak.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbcetak.Name = "sbcetak";
            this.sbcetak.Size = new System.Drawing.Size(85, 30);
            this.sbcetak.TabIndex = 9;
            this.sbcetak.Text = "Preview";
            this.sbcetak.Click += new System.EventHandler(this.sbcetak_Click);
            // 
            // daritahun
            // 
            this.daritahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.daritahun.Location = new System.Drawing.Point(402, 174);
            this.daritahun.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.daritahun.Name = "daritahun";
            this.daritahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.daritahun.Properties.DisplayFormat.FormatString = "d";
            this.daritahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.daritahun.Properties.EditFormat.FormatString = "d";
            this.daritahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.daritahun.Properties.MaskSettings.Set("mask", "d");
            this.daritahun.Size = new System.Drawing.Size(58, 20);
            this.daritahun.TabIndex = 8;
            // 
            // cmbbulan
            // 
            this.cmbbulan.Location = new System.Drawing.Point(285, 174);
            this.cmbbulan.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbbulan.Name = "cmbbulan";
            this.cmbbulan.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan.Properties.ImmediatePopup = true;
            this.cmbbulan.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan.Size = new System.Drawing.Size(112, 20);
            this.cmbbulan.TabIndex = 7;
            this.cmbbulan.SelectedIndexChanged += new System.EventHandler(this.cmbbulan_SelectedIndexChanged);
            // 
            // radioGroup1
            // 
            this.radioGroup1.Location = new System.Drawing.Point(16, 70);
            this.radioGroup1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioGroup1.Name = "radioGroup1";
            this.radioGroup1.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Laba / Rugi"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Neraca"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Neraca (Skontro)"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Neraca Saldo"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Neraca ( Semester 2 )"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Neraca Konsolidasi"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "Buku Besar")});
            this.radioGroup1.Size = new System.Drawing.Size(181, 269);
            this.radioGroup1.TabIndex = 10;
            this.radioGroup1.SelectedIndexChanged += new System.EventHandler(this.radioGroup1_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelControl2);
            this.panel1.Controls.Add(this.labelControl1);
            this.panel1.Controls.Add(this.searchLookUpEdit2);
            this.panel1.Controls.Add(this.searchLookUpEdit1);
            this.panel1.Location = new System.Drawing.Point(228, 70);
            this.panel1.Margin = new System.Windows.Forms.Padding(1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(239, 100);
            this.panel1.TabIndex = 11;
            this.panel1.Visible = false;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(7, 53);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(1);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(61, 13);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "Sampai Kode";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(7, 5);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(1);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(46, 13);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Dari Kode";
            // 
            // searchLookUpEdit2
            // 
            this.searchLookUpEdit2.Location = new System.Drawing.Point(7, 71);
            this.searchLookUpEdit2.Margin = new System.Windows.Forms.Padding(1);
            this.searchLookUpEdit2.Name = "searchLookUpEdit2";
            this.searchLookUpEdit2.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            this.searchLookUpEdit2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.searchLookUpEdit2.Properties.PopupView = this.gridView1;
            this.searchLookUpEdit2.Size = new System.Drawing.Size(225, 20);
            this.searchLookUpEdit2.TabIndex = 0;
            this.searchLookUpEdit2.Popup += new System.EventHandler(this.searchLookUpEdit2_Popup);
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn4,
            this.gridColumn5,
            this.gridColumn6,
            this.gridColumn8});
            this.gridView1.DetailHeight = 144;
            this.gridView1.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsFilter.AllowFilterEditor = false;
            this.gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.gridColumn4, DevExpress.Data.ColumnSortOrder.Ascending)});
            // 
            // gridColumn4
            // 
            this.gridColumn4.Caption = "KODE";
            this.gridColumn4.FieldName = "KODE";
            this.gridColumn4.MinWidth = 13;
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.Visible = true;
            this.gridColumn4.VisibleIndex = 0;
            this.gridColumn4.Width = 50;
            // 
            // gridColumn5
            // 
            this.gridColumn5.Caption = "PERKIRAAN";
            this.gridColumn5.FieldName = "PERKIRAAN";
            this.gridColumn5.MinWidth = 13;
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.Visible = true;
            this.gridColumn5.VisibleIndex = 1;
            this.gridColumn5.Width = 50;
            // 
            // gridColumn6
            // 
            this.gridColumn6.Caption = "POSISI";
            this.gridColumn6.FieldName = "POSISI";
            this.gridColumn6.MinWidth = 13;
            this.gridColumn6.Name = "gridColumn6";
            this.gridColumn6.Visible = true;
            this.gridColumn6.VisibleIndex = 2;
            this.gridColumn6.Width = 50;
            // 
            // gridColumn8
            // 
            this.gridColumn8.Caption = "GRP";
            this.gridColumn8.FieldName = "GRP";
            this.gridColumn8.Name = "gridColumn8";
            this.gridColumn8.Visible = true;
            this.gridColumn8.VisibleIndex = 3;
            // 
            // searchLookUpEdit1
            // 
            this.searchLookUpEdit1.Location = new System.Drawing.Point(7, 20);
            this.searchLookUpEdit1.Margin = new System.Windows.Forms.Padding(1);
            this.searchLookUpEdit1.Name = "searchLookUpEdit1";
            this.searchLookUpEdit1.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            this.searchLookUpEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.searchLookUpEdit1.Properties.PopupView = this.searchLookUpEdit1View;
            this.searchLookUpEdit1.Size = new System.Drawing.Size(225, 20);
            this.searchLookUpEdit1.TabIndex = 0;
            this.searchLookUpEdit1.EditValueChanged += new System.EventHandler(this.searchLookUpEdit1_EditValueChanged);
            // 
            // searchLookUpEdit1View
            // 
            this.searchLookUpEdit1View.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3,
            this.gridColumn7});
            this.searchLookUpEdit1View.DetailHeight = 144;
            this.searchLookUpEdit1View.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.searchLookUpEdit1View.Name = "searchLookUpEdit1View";
            this.searchLookUpEdit1View.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.searchLookUpEdit1View.OptionsView.ShowGroupPanel = false;
            this.searchLookUpEdit1View.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
            new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.gridColumn1, DevExpress.Data.ColumnSortOrder.Ascending)});
            // 
            // gridColumn1
            // 
            this.gridColumn1.Caption = "KODE";
            this.gridColumn1.FieldName = "KODE";
            this.gridColumn1.MinWidth = 13;
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.Visible = true;
            this.gridColumn1.VisibleIndex = 0;
            this.gridColumn1.Width = 50;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "PERKIRAAN";
            this.gridColumn2.FieldName = "PERKIRAAN";
            this.gridColumn2.MinWidth = 13;
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 1;
            this.gridColumn2.Width = 50;
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "POSISI";
            this.gridColumn3.FieldName = "POSISI";
            this.gridColumn3.MinWidth = 13;
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.Visible = true;
            this.gridColumn3.VisibleIndex = 2;
            this.gridColumn3.Width = 50;
            // 
            // gridColumn7
            // 
            this.gridColumn7.Caption = "GRP";
            this.gridColumn7.FieldName = "GRP";
            this.gridColumn7.Name = "gridColumn7";
            this.gridColumn7.Visible = true;
            this.gridColumn7.VisibleIndex = 3;
            // 
            // cmbbulan2
            // 
            this.cmbbulan2.Location = new System.Drawing.Point(285, 203);
            this.cmbbulan2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbbulan2.Name = "cmbbulan2";
            this.cmbbulan2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbbulan2.Properties.ImmediatePopup = true;
            this.cmbbulan2.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbbulan2.Size = new System.Drawing.Size(112, 20);
            this.cmbbulan2.TabIndex = 7;
            this.cmbbulan2.SelectedIndexChanged += new System.EventHandler(this.cmbbulan2_SelectedIndexChanged);
            // 
            // sbexport
            // 
            this.sbexport.Location = new System.Drawing.Point(285, 244);
            this.sbexport.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sbexport.Name = "sbexport";
            this.sbexport.Size = new System.Drawing.Size(85, 30);
            this.sbexport.TabIndex = 9;
            this.sbexport.Text = "Export";
            this.sbexport.Click += new System.EventHandler(this.sbexport_Click);
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(235, 208);
            this.labelControl4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(34, 13);
            this.labelControl4.TabIndex = 6;
            this.labelControl4.Text = "Sampai";
            // 
            // sampaitahun
            // 
            this.sampaitahun.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.sampaitahun.Location = new System.Drawing.Point(402, 203);
            this.sampaitahun.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sampaitahun.Name = "sampaitahun";
            this.sampaitahun.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.sampaitahun.Properties.DisplayFormat.FormatString = "d";
            this.sampaitahun.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.sampaitahun.Properties.EditFormat.FormatString = "d";
            this.sampaitahun.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.sampaitahun.Properties.MaskSettings.Set("mask", "d");
            this.sampaitahun.Size = new System.Drawing.Size(58, 20);
            this.sampaitahun.TabIndex = 8;
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.radioGroup1);
            this.groupControl1.Controls.Add(this.sbexport);
            this.groupControl1.Controls.Add(this.panel1);
            this.groupControl1.Controls.Add(this.sbcetak);
            this.groupControl1.Controls.Add(this.sampaitahun);
            this.groupControl1.Controls.Add(this.lblcompany);
            this.groupControl1.Controls.Add(this.daritahun);
            this.groupControl1.Controls.Add(this.labelControl3);
            this.groupControl1.Controls.Add(this.cmbbulan2);
            this.groupControl1.Controls.Add(this.labelControl4);
            this.groupControl1.Controls.Add(this.cmbbulan);
            this.groupControl1.Location = new System.Drawing.Point(9, 8);
            this.groupControl1.Margin = new System.Windows.Forms.Padding(2);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(498, 362);
            this.groupControl1.TabIndex = 12;
            // 
            // FrmReportParam
            // 
            this.Appearance.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Appearance.ForeColor = System.Drawing.Color.Purple;
            this.Appearance.Options.UseBackColor = true;
            this.Appearance.Options.UseForeColor = true;
            this.Appearance.Options.UseTextOptions = true;
            this.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(666, 391);
            this.Controls.Add(this.groupControl1);
            this.FormBorderEffect = DevExpress.XtraEditors.FormBorderEffect.Glow;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.LookAndFeel.SkinName = "DevExpress Dark Style";
            this.LookAndFeel.UseWindowsXPTheme = true;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmReportParam";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Laporan Keuangan";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.Load += new System.EventHandler(this.FrmReportParam_Load);
            ((System.ComponentModel.ISupportInitialize)(this.daritahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.searchLookUpEdit1View)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbbulan2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sampaitahun.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblcompany;
        private DevExpress.XtraEditors.SpinEdit daritahun;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.SimpleButton sbcetak;
        private DevExpress.XtraEditors.RadioGroup radioGroup1;
        private System.Windows.Forms.Panel panel1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.SearchLookUpEdit searchLookUpEdit2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SearchLookUpEdit searchLookUpEdit1;
        private DevExpress.XtraGrid.Views.Grid.GridView searchLookUpEdit1View;
        private DevExpress.XtraEditors.ComboBoxEdit cmbbulan2;
        private DevExpress.XtraEditors.SimpleButton sbexport;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.SpinEdit sampaitahun;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn8;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
    }
}