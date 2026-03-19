using Accounting.BusinessLayer;
using Accounting.Laporan;
using Accounting.Model;
using DevExpress.Charts.Native;
using DevExpress.Data.Helpers;
using DevExpress.Data.Linq;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmAkunEF_Luas : DevExpress.XtraEditors.XtraForm
    {

        int pbulan, p_sampaibulan, ptahun, x;
        string fileName;

        public FrmAkunEF_Luas()
        {
            InitializeComponent();
        }
        DataSet DSGL;

        private void UpdateFromNew(object sender, FrmAkunAdd.UpdateEventArgs args)
        {
            Load_COA();
        }
        private void UpdateFromEdit(object sender, FrmAkunEdit.UpdateEventArgs args)
        {
            Load_COA();
        }
        private void FrmAkunEF_Luas_Load(object sender, EventArgs e)
        {
            try
            {
                cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
                if (Acct.PeriodeMax.ToString().Length > 0)
                {
                    x = int.Parse(Acct.PeriodeMax.ToString().Substring(4, 2));
                }

                cmbbulan.SelectedIndex = x - 1;
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
                Load_TipeAkun();
                Load_COA();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }
        IQueryable<BlokAccount> JurnalDetail = null;
         DataTable tbl_Coa = new();
        private void Load_COA()
        {
            if (CompanyInfo.IDDATA == "KSKINTI")
            {
                EntityInstantFeedbackSource eifs = new();
                eifs.KeyExpression = "ID";
                eifs.GetQueryable += Dapper_GetQueryable;
                gridControl1.DataSource = eifs;
            }
            else
            {
                var p_iddata =CompanyInfo.IDDATA;
                var p_tahun = Convert.ToInt32(setahun.Value);
                var p_bulan = Convert.ToInt32(cmbbulan.SelectedIndex + 1);
                if (p_tahun != 0 && p_bulan != 0)
                {
                    var data = AccountServices.GetPerkiraanSaldo_ADO(p_iddata, p_tahun, p_bulan);
                    gridControl1.DataSource = data;
                }
            }
            
        }

        private void Dapper_GetQueryable(object sender, GetQueryableEventArgs e)
        {
            var p_iddata =CompanyInfo.IDDATA;
            var p_tahun = Convert.ToInt32(setahun.Value);
            var p_bulan = Convert.ToInt32(cmbbulan.SelectedIndex + 1);
            if (p_tahun != 0 && p_bulan != 0)
            {
                var data = AccountServices.GetPerkiraanSaldo_Dapper(p_iddata, p_tahun, p_bulan);
                e.QueryableSource =data;
            }
        }


        private void Load_TipeAkun()
        {
            var data = AccountServices.GetTipeAkun("needzero");
            lookUpEdit1.Properties.DataSource = data;
            lookUpEdit1.Properties.ValueMember = "ID";
            lookUpEdit1.Properties.DisplayMember = "TIPE_AKUN";
            lookUpEdit1.ItemIndex = 0;
        }
        private void lookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var TIPE = Convert.ToString(lookUpEdit1.EditValue);
                if (TIPE == "00")
                {
                    AkunNeraca.Checked = false;
                    AkunLabaRugi.Checked = false;
                    gridView1.ClearColumnsFilter();
                    gridView1.ExpandAllGroups();
                }
                else if (TIPE == "01")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='01'");
                }
                else if (TIPE == "02")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='02'");
                }
                else if (TIPE == "03")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='03'");
                }
                else if (TIPE == "04")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='04'");
                }
                else if (TIPE == "05")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='05'");
                }
                else if (TIPE == "06")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='06'");
                }
                else if (TIPE == "07")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='07'");
                }
                else if (TIPE == "08")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='08'");
                }
                else if (TIPE == "09")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='09'");
                }
                else if (TIPE == "10")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='10'");
                }
                else if (TIPE == "11")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='11'");
                }
                else if (TIPE == "12")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='12'");
                }
                else if (TIPE == "13")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='13'");
                }
                else if (TIPE == "14")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='14'");
                }
                else if (TIPE == "15")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='15'");
                }
                else if (TIPE == "16")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='16'");
                }
                else if (TIPE == "17")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='17'");
                }
                else if (TIPE == "18")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='18'");
                }
                else if (TIPE == "19")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='19'");
                }
                else if (TIPE == "20")
                {
                    gridView1.Columns["GRP"].FilterInfo = new ColumnFilterInfo("[GRP]='20'");
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        List<BlokAccount> GetDataRows(ColumnView view)
        {
            if (view == null) return null;

            List<BlokAccount> rowList = new List<BlokAccount>();
            for (int i = 0; i < view.DataRowCount; i++)
                rowList.Add(view.GetRow(i) as BlokAccount);

            return rowList;
        }
        private void sbexport_Click(object sender, EventArgs e)
        {
           
                //2 kode buka kode perkiraan
                bool akses = LevelAksesServices.CetakExport(2, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
               

                fileName =CompanyInfo.IDDATA + "DaftarPerkiraan.xlsx";

                if (CompanyInfo.JENIS_AKUNTING == "KEBUN")
                {
                    gridView1.Columns["AWALTAHUN"].VisibleIndex = 7;
                    gridView1.Columns["SALDOAWAL"].VisibleIndex = 8;
                    gridView1.Columns["DEBET"].VisibleIndex = 9;
                    gridView1.Columns["KREDIT"].VisibleIndex = 10;
                    gridView1.Columns["MUTASI"].VisibleIndex = 11;
                    gridView1.Columns["SALDOAKHIR"].VisibleIndex = 12;
                    gridView1.Columns["DIVISI"].VisibleIndex = 13;
                    gridView1.Columns["BLOK"].VisibleIndex = 14;
                    gridView1.Columns["TAHUNTANAM"].VisibleIndex = 15;

                    gridView1.Columns["AWALTAHUN"].Visible = true;
                    gridView1.Columns["MUTASI"].Visible = true;
                    gridView1.Columns["DIVISI"].Visible = true;
                    gridView1.Columns["BLOK"].Visible = true;
                    gridView1.Columns["TAHUNTANAM"].Visible = true;

                   



                }
                else
                {
                    gridView1.Columns["AWALTAHUN"].VisibleIndex = 7;
                    gridView1.Columns["SALDOAWAL"].VisibleIndex = 8;
                    gridView1.Columns["DEBET"].VisibleIndex = 9;
                    gridView1.Columns["KREDIT"].VisibleIndex = 10;
                    gridView1.Columns["MUTASI"].VisibleIndex = 11;
                    gridView1.Columns["SALDOAKHIR"].VisibleIndex = 12;
                    gridView1.Columns["AWALTAHUN"].Visible = true;
                    gridView1.Columns["MUTASI"].Visible = true;
                }

                //gridView1.BestFitColumns();
                string sheetname;
                sheetname =CompanyInfo.IDDATA + cmbbulan.Text + setahun.Value;

                XlsxExportOptionsEx xlsxOptions = new XlsxExportOptionsEx
                {
                    ShowGridLines = true,
                    SheetName = sheetname,
                    ExportType = DevExpress.Export.ExportType.Default,    // ExportType
                    TextExportMode = TextExportMode.Value,
                    RawDataMode = true
                };
                gridControl1.ExportToXlsx(@fileName, xlsxOptions);


                gridView1.Columns["AWALTAHUN"].Visible = false;
                gridView1.Columns["MUTASI"].Visible = false;
                gridView1.Columns["DIVISI"].Visible = false;
                gridView1.Columns["BLOK"].Visible = false;
                gridView1.Columns["TAHUNTANAM"].Visible = false;

                // number formats
                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "[Red](#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                //Opening an existing Excel file
                FileInfo fi = new FileInfo(@fileName);


                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new(fi))
                {

                    ExcelWorksheet namedWorksheet = excelPackage.Workbook.Worksheets[0];

                    namedWorksheet.Column(1).Width = 12;
                    namedWorksheet.Column(2).Width = 60;
                    namedWorksheet.Column(8).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(9).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(10).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(11).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(12).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(13).Style.Numberformat.Format = fullNumberFormat;

                    namedWorksheet.Cells[1, 3, 150, 17].AutoFitColumns();
                    //Save your file
                    excelPackage.Save();
                    excelPackage.Dispose();
                }
                if(cetbm.Checked==true || cetm.Checked == true)
                {
                // Mendapatkan DataTable dari DataSource DataGridView
                DataTable dataTable = (DataTable)gridView1.DataSource;

                // Membuat list untuk menyimpan DataRow
                List<DataRow> dataRowList = new List<DataRow>();

                // Iterasi melalui seluruh baris di DataTable
                foreach (DataRow row in dataTable.Rows)
                {
                    // Tambahkan DataRow ke dalam list
                    dataRowList.Add(row);
                }
                gridControl2.DataSource= dataRowList;   
                //using (var pck = new ExcelPackage("Block_Acct.xlsx"))
                //{
                //    var sheet = pck.Workbook.Worksheets["Sheet1"];
                //    sheet.Cells["A1"].LoadFromCollection(list, true);
                //}
                //ProcessStartInfo psi = new ProcessStartInfo("Block_Acct.xlsx");
                //psi.UseShellExecute = true;
                //Process.Start(psi);

                }
                else
                {
                    ProcessStartInfo psi = new ProcessStartInfo(@fileName);
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
               
           
        }


        private void sbadd_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    //2 kode buka kode perkiraan
            //    bool akses = LevelAksesServices.BaruImport(2, LoginInfo.userID);
            //    if (akses == false)
            //    {
            //        XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    FrmAkunAdd form = new FrmAkunAdd(this)
            //    {
            //        //MdiParent = this,
            //        StartPosition = FormStartPosition.CenterScreen
            //    };
            //    form.UpdateEventHandler += UpdateFromNew;
            //    form.ShowDialog();
            //}
            //catch (SystemException ex)
            //{
            //    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        }
        private void sbubah_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    //2 kode buka kode perkiraan
            //    bool akses = LevelAksesServices.Ubah(2, LoginInfo.userID);
            //    if (akses == false)
            //    {
            //        XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    if (this.gridView1.GetFocusedRowCellValue("ID") == null)
            //        return;
            //    if (this.gridView1.GetFocusedRowCellValue("INDUK") == null)
            //    {
            //        XtraMessageBox.Show("Perkiraan Level 1 tidak dapat diubah", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }

            //    var rowHandle = gridView1.FocusedRowHandle;
            //    EditCOA.COAID = gridView1.GetRowCellValue(rowHandle, "ID").ToString();
            //    EditCOA.TAHUN = Convert.ToInt32(setahun.Value);
            //    EditCOA.JENIS = gridView1.GetRowCellValue(rowHandle, "GRP").ToString();
            //    EditCOA.INDUK = gridView1.GetRowCellValue(rowHandle, "INDUK").ToString();
            //    EditCOA.GD = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "GD").ToString());
            //    EditCOA.KODE = gridView1.GetRowCellValue(rowHandle, "KODEACC").ToString();
            //    EditCOA.LEVEL = Convert.ToInt32(gridView1.GetRowCellValue(rowHandle, "LVL").ToString());
            //    EditCOA.DK = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "POSISI").ToString());
            //    EditCOA.PERKIRAAN = gridView1.GetRowCellValue(rowHandle, "NAMAACC").ToString();
            //    EditCOA.AKTIF = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "ISAKTIF").ToString());

            //    FrmAkunEdit form = new(this)
            //    {
            //        //MdiParent = this,
            //        StartPosition = FormStartPosition.CenterScreen
            //    };
            //    form.UpdateEventHandler += UpdateFromEdit;
            //    form.ShowDialog();

            //}
            //catch (SystemException ex)
            //{
            //    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
        private void sbhapus_Click(object sender, EventArgs e)
        {
            try
            {
                //2 kode buka kode perkiraan
                bool akses = LevelAksesServices.Hapus(2, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var rowhandle = gridView1.FocusedRowHandle;
                var ID = gridView1.GetRowCellValue(rowhandle, "ID").ToString();

                var KODE = gridView1.GetRowCellValue(rowhandle, "KODEACC").ToString();
                var NAMA = gridView1.GetRowCellValue(rowhandle, "NAMAACC").ToString();
                var GD = gridView1.GetRowCellValue(rowhandle, "GD").ToString();

                if(GD=="G" )
                {
                    if (XtraMessageBox.Show("Hapus Group Kode Perkiraan ?\n" + KODE + " " + NAMA +
                        "\nSemua kode dibawah group ini akan dihapus jika tidak memiliki tansaksi" , "Confirm Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                else
                {
                    if (XtraMessageBox.Show("Hapus Detail Kode Perkiraan ? \n" + KODE + " " + NAMA, "Confirm Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                DELETEAKUN(ID);
            }
            catch (Exception ex)
            {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DELETEAKUN(string iD)
        {
            try
            {
                AccountServices.DeleteCOA(iD);
                Load_COA();
                XtraMessageBox.Show("Account Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-02292"))
                {
                    XtraMessageBox.Show("Kode Perkiraan Telah diGunakan,tidak dapat dihapus.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                //hapus menu jika ada
                e.Menu.Items.Clear();

                DXMenuItem detail = CreateMenuItemDetail(view, rowHandle);
                DXMenuItem segar = CreateMenuItemSegar(view, rowHandle);

                detail.BeginGroup = true;
                segar.BeginGroup = true;

                e.Menu.Items.Add(detail);
                e.Menu.Items.Add(segar);

            }
        }

        private DXMenuItem CreateMenuItemSegar(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Refresh Data", new EventHandler(OnRefreshClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            Load_COA();
        }

        private void AkunNeraca_CheckedChanged(object sender, EventArgs e)
        {
            if (AkunNeraca.Checked == true)
            {
                AkunLabaRugi.Checked = false;
                cetbm.Checked = false;
                cetm.Checked = false;
                gridView1.Columns["KODEACC"].ClearFilter();

                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["GRP"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[GRP] = '01' OR [GRP] = '02' OR [GRP] = '03' OR [GRP] = '04' OR [GRP] = '05' OR [GRP] = '06' " +
                    "OR [GRP] = '07' OR [GRP] = '08' OR [GRP] = '09' OR [GRP] = '10' ", string.Empty);
                //ColumnFilterInfo filter = new ColumnFilterInfo("[GRP] IN '01','02','03,'04','05','06','07','08','09','10'", "");
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {
                gridView1.Columns["GRP"].ClearFilter();
                //gridView1.FormatRules["LEVEL1"].ApplyToRow = true;
            }
        }

        private void AkunLabaRugi_CheckedChanged(object sender, EventArgs e)
        {
            if (AkunLabaRugi.Checked == true)
            {
                AkunNeraca.Checked = false;
                cetbm.Checked = false;
                cetm.Checked = false;
                gridView1.Columns["KODEACC"].ClearFilter();

                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["GRP"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[GRP] = '11' OR [GRP] = '12' OR [GRP] = '13' OR [GRP] = '14' OR [GRP] = '15' OR [GRP] = '16' " +
                    "OR [GRP] = '17' OR [GRP] = '18' OR [GRP] = '19' OR [GRP] = '20' OR [GRP] = '21'  ", string.Empty);
                //ColumnFilterInfo filter = new ColumnFilterInfo("[GRP] IN '01','02','03,'04','05','06','07','08','09','10'", "");
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {
                // gridView1.FormatRules["GROUP"].ApplyToRow = true;
                gridView1.Columns["GRP"].ClearFilter();
            }
        }

        private void NilaiSaldo_CheckedChanged(object sender, EventArgs e)
        {
            if (NilaiSaldo.Checked == true)
            {
                CEMUTASI.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["SALDOAKHIR"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[SALDOAKHIR] !=0", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["SALDOAKHIR"].ClearFilter();
            }
        }


        private void cetm_Click(object sender, EventArgs e)
        {
            if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
            {
                XtraMessageBox.Show("Hanya untuk kebun", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void cetbm_Click(object sender, EventArgs e)
        {
            if (CompanyInfo.JENIS_AKUNTING != "KEBUN")
            {
                XtraMessageBox.Show("Hanya untuk kebun", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void cetbm_CheckedChanged(object sender, EventArgs e)
        {
            if (cetbm.Checked == true)
            {
                AkunNeraca.Checked = false;
                AkunLabaRugi.Checked = false;
                cetm.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["KODEACC"];
                ColumnFilterInfo filter = new ColumnFilterInfo("StartsWith([KODEACC], '20')", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["KODEACC"].ClearFilter();
            }
        }

        private void CEMUTASI_CheckedChanged(object sender, EventArgs e)
        {
            if (CEMUTASI.Checked == true)
            {
                NilaiSaldo.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["MUTASI"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[MUTASI] !=0", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["MUTASI"].ClearFilter();
            }
        }

        private void cetm_CheckedChanged(object sender, EventArgs e)
        {
            if (cetm.Checked == true)
            {
                AkunNeraca.Checked = false;
                AkunLabaRugi.Checked = false;
                cetbm.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["KODEACC"];
                ColumnFilterInfo filter = new ColumnFilterInfo("StartsWith([KODEACC], '80') or StartsWith([KODEACC], '81')", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["KODEACC"].ClearFilter();
            }
        }


        private DXMenuItem CreateMenuItemDetail(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Detail Transactions", new EventHandler(OnDetailClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbbulan.SelectedIndex != -1)
            {
                Load_COA();
            }
            
        }



        private void cedetail_CheckedChanged(object sender, EventArgs e)
        {
            if (cedetail.Checked == true)
            {
                cegroup.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["GD"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[GD] ='D'", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["GD"].ClearFilter();
            }
        }

        private void cegroup_CheckedChanged(object sender, EventArgs e)
        {
            if (cegroup.Checked == true)
            {
                cedetail.Checked = false;
                ColumnView view = gridView1;
                GridColumn colCategory = view.Columns["GD"];
                ColumnFilterInfo filter = new ColumnFilterInfo("[GD] ='G'", string.Empty);
                view.ActiveFilter.Add(colCategory, filter);
            }
            else
            {

                gridView1.Columns["GD"].ClearFilter();
            }
        }


        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            if (setahun.Value != 0)
            {
                Load_COA();
            }
        }

        private void OnDetailClick(object sender, EventArgs e)
        {
            try
            {
                if (this.gridView1.GetFocusedRowCellValue("GD") == null) return;
                bool akses = LevelAksesServices.CetakExport(4, LoginInfo.userID);
                if (akses == false)
                {
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string[] bulanbi = { "Bulan", "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };

                var rowhandle = gridView1.FocusedRowHandle;
                var kode = gridView1.GetRowCellValue(rowhandle, "KODEACC").ToString();
                var nama = gridView1.GetRowCellValue(rowhandle, "NAMAACC").ToString();
                var Group = gridView1.GetRowCellValue(rowhandle, "GD").ToString();
                var posisi = gridView1.GetRowCellValue(rowhandle, "POSISI").ToString();
                var debet = Convert.ToDecimal(gridView1.GetRowCellValue(rowhandle, "DEBET"));
                var kredit = Convert.ToDecimal(gridView1.GetRowCellValue(rowhandle, "KREDIT"));
                pbulan = cmbbulan.SelectedIndex + 1;
                p_sampaibulan = cmbbulan.SelectedIndex + 1;
                ptahun = (int)setahun.Value;
                var bulan = bulanbi[pbulan].ToString() + "-" + ptahun.ToString();
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();
                var iddata =CompanyInfo.IDDATA;
                var userid = LoginInfo.userID;

                if (debet == 0 && kredit == 0)
                {
                    MessageBox.Show("Tidak ada transaksi", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (Group == "G")
                {
                    //1st generate 
                    LaporanServices.GenerateSub_LabaRugi(iddata, pbulan, ptahun, kode, userid, "NERACA", posisi);
                    //2nd view data
                    DataSet DSSubRL = LaporanServices.ViewSub_LabaRugi(iddata, userid);
                    //DSSubRL.WriteXmlSchema("SubRL.xsd");
                    if (posisi == "D")
                    {
                        rsub_rl_DetailD detailReport = new rsub_rl_DetailD
                        {
                            DataSource = DSSubRL
                        };
                        detailReport.Parameters["SUB"].Value = nama;
                        detailReport.Parameters["PBULAN"].Value = pbulan;
                        detailReport.Parameters["PTAHUN"].Value = ptahun;
                        detailReport.Parameters["BULAN"].Value = bulan;
                        // detailReport.Parameters["PERIODE"].Value = periode;
                        detailReport.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        detailReport.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        detailReport.RequestParameters = true;
                        detailReport.ShowPreviewDialog();
                    }
                    else
                    {
                        rsub_rl_DetailK detailReport = new rsub_rl_DetailK
                        {
                            DataSource = DSSubRL
                        };
                        detailReport.Parameters["SUB"].Value = nama;
                        detailReport.Parameters["PBULAN"].Value = pbulan;
                        detailReport.Parameters["PTAHUN"].Value = ptahun;
                        detailReport.Parameters["BULAN"].Value = bulan;
                        // detailReport.Parameters["PERIODE"].Value = periode;
                        detailReport.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        detailReport.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        detailReport.RequestParameters = true;
                        detailReport.ShowPreviewDialog();
                    }

                }
                else
                {
                    var darikode = kode;
                    var sampaikode = kode;
                    //get data for report
                    DSGL = LaporanServices.ViewLap_BukuBesar(iddata, ptahun, pbulan, p_sampaibulan, darikode, sampaikode, userid, "NERACA");
                    //DSGL.WriteXmlSchema("GeneralLedger.xsd");
                    if (posisi == "D")
                    {

                        GeneralLedgerD2 laporan = new ()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = ptahun;
                        laporan.Parameters["BULAN"].Value = bulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new (laporan);
                        tool.ShowPreview();
                    }
                    else
                    {

                        GeneralLedgerK2 laporan = new ()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = ptahun;
                        laporan.Parameters["BULAN"].Value = bulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new (laporan);
                        tool.ShowPreview();
                    }
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //2 kode buka kode perkiraan
            bool akses = LevelAksesServices.CetakExport(2, LoginInfo.userID);
            if (akses == false)
            {
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
    }
}
