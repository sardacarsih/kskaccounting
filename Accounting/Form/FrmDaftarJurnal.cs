

using Accounting.BusinessLayer;
using DevExpress.XtraSplashScreen;
using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.Utils.Menu;
using System.Diagnostics;
namespace Accounting.Form
{
    public partial class FrmDaftarJurnal : DevExpress.XtraEditors.XtraForm
    {

        int pbulan,ptahun;
        bool NODATA;
        string periode;
        public FrmDaftarJurnal()
        {
            InitializeComponent();
        }
        private void FrmDaftarJurnal_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
            int x= int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length-2, 2));
            int ymax = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 6, 4));
            int ymin = int.Parse(Acct.PeriodeMin.ToString().Substring(Acct.PeriodeMax.ToString().Length - 6, 4));
            cmbbulan.SelectedIndex = x-1;
            setahun.Properties.MinValue = ymin;
            setahun.Properties.MaxValue = ymax;
            setahun.Value = ymax;
            pbulan = x;
            ptahun = ymax;

            gridView2.Focus();
            Cursor.Current = Cursors.Default;
            SplashScreenManager.CloseForm();
        }
       
        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            PilihanPeriodeAkuntansi();
        }

        private void gridControl2_Click(object sender, EventArgs e)
        {
            FilterByNoJurnal();
        }

        private void FilterByNoJurnal()
        {
            try
            {
                if (NODATA == true) return;

                var rowhandle = gridView2.FocusedRowHandle;
                //filterdivisi = true;

                gridView1.ActiveFilterString = "[NOJURNAL] ='" + gridView2.GetRowCellValue(rowhandle, "NOJURNAL").ToString() + "'";
                //gridView1.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;

            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter divisi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridControl2_KeyUp(object sender, KeyEventArgs e)
        {
            FilterByNoJurnal();
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            PilihanPeriodeAkuntansi();
        }

        private void gridView2_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                //hapus menu jika ada
                e.Menu.Items.Clear();
                
                DXMenuItem ubah = CreateMenuItemUbah(view, rowHandle);
                DXMenuItem hapus = CreateMenuItemHapus(view, rowHandle);


                ubah.BeginGroup = true;
                hapus.BeginGroup = true;
                
                e.Menu.Items.Add(ubah);
                e.Menu.Items.Add(hapus);
                
            }
        }

        private DXMenuItem CreateMenuItemHapus(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Hapus", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnHapusClick(object sender, EventArgs e)
        {
            var Periode = cmbbulan.Text + " - " + setahun.Value.ToString();
            var rowhandle = gridView2.FocusedRowHandle;
            var Nomor = gridView2.GetRowCellValue(rowhandle, "NOJURNAL").ToString();
            if (Acct.KunciPeriode == "Y")
            {
                XtraMessageBox.Show("Transaksi ini tidak dapat diHapus...!!!\nPeriode Akuntansi : "+Periode+ " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //4 kode daftar jurnal
            bool akses = LevelAksesServices.Hapus(4, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (XtraMessageBox.Show("Hapus Transaksi Jurnal ? \n\nNomor : " + Nomor+"\nPeriode : "+Periode, "Confirm Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
        }

        private DXMenuItem CreateMenuItemUbah(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Ubah", new EventHandler(OnUbahClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnUbahClick(object sender, EventArgs e)
        {
            var Periode = cmbbulan.Text + " - " + setahun.Value.ToString();
            var rowhandle = gridView2.FocusedRowHandle;
            if (Acct.KunciPeriode == "Y")
            {
                XtraMessageBox.Show("Transaksi ini tidak dapat diubah...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //4 kode daftar jurnal
            bool akses = LevelAksesServices.Ubah(4, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        }

        private void gridView2_GotFocus(object sender, EventArgs e)
        {
            FilterByNoJurnal();
        }

        private void btnexport_Click(object sender, EventArgs e)
        {
            SplashScreenManager.ShowForm(typeof(WaitForm_Exporting));
            Cursor.Current = Cursors.WaitCursor;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var data = ExportServices.ExportJurnalMonthly(CompanyInfo.IDDATA, periode);

            //ExcelUtility obj = new ExcelUtility();
            //obj.WriteDataTableToExcel(data, "Jurnal Entries", "D:\\JurnalTest.xlsx", "Details");
            watch.Stop();

            TimeSpan timeSpan = watch.Elapsed;
            string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
           
            MessageBox.Show("Excel created D:\\JurnalTest.xlsx\n" +
                waktuproses+"");
            Cursor.Current = Cursors.Default;
            SplashScreenManager.CloseForm();
        }


        private void PilihanPeriodeAkuntansi()
        {
            pbulan = cmbbulan.SelectedIndex + 1;
            ptahun = (int)setahun.Value;
            periode = pbulan.ToString("0#") + "/" + ptahun.ToString();
            Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.IDDATA,periode);
            //MessageBox.Show(periode+" Status Lock : "+Acct.KunciPeriode);

            var jurnalheader = JurnalServices.GetJurnalHeader(CompanyInfo.IDDATA,periode);
            if (jurnalheader.Rows.Count == 0)
            {
                NODATA = true;
                gridControl1.DataSource = null;
                gridControl2.DataSource = null;
                return;
            }
            NODATA = false;
            gridControl2.DataSource = jurnalheader;
            gridView2.Columns[0].Visible = false;
            gridView2.Columns[2].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridView2.Columns[2].DisplayFormat.FormatString = "dd-MMM-yyyy";

            var jurnaldetail = JurnalServices.GetJurnalDetails(CompanyInfo.IDDATA, periode);
            gridControl1.DataSource = jurnaldetail;
            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].Visible = false;
            gridView1.Columns[2].OptionsColumn.FixedWidth = true;
            gridView1.Columns[2].Width = 20;
            gridView1.Columns[3].OptionsColumn.FixedWidth = true;
            gridView1.Columns[3].Width = 120;

            gridView1.Columns[5].OptionsColumn.FixedWidth = true;
            gridView1.Columns[5].Width=150;
            gridView1.Columns[6].OptionsColumn.FixedWidth = true;
            gridView1.Columns[6].Width = 150;
            gridView1.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView1.Columns[5].DisplayFormat.FormatString = "n2";
            gridView1.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView1.Columns[6].DisplayFormat.FormatString = "n2";
            gridView1.Columns[5].Summary.Clear();
            gridView1.Columns[6].Summary.Clear();
            gridView1.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
            gridView1.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
            gridView1.BestFitColumns();

            gridView2.Focus();
            FilterByNoJurnal();
        }

        
    }
}
