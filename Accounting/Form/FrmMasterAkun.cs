using Accounting.BusinessLayer;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmMasterAkun : DevExpress.XtraEditors.XtraForm
    {
        public FrmMasterAkun()
        {
            InitializeComponent();   
        }
        private void FrmMasterAkun_Load(object sender, EventArgs e)
        {
            string KEL,KATEGORI;
            if (radioGroup1.SelectedIndex == 0)
            {
                KATEGORI = "TBM";
                KEL = "20";
            }
            else if  (radioGroup1.SelectedIndex == 1)
            {
                KATEGORI = "TM";
                KEL = "80";
            }
            else
            {
                KATEGORI = "TM";
                KEL = "81";
            }          
            Load_MasterAkun(KATEGORI,KEL);
            LEKELOMPOK.Properties.DataSource= CreateData();
            LEKELOMPOK.Properties.ValueMember = "ID";
            LEKELOMPOK.Properties.DisplayMember = "KATEGORI";
            radioGroup.SelectedIndex = 1;

        }

        private List<Data> CreateData()
        {
            List<Data> data = new List<Data>
            {
                new Data() { ID = 1, KATEGORI = "TBM", KELOMPOK = "20" },
                new Data() { ID = 2, KATEGORI = "TM PANEN", KELOMPOK = "80" },
                new Data() { ID = 3, KATEGORI = "TM PERAWATAN", KELOMPOK = "81" }
            };


            return data;
        }

        private void Load_MasterAkun(string KATEGORI, string KEL)
        {
            gridControl1.DataSource = AccountServices.GetMasterAkun(KATEGORI, KEL);
            gridView1.BestFitColumns();
        }

        string p_status = "New";
        string p_kategori;
        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
               
                if (string.IsNullOrEmpty(txtkode.Text)|| string.IsNullOrEmpty(txtnama.Text) || string.IsNullOrEmpty(LEKELOMPOK.Text) || string.IsNullOrEmpty(leinduk.Text))
                {
                    XtraMessageBox.Show("Semua Kolom Wajib Diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtkode.Focus();
                    return;
                }
                if ((int)LEKELOMPOK.EditValue == 1)
                {
                    p_kategori = "TBM";
                }
                else
                {
                    p_kategori = "TM";
                }

                using var localConn = new OracleConnection(LoginInfo.OracleConnString);
                using var _command = new OracleCommand("ACCT_TOOLS.AddorUpdateMasterAkun", localConn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                localConn.Open();
                _command.Parameters.Add(":p_status", OracleDbType.Varchar2, 30).Value = p_status;
                _command.Parameters.Add(":p_kode", OracleDbType.Varchar2, 30).Value = txtkode.Text+txtkodeakhir.Text;
                _command.Parameters.Add(":p_perkiraan", OracleDbType.Varchar2, 100).Value = txtnama.Text;
                _command.Parameters.Add(":p_jenis", OracleDbType.Varchar2, 30).Value = txtjenis.Text;
                _command.Parameters.Add(":p_level", OracleDbType.Varchar2, 30).Value = txtlvl.Text;
                _command.Parameters.Add(":p_induk", OracleDbType.Varchar2, 30).Value = leinduk.Text;
                _command.Parameters.Add(":p_gd", OracleDbType.Varchar2, 30).Value = txtdg.Text;
                _command.Parameters.Add(":p_kategori", OracleDbType.Varchar2, 30).Value = p_kategori;
                _command.ExecuteNonQuery();
                Bersihkan();
                XtraMessageBox.Show("Master Akun Blok Saved Successfully", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                p_status = "New";
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    txtkode.Focus();
                    XtraMessageBox.Show("Duplicate Master Akun Blok ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            LEKELOMPOK.EditValue = null;
            leinduk.EditValue = null;
            txtnamainduk.Text = "";
            txtjenis.Text = "";
            txtdg.Text = "";
            txtlvl.Text = "";
            txtkode.Text = "";
            txtnama.Text = "";
        }
        private void Load_BlokList()
        {
           var data = ToolsServices.GetBlokList(CompanyInfo.IDDATA);
            gridControl1.DataSource = data;
            gridView1.Columns[1].Visible = false;
            gridView1.Columns[2].Visible = false;
            gridView1.Columns[3].Visible = false;

            GridView view = gridControl1.MainView as GridView;
            GridColumnSortInfo[] sortInfo = {
        //new GridColumnSortInfo(view.Columns["KODEDIV"], ColumnSortOrder.Ascending),
        new GridColumnSortInfo(view.Columns["DIVISI"], ColumnSortOrder.Ascending),
         new GridColumnSortInfo(view.Columns["KODE"], ColumnSortOrder.Ascending)
                                 };
            view.SortInfo.ClearAndAddRange(sortInfo, 1);
            gridView1.BestFitColumns();
        }

       
    
        private void txtkodeblok_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txtnamablok_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void lookUpEditdivisi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txttahun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void btnbaru_Click(object sender, EventArgs e)
        {
            Bersihkan();
            txtkode.ReadOnly = false;
            txtnama.ReadOnly = false;
            p_status = "New";
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string KEL, KATEGORI;
            if (radioGroup1.SelectedIndex == 0)
            {
                KATEGORI = "TBM";
                KEL = "20";
            }
            else if (radioGroup1.SelectedIndex == 1)
            {
                KATEGORI = "TM";
                KEL = "80";
            }
            else
            {
                KATEGORI = "TM";
                KEL = "81";
            }


            Load_MasterAkun(KATEGORI, KEL);
        }

        string kategori, kel,kode;

       

        private void radioGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroup.SelectedIndex == 0)
            {
                txtdg.Text = "G";
            }
            else
            {
                txtdg.Text = "D";
            }
        }

        private void LEKELOMPOK_EditValueChanged(object sender, EventArgs e)
        {
            if (LEKELOMPOK.EditValue != null)
            {
                if ((int)LEKELOMPOK.EditValue == 1)
                {
                    kategori = "TBM";
                    kel = "20";
                    kode = "20.XXXXX.";
                    radioGroup1.SelectedIndex = 0;
                }
                else if ((int)LEKELOMPOK.EditValue == 2)
                {
                    kategori = "TM";
                    kel = "80";
                    kode = "80.XXXXX.";
                    radioGroup1.SelectedIndex = 1;
                }
                else if ((int)LEKELOMPOK.EditValue == 3)
                {
                    kategori = "TM";
                    kel = "81";
                    kode = "81.XXXXX.";
                    radioGroup1.SelectedIndex = 2;
                }
                txtkode.Text = kode;
                Load_IndukAkun(kategori, kel);
            }
        }

        private void Load_IndukAkun(string kategori, string kel)
        {
            var _dt = AccountServices.GetIndukAkun(kategori, kel);
            leinduk.Properties.DataSource = _dt;
            leinduk.Properties.ValueMember = "ACCOUNT";
            leinduk.Properties.DisplayMember = "ACCOUNT";
        }
    }

    public class Data
    {
        public Data()
        {
        }
        private int _id;
        private string _kat;
        private string _kel;
        public int ID
        {
            get { return this._id; }
            set { this._id = value; }
        }
        public string KATEGORI
        {
            get { return this._kat; }
            set { this._kat = value; }
        }
        public string KELOMPOK
        {
            get { return this._kel; }
            set { this._kel = value; }
        }
    }
}