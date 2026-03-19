
using Accounting.BusinessLayer;
using Accounting.Form;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
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
            lbliddata.Text =CompanyInfo.IDDATA;
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
            this.Close();
        }


        private void LoadGroup()
        {
            try
            {
                SplashScreenManager.ShowForm(typeof(WaitForm_Load));
                Cursor.Current = Cursors.WaitCursor;
                var tipe = lookUpEdikat.EditValue.ToString();

                grp = AccountServices.GetParentAccount(CompanyInfo.IDDATA, ptahun, tipe);
                lookUpEditbagiandari.Properties.DataSource = grp;
                lookUpEditbagiandari.Properties.ValueMember = "KODEACC";
                lookUpEditbagiandari.Properties.DisplayMember = "KODEACC";
                lookUpEditbagiandari.ItemIndex = 0;
                lookUpEditbagiandari.Properties.ForceInitialize();
                lookUpEditbagiandari.Properties.PopulateColumns();
                lookUpEditbagiandari.Properties.Columns["KEL"].Visible = false;
                lookUpEditbagiandari.Properties.BestFit();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void gd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txtnamaakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txtsaldoawal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void dateEditpertanggal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {                
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void lookUpEdikat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void checkEditbagianakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txtkepalaakun_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
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

                char status='-';
                if (checkEditnonaktif.Checked == true)
                {
                    status ='T';
                }

                AccountServices.UpdateCOA(
                    EditCOA.COAID,
                    txtnoakundetail.Text,
                    lookUpEdikat.EditValue.ToString(),
                    lookUpEditbagiandari.EditValue?.ToString(),
                    txtnamaakun.Text,
                    status,
                    txtlvel.Text);

                Update();
                XtraMessageBox.Show("Account Code Updated", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void txtnoakundetail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectNextControl(ActiveControl, true, true, true, true);
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
