using Accounting.BusinessLayer;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmMasterBlok : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new(Acct.OracleConnString);
        public FrmMasterBlok()
        {
            InitializeComponent();   
        }
        private void FrmBlok_Load(object sender, EventArgs e)
        {
          
            Load_Status();     
            Load_Divisi();
            Load_BlokList();
        }

        
        private void Load_Divisi()
        {
            String selectQuery = "select kode,divisi from master_divisi where iddata=:p_iddata order by kode";
            OracleCommand _command = new OracleCommand(selectQuery, conn)
            {
                CommandType = CommandType.Text
            };
            conn.Open();
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 30).Value = CompanyInfo.INIT;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            lookUpEditdivisi.Properties.DataSource = _dt;
            lookUpEditdivisi.Properties.ValueMember = "KODE";
            lookUpEditdivisi.Properties.DisplayMember = "DIVISI";
            lookUpEditdivisi.Properties.ForceInitialize();
            lookUpEditdivisi.Properties.PopulateColumns();
            lookUpEditdivisi.Properties.BestFit();
        }

        string p_blokid = "New";
        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
               
                if (string.IsNullOrEmpty(txtkodeblok.Text)|| string.IsNullOrEmpty(txtnamablok.Text) || string.IsNullOrEmpty(lookUpEditdivisi.Text) || string.IsNullOrEmpty(txttahun.Text) || string.IsNullOrEmpty(cmbstatus.Text))
                {
                    XtraMessageBox.Show("Semua Kolom Wajib Diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtkodeblok.Focus();
                    return;
                }
                string tm="T";
                if (ISTM.IsOn == true)
                {
                    tm = "Y";
                }

                string Aktif = "T";
                if (ISAKTIF.IsOn == true)
                {
                    Aktif = "Y";
                }
                OracleCommand _command = new OracleCommand("ACCT_TOOLS.AddorUpdateBlok", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_blokid", OracleDbType.Varchar2, 30).Value = p_blokid;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value = CompanyInfo.INIT;
                _command.Parameters.Add(":p_kode", OracleDbType.Varchar2, 30).Value = txtkodeblok.Text;
                _command.Parameters.Add(":p_nama", OracleDbType.Varchar2, 30).Value = txtnamablok.Text.ToUpper();
                _command.Parameters.Add(":p_divid", OracleDbType.Varchar2, 30).Value = lookUpEditdivisi.EditValue;
                _command.Parameters.Add(":p_luas", OracleDbType.Decimal).Value = decimal.Parse(TXTLUAS.Text);
                _command.Parameters.Add(":p_ttanam", OracleDbType.Char, 4).Value = txttahun.Text;
                _command.Parameters.Add(":p_status", OracleDbType.Varchar2, 10).Value = cmbstatus.Text;
                _command.Parameters.Add(":p_TM", OracleDbType.Char, 1).Value = tm;
                _command.Parameters.Add(":p_periodeTM", OracleDbType.Char, 7).Value = Aktif;
                _command.ExecuteReader();               
                conn.Close();
                Load_BlokList();
                Bersihkan();
                Update_LuasanDivisi(CompanyInfo.INIT);
                XtraMessageBox.Show("Blok Saved Successfully", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                p_blokid = "New";
                btnenable.Enabled = false;
                btndisable.Enabled = false;
                BTNTBM.Enabled = false;
                BTNTM.Enabled = false;
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    txtkodeblok.Focus();
                    XtraMessageBox.Show("Duplicate Block ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }

        private void Update_LuasanDivisi(string iddata)
        {
            OracleCommand _command = new OracleCommand("ACCT_TOOLS.UPDATE_LUASAN_DIVISI", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_divid", OracleDbType.Varchar2, 30).Value = iddata;
            _command.ExecuteNonQuery();
            conn.Close();
        }

        private void Bersihkan()
        {
            txtkodeblok.Text = "";
            txtnamablok.Text = "";
            ISTM.IsOn = false;
            ISAKTIF.IsOn = true;
            txttahun.Text = "";
            lookUpEditdivisi.Text = "";
            cmbstatus.Text = "";
            TXTLUAS.Text = "0";
        }
        private void Load_BlokList()
        {
           var data = ToolsServices.GetBlokList(CompanyInfo.INIT);
            gridControl1.DataSource = data;
            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].Visible = false;
            gridView1.Columns[2].Visible = false;

            GridView view = gridControl1.MainView as GridView;
            GridColumnSortInfo[] sortInfo = {
        //new GridColumnSortInfo(view.Columns["KODEDIV"], ColumnSortOrder.Ascending),
        new GridColumnSortInfo(view.Columns["DIVISI"], ColumnSortOrder.Ascending),
         new GridColumnSortInfo(view.Columns["KODE"], ColumnSortOrder.Ascending)
                                 };
            view.SortInfo.ClearAndAddRange(sortInfo, 1);
            gridView1.BestFitColumns();
        }

        private void Load_Status()
        {
            String selectQuery = "select ID,STATUS from master_STATUS order by ID asc";
            OracleCommand _command = new OracleCommand(selectQuery, conn)
            {
                CommandType = CommandType.Text
            };
            conn.Open();
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            cmbstatus.DataSource = _dt;
            cmbstatus.ValueMember = "ID";
            cmbstatus.DisplayMember = "STATUS";
           
        }
        private void gridView1_Click(object sender, EventArgs e)
        {

           // LoadPerkiraanBlok();
        }
        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            GridView view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.HitTest == GridHitTest.RowIndicator)
            {
                MessageBox.Show(string.Format("DoubleClick on row indicator, row #{0}", info.RowHandle));
            }
            if (this.gridView1.GetFocusedRowCellValue("BLOKID") == null)
                return;
            var rowhandle = gridView1.FocusedRowHandle;
            p_blokid= gridView1.GetRowCellValue(rowhandle, "BLOKID").ToString();
            txtkodeblok.Text = gridView1.GetRowCellValue(rowhandle, "KODE").ToString();
            txtnamablok.Text = gridView1.GetRowCellValue(rowhandle, "BLOK").ToString();
            lookUpEditdivisi.EditValue = gridView1.GetRowCellValue(rowhandle, "KODEDIV").ToString();
            TXTLUAS.Text = gridView1.GetRowCellValue(rowhandle, "LUAS").ToString();
            txttahun.Text = gridView1.GetRowCellValue(rowhandle, "TTANAM").ToString();
            cmbstatus.Text = gridView1.GetRowCellValue(rowhandle, "STATUS").ToString();
            var TM= gridView1.GetRowCellValue(rowhandle, "TM").ToString();
            var Aktif = gridView1.GetRowCellValue(rowhandle, "AKTIF").ToString();


            if (TM == "Y")
            {
                ISTM.IsOn = true;
            }
            else
            {
                ISTM.IsOn = false;
            }

            if (Aktif == "Y")
            {
                ISAKTIF.IsOn = true;
            }
            else
            {
                ISAKTIF.IsOn = false;
            }



            txtkodeblok.ReadOnly = true;
            txtnamablok.ReadOnly = true;
            //lookUpEditdivisi.ReadOnly = false;
            cmbstatus.Enabled = false;

            btnhapus.Enabled = true;
            BTNTBM.Enabled = true;
            BTNTM.Enabled = true;
            btnenable.Enabled = true;
            btndisable.Enabled = true;
            LoadPerkiraanBlok();
        }


        string divisi, blok;

        private void LoadPerkiraanBlok()
        {
            IOverlaySplashScreenHandle handle = null;
            try
            {
                if (this.gridView1.GetFocusedRowCellValue("BLOKID") == null)
                    return;
                handle = SplashScreenManager.ShowOverlayForm(this);
                var rowhandle = gridView1.FocusedRowHandle;
                divisi = gridView1.GetRowCellValue(rowhandle, "DIVISI").ToString().ToUpper();
                blok = gridView1.GetRowCellValue(rowhandle, "BLOK").ToString().ToUpper();
                var data = ToolsServices.LoadPerkiraanBlok(CompanyInfo.INIT, Acct.TahunMax, divisi, blok);
                gridControl2.DataSource = data;
                lblBlok.Text = divisi + " Blok " + blok;
               
                gridView2.BestFitColumns();
                gridView2.Columns[1].Width = 150;

                GridView view = gridControl2.MainView as GridView;
                GridColumnSortInfo[] sortInfo = {
        new GridColumnSortInfo(view.Columns["KELOMPOK"], ColumnSortOrder.Ascending),
        new GridColumnSortInfo(view.Columns["KODE"], ColumnSortOrder.Ascending)
                                 };
                view.SortInfo.ClearAndAddRange(sortInfo, 1);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SplashScreenManager.CloseOverlayForm(handle);
            }
        }

        private void btnhapus_Click(object sender, EventArgs e)
        {
            var ada = ToolsServices.CekAkunBlok(CompanyInfo.INIT, Acct.TahunMax, divisi, blok);
            if (ada > 0)
            {
                XtraMessageBox.Show("Data Blok tidak dapat dihapus, karena telah memiliki daftar perkiraan" , "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var namablok = gridView1.GetFocusedRowCellValue("BLOK").ToString();
            if (this.gridView1.GetFocusedRowCellValue("BLOKID") == null)
                return;

            if (XtraMessageBox.Show("Hapus BLOK ? : " + namablok, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            OracleCommand _command = new OracleCommand("delete from master_BLOK where BLOKID=:p_BLOKID", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_BLOKID", OracleDbType.Varchar2, 50).Value = gridView1.GetFocusedRowCellValue("BLOKID").ToString();
            _command.ExecuteReader();
            conn.Close();
            Update_LuasanDivisi(CompanyInfo.INIT);
            Load_BlokList();
            XtraMessageBox.Show(namablok + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }

        private void BTNTM_Click(object sender, EventArgs e)
        {
            try
            {
                var p_IDDATA = CompanyInfo.INIT;
                var p_tahun = Acct.TahunMax;
                var p_status = cmbstatus.Text;
                var p_divisiID = lookUpEditdivisi.EditValue.ToString();
                var p_divisi = lookUpEditdivisi.Text;
                var p_blokid = txtkodeblok.Text;
                var p_blok = txtnamablok.Text;
                var p_ttanam = txttahun.Text;

                if (XtraMessageBox.Show("Proses Generate Kode Perkiraan BLOK TM : " + p_blok + " " + p_divisi +
                       "\n\nTahun : " + p_tahun + " " +
                       "\nLokasi Data :" + p_IDDATA
                       , "Konfirmasi Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;


                ToolsServices.GenerateAkunTM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam);
                LoadPerkiraanBlok();
                XtraMessageBox.Show("Generate Kode Perkiraan Blok TM " + p_blok + " Selesai", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                //if (ex.Message.Contains("ORA-02291"))
                //{
                //    XtraMessageBox.Show("Induk Perkiraan tidak ditemukan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                //else
                //{
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
        }

        private void txtkodeblok_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnamablok_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void lookUpEditdivisi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txttahun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void cmbstatus_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void btnbaru_Click(object sender, EventArgs e)
        {
            Bersihkan();
            txtkodeblok.ReadOnly = false;
            txtnamablok.ReadOnly = false;
            lookUpEditdivisi.ReadOnly = false;
            cmbstatus.Enabled = true;

            btnhapus.Enabled = false;
            BTNTBM.Enabled = false;
            BTNTM.Enabled = false;
            btndisable.Enabled = false;
            p_blokid = "New";
            btnenable.Enabled = false;
            btndisable.Enabled = false;
            BTNTBM.Enabled = false;
            BTNTM.Enabled = false;
        }

        private void btndisable_Click(object sender, EventArgs e)
        {
            try
            {
                var p_IDDATA = CompanyInfo.INIT;
                var p_tahun = Acct.TahunMax;
                var p_status = cmbstatus.Text;
                var p_divisiID = lookUpEditdivisi.EditValue.ToString();
                var p_divisi = lookUpEditdivisi.Text;
                var p_blokid = txtkodeblok.Text;
                var p_blok = txtnamablok.Text;
                var p_ttanam = txttahun.Text;

                if (XtraMessageBox.Show("Proses Menonaktifkan Kode Perkiraan BLOK TBM : " + p_blok + " " + p_divisi +
                       "\n\nTahun : " + p_tahun + " " +
                       "\nLokasi Data :" + p_IDDATA
                       , "Konfirmasi Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;


                ToolsServices.GenerateAkunTBM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam, "DISABLE");
                LoadPerkiraanBlok();
                XtraMessageBox.Show("Kode Perkiraan Blok  TBM telah dinonaktifkan " + p_blok + " Selesai", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
               
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnenable_Click(object sender, EventArgs e)
        {
            try
            {
                var p_IDDATA = CompanyInfo.INIT;
                var p_tahun = Acct.TahunMax;
                var p_status = cmbstatus.Text;
                var p_divisiID = lookUpEditdivisi.EditValue.ToString();
                var p_divisi = lookUpEditdivisi.Text;
                var p_blokid = txtkodeblok.Text;
                var p_blok = txtnamablok.Text;
                var p_ttanam = txttahun.Text;

                if (XtraMessageBox.Show("Proses mengAktifkan kembali Kode Perkiraan BLOK TBM : " + p_blok + " " + p_divisi +
                       "\n\nTahun : " + p_tahun + " " +
                       "\nLokasi Data :" + p_IDDATA
                       , "Konfirmasi Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;


                ToolsServices.GenerateAkunTBM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam, "ENABLE");
                LoadPerkiraanBlok();
                XtraMessageBox.Show("Kode Perkiraan Blok  TBM telah diaktifkan kembali" + p_blok + " Selesai", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                if (ex.Message.Contains("ORA-02291"))
                {
                    XtraMessageBox.Show("Induk Perkiraan tidak ditemukan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void sbexport_Click(object sender, EventArgs e)
        {
            IOverlaySplashScreenHandle handle = null;
            try
            {
               
                handle = SplashScreenManager.ShowOverlayForm(this);
                var pivotExportOptions = new DevExpress.XtraPivotGrid.PivotXlsxExportOptions
                {
                    ExportType = DevExpress.Export.ExportType.WYSIWYG
                };
                gridView1.ExportToXlsx("BlokList.xlsx", pivotExportOptions);
               
                //These lines will open it in Excel
                ProcessStartInfo pi = new ProcessStartInfo("BlokList.xlsx");
                pi.UseShellExecute = true;
                Process.Start(pi);
            }
            catch (SystemException ex)
            {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SplashScreenManager.CloseOverlayForm(handle);
            }
        }

        private void BTNTBM_Click(object sender, EventArgs e)
        {
            try
            {
                var p_IDDATA = CompanyInfo.INIT;
                var p_tahun = Acct.TahunMax;
                var p_status = cmbstatus.Text;
                var p_divisiID = lookUpEditdivisi.EditValue.ToString();
                var p_divisi = lookUpEditdivisi.Text;
                var p_blokid = txtkodeblok.Text;
                var p_blok = txtnamablok.Text;
                var p_ttanam = txttahun.Text;

                if (XtraMessageBox.Show("Proses Generate Kode Perkiraan BLOK TBM : " + p_blok + " " + p_divisi +
                       "\n\nTahun : " + p_tahun + " " +
                       "\nLokasi Data :" + p_IDDATA
                       , "Konfirmasi Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;


                ToolsServices.GenerateAkunTBM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam,"CREATE");
                LoadPerkiraanBlok();
                XtraMessageBox.Show("Generate Kode Perkiraan Blok  TBM " + p_blok + " Selesai", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                if (ex.Message.Contains("ORA-02291"))
                {
                    XtraMessageBox.Show("Induk Perkiraan tidak ditemukan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}