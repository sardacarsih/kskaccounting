using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using ExcelDataReader;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmImportJurnalSheet : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        private SoundPlayer Player = new SoundPlayer();
        public FrmImportJurnalSheet()
        {
            InitializeComponent();
        }
        DataTable dt;
        int pbulan, ptahunsumber,ptahuntujuan;
        string bulan,periodetujuan;
        DataTableCollection tables;
        readonly string[] KolomWajib = { "NoJurnal", "Tanggal", "RowNo", "Kode", "Rekening", "Debet", "Kredit", "Keterangan", "Posted", "Periode" };
        private void FrmImportJurnalSheet_Load(object sender, EventArgs e)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
                int x = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));
               
                cmbbulan.SelectedIndex = x - 1;
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sbbrowse_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            try
            {
                gridControl1.DataSource = null;
                cboSheet.Text = "";
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx| Excel 97-2003 Workbook|*.xls"})
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtPath.Text = ofd.FileName;
                        using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read))
                        {
                            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                            {
                                DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                    {
                                        UseHeaderRow = true
                                    }
                                });
                                tables = result.Tables;
                                cboSheet.Properties.Items.Clear();
                                foreach (DataTable table in tables)
                                    cboSheet.Properties.Items.Add(table.TableName);
                            }
                            cboSheet.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            try
            {               
                dt = tables[cboSheet.SelectedItem.ToString()];
                if (dt != null)
                {
                    bool isValid = JurnalServices.ValidateColumnNames(dt, KolomWajib);
                    if (isValid)
                    {
                        var list = ConvertDataTableToList(dt);

                        var cekbaris = list.Where(x => x.RowNo < 0);
                        var duplicateEntries = list.GroupBy(j => new { j.NoJurnal, j.RowNo })
                                    .Where(g => g.Count() > 1)
                                    .SelectMany(g => g);
                        var transaksiMinus = list.Where(x => x.Debet < 0 || x.Kredit<0);


                        if (transaksiMinus.Any())
                        {
                            gridControl1.DataSource = transaksiMinus;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Nilai Transaksi tidak boleh minust", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        if (cekbaris.Any())
                        {
                            gridControl1.DataSource = cekbaris;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Tentukan Nomor Baris dengan benar pada daftar nomor jurnal berikut", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        if (duplicateEntries.Any())
                        {
                            gridControl1.DataSource = duplicateEntries;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Double Nomor urut pada nomor jurnal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            gridControl1.DataSource = list;
                            SBImport.Enabled = true;
                        }
                        gridView1.Columns[0].Visible = false;
                        gridView1.Columns[2].OptionsColumn.FixedWidth = true;
                        gridView1.Columns[2].Width = 100;
                        gridView1.Columns[2].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        gridView1.Columns[2].DisplayFormat.FormatString = "dd-MMM-yyyy";
                        gridView1.Columns[4].OptionsColumn.FixedWidth = true;
                        gridView1.Columns[4].Width = 120;
                        //gridView1.Columns[2].Width = 50;
                        gridView1.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[6].DisplayFormat.FormatString = "n2";
                        gridView1.Columns[7].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[7].DisplayFormat.FormatString = "n2";

                        gridView1.BestFitColumns();

                        lblrecord.Text = dt.Rows.Count.ToString("##,###") + " Record";


                    }
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SBImport_Click(object sender, EventArgs e)
        {
            try
            {
                pbulan = Convert.ToInt32(cmbbulan.SelectedIndex + 1);
                ptahuntujuan = Convert.ToInt32(setahun.Value);
                bulan = cmbbulan.Text + " - " + setahun.Value.ToString();
                
                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal ? " +
                    "\n\nPeriode : " + bulan + " " +
                    "\nLokasi Data :" +CompanyInfo.IDDATA
                    , "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                //jika periode telah dikunci,  batalkan proses import jurnal
                periodetujuan = pbulan.ToString("0#") + "/" + ptahuntujuan.ToString();
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.IDDATA, periodetujuan);
               
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_dikunci.wav";
                    this.Player.Play();
                    sbbrowse.Enabled = false;
                    SBImport.Enabled = false;
                    XtraMessageBox.Show("Tidak dapat melakukan proses import Jurnal pada periode ini...!!!\nPeriode Akuntansi : " + bulan + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //Cursor.Current = Cursors.Default;
                    //SplashScreenManager.CloseForm();
                    return;
                }
                else
                {
                    sbbrowse.Enabled = true;
                }
                //jika periode belum ada, buat periode
                string periodedipilih = pbulan.ToString("0#")+"/"+ptahuntujuan.ToString();
                int pexist = JurnalServices.CekPeriodeExist(CompanyInfo.IDDATA, periodedipilih);
                if (pexist==0)
                {                   
                    AccountServices.CreateNextPeriode(CompanyInfo.IDDATA, pbulan-1, ptahuntujuan);
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();

                foreach (DataRow row in dt.Rows)
                {
                    object periodValue = row["Periode"];

                    if (periodValue == DBNull.Value)
                    {
                        // The period is null
                        // Perform your desired actions here
                        XtraMessageBox.Show("Import Jurnal di Batalkan \nKolom Periode WAJIB diisi ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string PeriodeAsal = (string)dt.AsEnumerable().Max(x => x["Periode"].ToString());
                if(periodetujuan!= PeriodeAsal)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_beda.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nPilihan Periode tidak sama dengan sumber data ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Cursor.Current = Cursors.Default;
                    //SplashScreenManager.CloseForm();
                    return;
                }
                //MessageBox.Show(Periode);
                //Kosongkan Data Table Oracle
                Hapus_Data_Table_Tmp();
                //Copy Data dari DataTable ke Oracle
                JurnalServices.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", dt);
                //Update IDDATA dan UserID
                Update_IDData_Userid();

            //CEK KODE NULL
            var KODENULL = JurnalServices.CekJurnal_KODENULL();
            if (KODENULL.Rows.Count > 0)
            {
                List<string> list = KODENULL.AsEnumerable()
                       .Select(r => r.Field<string>("NOJURNAL"))
                       .ToList();
                var daftarKODENULL = string.Join(Environment.NewLine, list);
                XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                    "\nKode Jurnal belum diisi sebanyak " + KODENULL.Rows.Count.ToString("##,###") + " Nomor." +
                    "\n" + daftarKODENULL, "Kode Jurnal Kosong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ////cek duplikasi nomor jurnal, nomor jurnal sama tanggal beda, jika ada proses import batal

            //var duplikasi = JurnalServices.CekDuplikasiJurnal();
            //if (duplikasi.Rows.Count > 0)
            //{
            //    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
            //    this.Player.Play();
            //    List<string> list = duplikasi.AsEnumerable()
            //           .Select(r => r.Field<string>("NOJURNAL"))
            //           .ToList();
            //    var daftarnojurnal = string.Join(Environment.NewLine, list);
            //    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
            //        "\nDuplikasi NoJurnal sebanyak " + duplikasi.Rows.Count.ToString("##,###") + " Nomor." +
            //        "\nBerikut ini NoJurnalnya : \n\n" + daftarnojurnal, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    //Cursor.Current = Cursors.Default;
            //    //SplashScreenManager.CloseForm();
            //    return;
            //}


            //cek aku master sudah ada ?
            ptahunsumber = Convert.ToInt32(PeriodeAsal.Substring(3, 4));
                var akun = JurnalServices.CekAkunMaster(ptahunsumber);
                if (akun.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_daftarperk.wav";
                    this.Player.Play();
                    List<string> list = akun.AsEnumerable()
                           .Select(r => r.Field<string>("ASAL"))
                           .ToList();
                    var daftarakun = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nJumlah Kode tidak terdaftar sebanyak " + akun.Rows.Count.ToString("##,###") + " Akun." +
                        "\nKode Akun dibawah ini belum ada di Daftar Perkiraan \n" + daftarakun, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) ;
                    //Cursor.Current = Cursors.Default;
                    //SplashScreenManager.CloseForm();
                    return;
                }
               
                //Copy data dari table tmp ke table Acct_Jurnal_Dtl
                var sukses = JurnalServices.ImportJurnalGlobal(CompanyInfo.IDDATA, pbulan,ptahuntujuan, PeriodeAsal);
                if (sukses == 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_bedaperiode.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nCek Periode Pada Lembar Excel Double ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);                   
                    return;
                }
                if (sukses == 1)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_imbang.wav";
                    this.Player.Play();
                    Jurnal_NotBalanced f = new();
                    f.ShowDialog();
                    return;
                }

                //analisa kesalahan COA
                var Errcoa_check = ToolsServices.Analisa_kesalahan_COA(CompanyInfo.IDDATA, ptahuntujuan);
                if (Errcoa_check.Rows.Count > 0)
                {
                    COAError error_coa = new()
                    {
                        ibulan = 12,
                        itahun = ptahuntujuan
                    };
                    error_coa.ShowDialog();
                    return;
                }
                AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, pbulan, ptahuntujuan, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                SBImport.Enabled = false;
                //Cursor.Current = Cursors.Default;
                //SplashScreenManager.CloseForm();
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
                this.Player.Play();
                XtraMessageBox.Show("Import Jurnal Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Update_IDData_Userid()
        {
            try
            {
                string query = "update ACCT_JURNAL_TMP set IDDATA=:piddata,GLYEAR=:PTAHUN,GLMONTH=:PBULAN,userid=:puserid";
                conn.Open();
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value =CompanyInfo.IDDATA;
                cmd.Parameters.Add(":PTAHUN", OracleDbType.Int16).Value = Convert.ToInt32(setahun.Value);
                cmd.Parameters.Add(":PBULAN", OracleDbType.Int16).Value = Convert.ToInt32(cmbbulan.SelectedIndex + 1);
                cmd.Parameters.Add(":puserid", OracleDbType.Varchar2, 20).Value = LoginInfo.userID;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Hapus_Data_Table_Tmp()
        {
            try
            {
                string query = "TRUNCATE TABLE ACCT_JURNAL_TMP";
                conn.Open();
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static List<ImportJurnalList> ConvertDataTableToList(DataTable table)
        {
            // Create a new list to hold the objects
            List<ImportJurnalList> dataRowObjects = new();
            // Loop through each row in the DataTable
            foreach (DataRow row in table.Rows)
            {
                // Create a new object to hold the data from the row
                ImportJurnalList dataRowObject = new()
                {
                    // Set the values of the object from the data in the row
                    NoJurnal = row["NoJurnal"].ToString(),
                    Tanggal = Convert.ToDateTime(row["Tanggal"].ToString()),
                    RowNo = Convert.ToInt32(row["RowNo"].ToString()),
                    Kode = row["Kode"].ToString(),
                    Rekening = row["Rekening"].ToString(),
                    Debet = Convert.ToDecimal(row["Debet"].ToString()),
                    Kredit = Convert.ToDecimal(row["Kredit"].ToString()),
                    Keterangan = row["Keterangan"].ToString(),
                    Posted = row["Posted"].ToString(),
                    Periode = row["Periode"].ToString()
                };

                // Add the object to the list
                dataRowObjects.Add(dataRowObject);
            }

            // Return the list of objects
            return dataRowObjects;
        }
    }
}
