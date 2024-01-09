using Accounting.BusinessLayer;
using Accounting.Form;
using Accounting.Laporan;
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
                var y= "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.INIT).ToString();
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
        private void barButtonItem35_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);
                //2 kode buka kode perkiraan
                bool akses = LevelAksesServices.OpenForm(2, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //17 kode buka input jurnal
                bool akses = LevelAksesServices.OpenForm(17, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //4 kode buka daftar jurnal
                bool akses = LevelAksesServices.OpenForm(4, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);
                //16 kode buka input jurnal
                bool akses = LevelAksesServices.OpenForm(16, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                //OpenMDI<FrmJurnal>(true);
                FrmJurnal fjurnal = new();
                fjurnal.Show(this);   
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                //19 kode tutup buku bulanan
                bool akses = LevelAksesServices.OpenForm(19, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
             
               
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
                //20 kode tutup buku tahun
                bool akses = LevelAksesServices.OpenForm(20, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
               
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
            //23 kode buka laporan daftar perkiraan
            bool akses = LevelAksesServices.OpenForm(23, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void barButtonItem19_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //24 kode buka laporan history gl
            bool akses = LevelAksesServices.OpenForm(24, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

      
        private void BSETIMPORTCOA_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                //39 kode buka SETTING PERIODE
                bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
         
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
                bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        private void BSETUSER_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                bool akses = LevelAksesServices.OpenForm(40, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
               
                OpenMDI<FrmUsers>(false);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BSETPERIODE_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            try
            {
                bool akses = LevelAksesServices.OpenForm(41, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
             
                OpenMDI<FrmAksesLevel>(false);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                bool akses = LevelAksesServices.OpenForm(34, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(33, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //39 kode buka SETTING PERIODE
                bool akses = LevelAksesServices.OpenForm(43, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //39 kode buka SETTING PERIODE
                bool akses = LevelAksesServices.OpenForm(39, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //17 kode buka input jurnal
                bool akses = LevelAksesServices.OpenForm(17, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(32, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                {
                    XtraMessageBox.Show("Laporan Estate hanya Berlaku untuk Lokasi Kebun", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                var NeracaLajur = LaporanServices.ViewLap_NeracaLajur(CompanyInfo.INIT, 1, 2021);

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
                bool akses = LevelAksesServices.OpenForm(45, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
            bool akses = LevelAksesServices.OpenForm(46, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void barButtonItem19_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                bool akses = LevelAksesServices.OpenForm(15, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
        
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
                bool akses = LevelAksesServices.OpenForm(15, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
          

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
                //39 kode buka SETTING PERIODE
                bool akses = LevelAksesServices.OpenForm(43, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                //16 kode buka input jurnal
                bool akses = LevelAksesServices.OpenForm(16, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                bool akses = LevelAksesServices.OpenForm(42, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
                {
                    XtraMessageBox.Show("Module ini hanya dapat digunakan untuk COA berjenis kebun...!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                bool akses = LevelAksesServices.OpenForm(35, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
            OpenMDI<FrmAkunEF_Luas>(false);
        }

        private void barButtonItem40_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
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
                    FrmPeriodeNew frm = new FrmPeriodeNew();
                    frm.ShowDialog();
                };

                string periodedipilih = DateTime.Today.Year + DateTime.Today.Month.ToString("0#");
                int newperiode = int.Parse(periodedipilih);
                if (newperiode > Acct.PeriodeMax)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.INIT, DateTime.Today.Month - 1, DateTime.Today.Year);
                    XtraMessageBox.Show($"New Period Created {periodedipilih}");
                }


                //string configvalue1 = ConfigurationManager.AppSettings.GetValues(descriptor);
                this.Text = "GENERAL LEDGER - " + CompanyInfo.NAMAPT + " - " + CompanyInfo.WILAYAH + " (" + CompanyInfo.INIT + ")";
                //bbiinit.Caption = configvalue1;
                bbiinit.Caption = CompanyInfo.INIT;
                bbinamapt.Caption = CompanyInfo.NAMAPT;
                bbiwilayah.Caption = CompanyInfo.WILAYAH.ToUpper();
                bsversion.Caption = "Version : " + Acct.AppVersion;
                bbiuserid.Caption = LoginInfo.userID + " (" + LoginInfo.role + ")";
                bbiperiode.Caption = "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.INIT).ToString();
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
    }
}
