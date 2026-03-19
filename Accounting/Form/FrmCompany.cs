using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmCompany : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        
        public FrmCompany()
        {
            InitializeComponent();   
        }
        private void FrmCompany_Load(object sender, EventArgs e)
        {         
           
            Load_Perusahaan();
            Load_IDDATA();
            Load_GroupPT();
        }


        private void Load_Perusahaan()
        {
            String selectQuery = "select IDPT,NAMAPT,IDGROUP from master_PT_HDR order by NAMAPT asc";
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
            gridControl2.DataSource = _dt;
            gridView2.BestFitColumns();
            GridView view = gridControl2.MainView as GridView;
            GridColumnSortInfo[] sortInfo = {new GridColumnSortInfo(view.Columns["IDGROUP"], ColumnSortOrder.Ascending)};
            view.SortInfo.ClearAndAddRange(sortInfo, 2);           
                gridView2.ExpandAllGroups();

            LEPT.Properties.DataSource = _dt;           
            LEPT.Properties.ValueMember = "IDPT";
            LEPT.Properties.DisplayMember = "NAMAPT";
            LEPT.Properties.ForceInitialize();
            LEPT.Properties.PopulateColumns();
            LEPT.Properties.BestFit();

            //gridControl2.DataSource= _dt.Copy();
           
        }


        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string pwd = TXTWILAYAH.Text;
                if (string.IsNullOrEmpty(TXTIDDATA.Text)|| string.IsNullOrEmpty(TXTWILAYAH.Text))
                {
                    XtraMessageBox.Show("UserID dan Password harus diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TXTIDDATA.Focus();
                    return;
                }
               
                var p = new PasswordCryptographyPbkdf2();
                string savePasswordHash = p.GetHashPassword(pwd);
                OracleCommand _command = new OracleCommand("ACCT_TOOLS.AddUserLogin", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
                _command.Parameters.Add(":apps", OracleDbType.Varchar2, 30).Value = "GL";
                _command.Parameters.Add(":userid", OracleDbType.Varchar2, 30).Value = TXTIDDATA.Text.ToLower();
                _command.Parameters.Add(":pass", OracleDbType.Varchar2, 100).Value = savePasswordHash;
                _command.ExecuteReader();               
                conn.Close();
                Load_IDDATA();
                XtraMessageBox.Show("UserID Saved", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    TXTIDDATA.Focus();
                    XtraMessageBox.Show("Duplicate UserID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }

       
        private void Load_IDDATA()
        {
            String selectQuery = "select d.iddata,h.namapt,d.wilayah,d.jenis_akuntansi from master_pt_dtl d "+
                                "join master_pt_hdr h on h.idpt = d.idpt order by h.namapt asc";
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
            gridControl1.DataSource = _dt;
            gridView1.BestFitColumns();
        }

        private void Load_GroupPT()
        {
            String selectQuery = "select NAMAGROUP from master_PTGROUP order by NAMAGROUP asc";
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
            CMBGROUP.DataSource = _dt;
            CMBGROUP.ValueMember = "NAMAGROUP";
            CMBGROUP.DisplayMember = "NAMAGROUP";
           
        }
        private void btnhapus_Click(object sender, EventArgs e)
        {
            string PIDDATA = "";
            try
            {
                if (this.gridView1.GetFocusedRowCellValue("IDDATA") == null)
                    return;

                PIDDATA = gridView1.GetFocusedRowCellValue("IDDATA").ToString();

                if (XtraMessageBox.Show("Hapus IDDATA ? : " + PIDDATA + "", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                OracleCommand _command = new OracleCommand("delete from master_PT_DTL where IDDATA=:p_IDDATA", conn)
                {
                    CommandType = CommandType.Text
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value = PIDDATA;
                _command.ExecuteReader();
                conn.Close();
                Load_IDDATA();
                XtraMessageBox.Show(" PIDDATA" + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-02292"))
                {
                    XtraMessageBox.Show("tidak dapat dihapus,IDDATA telah digunakan pada daftar Login Users : " +
                        PIDDATA + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }

        }

        private void uSERIDTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void pASSWORDTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void confirmasi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void nAMATextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void dEPTTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void jABATANTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void lEVELIDComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (this.gridView2.GetFocusedRowCellValue("IDDATA") == null)
                return;
            var rowhandle = gridView2.FocusedRowHandle;
            var P_IDDATA = gridView2.GetRowCellValue(rowhandle, "IDDATA").ToString();

            if (XtraMessageBox.Show("Hapus Akses ke Lokasi Data ? : " + P_IDDATA, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            OracleCommand _command = new OracleCommand("delete from master_apps_detail where userid=:p_userid and iddata=:p_iddata and appid='GL'", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 30).Value = "p_userid";
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 30).Value = P_IDDATA;
            _command.ExecuteReader();
            conn.Close();
            XtraMessageBox.Show("Akses ke lokasi pembukuan "+P_IDDATA + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
        }

        private void SBSIMPAN_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CMBGROUP.Text)||string.IsNullOrEmpty(TXTKODEPT.Text)||string.IsNullOrEmpty(TXTNAMAPT.Text))
                {
                    XtraMessageBox.Show("Semua Informasi Wajib Diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            string insertpt = "INSERT INTO master_pt_hdr (IDPT, NAMAPT, IDGROUP) VALUES (:pidpt, :pnamapt, :pgroup)";

            using (OracleCommand _command = new(insertpt, conn))
            {
                _command.CommandType = CommandType.Text;

                _command.Parameters.Add(":pidpt", OracleDbType.Varchar2, 30).Value = TXTKODEPT.Text;
                _command.Parameters.Add(":pnamapt", OracleDbType.Varchar2, 50).Value = TXTNAMAPT.Text;
                _command.Parameters.Add(":pgroup", OracleDbType.Varchar2, 30).Value = CMBGROUP.Text;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

                _command.ExecuteNonQuery(); // Use ExecuteNonQuery instead of ExecuteReader for INSERT

                conn.Close();
            }

            Load_Perusahaan();
            XtraMessageBox.Show(TXTNAMAPT.Text + " disimpan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Duplicate Kode Perusahaan : " +
                         TXTKODEPT.Text + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }

        private void SBHAPUS_Click(object sender, EventArgs e)
        {
            string P_PT = "";
            try
            {
                if (this.gridView2.GetFocusedRowCellValue("IDPT") == null)
                    return;
                var rowhandle = gridView2.FocusedRowHandle;
                P_PT = gridView2.GetRowCellValue(rowhandle, "NAMAPT").ToString();
                var P_IDPT = gridView2.GetRowCellValue(rowhandle, "IDPT").ToString();

                if (XtraMessageBox.Show("Hapus Nama Perusahaan ? : " + P_PT, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                OracleCommand _command = new OracleCommand("delete from master_PT_HDR where IDPT=:p_IDPT", conn)
                {
                    CommandType = CommandType.Text
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_IDPT", OracleDbType.Varchar2, 30).Value = P_IDPT;
                _command.ExecuteReader();
                conn.Close();
                Load_Perusahaan();
                XtraMessageBox.Show("Nama Perusahaan " + P_PT + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-02292"))
                {
                    XtraMessageBox.Show("tidak dapat dihapus,Nama Perusahaan telah digunakan dalam transaksi : " +
                         P_PT + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }
        }

        private void btnsimpan_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (TXTIDDATA.Text.Length > 20)
                {

                    XtraMessageBox.Show("Kode Lokasi Perusahaan MAX 20 Karakter,panjang karakter sekarang : " + TXTIDDATA.Text.Length.ToString(), "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    TXTIDDATA.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(TXTIDDATA.Text) || string.IsNullOrEmpty(TXTWILAYAH.Text) || LEPT.EditValue==null)
                {
                    XtraMessageBox.Show("Semua Informasi Wajib Diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string insertpt = "insert into master_pt_dtl values(:piddata,:pidpt,:pwilayah,:pjenis)";

                string jenis="";
                if (radioGroup1.SelectedIndex == 0)
                {
                    jenis = "PUSAT";
                }
                else if (radioGroup1.SelectedIndex == 1)
                {
                    jenis = "PWK";
                }
                else if (radioGroup1.SelectedIndex == 2)
                {
                    jenis = "KEBUN";
                }
                else if (radioGroup1.SelectedIndex == 3)
                {
                    jenis = "PKS";
                }

                OracleCommand _command = new OracleCommand(insertpt, conn)
                {
                    CommandType = CommandType.Text
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 30).Value = TXTIDDATA.Text;
                _command.Parameters.Add(":pidpt", OracleDbType.Varchar2, 30).Value = LEPT.EditValue;
                _command.Parameters.Add(":pwilayah", OracleDbType.Varchar2, 50).Value = TXTWILAYAH.Text;                
                _command.Parameters.Add(":pjenis", OracleDbType.Varchar2, 30).Value = jenis;
                _command.ExecuteReader();
                conn.Close();
                Load_IDDATA();
                XtraMessageBox.Show(TXTIDDATA.Text + " diSimpan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Duplicate IDDATA  : " +
                         TXTIDDATA.Text + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }

        }

        private void TXTIDDATA_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void TXTWILAYAH_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void LEPT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void TXTKODEPT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void TXTNAMAPT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void CMBGROUP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

       
    }
}