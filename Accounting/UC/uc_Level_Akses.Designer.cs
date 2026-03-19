using System.Drawing;
using System.Windows.Forms;

namespace Accounting.UC
{
    partial class uc_Level_Akses
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(uc_Level_Akses));
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            lookUpEdit1 = new DevExpress.XtraEditors.LookUpEdit();
            xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            xtraTabPage1 = new DevExpress.XtraTab.XtraTabPage();
            sidePanel3 = new DevExpress.XtraEditors.SidePanel();
            sidePanel4 = new DevExpress.XtraEditors.SidePanel();
            btnremoveakses = new DevExpress.XtraEditors.SimpleButton();
            btncopycheckrow = new DevExpress.XtraEditors.SimpleButton();
            gridControl2 = new DevExpress.XtraGrid.GridControl();
            gridView2 = new DevExpress.XtraGrid.Views.Grid.GridView();
            sidePanel2 = new DevExpress.XtraEditors.SidePanel();
            groupControl1 = new DevExpress.XtraEditors.GroupControl();
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            xtraTabPage2 = new DevExpress.XtraTab.XtraTabPage();
            sidePanel1 = new DevExpress.XtraEditors.SidePanel();
            groupControl3 = new DevExpress.XtraEditors.GroupControl();
            gridControl4 = new DevExpress.XtraGrid.GridControl();
            gridView4 = new DevExpress.XtraGrid.Views.Grid.GridView();
            sidePanel6 = new DevExpress.XtraEditors.SidePanel();
            BtnRemoveUser = new DevExpress.XtraEditors.SimpleButton();
            BtnAddUser = new DevExpress.XtraEditors.SimpleButton();
            sidePanel5 = new DevExpress.XtraEditors.SidePanel();
            groupControl2 = new DevExpress.XtraEditors.GroupControl();
            gridControl3 = new DevExpress.XtraGrid.GridControl();
            gridView3 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)lookUpEdit1.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).BeginInit();
            xtraTabControl1.SuspendLayout();
            xtraTabPage1.SuspendLayout();
            sidePanel3.SuspendLayout();
            sidePanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView2).BeginInit();
            sidePanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl1).BeginInit();
            groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).BeginInit();
            xtraTabPage2.SuspendLayout();
            sidePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl3).BeginInit();
            groupControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView4).BeginInit();
            sidePanel6.SuspendLayout();
            sidePanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)groupControl2).BeginInit();
            groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridView3).BeginInit();
            SuspendLayout();
            // 
            // labelControl1
            // 
            labelControl1.Location = new Point(110, 23);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new Size(51, 13);
            labelControl1.TabIndex = 1;
            labelControl1.Text = "Akes Level";
            // 
            // lookUpEdit1
            // 
            lookUpEdit1.Location = new Point(167, 20);
            lookUpEdit1.Name = "lookUpEdit1";
            lookUpEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] { new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            lookUpEdit1.Size = new Size(132, 20);
            lookUpEdit1.TabIndex = 0;
            lookUpEdit1.EditValueChanged += lookUpEdit1_EditValueChanged;
            // 
            // xtraTabControl1
            // 
            xtraTabControl1.Dock = DockStyle.Fill;
            xtraTabControl1.Location = new Point(0, 0);
            xtraTabControl1.Name = "xtraTabControl1";
            xtraTabControl1.SelectedTabPage = xtraTabPage1;
            xtraTabControl1.Size = new Size(670, 391);
            xtraTabControl1.TabIndex = 2;
            xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { xtraTabPage1, xtraTabPage2 });
            // 
            // xtraTabPage1
            // 
            xtraTabPage1.Controls.Add(sidePanel3);
            xtraTabPage1.Controls.Add(sidePanel2);
            xtraTabPage1.Name = "xtraTabPage1";
            xtraTabPage1.Size = new Size(668, 366);
            xtraTabPage1.Text = "Roles";
            // 
            // sidePanel3
            // 
            sidePanel3.Controls.Add(sidePanel4);
            sidePanel3.Controls.Add(gridControl2);
            sidePanel3.Dock = DockStyle.Fill;
            sidePanel3.Location = new Point(366, 0);
            sidePanel3.Name = "sidePanel3";
            sidePanel3.Size = new Size(302, 366);
            sidePanel3.TabIndex = 3;
            sidePanel3.Text = "sidePanel3";
            // 
            // sidePanel4
            // 
            sidePanel4.Controls.Add(btnremoveakses);
            sidePanel4.Controls.Add(btncopycheckrow);
            sidePanel4.Controls.Add(labelControl1);
            sidePanel4.Controls.Add(lookUpEdit1);
            sidePanel4.Dock = DockStyle.Top;
            sidePanel4.Location = new Point(0, 0);
            sidePanel4.Name = "sidePanel4";
            sidePanel4.Size = new Size(302, 60);
            sidePanel4.TabIndex = 1;
            sidePanel4.Text = "sidePanel4";
            // 
            // btnremoveakses
            // 
            btnremoveakses.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btnremoveakses.ImageOptions.SvgImage");
            btnremoveakses.Location = new Point(7, 31);
            btnremoveakses.Name = "btnremoveakses";
            btnremoveakses.Size = new Size(85, 23);
            btnremoveakses.TabIndex = 2;
            btnremoveakses.Text = "Remove";
            btnremoveakses.Click += btnremoveakses_Click;
            // 
            // btncopycheckrow
            // 
            btncopycheckrow.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("btncopycheckrow.ImageOptions.SvgImage");
            btncopycheckrow.Location = new Point(7, 6);
            btncopycheckrow.Name = "btncopycheckrow";
            btncopycheckrow.Size = new Size(85, 23);
            btncopycheckrow.TabIndex = 2;
            btncopycheckrow.Text = "Add";
            btncopycheckrow.Click += btncopycheckrow_Click;
            // 
            // gridControl2
            // 
            gridControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridControl2.Location = new Point(0, 66);
            gridControl2.MainView = gridView2;
            gridControl2.Name = "gridControl2";
            gridControl2.Size = new Size(302, 300);
            gridControl2.TabIndex = 0;
            gridControl2.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView2 });
            // 
            // gridView2
            // 
            gridView2.GridControl = gridControl2;
            gridView2.Name = "gridView2";
            gridView2.OptionsView.ShowGroupPanel = false;
            // 
            // sidePanel2
            // 
            sidePanel2.Controls.Add(groupControl1);
            sidePanel2.Dock = DockStyle.Left;
            sidePanel2.Location = new Point(0, 0);
            sidePanel2.Name = "sidePanel2";
            sidePanel2.Size = new Size(366, 366);
            sidePanel2.TabIndex = 2;
            sidePanel2.Text = "sidePanel2";
            // 
            // groupControl1
            // 
            groupControl1.Controls.Add(gridControl1);
            groupControl1.Dock = DockStyle.Fill;
            groupControl1.Location = new Point(0, 0);
            groupControl1.Name = "groupControl1";
            groupControl1.Size = new Size(365, 366);
            groupControl1.TabIndex = 1;
            groupControl1.Text = "Daftar Akses";
            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.Location = new Point(2, 23);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new Size(361, 341);
            gridControl1.TabIndex = 0;
            gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView1 });
            // 
            // gridView1
            // 
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // xtraTabPage2
            // 
            xtraTabPage2.Controls.Add(sidePanel1);
            xtraTabPage2.Controls.Add(sidePanel5);
            xtraTabPage2.Name = "xtraTabPage2";
            xtraTabPage2.Size = new Size(668, 366);
            xtraTabPage2.Text = "Users";
            // 
            // sidePanel1
            // 
            sidePanel1.Controls.Add(groupControl3);
            sidePanel1.Controls.Add(sidePanel6);
            sidePanel1.Dock = DockStyle.Fill;
            sidePanel1.Location = new Point(347, 0);
            sidePanel1.Name = "sidePanel1";
            sidePanel1.Size = new Size(321, 366);
            sidePanel1.TabIndex = 2;
            sidePanel1.Text = "sidePanel1";
            // 
            // groupControl3
            // 
            groupControl3.Controls.Add(gridControl4);
            groupControl3.Dock = DockStyle.Fill;
            groupControl3.Location = new Point(0, 60);
            groupControl3.Name = "groupControl3";
            groupControl3.Size = new Size(321, 306);
            groupControl3.TabIndex = 4;
            groupControl3.Text = "Daftar Level User";
            // 
            // gridControl4
            // 
            gridControl4.Dock = DockStyle.Fill;
            gridControl4.Location = new Point(2, 23);
            gridControl4.MainView = gridView4;
            gridControl4.Name = "gridControl4";
            gridControl4.Size = new Size(317, 281);
            gridControl4.TabIndex = 3;
            gridControl4.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView4 });
            // 
            // gridView4
            // 
            gridView4.GridControl = gridControl4;
            gridView4.Name = "gridView4";
            gridView4.OptionsView.ShowGroupPanel = false;
            // 
            // sidePanel6
            // 
            sidePanel6.Controls.Add(BtnRemoveUser);
            sidePanel6.Controls.Add(BtnAddUser);
            sidePanel6.Dock = DockStyle.Top;
            sidePanel6.Location = new Point(0, 0);
            sidePanel6.Name = "sidePanel6";
            sidePanel6.Size = new Size(321, 60);
            sidePanel6.TabIndex = 2;
            sidePanel6.Text = "sidePanel6";
            // 
            // BtnRemoveUser
            // 
            BtnRemoveUser.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("BtnRemoveUser.ImageOptions.SvgImage");
            BtnRemoveUser.Location = new Point(7, 31);
            BtnRemoveUser.Name = "BtnRemoveUser";
            BtnRemoveUser.Size = new Size(85, 23);
            BtnRemoveUser.TabIndex = 2;
            BtnRemoveUser.Text = "Remove";
            BtnRemoveUser.Click += BtnRemoveUser_Click;
            // 
            // BtnAddUser
            // 
            BtnAddUser.ImageOptions.SvgImage = (DevExpress.Utils.Svg.SvgImage)resources.GetObject("BtnAddUser.ImageOptions.SvgImage");
            BtnAddUser.Location = new Point(7, 6);
            BtnAddUser.Name = "BtnAddUser";
            BtnAddUser.Size = new Size(85, 23);
            BtnAddUser.TabIndex = 2;
            BtnAddUser.Text = "Add";
            BtnAddUser.Click += BtnAddUser_Click;
            // 
            // sidePanel5
            // 
            sidePanel5.Controls.Add(groupControl2);
            sidePanel5.Dock = DockStyle.Left;
            sidePanel5.Location = new Point(0, 0);
            sidePanel5.Name = "sidePanel5";
            sidePanel5.Size = new Size(347, 366);
            sidePanel5.TabIndex = 1;
            sidePanel5.Text = "sidePanel5";
            // 
            // groupControl2
            // 
            groupControl2.Controls.Add(gridControl3);
            groupControl2.Dock = DockStyle.Fill;
            groupControl2.Location = new Point(0, 0);
            groupControl2.Name = "groupControl2";
            groupControl2.Size = new Size(346, 366);
            groupControl2.TabIndex = 2;
            groupControl2.Text = "Daftar User";
            // 
            // gridControl3
            // 
            gridControl3.Dock = DockStyle.Fill;
            gridControl3.Location = new Point(2, 23);
            gridControl3.MainView = gridView3;
            gridControl3.Name = "gridControl3";
            gridControl3.Size = new Size(342, 341);
            gridControl3.TabIndex = 0;
            gridControl3.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { gridView3 });
            // 
            // gridView3
            // 
            gridView3.GridControl = gridControl3;
            gridView3.Name = "gridView3";
            gridView3.OptionsView.ShowGroupPanel = false;
            // 
            // uc_Level_Akses
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(xtraTabControl1);
            Name = "uc_Level_Akses";
            Size = new Size(670, 391);
            Load += uc_Level_Akses_Load;
            ((System.ComponentModel.ISupportInitialize)lookUpEdit1.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)xtraTabControl1).EndInit();
            xtraTabControl1.ResumeLayout(false);
            xtraTabPage1.ResumeLayout(false);
            sidePanel3.ResumeLayout(false);
            sidePanel4.ResumeLayout(false);
            sidePanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridControl2).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView2).EndInit();
            sidePanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl1).EndInit();
            groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl1).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView1).EndInit();
            xtraTabPage2.ResumeLayout(false);
            sidePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl3).EndInit();
            groupControl3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl4).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView4).EndInit();
            sidePanel6.ResumeLayout(false);
            sidePanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)groupControl2).EndInit();
            groupControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridControl3).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridView3).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LookUpEdit lookUpEdit1;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage1;
        private DevExpress.XtraTab.XtraTabPage xtraTabPage2;
        private DevExpress.XtraEditors.SidePanel sidePanel2;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraEditors.SidePanel sidePanel3;
        private DevExpress.XtraGrid.GridControl gridControl2;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView2;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.SidePanel sidePanel4;
        private DevExpress.XtraEditors.SimpleButton btnremoveakses;
        private DevExpress.XtraEditors.SimpleButton btncopycheckrow;
        private DevExpress.XtraEditors.SidePanel sidePanel5;
        private DevExpress.XtraEditors.SidePanel sidePanel1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraGrid.GridControl gridControl3;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView3;
        private DevExpress.XtraEditors.GroupControl groupControl3;
        private DevExpress.XtraGrid.GridControl gridControl4;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView4;
        private DevExpress.XtraEditors.SidePanel sidePanel6;
        private DevExpress.XtraEditors.SimpleButton BtnRemoveUser;
        private DevExpress.XtraEditors.SimpleButton BtnAddUser;
    }
}
