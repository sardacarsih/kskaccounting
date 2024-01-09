using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Diagnostics;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmClosingYear : DevExpress.XtraEditors.XtraForm
    {
        
        public FrmClosingYear()
        {
            InitializeComponent();
           
        }
        int  p_tahun, nextyear;
        string periode;        // '01/2021'
        string bulan;         // Januari-2021
                              // string periodedipilih;  // 202101        

        private SoundPlayer Player = new SoundPlayer();

        private void FrmClosingYear_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Desember" });
            
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
            cmbbulan.SelectedIndex =0;
            lblpt.Text = CompanyInfo.NAMAPT;
            lbldata.Text = CompanyInfo.INIT;
            lblwilayah.Text = CompanyInfo.WILAYAH;
        }


        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {
                Stopwatch watch = new ();
                watch.Start();
                p_tahun = Convert.ToInt32(setahun.Value);
                //jika periode telah dikunci,  batalkan proses import jurnal            
                periode = 12.ToString("0#") + "/" + p_tahun.ToString();
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, periode);
                bulan = cmbbulan.Text + " - " + setahun.Value.ToString();
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\akhir_tahun_kunci.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Proses Closing diBatalkan...!!!\nPeriode Akuntansi : " + bulan + " Telah Dikunci.", "Error Closing", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                
                var record = JurnalServices.CekRecordJurnalExist(CompanyInfo.INIT, periode);
               
                if (record == 0) {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\akhir_tahun_jurnal.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Belum ada transaksi jurnal", "info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                

                //analisa kesalahan COA
                var Errcoa_check = ToolsServices.Analisa_kesalahan_COA(CompanyInfo.INIT, p_tahun);
                if (Errcoa_check.Rows.Count > 0)
                {
                    COAError error_coa = new()
                    {
                        Myperiode = periode,
                        ibulan = 12,
                        itahun = p_tahun
                    };
                    error_coa.ShowDialog();
                    return;
                }

                //update level account
                AccountServices.UpdateLevelAccount(CompanyInfo.INIT, p_tahun);

                //rekalkulasi bulan desember
                AccountServices.RekalkulasiSaldo(CompanyInfo.INIT, 12, p_tahun, LoginInfo.userID);

                //generate data laba rugi dan jurnal closing
                if (CompanyInfo.JENIS_AKUNTING != "LAIN")
                {
                    if (checkEditjurnalclosing.Checked == true)
                    {
                        LaporanServices.Generate_Jurnal_Closing(CompanyInfo.INIT, 12, p_tahun, LoginInfo.userID, CompanyInfo.JENIS_AKUNTING);
                    }
                    //CAK APAKAH NECARA SUDAH BALACE
                    var selisih = LaporanServices.Balanced_Check(CompanyInfo.INIT, 12, p_tahun);
                    if (selisih != 0)
                    {
                        this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\neraca_error.wav";
                        this.Player.Play();
                        BalancedError be = new BalancedError
                        {
                            Myperiode = periode,
                            ibulan=12,
                            itahun= p_tahun
                        };
                        be.ShowDialog();
                        return;

                    }
                    else //jika neraca balance cek coa tahun berikutnya
                    {
                        //cek coa next year 
                        nextyear = p_tahun + 1;
                        var coa_nextyear = AccountServices.CekCOAExist(CompanyInfo.INIT, nextyear);
                        if (coa_nextyear == 1) //1 tidak ada coa, buat coa 
                        {
                            //buat c0a baru untuk tahun depan
                            AccountServices.ClosingEndYear(CompanyInfo.INIT, p_tahun, LoginInfo.userID);

                        }
                        else
                        {
                            //hanya update saldo akhir tahun ke saldo awal tahun  berikutnya
                            AccountServices.ClosingEndYearUpdateOnly(CompanyInfo.INIT, p_tahun, LoginInfo.userID);
                        }

                        if (checkEditjurnalclosing.Checked == true)
                        {
                            //reclass saldo LR tahun berjalan ke laba ditahan
                            AccountServices.ReclassLabaRugi(CompanyInfo.INIT, p_tahun, LoginInfo.userID);
                        }
                    }
                }
                else //jenis akunting LAIN tidak melakukan pengecekan neraca balance
                {
                    //cek coa next year 
                    nextyear = p_tahun + 1;
                    var coa_nextyear = AccountServices.CekCOAExist(CompanyInfo.INIT, nextyear);
                    if (coa_nextyear == 1) //1 tidak ada coa, buat coa 
                    {
                        //buat c0a baru untuk tahun depan
                        AccountServices.ClosingEndYear(CompanyInfo.INIT, p_tahun, LoginInfo.userID);

                    }
                    else
                    {
                        //hanya update saldo akhir tahun ke saldo awal tahun  berikutnya
                        AccountServices.ClosingEndYearUpdateOnly(CompanyInfo.INIT, p_tahun, LoginInfo.userID);
                    }

                }               
               

                //create jurnal reverse
                JurnalServices.JurnalRE(CompanyInfo.INIT, periode, LoginInfo.userID);
                nextyear = p_tahun + 1;
                //ANTISIPASI jika sudah ada jurnal hitung ulang saldo
                AccountServices.RekalkulasiSaldo(CompanyInfo.INIT, 1, nextyear, LoginInfo.userID);
                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.INIT);

                watch.Stop();
                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

               

                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\akhir_tahun.wav";
                this.Player.Play();
                XtraMessageBox.Show("Proses Tutup Tahun Selesai\n\n" +
                    "Periode Akuntansi : " + bulan + "\nLokasi Data : " + CompanyInfo.INIT + "\n" + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.INIT);
            }
            catch (Exception ex)
            {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}