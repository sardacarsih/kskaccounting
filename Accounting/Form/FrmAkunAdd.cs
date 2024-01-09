
using Accounting.BusinessLayer;
using Accounting.Form;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting
{
    public partial class FrmAkunAdd : DevExpress.XtraEditors.XtraForm
    {
        public FrmAkunAdd(FrmAkunEF frm1)
        {
            InitializeComponent();
        }
        int ptahun;
        DataTable grp;
        string valueLVL;

        public delegate void UpdateDelegate(object sender, UpdateEventArgs args);
        public event UpdateDelegate UpdateEventHandler;

        public class UpdateEventArgs : EventArgs
        {
            public string Data { get; set; }
        }
        protected void insert()
        {
            UpdateEventArgs args = new UpdateEventArgs();
            UpdateEventHandler.Invoke(this, args);
        }
        private void FrmAkunAddEdit_Load(object sender, EventArgs e)
        {
           
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            lbliddata.Text = CompanyInfo.INIT;
            Load_TipeAkun();
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


        private void checkEditbagianakun_CheckStateChanged(object sender, EventArgs e)
        {
           
            Cursor.Current = Cursors.WaitCursor;

            if (checkEditbagianakun.Checked == true)
            {
                lookUpEdikat.Enabled = false;
                lookUpEditbagiandari.Enabled = true;
                LoadGroup();
            }
            else
            {
                lookUpEdikat.Enabled = true;
                lookUpEditbagiandari.EditValue = null;
                lookUpEditbagiandari.Enabled = false;
            }
            Cursor.Current = Cursors.Default;
        }

        private void LoadGroup()
        {
            try
            {
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
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        }

        private void lookUpEditbagiandari_EditValueChanged(object sender, EventArgs e)
        {
            //SplashScreenManager.ShowForm(typeof(WaitForm_Prosess));
            //Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (lookUpEditbagiandari.EditValue != null)
                {
                    
                    DevExpress.XtraEditors.LookUpEdit editor = sender as DevExpress.XtraEditors.LookUpEdit;
                    valueLVL = editor.GetColumnValue("LVL").ToString();
                    string valueKEL = editor.GetColumnValue("KEL").ToString();
                    string valueNama = editor.GetColumnValue("PERKIRAAN").ToString();
                    txtlvel.Text = (int.Parse(valueLVL) + 1).ToString();
                    txtkepalaakun.Text = valueKEL;
                    lblinduk.Text = valueNama;
                    txtnoakungroup.Text = lookUpEditbagiandari.Text.Substring(3, 6);
                    GenerateNewCode(valueLVL);

                    if (checkEditbagianakun.Checked == true)
                    {
                        lookUpEditbagiandari.Enabled = true;
                        gd.Enabled = true;
                        txtkepalaakun.Enabled = false;
                        //txtlvel.Text = "2";
                        txtkepalaakun.Text = lookUpEditbagiandari.Text.Substring(0, 3);
                        if (gd.SelectedIndex == 0)
                        {
                            txtnoakungroup.Enabled = true;
                        }
                        else
                        {
                            txtnoakungroup.Enabled = false;
                        }
                        GenerateNewCode(valueLVL);
                    }
                    else
                    {
                        lookUpEditbagiandari.Properties.DataSource = null;
                        txtlvel.Text = "1";
                        txtkepalaakun.Enabled = true;
                        txtnoakungroup.Enabled = true;
                        gd.SelectedIndex = 0;
                        gd.Enabled = false;
                        lookUpEditbagiandari.Enabled = false;
                        txtkepalaakun.Text = "00.";
                        txtnoakungroup.Text = "00000";
                        txtnoakundetail.Text = "000";
                        lblinduk.Text = "";
                    }
                }
            }
            catch
            {

            }
            finally
            {
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();
            }
            
        }

        private void GenerateNewCode(string valueLVL)
        {
            SplashScreenManager.ShowForm(typeof(WaitForm_Prosess));
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (valueLVL == "1")
                {
                    if (gd.SelectedIndex == 1)
                    {
                        var jlhkode = AccountServices.CekCountKode(CompanyInfo.INIT, Convert.ToInt32(setahun.Value), lookUpEditbagiandari.EditValue.ToString(), "D");
                        txtnoakungroup.Enabled = false;
                        txtnoakundetail.Text = (jlhkode + 1).ToString("000");
                    }
                    else
                    {
                        var jlhkode = AccountServices.CekCountKode(CompanyInfo.INIT, Convert.ToInt32(setahun.Value), lookUpEditbagiandari.EditValue.ToString(), "G");
                        txtnoakungroup.Enabled = true;
                        int jlh = jlhkode * 1000;
                        txtnoakungroup.Text = (jlh + 1000).ToString("00000");
                        txtnoakundetail.Text = "000";
                    }
                }
                else
                {
                    if (gd.SelectedIndex == 1)
                    {
                        CalcDetailCode();
                    }
                    else
                    {
                        CalcGroupCode();
                    }
                }
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

        private void gd_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateNewCode(valueLVL);
        }

        private void CalcGroupCode()
        {
            if (lookUpEditbagiandari.EditValue != null)
            {
                txtnoakungroup.Enabled = true;
                var jlhkode = AccountServices.CekCountKode(CompanyInfo.INIT, Convert.ToInt32(setahun.Value), lookUpEditbagiandari.EditValue.ToString(), "G");

                int sub = int.Parse(lookUpEditbagiandari.EditValue.ToString().Substring(3, 5));
                txtnoakungroup.Enabled = true;
                int jlh = jlhkode * 1000;
                txtnoakungroup.Text = (sub + jlh + 1).ToString("00000");
                txtnoakundetail.Text = "000";
            }
            
        }

        private void CalcDetailCode()
        {
            if (lookUpEditbagiandari.EditValue != null)
            {
                txtnoakungroup.Enabled = false;
                var sub = lookUpEditbagiandari.EditValue.ToString().Substring(3, 5);
                txtnoakungroup.Text = sub;
                var jlhkode = AccountServices.CekCountKode(CompanyInfo.INIT, Convert.ToInt32(setahun.Value), lookUpEditbagiandari.EditValue.ToString(), "D");
                txtnoakundetail.Text = (jlhkode + 1).ToString("000");
            }
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

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            SplashScreenManager.ShowForm(typeof(WaitForm_Prosess));
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (txtkepalaakun.Text.Length!=3 &&
                txtnoakungroup.Text.Length!=6 &&
                txtnoakundetail.Text.Length!=3)
                {
                    XtraMessageBox.Show("Kode Akun wajib diisi lengkap", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtnamaakun.Text))
                {
                    XtraMessageBox.Show("Nama Akun wajib diisi", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var p_userid = LoginInfo.userID;

                var pgrp = lookUpEdikat.EditValue.ToString();
                string pinduk;
                if (txtlvel.Text == "1")
                {
                    pinduk = null;
                }
                else
                {
                    pinduk = lookUpEditbagiandari.Text;
                }

                char pgd;
                if (gd.SelectedIndex == 0)
                {
                    pgd = 'G';
                }
                else
                {
                    pgd = 'D';
                }
                var pkode = txtkepalaakun.Text + txtnoakungroup.Text+txtnoakundetail.Text;
                var plvl = int.Parse(txtlvel.Text);
                char pposisi;
                if (rgsisi.SelectedIndex == 0)
                {
                    pposisi = 'D';
                }
                else
                {
                    pposisi = 'K';
                }
                var pnama = txtnamaakun.Text.Trim();
                var piddata = CompanyInfo.INIT;
                var p_tahun = ptahun;

                if(pinduk== pkode)
                {
                    XtraMessageBox.Show("Induk Akun tidak boleh sama dengan Kode Akun", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AccountServices.InsertCOA(piddata, p_tahun, pgrp, pinduk, pgd, pkode, plvl, pposisi, pnama, 0);
                //if (checkEditbagianakun.Checked == true)
                //{
                //    LoadGroup();
                //}
               
               
                insert();
               
                XtraMessageBox.Show("Account Code SAved", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                checkEditbagianakun.Checked = false;
                txtnamaakun.Text = "";
                txtkepalaakun.Text = "";
                txtnoakungroup.Text = "";
                txtnoakundetail.Text = "";
                
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Duplicate Kode Perkiraaan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("ORA-01400"))
                {
                    XtraMessageBox.Show("Nama Kode tidak boleh kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }               
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
            }
        }

        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            ptahun = Convert.ToInt32(setahun.Value);
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
    }
}
