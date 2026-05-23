
namespace Accounting.Form
{
    partial class FrmAuditTrail
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
            this.sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            this.btnCari = new DevExpress.XtraEditors.SimpleButton();
            this.txtNoJurnal = new DevExpress.XtraEditors.TextEdit();
            this.lblNoJurnal = new DevExpress.XtraEditors.LabelControl();
            this.txtUser = new DevExpress.XtraEditors.TextEdit();
            this.lblUser = new DevExpress.XtraEditors.LabelControl();
            this.cmbTipe = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblTipe = new DevExpress.XtraEditors.LabelControl();
            this.dtSampai = new DevExpress.XtraEditors.DateEdit();
            this.lblSampai = new DevExpress.XtraEditors.LabelControl();
            this.dtDari = new DevExpress.XtraEditors.DateEdit();
            this.lblDari = new DevExpress.XtraEditors.LabelControl();
            this.gridControlHeader = new DevExpress.XtraGrid.GridControl();
            this.gridViewHeader = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.gridControlAuditEvents = new DevExpress.XtraGrid.GridControl();
            this.gridViewAuditEvents = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridControlDetail = new DevExpress.XtraGrid.GridControl();
            this.gridViewDetail = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtDari.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDari.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtSampai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtSampai.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTipe.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUser.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoJurnal.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlHeader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewHeader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAuditEvents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAuditEvents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDetail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDetail)).BeginInit();
            this.SuspendLayout();
            //
            // sidePanel1
            //
            this.sidePanel1.Controls.Add(this.btnCari);
            this.sidePanel1.Controls.Add(this.txtNoJurnal);
            this.sidePanel1.Controls.Add(this.lblNoJurnal);
            this.sidePanel1.Controls.Add(this.txtUser);
            this.sidePanel1.Controls.Add(this.lblUser);
            this.sidePanel1.Controls.Add(this.cmbTipe);
            this.sidePanel1.Controls.Add(this.lblTipe);
            this.sidePanel1.Controls.Add(this.dtSampai);
            this.sidePanel1.Controls.Add(this.lblSampai);
            this.sidePanel1.Controls.Add(this.dtDari);
            this.sidePanel1.Controls.Add(this.lblDari);
            this.sidePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.sidePanel1.Location = new System.Drawing.Point(0, 0);
            this.sidePanel1.Name = "sidePanel1";
            this.sidePanel1.Size = new System.Drawing.Size(1305, 80);
            this.sidePanel1.TabIndex = 0;
            this.sidePanel1.Text = "sidePanel1";
            //
            // lblDari
            //
            this.lblDari.Location = new System.Drawing.Point(7, 15);
            this.lblDari.Name = "lblDari";
            this.lblDari.Size = new System.Drawing.Size(28, 19);
            this.lblDari.TabIndex = 0;
            this.lblDari.Text = "Dari";
            //
            // dtDari
            //
            this.dtDari.EditValue = null;
            this.dtDari.Location = new System.Drawing.Point(45, 12);
            this.dtDari.Name = "dtDari";
            this.dtDari.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDari.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtDari.Properties.DisplayFormat.FormatString = "dd-MMM-yyyy";
            this.dtDari.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtDari.Properties.EditFormat.FormatString = "dd-MMM-yyyy";
            this.dtDari.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtDari.Properties.Mask.EditMask = "dd-MMM-yyyy";
            this.dtDari.Size = new System.Drawing.Size(130, 26);
            this.dtDari.TabIndex = 1;
            //
            // lblSampai
            //
            this.lblSampai.Location = new System.Drawing.Point(185, 15);
            this.lblSampai.Name = "lblSampai";
            this.lblSampai.Size = new System.Drawing.Size(50, 19);
            this.lblSampai.TabIndex = 2;
            this.lblSampai.Text = "Sampai";
            //
            // dtSampai
            //
            this.dtSampai.EditValue = null;
            this.dtSampai.Location = new System.Drawing.Point(240, 12);
            this.dtSampai.Name = "dtSampai";
            this.dtSampai.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtSampai.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dtSampai.Properties.DisplayFormat.FormatString = "dd-MMM-yyyy";
            this.dtSampai.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtSampai.Properties.EditFormat.FormatString = "dd-MMM-yyyy";
            this.dtSampai.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.dtSampai.Properties.Mask.EditMask = "dd-MMM-yyyy";
            this.dtSampai.Size = new System.Drawing.Size(130, 26);
            this.dtSampai.TabIndex = 3;
            //
            // lblTipe
            //
            this.lblTipe.Location = new System.Drawing.Point(385, 15);
            this.lblTipe.Name = "lblTipe";
            this.lblTipe.Size = new System.Drawing.Size(29, 19);
            this.lblTipe.TabIndex = 4;
            this.lblTipe.Text = "Tipe";
            //
            // cmbTipe
            //
            this.cmbTipe.Location = new System.Drawing.Point(420, 12);
            this.cmbTipe.Name = "cmbTipe";
            this.cmbTipe.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbTipe.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbTipe.Size = new System.Drawing.Size(110, 26);
            this.cmbTipe.TabIndex = 5;
            //
            // lblUser
            //
            this.lblUser.Location = new System.Drawing.Point(7, 48);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(30, 19);
            this.lblUser.TabIndex = 6;
            this.lblUser.Text = "User";
            //
            // txtUser
            //
            this.txtUser.Location = new System.Drawing.Point(45, 45);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(150, 26);
            this.txtUser.TabIndex = 7;
            //
            // lblNoJurnal
            //
            this.lblNoJurnal.Location = new System.Drawing.Point(205, 48);
            this.lblNoJurnal.Name = "lblNoJurnal";
            this.lblNoJurnal.Size = new System.Drawing.Size(63, 19);
            this.lblNoJurnal.TabIndex = 8;
            this.lblNoJurnal.Text = "No Jurnal";
            //
            // txtNoJurnal
            //
            this.txtNoJurnal.Location = new System.Drawing.Point(275, 45);
            this.txtNoJurnal.Name = "txtNoJurnal";
            this.txtNoJurnal.Size = new System.Drawing.Size(150, 26);
            this.txtNoJurnal.TabIndex = 9;
            //
            // btnCari
            //
            this.btnCari.Location = new System.Drawing.Point(545, 12);
            this.btnCari.Name = "btnCari";
            this.btnCari.Size = new System.Drawing.Size(100, 55);
            this.btnCari.TabIndex = 10;
            this.btnCari.Text = "Cari";
            this.btnCari.Click += new System.EventHandler(this.btnCari_Click);
            //
            // gridControlHeader
            //
            this.gridControlHeader.Dock = System.Windows.Forms.DockStyle.Left;
            this.gridControlHeader.Location = new System.Drawing.Point(0, 80);
            this.gridControlHeader.MainView = this.gridViewHeader;
            this.gridControlHeader.Name = "gridControlHeader";
            this.gridControlHeader.Size = new System.Drawing.Size(520, 554);
            this.gridControlHeader.TabIndex = 1;
            this.gridControlHeader.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewHeader});
            //
            // gridViewHeader
            //
            this.gridViewHeader.GridControl = this.gridControlHeader;
            this.gridViewHeader.Name = "gridViewHeader";
            this.gridViewHeader.OptionsBehavior.Editable = false;
            this.gridViewHeader.OptionsFind.AlwaysVisible = true;
            this.gridViewHeader.OptionsFind.SearchInPreview = true;
            this.gridViewHeader.OptionsView.ShowGroupPanel = false;
            this.gridViewHeader.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewHeader_FocusedRowChanged);
            //
            // splitContainerRight
            //
            this.splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRight.Location = new System.Drawing.Point(520, 80);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainerRight.Panel1
            //
            this.splitContainerRight.Panel1.Controls.Add(this.gridControlAuditEvents);
            //
            // splitContainerRight.Panel2
            //
            this.splitContainerRight.Panel2.Controls.Add(this.gridControlDetail);
            this.splitContainerRight.Size = new System.Drawing.Size(785, 554);
            this.splitContainerRight.SplitterDistance = 250;
            this.splitContainerRight.TabIndex = 2;
            //
            // gridControlAuditEvents
            //
            this.gridControlAuditEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlAuditEvents.MainView = this.gridViewAuditEvents;
            this.gridControlAuditEvents.Name = "gridControlAuditEvents";
            this.gridControlAuditEvents.Size = new System.Drawing.Size(785, 250);
            this.gridControlAuditEvents.TabIndex = 0;
            this.gridControlAuditEvents.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewAuditEvents});
            //
            // gridViewAuditEvents
            //
            this.gridViewAuditEvents.GridControl = this.gridControlAuditEvents;
            this.gridViewAuditEvents.Name = "gridViewAuditEvents";
            this.gridViewAuditEvents.OptionsBehavior.Editable = false;
            this.gridViewAuditEvents.OptionsView.ShowGroupPanel = false;
            this.gridViewAuditEvents.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(this.gridViewAuditEvents_FocusedRowChanged);
            this.gridViewAuditEvents.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(this.gridViewAuditEvents_RowStyle);
            //
            // gridControlDetail
            //
            this.gridControlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlDetail.MainView = this.gridViewDetail;
            this.gridControlDetail.Name = "gridControlDetail";
            this.gridControlDetail.Size = new System.Drawing.Size(785, 300);
            this.gridControlDetail.TabIndex = 0;
            this.gridControlDetail.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewDetail});
            //
            // gridViewDetail
            //
            this.gridViewDetail.GridControl = this.gridControlDetail;
            this.gridViewDetail.Name = "gridViewDetail";
            this.gridViewDetail.OptionsBehavior.Editable = false;
            this.gridViewDetail.OptionsView.ShowFooter = true;
            this.gridViewDetail.OptionsView.ShowGroupPanel = false;
            this.gridViewDetail.RowStyle += new DevExpress.XtraGrid.Views.Grid.RowStyleEventHandler(this.gridViewDetail_RowStyle);
            //
            // FrmAuditTrail
            //
            this.Appearance.Options.UseFont = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1305, 634);
            this.Controls.Add(this.splitContainerRight);
            this.Controls.Add(this.gridControlHeader);
            this.Controls.Add(this.sidePanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FrmAuditTrail";
            this.Text = "Audit Trail";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmAuditTrail_Load);
            this.sidePanel1.ResumeLayout(false);
            this.sidePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtDari.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDari.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtSampai.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtSampai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbTipe.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUser.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNoJurnal.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlHeader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewHeader)).EndInit();
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAuditEvents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewAuditEvents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDetail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDetail)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.LabelControl lblDari;
        private DevExpress.XtraEditors.DateEdit dtDari;
        private DevExpress.XtraEditors.LabelControl lblSampai;
        private DevExpress.XtraEditors.DateEdit dtSampai;
        private DevExpress.XtraEditors.LabelControl lblTipe;
        private DevExpress.XtraEditors.ComboBoxEdit cmbTipe;
        private DevExpress.XtraEditors.LabelControl lblUser;
        private DevExpress.XtraEditors.TextEdit txtUser;
        private DevExpress.XtraEditors.LabelControl lblNoJurnal;
        private DevExpress.XtraEditors.TextEdit txtNoJurnal;
        private DevExpress.XtraEditors.SimpleButton btnCari;
        private DevExpress.XtraGrid.GridControl gridControlHeader;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewHeader;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private DevExpress.XtraGrid.GridControl gridControlAuditEvents;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewAuditEvents;
        private DevExpress.XtraGrid.GridControl gridControlDetail;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewDetail;
    }
}
