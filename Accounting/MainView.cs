using Accounting._3.Services;
using Accounting.BusinessLayer;
using Accounting.Form;
using Accounting.Laporan;
using Accounting.Services;
using Accounting.UC;
using DevExpress.XtraBars;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraBars.Docking2010.Views.Tabbed;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Accounting
{
    public partial class MainView : DevExpress.XtraEditors.XtraForm
    {
        private enum FixedAssetMenuAction
        {
            MasterData,
            Cip,
            Reports,
            DepreciationRun,
            LifecycleTransaction,
            ApprovalAction
        }

        public MainView()
        {
            InitializeComponent();
            this.FormClosing += FrmMain_FormClosing;
        }
        //setting skins

        private void ShowSwatchPicker(XtraForm owner)
        {
            using (var dialog = new DevExpress.Customization.SvgSkinPaletteSelector(owner))
            {
                dialog.ShowDialog();
                SavePalette();
            }
        }

        private void SavePalette()
        {
            var settings = Properties.Settings.Default;
            settings.SkinName = UserLookAndFeel.Default.SkinName;
            settings.Palette = UserLookAndFeel.Default.ActiveSvgPaletteName;
            settings.Save();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RestorePalette();
        }

        private void RestorePalette()
        {
            var settings = Properties.Settings.Default;
            if (!String.IsNullOrEmpty(settings.SkinName))
            {
                if (!String.IsNullOrEmpty(settings.SkinName))
                {
                    UserLookAndFeel.Default.SetSkinStyle(settings.SkinName, settings.Palette);
                }
                else
                    UserLookAndFeel.Default.SetSkinStyle(settings.SkinName);
            }
        }

        public string SetValueForPeriode
        {
            set
            {
                var y= "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.IDDATA).ToString();
                if (value == y)
                    bbiperiode.Caption = value;
            }
        }
        private void barButtonItem31_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
            Application.Exit();
           // Environment.Exit(0);
        }
        private void AddDocumentManager()
        {
            DocumentManager manager = new()
            {
                MdiParent = this,
                View = new TabbedView()
            };
        }

        private void OnFixedAssetMasterClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.MasterData))
            {
                return;
            }

            OpenMDI<FrmFixedAssetMaster>(false);
        }

        private void OnFixedAssetCipClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.Cip))
            {
                return;
            }

            OpenMDI<FrmFixedAssetCip>(false);
        }

        private void OnFixedAssetReportsClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.Reports))
            {
                return;
            }

            OpenMDI<FrmFixedAssetReports>(false);
        }

        private void OnFixedAssetLifecycleClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.LifecycleTransaction))
            {
                return;
            }

            OpenMDI<FrmFixedAssetLifecycle>(false);
        }

        private void OnFixedAssetDepreciationClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.DepreciationRun))
            {
                return;
            }

            OpenMDI<FrmFixedAssetDepreciation>(false);
        }

        private void OnFixedAssetApprovalClick(object sender, ItemClickEventArgs e)
        {
            if (!EnsureFixedAssetAccess(FixedAssetMenuAction.ApprovalAction))
            {
                return;
            }

            OpenMDI<FrmFixedAssetApproval>(false);
        }

        private bool EnsureFixedAssetAccess(FixedAssetMenuAction action)
        {
            bool isAllowed = action switch
            {
                FixedAssetMenuAction.MasterData => AuthorizationService.CanViewFixedAssetWorkspace(),
                FixedAssetMenuAction.Cip => AuthorizationService.CanViewFixedAssetWorkspace(),
                FixedAssetMenuAction.Reports => AuthorizationService.CanViewFixedAssetWorkspace(),
                FixedAssetMenuAction.DepreciationRun => AuthorizationService.CanRunFixedAssetDepreciation(),
                FixedAssetMenuAction.LifecycleTransaction => AuthorizationService.CanCreateFixedAssetLifecycle(),
                FixedAssetMenuAction.ApprovalAction => AuthorizationService.CanApproveFixedAssetTransaction(),
                _ => false
            };
            if (isAllowed)
            {
                return true;
            }

            string actionName = action switch
            {
                FixedAssetMenuAction.MasterData => "Data Master Aset",
                FixedAssetMenuAction.Cip => "Konstruksi Dalam Pengerjaan",
                FixedAssetMenuAction.Reports => "Laporan Aset Tetap",
                FixedAssetMenuAction.DepreciationRun => "Proses Penyusutan",
                FixedAssetMenuAction.LifecycleTransaction => "Transaksi Siklus Aset",
                FixedAssetMenuAction.ApprovalAction => "Persetujuan",
                _ => "Menu Aset Tetap"
            };
            string role = string.IsNullOrWhiteSpace(LoginInfo.role) ? "-" : LoginInfo.role.Trim();
            XtraMessageBox.Show(
                $"Role '{role}' tidak memiliki akses ke menu {actionName}.",
                "Akses Ditolak",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private bool EnsureAccess(Func<bool> accessCheck, string message)
        {
            if (accessCheck())
            {
                return true;
            }

            XtraMessageBox.Show(message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        private void barButtonItem35_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanViewCoaWorkspace, "Anda tidak memiliki izin membuka workspace chart of account."))
                {
                    return;
                }

                OpenMDI<FrmAkunEF>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OpenMDI<T>(bool multipleInstances)    where T : XtraForm, new()
        {
            if (multipleInstances == false)
            {
                // Look if the form is open
                foreach (XtraForm f in this.MdiChildren)
                {
                    if (f.GetType() == typeof(T))
                    {
                        // Found an open instance. If minimized, maximize and activate
                        if (f.WindowState == FormWindowState.Minimized)
                        {
                            f.WindowState = FormWindowState.Maximized;
                        }

                        f.Activate();
                        return;
                    }
                }
            }

            T newForm = new T();
            newForm.MdiParent = this;
            newForm.Show();
            newForm.Focus();
        }
        private void barButtonItem39_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanImportJurnal, "Anda tidak memiliki izin membuka import jurnal."))
                {
                    return;
                }

                OpenMDI<FrmImportJurnal_Parsial>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem11_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanViewJurnalWorkspace, "Anda tidak memiliki izin membuka daftar jurnal."))
                {
                    return;
                }
                
                OpenMDI<FrmDaftarJurnal>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem18_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!EnsureAccess(AuthorizationService.CanViewJurnalWorkspace, "Anda tidak memiliki izin membuka input jurnal."))
            {
                return;
            }

                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);
                FrmJurnal fjurnal = new();
                fjurnal.Show(this);   
            
        }

       

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePalette();
            Application.Exit();

        }

        private void barButtonItem34_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////19 kode tutup buku bulanan
                //bool akses = LevelAksesServices.OpenForm(19, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
             
               
                FrmClosingMonth f1 = new ();
                f1.StartPosition = FormStartPosition.CenterScreen;
                f1.ShowDialog();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem33_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////20 kode tutup buku tahun
                //bool akses = LevelAksesServices.OpenForm(20, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
               
                FrmClosingYear f1 = new();
                f1.StartPosition = FormStartPosition.CenterScreen;
                f1.ShowDialog();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem23_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ////23 kode buka laporan daftar perkiraan
            //bool akses = LevelAksesServices.OpenForm(23, LoginInfo.userID);
            //if (akses == false)
            //{
            //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
        }

        private void barButtonItem19_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //24 kode buka laporan history gl
            if (!EnsureAccess(AuthorizationService.CanViewReports, "Anda tidak memiliki izin membuka laporan history GL."))
            {
                return;
            }
        }

      
        private void BSETIMPORTCOA_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////39 kode buka SETTING PERIODE
                //bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
              
                if (!EnsureAccess(AuthorizationService.CanImportCoa, "Anda tidak memiliki izin membuka import chart of account."))
                {
                    return;
                }

                OpenMDI<FrmImportCOA>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BSETIMPORTJUR_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //39 kode buka SETTING PERIODE
                //bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
         
                OpenMDI<FrmImportJurnalSheet>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BSETGENERATE_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                //if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                //{
                //    XtraMessageBox.Show("Module ini hanya dapat digunakan untuk COA berjenis kebun...!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
             
                if (!EnsureAccess(AuthorizationService.CanUpdateCoa, "Anda tidak memiliki izin membuka pengaturan blok."))
                {
                    return;
                }

                OpenMDI<FrmMasterBlok>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void BSETUSER_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!AuthorizationService.CanViewUserManagement())
            {
                XtraMessageBox.Show("Anda tidak memiliki izin membuka manajemen user.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenMDI<FrmUsers>(false);
        }

        private void BSETPERIODE_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
           
                if (!EnsureAccess(AuthorizationService.CanManageAccountingPeriods, "Anda tidak memiliki izin membuka pengaturan periode akuntansi."))
                {
                    return;
                }

                OpenMDI<FrmPeriode>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BSETAKSES_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!AuthorizationService.CanViewRoleManagement())
            {
                XtraMessageBox.Show("Anda tidak memiliki izin membuka pengaturan role dan permission.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddUserControlTab(typeof(uc_Level_Akses), "Pengaturan Akses User");
        }
        private void AddUserControlTab(Type userControlType, string tabText)
        {
            // Ensure the provided type is a UserControl
            if (!typeof(UserControl).IsAssignableFrom(userControlType))
            {
                throw new ArgumentException("Type must be a UserControl.", nameof(userControlType));
            }

            // Check if the user control is already loaded
            foreach (System.Windows.Forms.Form mdiChild in this.MdiChildren)
            {
                foreach (Control control in mdiChild.Controls)
                {
                    if (control.GetType() == userControlType)
                    {
                        // Activate the existing tab with the user control
                        mdiChild.Activate();
                        return;
                    }
                }
            }

            // If not found, create a new child form with the user control
            var childForm = new System.Windows.Forms.Form
            {
                MdiParent = this,
                Text = tabText,
                WindowState = FormWindowState.Maximized
            };

            try
            {
                var userControl = (UserControl)Activator.CreateInstance(userControlType);
                userControl.Dock = DockStyle.Fill;
                childForm.Controls.Add(userControl);
                childForm.Show();
            }
            catch (Exception ex)
            {
                // Handle potential exceptions, e.g., log them or show a message to the user
                MessageBox.Show($"Failed to create UserControl: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void barButtonItem36_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                foreach (XtraForm childForm in MdiChildren)
                {
                    childForm.Close();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem37_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                SplashScreenManager.ShowForm(this, typeof(About));

            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }           

        }


        private void bbirreportlr_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(34, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

        
                if (!EnsureAccess(AuthorizationService.CanViewReports, "Anda tidak memiliki izin membuka laporan."))
                {
                    return;
                }

                OpenMDI<FrmReportParam>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void bbireportneraca_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanViewReports, "Anda tidak memiliki izin membuka laporan."))
                {
                    return;
                }
                OpenMDI<FrmReportParam>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

     

        private void barButtonItem45_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////39 kode buka SETTING PERIODE
                //bool akses = LevelAksesServices.OpenForm(43, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
             
                if (!EnsureAccess(AuthorizationService.CanManageCompanyProfile, "Anda tidak memiliki izin membuka profil company."))
                {
                    return;
                }

                OpenMDI<FrmCompanyProfile>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem42_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////39 kode buka SETTING PERIODE
                //bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (!EnsureAccess(AuthorizationService.CanManageProfitLossSetup, "Anda tidak memiliki izin membuka pengaturan laba rugi."))
                {
                    return;
                }

                OpenMDI<FrmSettingRL>(false);

            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem32_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanExportJurnal, "Anda tidak memiliki izin membuka export jurnal."))
                {
                    return;
                }

                OpenMDI<FrmExportJurnal>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void bbiestate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(32, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}

                if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                {
                    XtraMessageBox.Show("Laporan Estate hanya Berlaku untuk Lokasi Kebun", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!EnsureAccess(AuthorizationService.CanViewEstateReports, "Anda tidak memiliki izin membuka laporan estate."))
                {
                    return;
                }

                OpenMDI<FrmReportParamKebun>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem46_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //SplashScreenManager.ShowForm(typeof(WaitForm_Load));
            //Cursor.Current = Cursors.WaitCursor;
            foreach (XtraForm childForm in MdiChildren)
            {
                childForm.Close();
            }

            Hide();
            new FrmLokasi().Show();
        }

        private void barButtonItem47_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (XtraMessageBox.Show("Keluar dari Aplikasi General Ledger ? ",
          "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
            { return; }
            else
            {
                this.Close();
                Application.Exit();
            }

        }

        private void barButtonItem25_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //int tahun, bulan;
                //tahun = Convert.ToInt32(this.Parameters["Tahun"].Value);
                //bulan = Convert.ToInt32(this.Parameters["Bulan"].Value);
                var NeracaLajur = LaporanServices.ViewLap_NeracaLajur(CompanyInfo.IDDATA, 1, 2021);

                //NeracaLajur.WriteXmlSchema("NeracaLajur.xsd");
                NeracaLajur laporan = new NeracaLajur();


                laporan.DataSource = NeracaLajur;

                laporan.Parameters["PERIODE"].Value = NeracaLajur.Tables.Count;
                laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                laporan.RequestParameters = true;


                var tool = new ReportPrintTool(laporan);
                tool.ShowPreview();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem26_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(45, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                FrmSetAkun f1 = new FrmSetAkun();
                f1.StartPosition = FormStartPosition.CenterScreen;
                f1.ShowDialog();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem41_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                FrmChangePass fc = new FrmChangePass();
                fc.StartPosition = FormStartPosition.CenterScreen;
                fc.ShowDialog();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanViewAuditTrail, "Anda tidak memiliki izin membuka audit trail."))
                {
                    return;
                }

                OpenMDI<FrmAuditTrail>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem19_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(15, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
        
                OpenMDI<FrmNotaDebet>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem20_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(15, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
          

                 OpenMDI<FrmNotaKredit>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void barButtonItem22_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                ////39 kode buka SETTING PERIODE
                //bool akses = LevelAksesServices.OpenForm(43, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
           

                if (!EnsureAccess(AuthorizationService.CanManageCompanyMaster, "Anda tidak memiliki izin membuka master company."))
                {
                    return;
                }

                OpenMDI<FrmCompany>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem21_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanViewJurnalWorkspace, "Anda tidak memiliki izin membuka input jurnal."))
                {
                    return;
                }
               
                OpenMDI<FrmJurnal>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem25_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                if (!EnsureAccess(AuthorizationService.CanUpdateCoa, "Anda tidak memiliki izin membuka pengaturan blok."))
                {
                    return;
                }
                if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                {
                    XtraMessageBox.Show("Module ini hanya dapat digunakan untuk COA berjenis kebun...!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                OpenMDI<FrmMasterBlok>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem24_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (!EnsureAccess(AuthorizationService.CanUpdateCoa, "Anda tidak memiliki izin membuka pengaturan divisi."))
                {
                    return;
                }

                if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                {
                    XtraMessageBox.Show("Module ini hanya dapat digunakan untuk COA berjenis kebun...!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            
                OpenMDI<FrmMasterDivisi>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void barButtonItem21_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                //if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                //{
                //    XtraMessageBox.Show("Module ini hanya dapat digunakan untuk COA berjenis kebun...!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                 
                if (!EnsureAccess(AuthorizationService.CanUpdateCoa, "Anda tidak memiliki izin membuka master akun blok."))
                {
                    return;
                }

                OpenMDI<FrmMasterAkun>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem25_ItemClick_2(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //bool akses = LevelAksesServices.OpenForm(35, LoginInfo.userID);
                //if (akses == false)
                //{
                //    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return;
                //}
                if (CompanyInfo.JENIS_AKUNTING == "PUSAT")
                {
                    if (Application.OpenForms.OfType<FrmBiayaBlokPusat>().Count() == 1)
                        Application.OpenForms.OfType<FrmBiayaBlokPusat>().First().Close();
                    FrmBiayaBlokPusat form1 = new FrmBiayaBlokPusat()
                    {
                        //MdiParent = this,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    form1.ShowDialog();
                }
                else
                {
                    if (Application.OpenForms.OfType<FrmBiayaBlok>().Count() == 1)
                        Application.OpenForms.OfType<FrmBiayaBlok>().First().Close();
                    FrmBiayaBlok form2 = new FrmBiayaBlok()
                    {
                        //MdiParent = this,
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    form2.ShowDialog();
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
  

        private void bbcheckupdate_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo(@"Change Note.txt");
            psi.UseShellExecute = true;
            Process.Start(psi);

        }

        private void barButtonItem15_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!EnsureAccess(AuthorizationService.CanViewCoaWorkspace, "Anda tidak memiliki izin membuka workspace chart of account."))
            {
                return;
            }

            OpenMDI<FrmAkunEF_Luas>(false);
        }

        private void barButtonItem40_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!EnsureAccess(AuthorizationService.CanAccessDeveloperTools, "Anda tidak memiliki izin membuka developer tools."))
            {
                return;
            }

            OpenMDI<FrmDeveloper>(false);
        }

        private void bbialokasikerja_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenMDI<FrmSetAkunAgronomy>(false);
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            try
            {
                ribbonControl1.Minimized = true;
                if (Acct.PeriodeMax == 0)
                {
                    FrmPeriodeNew frm = new();
                    frm.ShowDialog();
                };

                string periodedipilih = DateTime.Today.Year + DateTime.Today.Month.ToString("0#");
                int newperiode = int.Parse(periodedipilih);
                if (newperiode > Acct.PeriodeMax)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.IDDATA, DateTime.Today.Month - 1, DateTime.Today.Year);
                    XtraMessageBox.Show($"New Period Created {periodedipilih}");
                }


                //string configvalue1 = ConfigurationManager.AppSettings.GetValues(descriptor);
                this.Text = "GENERAL LEDGER - " + CompanyInfo.NAMAPT + " - " + CompanyInfo.WILAYAH + " (" +CompanyInfo.IDDATA + ")";
                //bbiinit.Caption = configvalue1;
                bbiinit.Caption =CompanyInfo.IDDATA;
                bbinamapt.Caption = CompanyInfo.NAMAPT;
                bbiwilayah.Caption = CompanyInfo.WILAYAH.ToUpper();
                bsversion.Caption = "Version : " + Acct.AppVersion;
                bbiuserid.Caption = LoginInfo.userID + " (" + LoginInfo.role + ")";
                bbiperiode.Caption = "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.IDDATA).ToString();
                AddDocumentManager();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bbiupdatesaldo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenMDI<FrmUpdateSaldoAwal>(false);
        }

        private void barButtonItem48_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetDepreciationClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem49_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetLifecycleClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem50_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetApprovalClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem51_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetMasterClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem52_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetCipClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void barButtonItem53_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                OnFixedAssetReportsClick(sender, e);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
