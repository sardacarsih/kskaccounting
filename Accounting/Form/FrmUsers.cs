using Accounting.BusinessLayer;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraSplashScreen;

namespace Accounting.Form
{
    public partial class FrmUsers : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new(Acct.OracleConnString);
        string p_userid;
        public FrmUsers()
        {
            InitializeComponent();   
        }
        private void FrmUsers_Load(object sender, EventArgs e)
        {
          
            Load_UserList();     
            Load_level();
            Load_IDData();
        }

        
        private void Load_IDData()
        {
            String selectQuery = "select d.iddata,d.wilayah,  p.namapt from master_pt_hdr p "+
                                    "join master_pt_dtl d on d.idpt = p.idpt ";
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
            lookUpEdit1.Properties.DataSource = _dt;
            lookUpEdit1.Properties.ValueMember = "IDDATA";
            lookUpEdit1.Properties.DisplayMember = "WILAYAH";
            lookUpEdit1.Properties.ForceInitialize();
            lookUpEdit1.Properties.PopulateColumns();
            lookUpEdit1.Properties.BestFit();
        }


        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string pwd = pASSWORDTextEdit.Text;
                if (string.IsNullOrEmpty(uSERIDTextEdit.Text)|| string.IsNullOrEmpty(pASSWORDTextEdit.Text))
                {
                    XtraMessageBox.Show("UserID dan Password harus diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    uSERIDTextEdit.Focus();
                    return;
                }
                if(pASSWORDTextEdit.Text != confirmasi.Text)
                {
                    XtraMessageBox.Show("Konfirmasi Password tidak cocok", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    confirmasi.Focus();
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
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value = CompanyInfo.INIT;
                _command.Parameters.Add(":apps", OracleDbType.Varchar2, 30).Value = "GL";
                _command.Parameters.Add(":userid", OracleDbType.Varchar2, 30).Value = uSERIDTextEdit.Text.ToLower();
                _command.Parameters.Add(":nama", OracleDbType.Varchar2, 30).Value = nAMATextEdit.Text;
                _command.Parameters.Add(":dept", OracleDbType.Varchar2, 30).Value = dEPTTextEdit.Text;
                _command.Parameters.Add(":pass", OracleDbType.Varchar2, 100).Value = savePasswordHash;
                _command.Parameters.Add(":roleid", OracleDbType.Int16).Value = Convert.ToInt32(lEVELIDComboBox.SelectedValue);
                _command.Parameters.Add(":jabatan", OracleDbType.Varchar2,30).Value = jABATANTextEdit.Text;
                _command.ExecuteReader();               
                conn.Close();
                Load_UserList();
                Bersihkan();
                XtraMessageBox.Show("UserID Saved", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    uSERIDTextEdit.Focus();
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

        private void Bersihkan()
        {
            uSERIDTextEdit.Text = "";
            pASSWORDTextEdit.Text = "";
            confirmasi.Text = "";
            nAMATextEdit.Text = "";
            dEPTTextEdit.Text = "";
            jABATANTextEdit.Text = "";

        }
        private void Load_UserList()
        {
           var data = ToolsServices.UserLogin();
            gridControl1.DataSource = data;
            gridView1.Columns[3].Visible = false;
            gridView1.Columns[4].Visible = false;
            gridView1.BestFitColumns();
        }

        private void Load_level()
        {
            String selectQuery = "select LevelID ID,Nama from master_login_level order by LevelID asc";
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
            lEVELIDComboBox.DataSource = _dt;
            lEVELIDComboBox.ValueMember = "ID";
            lEVELIDComboBox.DisplayMember = "NAMA";
           
        }
        string userid;
        private void btnaddcompany_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.gridView1.GetFocusedRowCellValue("USERID") == null)
                {
                    XtraMessageBox.Show("Pilih UserID pada Grid", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                userid = gridView1.GetFocusedRowCellValue("USERID").ToString();

                OracleCommand _command = new OracleCommand("ACCT_TOOLS.AddIDDATA", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 30).Value = lookUpEdit1.EditValue;
                _command.Parameters.Add(":apps", OracleDbType.Varchar2, 30).Value = "GL";
                _command.Parameters.Add(":userid", OracleDbType.Varchar2, 30).Value = userid;                
                _command.ExecuteReader();
                conn.Close();
                Bersihkan();
                LoadUsersAkses();
                XtraMessageBox.Show(lookUpEdit1.EditValue.ToString()+" Saved", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Duplicate IDDATA : " +
                         lookUpEdit1.EditValue.ToString() + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();

            }

        }
        private void gridView1_Click(object sender, EventArgs e)
        {
            LoadUsersAkses();
        }

        private void LoadUsersAkses()
        {
            if (this.gridView1.GetFocusedRowCellValue("USERID") == null)
                return;
            var rowhandle = gridView1.FocusedRowHandle;
            p_userid = gridView1.GetRowCellValue(rowhandle, "USERID").ToString();
            var data = ToolsServices.GetAksesLocations(p_userid);
            gridControl2.DataSource = data;
            lbluser.Text = p_userid;
        }

        private void btnhapus_Click(object sender, EventArgs e)
        {
            if (this.gridView1.GetFocusedRowCellValue("USERID") == null)
                return;

            if (XtraMessageBox.Show("Hapus UserID ? : " + p_userid , "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            OracleCommand _command = new OracleCommand("delete from master_login where userid=:p_userid", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 30).Value = p_userid;
            _command.ExecuteReader();
            conn.Close();
            XtraMessageBox.Show(p_userid + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUsersAkses();
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
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 30).Value = p_userid;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 30).Value = P_IDDATA;
            _command.ExecuteReader();
            conn.Close();
            XtraMessageBox.Show("Akses ke lokasi pembukuan "+P_IDDATA + " Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUsersAkses();
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
            if (this.gridView1.GetFocusedRowCellValue("USERID") == null)
                return;
            var rowhandle = gridView1.FocusedRowHandle;
            uSERIDTextEdit.Text = gridView1.GetRowCellValue(rowhandle, "USERID").ToString();
            nAMATextEdit.Text = gridView1.GetRowCellValue(rowhandle, "NAMA").ToString();
            dEPTTextEdit.Text = gridView1.GetRowCellValue(rowhandle, "DEPT").ToString();
            jABATANTextEdit.Text = gridView1.GetRowCellValue(rowhandle, "JABATAN").ToString();
            lEVELIDComboBox.Text = gridView1.GetRowCellValue(rowhandle, "ROLE").ToString();
        }
    }
}