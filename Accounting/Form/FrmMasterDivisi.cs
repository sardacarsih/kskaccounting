using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors;

namespace Accounting.Form
{
    public partial class FrmMasterDivisi : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public FrmMasterDivisi()
        {
            InitializeComponent();   
        }
        private void FrmMasterDivisi_Load(object sender, EventArgs e)
        {
          
            Load_Divisi();
        }

        
        private void Load_Divisi()
        {
            String selectQuery = "select ESTATEID,divisiid, kode,divisi,luastbm,luastm,AKTIF,TRANSIT_PANEN,TRANSIT_RAWAT from master_divisi where iddata=:p_iddata order by kode";
            OracleCommand _command = new (selectQuery, conn)
            {
                CommandType = CommandType.Text
            };
            conn.Open();
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            gridControl1.DataSource = _dt;
            gridView1.BestFitColumns();
        }

        string p_divisiid = "New";
        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
               
                if (string.IsNullOrEmpty(txtkodediv.Text)|| string.IsNullOrEmpty(txtnamadiv.Text))
                {
                    XtraMessageBox.Show("Semua Kolom Wajib Diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtkodediv.Focus();
                    return;
                }

                OracleCommand _command = new OracleCommand("ACCT_TOOLS.AddorUpdateDivisi", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_divisiid", OracleDbType.Varchar2, 30).Value = p_divisiid;
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
                _command.Parameters.Add(":p_kode", OracleDbType.Varchar2, 30).Value = txtkodediv.Text;
                _command.Parameters.Add(":p_nama", OracleDbType.Varchar2, 30).Value = txtnamadiv.Text.ToUpper();
                _command.ExecuteReader();               
                conn.Close();
                Load_Divisi();
                Bersihkan();
                XtraMessageBox.Show("Divisi Saved Successfully", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                p_divisiid = "New";
                btnhapus.Enabled = false;
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    txtkodediv.Focus();
                    XtraMessageBox.Show("Duplicate Divisi ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }

        private void Bersihkan()
        {
            p_divisiid = "New";
            txtkodediv.Text = "";
            txtnamadiv.Text = "";
            btnhapus.Enabled = false;
        }
       
     
        private void btnhapus_Click(object sender, EventArgs e)
        {
           

            var namadivisi = gridView1.GetFocusedRowCellValue("DIVISI").ToString();
            if (this.gridView1.GetFocusedRowCellValue("DIVISIID") == null)
                return;

            if (XtraMessageBox.Show("Hapus DIVISI ? : " + namadivisi, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            OracleCommand _command = new OracleCommand("delete from master_DIVISI where DIVISIID=:p_DIVISIID", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_DIVISIID", OracleDbType.Varchar2, 50).Value = gridView1.GetFocusedRowCellValue("DIVISIID").ToString();
            _command.ExecuteReader();
            conn.Close();
            Bersihkan();
            XtraMessageBox.Show(namadivisi + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Load_Divisi();
           
        }

      
        private void btnbaru_Click(object sender, EventArgs e)
        {
            Bersihkan();
            txtkodediv.ReadOnly = false;
            txtnamadiv.ReadOnly = false;

            btnhapus.Enabled = false;
            p_divisiid = "New";
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
            if (this.gridView1.GetFocusedRowCellValue("DIVISIID") == null)
                return;
            var rowhandle = gridView1.FocusedRowHandle;
            p_divisiid = gridView1.GetRowCellValue(rowhandle, "DIVISIID").ToString();
            txtkodediv.Text = gridView1.GetRowCellValue(rowhandle, "KODE").ToString();
            txtnamadiv.Text = gridView1.GetRowCellValue(rowhandle, "DIVISI").ToString();
            btnhapus.Enabled = true;
        }

        private void txtkodediv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnamadiv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
    }
}