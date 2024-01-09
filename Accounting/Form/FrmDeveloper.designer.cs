
namespace Accounting.Form
{
    partial class FrmDeveloper
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
            this.sbbrowse = new DevExpress.XtraEditors.SimpleButton();
            this.txtPath = new DevExpress.XtraEditors.LabelControl();
            this.SBImport = new DevExpress.XtraEditors.SimpleButton();
            this.lblrandom = new DevExpress.XtraEditors.LabelControl();
            this.memoEdit1 = new DevExpress.XtraEditors.MemoEdit();
            this.txtverifikasi = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtverifikasi.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // sbbrowse
            // 
            this.sbbrowse.ImageOptions.Image = global::Accounting.Properties.Resources.open2_16x16;
            this.sbbrowse.Location = new System.Drawing.Point(11, 7);
            this.sbbrowse.Margin = new System.Windows.Forms.Padding(2);
            this.sbbrowse.Name = "sbbrowse";
            this.sbbrowse.Size = new System.Drawing.Size(89, 20);
            this.sbbrowse.TabIndex = 0;
            this.sbbrowse.Text = "Browse File";
            this.sbbrowse.Click += new System.EventHandler(this.sbbrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(125, 10);
            this.txtPath.Margin = new System.Windows.Forms.Padding(2);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(48, 13);
            this.txtPath.TabIndex = 1;
            this.txtPath.Text = "Lokasi File";
            // 
            // SBImport
            // 
            this.SBImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SBImport.ImageOptions.Image = global::Accounting.Properties.Resources.newdatasource_16x16;
            this.SBImport.Location = new System.Drawing.Point(748, 11);
            this.SBImport.Margin = new System.Windows.Forms.Padding(2);
            this.SBImport.Name = "SBImport";
            this.SBImport.Size = new System.Drawing.Size(75, 20);
            this.SBImport.TabIndex = 3;
            this.SBImport.Text = "Execute";
            this.SBImport.Click += new System.EventHandler(this.SBImport_Click);
            // 
            // lblrandom
            // 
            this.lblrandom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblrandom.Location = new System.Drawing.Point(456, 14);
            this.lblrandom.Margin = new System.Windows.Forms.Padding(2);
            this.lblrandom.Name = "lblrandom";
            this.lblrandom.Size = new System.Drawing.Size(88, 13);
            this.lblrandom.TabIndex = 1;
            this.lblrandom.Text = "Verification Code :";
            // 
            // memoEdit1
            // 
            this.memoEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memoEdit1.Location = new System.Drawing.Point(12, 53);
            this.memoEdit1.Name = "memoEdit1";
            this.memoEdit1.Properties.ReadOnly = true;
            this.memoEdit1.Size = new System.Drawing.Size(822, 329);
            this.memoEdit1.TabIndex = 4;
            // 
            // txtverifikasi
            // 
            this.txtverifikasi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtverifikasi.Location = new System.Drawing.Point(643, 11);
            this.txtverifikasi.Name = "txtverifikasi";
            this.txtverifikasi.Size = new System.Drawing.Size(100, 20);
            this.txtverifikasi.TabIndex = 5;
            // 
            // FrmDeveloper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(846, 394);
            this.Controls.Add(this.txtverifikasi);
            this.Controls.Add(this.memoEdit1);
            this.Controls.Add(this.SBImport);
            this.Controls.Add(this.lblrandom);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.sbbrowse);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FrmDeveloper";
            this.Text = "Update Oracle Script";
            this.Load += new System.EventHandler(this.FrmDeveloper_Load);
            ((System.ComponentModel.ISupportInitialize)(this.memoEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtverifikasi.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton sbbrowse;
        private DevExpress.XtraEditors.LabelControl txtPath;
        private DevExpress.XtraEditors.SimpleButton SBImport;
        private DevExpress.XtraEditors.LabelControl lblrandom;
        private DevExpress.XtraEditors.MemoEdit memoEdit1;
        private DevExpress.XtraEditors.TextEdit txtverifikasi;
    }
}