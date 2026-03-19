
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    partial class FrmUsers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmUsers));
            gridControl2 = new DevExpress.XtraGrid.GridControl();
            gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            lookUpEdit1 = new DevExpress.XtraEditors.LookUpEdit();
            layoutControl2 = new DevExpress.XtraLayout.LayoutControl();
            lbluser = new DevExpress.XtraEditors.LabelControl();
            lblmodule = new DevExpress.XtraEditors.LabelControl();
            layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem10 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem11 = new DevExpress.XtraLayout.LayoutControlItem();
            btnaddestate = new DevExpress.XtraEditors.SimpleButton();
            dEPTTextEdit = new DevExpress.XtraEditors.TextEdit();
            layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            uSERIDTextEdit = new DevExpress.XtraEditors.TextEdit();
            pASSWORDTextEdit = new DevExpress.XtraEditors.TextEdit();
            jABATANTextEdit = new DevExpress.XtraEditors.TextEdit();
            confirmasi = new DevExpress.XtraEditors.TextEdit();
            nAMATextEdit = new DevExpress.XtraEditors.TextEdit();
            checkEditaktif = new DevExpress.XtraEditors.CheckEdit();
            Root = new DevExpress.XtraLayout.LayoutControlGroup();
            layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem();
            layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            layoutControlItem15 = new DevExpress.XtraLayout.LayoutControlItem();
            btnhapus = new DevExpress.XtraEditors.SimpleButton();
            btnsimpan = new DevExpress.XtraEditors.SimpleButton();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            layoutControlItem12 = new DevExpress.XtraLayout.LayoutControlItem();
            imageCollection1 = new DevExpress.Utils.ImageCollection(components);
            sbbaru = new DevExpress.XtraEditors.SimpleButton();
            btnResetPassword = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)gridControl2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lookUpEdit1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControl2).BeginInit();
            layoutControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)layoutControlGroup1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem10).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem11).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dEPTTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControl1).BeginInit();
            layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uSERIDTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pASSWORDTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)jABATANTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)confirmasi.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nAMATextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)checkEditaktif.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Root).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem15).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem12).BeginInit();
            ((System.ComponentModel.ISupportInitialize)imageCollection1).BeginInit();
            SuspendLayout();
            // 
            // gridControl2
            // 
            gridControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridControl2.EmbeddedNavigator.Margin = new Padding(2);
            gridControl2.Location = new Point(633, 273);
            gridControl2.MainView = gridView2;
            gridControl2.Margin = new Padding(2);
            gridControl2.Name = "gridControl2";
            gridControl2.Size = new Size(562, 253);
            gridControl2.TabIndex = 19;
            gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView2 });
            // 
            // gridView2
            // 
            gridView2.DetailHeight = 217;
            gridView2.GridControl = gridControl2;
            gridView2.Name = "gridView2";
            gridView2.OptionsBehavior.Editable = false;
            gridView2.OptionsView.ShowGroupPanel = false;
            gridView2.PopupMenuShowing += gridView2_PopupMenuShowing;
            // 
            // lookUpEdit1
            // 
            lookUpEdit1.Location = new Point(60, 70);
            lookUpEdit1.Margin = new Padding(2);
            lookUpEdit1.Name = "lookUpEdit1";
            lookUpEdit1.Properties.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            lookUpEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookUpEdit1.Size = new Size(339, 20);
            lookUpEdit1.StyleController = layoutControl2;
            lookUpEdit1.TabIndex = 0;
            // 
            // layoutControl2
            // 
            layoutControl2.Controls.Add(lookUpEdit1);
            layoutControl2.Controls.Add(lbluser);
            layoutControl2.Controls.Add(lblmodule);
            layoutControl2.Location = new Point(623, 156);
            layoutControl2.Name = "layoutControl2";
            layoutControl2.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new Rectangle(435, 301, 650, 400);
            layoutControl2.Root = layoutControlGroup1;
            layoutControl2.Size = new Size(411, 112);
            layoutControl2.TabIndex = 29;
            layoutControl2.Text = "layoutControl2";
            // 
            // lbluser
            // 
            lbluser.Appearance.Font = new Font("Tahoma", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            lbluser.Appearance.Options.UseFont = true;
            lbluser.Location = new Point(60, 12);
            lbluser.Margin = new Padding(2);
            lbluser.Name = "lbluser";
            lbluser.Size = new Size(45, 25);
            lbluser.StyleController = layoutControl2;
            lbluser.TabIndex = 1;
            lbluser.Text = "user";
            // 
            // lblmodule
            // 
            lblmodule.Appearance.Font = new Font("Tahoma", 15.75F, FontStyle.Bold, GraphicsUnit.Point);
            lblmodule.Appearance.Options.UseFont = true;
            lblmodule.Location = new Point(60, 41);
            lblmodule.Margin = new Padding(2);
            lblmodule.Name = "lblmodule";
            lblmodule.Size = new Size(76, 25);
            lblmodule.StyleController = layoutControl2;
            lblmodule.TabIndex = 1;
            lblmodule.Text = "Module";
            // 
            // layoutControlGroup1
            // 
            layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            layoutControlGroup1.GroupBordersVisible = false;
            layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutControlItem7, layoutControlItem10, layoutControlItem11 });
            layoutControlGroup1.Name = "Root";
            layoutControlGroup1.Size = new Size(411, 112);
            layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            layoutControlItem7.Control = lbluser;
            layoutControlItem7.Location = new Point(0, 0);
            layoutControlItem7.Name = "layoutControlItem7";
            layoutControlItem7.Size = new Size(391, 29);
            layoutControlItem7.Text = "User ID";
            layoutControlItem7.TextSize = new Size(36, 13);
            // 
            // layoutControlItem10
            // 
            layoutControlItem10.Control = lblmodule;
            layoutControlItem10.Location = new Point(0, 29);
            layoutControlItem10.Name = "layoutControlItem10";
            layoutControlItem10.Size = new Size(391, 29);
            layoutControlItem10.Text = "Module";
            layoutControlItem10.TextSize = new Size(36, 13);
            // 
            // layoutControlItem11
            // 
            layoutControlItem11.Control = lookUpEdit1;
            layoutControlItem11.Location = new Point(0, 58);
            layoutControlItem11.Name = "layoutControlItem11";
            layoutControlItem11.Size = new Size(391, 34);
            layoutControlItem11.Text = "Lokasi";
            layoutControlItem11.TextSize = new Size(36, 13);
            // 
            // btnaddestate
            // 
            btnaddestate.Location = new Point(1039, 218);
            btnaddestate.Margin = new Padding(2);
            btnaddestate.Name = "btnaddestate";
            btnaddestate.Size = new Size(67, 36);
            btnaddestate.TabIndex = 3;
            btnaddestate.Text = "Add";
            btnaddestate.Click += btnaddestate_Click;
            // 
            // dEPTTextEdit
            // 
            dEPTTextEdit.Location = new Point(110, 84);
            dEPTTextEdit.Margin = new Padding(2);
            dEPTTextEdit.Name = "dEPTTextEdit";
            dEPTTextEdit.Size = new Size(84, 20);
            dEPTTextEdit.StyleController = layoutControl1;
            dEPTTextEdit.TabIndex = 5;
            dEPTTextEdit.KeyDown += dEPTTextEdit_KeyDown;
            // 
            // layoutControl1
            // 
            layoutControl1.Controls.Add(uSERIDTextEdit);
            layoutControl1.Controls.Add(pASSWORDTextEdit);
            layoutControl1.Controls.Add(jABATANTextEdit);
            layoutControl1.Controls.Add(dEPTTextEdit);
            layoutControl1.Controls.Add(confirmasi);
            layoutControl1.Controls.Add(nAMATextEdit);
            layoutControl1.Controls.Add(checkEditaktif);
            layoutControl1.Location = new Point(22, 2);
            layoutControl1.Name = "layoutControl1";
            layoutControl1.OptionsCustomizationForm.DesignTimeCustomizationFormPositionAndSize = new Rectangle(464, 0, 650, 400);
            layoutControl1.Root = Root;
            layoutControl1.Size = new Size(394, 155);
            layoutControl1.TabIndex = 28;
            layoutControl1.Text = "layoutControl1";
            // 
            // uSERIDTextEdit
            // 
            uSERIDTextEdit.Location = new Point(110, 12);
            uSERIDTextEdit.Margin = new Padding(2);
            uSERIDTextEdit.Name = "uSERIDTextEdit";
            uSERIDTextEdit.Size = new Size(272, 20);
            uSERIDTextEdit.StyleController = layoutControl1;
            uSERIDTextEdit.TabIndex = 0;
            uSERIDTextEdit.KeyDown += uSERIDTextEdit_KeyDown;
            // 
            // pASSWORDTextEdit
            // 
            pASSWORDTextEdit.Location = new Point(110, 36);
            pASSWORDTextEdit.Margin = new Padding(2);
            pASSWORDTextEdit.Name = "pASSWORDTextEdit";
            pASSWORDTextEdit.Properties.PasswordChar = '*';
            pASSWORDTextEdit.Size = new Size(84, 20);
            pASSWORDTextEdit.StyleController = layoutControl1;
            pASSWORDTextEdit.TabIndex = 2;
            pASSWORDTextEdit.KeyDown += pASSWORDTextEdit_KeyDown;
            // 
            // jABATANTextEdit
            // 
            jABATANTextEdit.Location = new Point(296, 84);
            jABATANTextEdit.Margin = new Padding(2);
            jABATANTextEdit.Name = "jABATANTextEdit";
            jABATANTextEdit.Size = new Size(86, 20);
            jABATANTextEdit.StyleController = layoutControl1;
            jABATANTextEdit.TabIndex = 6;
            jABATANTextEdit.KeyDown += jABATANTextEdit_KeyDown;
            // 
            // confirmasi
            // 
            confirmasi.Location = new Point(296, 36);
            confirmasi.Margin = new Padding(2);
            confirmasi.Name = "confirmasi";
            confirmasi.Properties.PasswordChar = '*';
            confirmasi.Size = new Size(86, 20);
            confirmasi.StyleController = layoutControl1;
            confirmasi.TabIndex = 3;
            confirmasi.KeyDown += confirmasi_KeyDown;
            // 
            // nAMATextEdit
            // 
            nAMATextEdit.Location = new Point(110, 60);
            nAMATextEdit.Margin = new Padding(2);
            nAMATextEdit.Name = "nAMATextEdit";
            nAMATextEdit.Size = new Size(272, 20);
            nAMATextEdit.StyleController = layoutControl1;
            nAMATextEdit.TabIndex = 4;
            nAMATextEdit.KeyDown += nAMATextEdit_KeyDown;
            // 
            // checkEditaktif
            // 
            checkEditaktif.Location = new Point(12, 108);
            checkEditaktif.Name = "checkEditaktif";
            checkEditaktif.Properties.Caption = "Aktif";
            checkEditaktif.Size = new Size(370, 20);
            checkEditaktif.StyleController = layoutControl1;
            checkEditaktif.TabIndex = 7;
            checkEditaktif.Visible = false;
            // 
            // Root
            // 
            Root.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            Root.GroupBordersVisible = false;
            Root.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] { layoutControlItem1, emptySpaceItem1, layoutControlItem2, layoutControlItem4, layoutControlItem5, layoutControlItem3, layoutControlItem6, layoutControlItem15 });
            Root.Name = "Root";
            Root.Size = new Size(394, 155);
            Root.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            layoutControlItem1.Control = uSERIDTextEdit;
            layoutControlItem1.Location = new Point(0, 0);
            layoutControlItem1.Name = "layoutControlItem1";
            layoutControlItem1.Size = new Size(374, 24);
            layoutControlItem1.Text = "User ID";
            layoutControlItem1.TextSize = new Size(86, 13);
            // 
            // emptySpaceItem1
            // 
            emptySpaceItem1.AllowHotTrack = false;
            emptySpaceItem1.Location = new Point(0, 120);
            emptySpaceItem1.Name = "emptySpaceItem1";
            emptySpaceItem1.Size = new Size(374, 15);
            emptySpaceItem1.TextSize = new Size(0, 0);
            // 
            // layoutControlItem2
            // 
            layoutControlItem2.Control = pASSWORDTextEdit;
            layoutControlItem2.Location = new Point(0, 24);
            layoutControlItem2.Name = "layoutControlItem2";
            layoutControlItem2.Size = new Size(186, 24);
            layoutControlItem2.Text = "Password";
            layoutControlItem2.TextSize = new Size(86, 13);
            // 
            // layoutControlItem4
            // 
            layoutControlItem4.Control = nAMATextEdit;
            layoutControlItem4.Location = new Point(0, 48);
            layoutControlItem4.Name = "layoutControlItem4";
            layoutControlItem4.Size = new Size(374, 24);
            layoutControlItem4.Text = "Nama";
            layoutControlItem4.TextSize = new Size(86, 13);
            // 
            // layoutControlItem5
            // 
            layoutControlItem5.Control = dEPTTextEdit;
            layoutControlItem5.Location = new Point(0, 72);
            layoutControlItem5.Name = "layoutControlItem5";
            layoutControlItem5.Size = new Size(186, 24);
            layoutControlItem5.Text = "Departement";
            layoutControlItem5.TextSize = new Size(86, 13);
            // 
            // layoutControlItem3
            // 
            layoutControlItem3.Control = confirmasi;
            layoutControlItem3.Location = new Point(186, 24);
            layoutControlItem3.Name = "layoutControlItem3";
            layoutControlItem3.Size = new Size(188, 24);
            layoutControlItem3.Text = "Confirm Password";
            layoutControlItem3.TextSize = new Size(86, 13);
            // 
            // layoutControlItem6
            // 
            layoutControlItem6.Control = jABATANTextEdit;
            layoutControlItem6.Location = new Point(186, 72);
            layoutControlItem6.Name = "layoutControlItem6";
            layoutControlItem6.Size = new Size(188, 24);
            layoutControlItem6.Text = "Jabatan";
            layoutControlItem6.TextSize = new Size(86, 13);
            // 
            // layoutControlItem15
            // 
            layoutControlItem15.Control = checkEditaktif;
            layoutControlItem15.Location = new Point(0, 96);
            layoutControlItem15.Name = "layoutControlItem15";
            layoutControlItem15.Size = new Size(374, 24);
            layoutControlItem15.Text = "laktif";
            layoutControlItem15.TextSize = new Size(0, 0);
            layoutControlItem15.TextVisible = false;
            // 
            // btnhapus
            // 
            btnhapus.Location = new Point(434, 92);
            btnhapus.Margin = new Padding(2);
            btnhapus.Name = "btnhapus";
            btnhapus.Size = new Size(90, 36);
            btnhapus.TabIndex = 8;
            btnhapus.Text = "Hapus";
            btnhapus.Visible = false;
            btnhapus.Click += btnhapus_Click;
            // 
            // btnsimpan
            // 
            btnsimpan.Location = new Point(434, 54);
            btnsimpan.Margin = new Padding(2);
            btnsimpan.Name = "btnsimpan";
            btnsimpan.Size = new Size(90, 36);
            btnsimpan.TabIndex = 9;
            btnsimpan.Text = "Add";
            btnsimpan.Click += btnsimpan_Click;
            // 
            // gridControl1
            // 
            gridControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            gridControl1.EmbeddedNavigator.Margin = new Padding(2);
            gridControl1.Location = new Point(22, 162);
            gridControl1.MainView = gridView1;
            gridControl1.Margin = new Padding(2);
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new Size(551, 364);
            gridControl1.TabIndex = 26;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.DetailHeight = 217;
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsFind.AlwaysVisible = true;
            gridView1.OptionsFind.ShowFindButton = false;
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            // 
            // layoutControlItem12
            // 
            layoutControlItem12.Location = new Point(0, 83);
            layoutControlItem12.Name = "layoutControlItem12";
            layoutControlItem12.Size = new Size(168, 25);
            layoutControlItem12.TextSize = new Size(50, 20);
            // 
            // imageCollection1
            // 
            imageCollection1.ImageStream = (DevExpress.Utils.ImageCollectionStreamer)resources.GetObject("imageCollection1.ImageStream");
            imageCollection1.Images.SetKeyName(0, "deletelist_16x16.png");
            // 
            // sbbaru
            // 
            sbbaru.Location = new Point(434, 17);
            sbbaru.Margin = new Padding(2);
            sbbaru.Name = "sbbaru";
            sbbaru.Size = new Size(90, 36);
            sbbaru.TabIndex = 8;
            sbbaru.Text = "Baru";
            sbbaru.Click += sbbaru_Click;
            //
            // btnResetPassword
            //
            btnResetPassword.Location = new Point(434, 130);
            btnResetPassword.Margin = new Padding(2);
            btnResetPassword.Name = "btnResetPassword";
            btnResetPassword.Size = new Size(90, 36);
            btnResetPassword.TabIndex = 30;
            btnResetPassword.Text = "Reset Pwd";
            btnResetPassword.Visible = false;
            btnResetPassword.Click += btnResetPassword_Click;
            //
            // FrmUsers
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1202, 552);
            Controls.Add(layoutControl2);
            Controls.Add(layoutControl1);
            Controls.Add(gridControl1);
            Controls.Add(gridControl2);
            Controls.Add(btnaddestate);
            Controls.Add(sbbaru);
            Controls.Add(btnResetPassword);
            Controls.Add(btnhapus);
            Controls.Add(btnsimpan);
            Margin = new Padding(2);
            Name = "FrmUsers";
            Text = "Daftar User";
            Load += FrmUsers_Load;
            ((System.ComponentModel.ISupportInitialize)gridControl2).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)lookUpEdit1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControl2).EndInit();
            layoutControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)layoutControlGroup1).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem7).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem10).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem11).EndInit();
            ((System.ComponentModel.ISupportInitialize)dEPTTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControl1).EndInit();
            layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)uSERIDTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pASSWORDTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)jABATANTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)confirmasi.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)nAMATextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)checkEditaktif.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)Root).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem1).EndInit();
            ((System.ComponentModel.ISupportInitialize)emptySpaceItem1).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem2).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem4).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem5).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem3).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem6).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem15).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)layoutControlItem12).EndInit();
            ((System.ComponentModel.ISupportInitialize)imageCollection1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraEditors.LookUpEdit lookUpEdit1;
        private DevExpress.XtraEditors.SimpleButton btnaddestate;
        private DevExpress.XtraEditors.TextEdit dEPTTextEdit;
        private DevExpress.XtraEditors.TextEdit jABATANTextEdit;
        private DevExpress.XtraEditors.TextEdit nAMATextEdit;
        private DevExpress.XtraEditors.TextEdit pASSWORDTextEdit;
        private DevExpress.XtraEditors.TextEdit uSERIDTextEdit;
        private DevExpress.XtraEditors.SimpleButton btnsimpan;
        private DevExpress.XtraEditors.SimpleButton btnhapus;
        private DevExpress.XtraEditors.TextEdit confirmasi;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.LabelControl lbluser;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup Root;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.LabelControl lblmodule;
        private DevExpress.XtraLayout.LayoutControl layoutControl2;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem12;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem10;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem11;
        private DevExpress.Utils.ImageCollection imageCollection1;
        private DevExpress.XtraEditors.CheckEdit checkEditaktif;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem15;
        private DevExpress.XtraEditors.SimpleButton sbbaru;
        private DevExpress.XtraEditors.SimpleButton btnResetPassword;
    }
}