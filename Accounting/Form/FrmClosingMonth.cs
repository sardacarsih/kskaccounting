using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmClosingMonth : DevExpress.XtraEditors.XtraForm
    {
        int x;
        public FrmClosingMonth()
        {
            InitializeComponent();
           
        }
        int p_bulan, p_tahun;
        string periode;        // '01/2021'
        string bulan;         // Januari-2021
        string periodedipilih;  // 202101

        private SoundPlayer Player = new SoundPlayer();

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            Update_Periode();
            
        }
        private void Update_Periode()
        {
            p_tahun = Convert.ToInt32(setahun.Value);
            p_bulan = cmbbulan.SelectedIndex + 1;
            periodedipilih = p_tahun.ToString() + p_bulan.ToString("0#");
           
        }

        private void FrmClosingMonth_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
            x = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));
           
            cmbbulan.SelectedIndex = x - 1;
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            periodedipilih = p_tahun.ToString() + p_bulan.ToString("0#");

            lblnamapt.Text = CompanyInfo.NAMAPT;
            lbldata.Text =CompanyInfo.IDDATA;
            lblwilayah.Text = CompanyInfo.WILAYAH;
            //Cursor.Current = Cursors.Default;
            //SplashScreenManager.CloseForm();
        }

        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
           
                Update_Periode();
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            //try
            //{
                Stopwatch watch = new Stopwatch();
                watch.Start();

                //cek apakah periode yg dipilih ada, jika ada tombol proses aktif, jika tidak tombol proses disable
                var ada = AccountServices.CekPeriodeExist(CompanyInfo.IDDATA, p_bulan, p_tahun);
                if (ada == 0)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.IDDATA, p_bulan - 1, p_tahun);
                }

                //jika periode telah dikunci,  batalkan proses import jurnal            
                periode = p_bulan.ToString("0#") + "/" + p_tahun.ToString();
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.IDDATA, periode);
                bulan = cmbbulan.Text + " - " + setahun.Value.ToString();
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\akhir_bulan_kunci.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Proses Closing diBatalkan...!!!\nPeriode Akuntansi : " + bulan + " Telah Dikunci.", "Error Closing", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                var coaexist = AccountServices.CekCOAExist(CompanyInfo.IDDATA, p_tahun);
                if (coaexist == 1) //1 tidak ada coa, buat coa 
                {
                    XtraMessageBox.Show("Daftar Perkiraan Belum tersedia", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //analisa kesalahan COA
                var Errcoa_check = ToolsServices.Analisa_kesalahan_COA(CompanyInfo.IDDATA, p_tahun);
                if (Errcoa_check.Rows.Count > 0)
                {
                    COAError error_coa = new ()
                    {
                        Myperiode = periode,
                        ibulan = 12,
                        itahun = p_tahun
                    };
                    error_coa.ShowDialog();
                    return;
                }
                //cek kode perkiraan detail yang tidak memiliki induk akun
                //cek apakah parentacc valid 2?
                //var isvalidparent2 = AccountServices.CekParentNotExist3(CompanyInfo.IDDATA, p_tahun);
                //if (isvalidparent2.Rows.Count > 0)
                //{
                //    List<string> list = isvalidparent2.AsEnumerable()
                //           .Select(r => r.Field<string>("KODEACC"))
                //           .ToList();
                //    var parentnotexist2 = string.Join(Environment.NewLine, list);
                //    XtraMessageBox.Show("Rekalkulasi di Batalkan \n" +
                //        "\nKode Perkiraan Detail tidak memiliki INDUK :"+
                //        "\n" + parentnotexist2, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}


                //update level account
                AccountServices.UpdateLevelAccount(CompanyInfo.IDDATA, p_tahun);


                //create jurnal reverse
                JurnalServices.JurnalRE(CompanyInfo.IDDATA, periode, LoginInfo.userID);

                //mulai rekalkulasi saldo berdasarkan periode
                if (p_bulan == 1)
                {
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 1, p_tahun, LoginInfo.userID);

                }
                else if (p_bulan == 2)
                {
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 2, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 3)
                {                   
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 3, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 4)
                {
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 4, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 5)
                {
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 5, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 6)
                {                
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 6, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 7)
                {                
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 7, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 8)
                {                   
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 8, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 9)
                {                    
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 9, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 10)
                {                    
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 10, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 11)
                {                 
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 11, p_tahun, LoginInfo.userID);
                }
                else if (p_bulan == 12)
                {
                    AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 12, p_tahun, LoginInfo.userID);
                }
                else
                {
                    XtraMessageBox.Show("Periode Salah");
                }

                if (checkEditjurnalclosing.Checked == true) {

                    //generate data laba rugi dan jurnal closing
                    if (CompanyInfo.JENIS_AKUNTING != "LAIN")
                    {
                        if (p_bulan == 1)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 1, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 2)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 2, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 3)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 3, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 4)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 4, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 5)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 5, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 6)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 6, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 7)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 7, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 8)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 8, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 9)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 9, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 10)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 10, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 11)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 11, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                        else if (p_bulan == 12)
                        {
                            LaporanServices.Generate_Jurnal_Closing(CompanyInfo.IDDATA, 12, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                        }
                    }

                    //cek neraca
                    var selisih = LaporanServices.Balanced_Check(CompanyInfo.IDDATA, p_bulan, p_tahun);
                    if (selisih != 0)
                    {
                        this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\neraca_error.wav";
                        this.Player.Play();
                        BalancedError f = new BalancedError
                        {
                            Myperiode = periode,
                            ibulan = p_bulan,
                            itahun = p_tahun
                        };
                        f.ShowDialog();
                        return;
                    }
                }              

                watch.Stop();
                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\closing_month.wav";
                this.Player.Play();
                XtraMessageBox.Show("Proses Tutup buku Bulanan telah Selesai\n\n" +
                    "Periode Akuntansi : " + bulan + "\nLokasi Data : " +CompanyInfo.IDDATA + "\n" + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
            //catch (Exception ex)
            //{
            //        XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        //}
    }
}