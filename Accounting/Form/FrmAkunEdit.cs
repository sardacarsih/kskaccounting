
using Accounting.BusinessLayer;
using Accounting.Form;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace Accounting
{
    public partial class FrmAkunEdit : DevExpress.XtraEditors.XtraForm
    {
        public FrmAkunEdit(FrmAkunEF frm1)
        {
            InitializeComponent();
        }
        int ptahun;
        DataTable grp;
       

        public delegate void UpdateDelegate(object sender, UpdateEventArgs args);
        public event UpdateDelegate UpdateEventHandler;

        public class UpdateEventArgs : EventArgs
        {
            public string Data { get; set; }
        }
        protected new void Update()
        {
            UpdateEventArgs args = new UpdateEventArgs();
            UpdateEventHandler.Invoke(this, args);
        }
        int x;
        private void FrmAkunEdit_Load(object sender, EventArgs e)
        {
           
            if (Acct.PeriodeMax.ToString().Length > 0)
            {
                x = int.Parse(Acct.PeriodeMax.ToString().Substring(4, 2));
            }
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            lbliddata.Text = CompanyInfo.INIT;
            Load_TipeAkun();           
            Edit_COA();


        }

        private void Edit_COA()
        {
            if (!string.IsNullOrEmpty(EditCOA.INDUK))
            {
                checkEditbagianakun.Checked = true;
            }
            else
            {
                checkEditbagianakun.Checked = false;
            }           
            setahun.Value = EditCOA.TAHUN;
            lookUpEdikat.EditValue = EditCOA.JENIS;
            //lookUpEditbagiandari.EditValue = EditCOA.INDUK;
            lookUpEditbagiandari.Text = EditCOA.INDUK;
            txtlvel.Text = EditCOA.LEVEL.ToString();

            if (EditCOA.GD == 'G')
            {
                gd.SelectedIndex = 0;
            }
            else
            {
                gd.SelectedIndex = 1;
            }
            txtlvel.Text = EditCOA.LEVEL.ToString();
            if (EditCOA.DK == 'D')
            {
                rgsisi.SelectedIndex = 0;
            }
            else
            {
                rgsisi.SelectedIndex = 1;
            }
            txtnamaakun.Text = EditCOA.PERKIRAAN;
            if (EditCOA.AKTIF == 'T')
            {
                checkEditnonaktif.Checked = true;
            }
            else
            {
                checkEditnonaktif.Checked = false;
            }
            txtnoakundetail.Text = EditCOA.KODE;
        }

        private void Load_TipeAkun()
        {
            var data = AccountServices.GetTipeAkun("COA");
            lookUpEdikat.Properties.DataSource = data;
            lookUpEdikat.Properties.ValueMember = "ID";
            lookUpEdikat.Properties.DisplayMember = "TIPE_AKUN";
            lookUpEdikat.Properties.ForceInitialize();
            lookUpEdikat.Properties.PopulateColumns();
            lookUpEdikat.Properties.Columns["DK"].Visible = false;
            lookUpEdikat.ItemIndex = 0;
            lookUpEdikat.Properties.BestFit();
            
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
           
        }


        private void LoadGroup()
        {
            try
            {
                SplashScreenManager.ShowForm(typeof(WaitForm_Load));
                Cursor.Current = Cursors.WaitCursor;
                var tipe = lookUpEdikat.EditValue.ToString();

                grp = AccountServices.GetParentAccount(CompanyInfo.INIT, ptahun, tipe);
                lookUpEditbagiandari.Properties.DataSource = grp;
                lookUpEditbagiandari.Properties.ValueMember = "KODEACC";
                lookUpEditbagiandari.Properties.DisplayMember = "KODEACC";
                lookUpEditbagiandari.ItemIndex = 0;
                lookUpEditbagiandari.Properties.ForceInitialize();
                lookUpEditbagiandari.Properties.PopulateColumns();
                lookUpEditbagiandari.Properties.Columns["KEL"].Visible = false;
                lookUpEditbagiandari.Properties.BestFit();
            }
            catch
            {

            }
            finally
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
            }
                
        }


        private void lookUpEdikat_EditValueChanged(object sender, EventArgs e)
        {
            
            DevExpress.XtraEditors.LookUpEdit editor = sender as DevExpress.XtraEditors.LookUpEdit;
            object valuesisi = editor.GetColumnValue("DK");

            var sisi = valuesisi.ToString();
            if (sisi == "D")
            {
                rgsisi.SelectedIndex = 0;
            }
            else
            {
                rgsisi.SelectedIndex = 1;
            }
            LoadGroup();
            
        }



      
    

        private void lookUpEditbagiandari_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void gd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnoakun_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtnamaakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtsaldoawal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void dateEditpertanggal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {                
                SendKeys.Send("{TAB}");
            }
        }

        private void lookUpEdikat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void checkEditbagianakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtkepalaakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

      
        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            ptahun = Convert.ToInt32(setahun.Value);
        }

        private void checkEditnonaktif_Click(object sender, EventArgs e)
        {
            if (gd.SelectedIndex == 0)
            {
                XtraMessageBox.Show("Group Akun tidak dapat dinonaktifkan", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        private void sbupdate_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                
                if (string.IsNullOrEmpty(txtnamaakun.Text))
                {
                    XtraMessageBox.Show("Nama Akun wajib diisi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (txtnoakundetail.Text == lookUpEditbagiandari.Text)
                {
                    XtraMessageBox.Show("Induk Akun tidak boleh sama dengan Kode Akun", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string update = "UPDATE ACCT_COA SET KODEACC=:p_KODEACC,GRP=:p_GRP,PARENTACC=:p_PARENTACC,NAMAACC=:p_NAMAACC,ISAKTIF=:p_NONAKTIF,lvl=:p_lvl WHERE ACCTCOAID=:p_ID";
                char status='-';
                if (checkEditnonaktif.Checked == true)
                {
                    status ='T';
                }
                
                using (OracleCommand _command = new OracleCommand(update, conn)
                {
                    CommandType = CommandType.Text
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    _command.Parameters.Add(":p_KODEACC", OracleDbType.Varchar2, 20).Value = txtnoakundetail.Text;
                    _command.Parameters.Add(":p_GRP", OracleDbType.Varchar2, 20).Value = lookUpEdikat.EditValue;
                    _command.Parameters.Add(":p_PARENTACC", OracleDbType.Varchar2, 20).Value = lookUpEditbagiandari.EditValue;
                    _command.Parameters.Add(":p_NAMAACC", OracleDbType.Varchar2, 100).Value = txtnamaakun.Text;
                    _command.Parameters.Add(":p_NONAKTIF", OracleDbType.Char, 1).Value = status;
                    _command.Parameters.Add(":p_lvl", OracleDbType.Char, 1).Value = txtlvel.Text;
                    _command.Parameters.Add(":p_ID", OracleDbType.Varchar2, 50).Value = EditCOA.COAID;

                    _command.ExecuteNonQuery();
                    Update();
                    XtraMessageBox.Show("Account Code Updated", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //AccountServices.UpdateLevelAccount(lbliddata.Text, Convert.ToInt32(setahun.Value));
                this.Close();

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-02292"))
                {
                    XtraMessageBox.Show("Kode Perkiraan tidak dapat diubah, karena telah memiliki transaksi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }              

            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void txtnoakungroup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void txtnoakundetail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }


        private void lookUpEditbagiandari_EditValueChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.LookUpEdit editor = sender as DevExpress.XtraEditors.LookUpEdit;
            int valuelevel = Convert.ToInt32(editor.GetColumnValue("LVL"));

            txtlvel.Text = (valuelevel + 1).ToString();
            //MessageBox.Show(valuelevel.ToString());
        }

        private void checkEditbagianakun_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkEditbagianakun.Checked == true)
            {
                LoadGroup();
            }
        }
    }
}
