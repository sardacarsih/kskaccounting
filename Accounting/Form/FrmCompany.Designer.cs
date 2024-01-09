
namespace Accounting.Form
{
    partial class FrmCompany
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
            System.Windows.Forms.Label lEVELIDLabel;
            System.Windows.Forms.Label pASSWORDLabel;
            System.Windows.Forms.Label uSERIDLabel;
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label5;
            System.Windows.Forms.Label label7;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCompany));
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            this.LEPT = new DevExpress.XtraEditors.LookUpEdit();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.radioGroup1 = new DevExpress.XtraEditors.RadioGroup();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.btnhapus = new DevExpress.XtraEditors.SimpleButton();
            this.btnsimpan = new DevExpress.XtraEditors.SimpleButton();
            this.TXTWILAYAH = new DevExpress.XtraEditors.TextEdit();
            this.TXTIDDATA = new DevExpress.XtraEditors.TextEdit();
            this.xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            this.gridControl2 = new DevExpress.XtraGrid.GridControl();
            this.gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.SBHAPUS = new DevExpress.XtraEditors.SimpleButton();
            this.SBSIMPAN = new DevExpress.XtraEditors.SimpleButton();
            this.CMBGROUP = new System.Windows.Forms.ComboBox();
            this.TXTNAMAPT = new DevExpress.XtraEditors.TextEdit();
            this.TXTKODEPT = new DevExpress.XtraEditors.TextEdit();
            lEVELIDLabel = new System.Windows.Forms.Label();
            pASSWORDLabel = new System.Windows.Forms.Label();
            uSERIDLabel = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LEPT.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTWILAYAH.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTIDDATA.Properties)).BeginInit();
            this.xtraTabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTNAMAPT.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTKODEPT.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lEVELIDLabel
            // 
            lEVELIDLabel.AutoSize = true;
            lEVELIDLabel.Location = new System.Drawing.Point(204, 82);
            lEVELIDLabel.Name = "lEVELIDLabel";
            lEVELIDLabel.Size = new System.Drawing.Size(112, 19);
            lEVELIDLabel.TabIndex = 38;
            lEVELIDLabel.Text = "PERUSAHAAN";
            // 
            // pASSWORDLabel
            // 
            pASSWORDLabel.AutoSize = true;
            pASSWORDLabel.Location = new System.Drawing.Point(204, 49);
            pASSWORDLabel.Name = "pASSWORDLabel";
            pASSWORDLabel.Size = new System.Drawing.Size(80, 19);
            pASSWORDLabel.TabIndex = 41;
            pASSWORDLabel.Text = "WILAYAH";
            // 
            // uSERIDLabel
            // 
            uSERIDLabel.AutoSize = true;
            uSERIDLabel.Location = new System.Drawing.Point(204, 19);
            uSERIDLabel.Name = "uSERIDLabel";
            uSERIDLabel.Size = new System.Drawing.Size(69, 19);
            uSERIDLabel.TabIndex = 42;
            uSERIDLabel.Text = "IDDATA";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(607, 56);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(62, 19);
            label4.TabIndex = 54;
            label4.Text = "GROUP";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(19, 56);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(162, 19);
            label5.TabIndex = 55;
            label5.Text = "NAMA PERUSAHAAN";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(19, 25);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(50, 19);
            label7.TabIndex = 57;
            label7.Text = "KODE";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.xtraTabPage1;
            this.xtraTabControl1.Size = new System.Drawing.Size(965, 673);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
            // 
            // xtraTabPage1
            // 
            this.xtraTabPage1.Controls.Add(this.LEPT);
            this.xtraTabPage1.Controls.Add(this.groupControl1);
            this.xtraTabPage1.Controls.Add(this.gridControl1);
            this.xtraTabPage1.Controls.Add(this.btnhapus);
            this.xtraTabPage1.Controls.Add(this.btnsimpan);
            this.xtraTabPage1.Controls.Add(lEVELIDLabel);
            this.xtraTabPage1.Controls.Add(pASSWORDLabel);
            this.xtraTabPage1.Controls.Add(this.TXTWILAYAH);
            this.xtraTabPage1.Controls.Add(uSERIDLabel);
            this.xtraTabPage1.Controls.Add(this.TXTIDDATA);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(959, 639);
            this.xtraTabPage1.Text = "IDDATA";
            // 
            // LEPT
            // 
            this.LEPT.Location = new System.Drawing.Point(322, 79);
            this.LEPT.Name = "LEPT";
            this.LEPT.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            this.LEPT.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.LEPT.Size = new System.Drawing.Size(393, 26);
            this.LEPT.TabIndex = 2;
            this.LEPT.ToolTip = "Nama Perusahaan";
            this.LEPT.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Information;
            this.LEPT.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LEPT_KeyDown);
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.radioGroup1);
            this.groupControl1.Location = new System.Drawing.Point(22, 4);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(157, 193);
            this.groupControl1.TabIndex = 45;
            this.groupControl1.Text = "Jenis Pembukuan";
            // 
            // radioGroup1
            // 
            this.radioGroup1.Location = new System.Drawing.Point(19, 33);
            this.radioGroup1.Name = "radioGroup1";
            this.radioGroup1.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "PUSAT"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "PWK"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "KEBUN", true, null, ""),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "PKS")});
            this.radioGroup1.Size = new System.Drawing.Size(121, 142);
            this.radioGroup1.TabIndex = 0;
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gridControl1.Location = new System.Drawing.Point(22, 203);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(916, 429);
            this.gridControl1.TabIndex = 4;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.DetailHeight = 317;
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsBehavior.Editable = false;
            this.gridView1.OptionsFind.AlwaysVisible = true;
            this.gridView1.OptionsFind.ShowFindButton = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // btnhapus
            // 
            this.btnhapus.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnhapus.ImageOptions.Image")));
            this.btnhapus.Location = new System.Drawing.Point(326, 128);
            this.btnhapus.Name = "btnhapus";
            this.btnhapus.Size = new System.Drawing.Size(112, 31);
            this.btnhapus.TabIndex = 35;
            this.btnhapus.Text = "Hapus";
            this.btnhapus.Click += new System.EventHandler(this.btnhapus_Click);
            // 
            // btnsimpan
            // 
            this.btnsimpan.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnsimpan.ImageOptions.Image")));
            this.btnsimpan.Location = new System.Drawing.Point(208, 128);
            this.btnsimpan.Name = "btnsimpan";
            this.btnsimpan.Size = new System.Drawing.Size(112, 31);
            this.btnsimpan.TabIndex = 3;
            this.btnsimpan.Text = "Simpan";
            this.btnsimpan.Click += new System.EventHandler(this.btnsimpan_Click_1);
            // 
            // TXTWILAYAH
            // 
            this.TXTWILAYAH.Location = new System.Drawing.Point(322, 46);
            this.TXTWILAYAH.Name = "TXTWILAYAH";
            this.TXTWILAYAH.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TXTWILAYAH.Size = new System.Drawing.Size(264, 26);
            this.TXTWILAYAH.TabIndex = 1;
            this.TXTWILAYAH.ToolTip = "Wilayah";
            this.TXTWILAYAH.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Information;
            this.TXTWILAYAH.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TXTWILAYAH_KeyDown);
            // 
            // TXTIDDATA
            // 
            this.TXTIDDATA.Location = new System.Drawing.Point(322, 15);
            this.TXTIDDATA.Name = "TXTIDDATA";
            this.TXTIDDATA.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TXTIDDATA.Size = new System.Drawing.Size(150, 26);
            this.TXTIDDATA.TabIndex = 0;
            this.TXTIDDATA.ToolTip = "ID Data";
            this.TXTIDDATA.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Information;
            this.TXTIDDATA.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TXTIDDATA_KeyDown);
            // 
            // xtraTabPage2
            // 
            this.xtraTabPage2.Controls.Add(this.gridControl2);
            this.xtraTabPage2.Controls.Add(this.SBHAPUS);
            this.xtraTabPage2.Controls.Add(this.SBSIMPAN);
            this.xtraTabPage2.Controls.Add(label4);
            this.xtraTabPage2.Controls.Add(this.CMBGROUP);
            this.xtraTabPage2.Controls.Add(label5);
            this.xtraTabPage2.Controls.Add(this.TXTNAMAPT);
            this.xtraTabPage2.Controls.Add(label7);
            this.xtraTabPage2.Controls.Add(this.TXTKODEPT);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(959, 639);
            this.xtraTabPage2.Text = "PERUSAHAAN";
            // 
            // gridControl2
            // 
            this.gridControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gridControl2.Location = new System.Drawing.Point(23, 121);
            this.gridControl2.MainView = this.gridView2;
            this.gridControl2.Name = "gridControl2";
            this.gridControl2.Size = new System.Drawing.Size(916, 511);
            this.gridControl2.TabIndex = 4;
            this.gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView2});
            // 
            // gridView2
            // 
            this.gridView2.DetailHeight = 317;
            this.gridView2.GridControl = this.gridControl2;
            this.gridView2.Name = "gridView2";
            this.gridView2.OptionsBehavior.Editable = false;
            this.gridView2.OptionsView.ShowGroupPanel = false;
            // 
            // SBHAPUS
            // 
            this.SBHAPUS.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SBHAPUS.ImageOptions.Image")));
            this.SBHAPUS.Location = new System.Drawing.Point(306, 85);
            this.SBHAPUS.Name = "SBHAPUS";
            this.SBHAPUS.Size = new System.Drawing.Size(112, 31);
            this.SBHAPUS.TabIndex = 51;
            this.SBHAPUS.Text = "Hapus";
            this.SBHAPUS.Click += new System.EventHandler(this.SBHAPUS_Click);
            // 
            // SBSIMPAN
            // 
            this.SBSIMPAN.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("SBSIMPAN.ImageOptions.Image")));
            this.SBSIMPAN.Location = new System.Drawing.Point(188, 85);
            this.SBSIMPAN.Name = "SBSIMPAN";
            this.SBSIMPAN.Size = new System.Drawing.Size(112, 31);
            this.SBSIMPAN.TabIndex = 3;
            this.SBSIMPAN.Text = "Simpan";
            this.SBSIMPAN.Click += new System.EventHandler(this.SBSIMPAN_Click);
            // 
            // CMBGROUP
            // 
            this.CMBGROUP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMBGROUP.FormattingEnabled = true;
            this.CMBGROUP.Location = new System.Drawing.Point(675, 52);
            this.CMBGROUP.Name = "CMBGROUP";
            this.CMBGROUP.Size = new System.Drawing.Size(150, 27);
            this.CMBGROUP.TabIndex = 2;
            this.CMBGROUP.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CMBGROUP_KeyDown);
            // 
            // TXTNAMAPT
            // 
            this.TXTNAMAPT.Location = new System.Drawing.Point(188, 53);
            this.TXTNAMAPT.Name = "TXTNAMAPT";
            this.TXTNAMAPT.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TXTNAMAPT.Size = new System.Drawing.Size(413, 26);
            this.TXTNAMAPT.TabIndex = 1;
            this.TXTNAMAPT.ToolTip = "Nama Perusahaan";
            this.TXTNAMAPT.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Information;
            this.TXTNAMAPT.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TXTNAMAPT_KeyDown);
            // 
            // TXTKODEPT
            // 
            this.TXTKODEPT.Location = new System.Drawing.Point(188, 23);
            this.TXTKODEPT.Name = "TXTKODEPT";
            this.TXTKODEPT.Properties.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.TXTKODEPT.Size = new System.Drawing.Size(150, 26);
            this.TXTKODEPT.TabIndex = 0;
            this.TXTKODEPT.ToolTip = "Kode Perusahaan";
            this.TXTKODEPT.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Information;
            this.TXTKODEPT.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TXTKODEPT_KeyDown);
            // 
            // FrmCompany
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 673);
            this.Controls.Add(this.xtraTabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "FrmCompany";
            this.Text = "DAFTAR PERUSAHAAN";
            this.Load += new System.EventHandler(this.FrmCompany_Load);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            this.xtraTabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LEPT.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.radioGroup1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTWILAYAH.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTIDDATA.Properties)).EndInit();
            this.xtraTabPage2.ResumeLayout(false);
            this.xtraTabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTNAMAPT.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TXTKODEPT.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.RadioGroup radioGroup1;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SimpleButton btnhapus;
        private DevExpress.XtraEditors.SimpleButton btnsimpan;
        private DevExpress.XtraEditors.TextEdit TXTWILAYAH;
        private DevExpress.XtraEditors.TextEdit TXTIDDATA;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraEditors.SimpleButton SBHAPUS;
        private DevExpress.XtraEditors.SimpleButton SBSIMPAN;
        private System.Windows.Forms.ComboBox CMBGROUP;
        private DevExpress.XtraEditors.TextEdit TXTNAMAPT;
        private DevExpress.XtraEditors.TextEdit TXTKODEPT;
        private DevExpress.XtraEditors.LookUpEdit LEPT;
    }
}