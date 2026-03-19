using Accounting.BusinessLayer;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraEditors;
using ExcelDataReader;
using System.Text;

namespace Accounting.Form
{
    public partial class FrmImportCOA : DevExpress.XtraEditors.XtraForm
    {
        public FrmImportCOA()
        {
            InitializeComponent();
        }
        DataTable dt;
        DataTableCollection tables;
        int pbulan, ptahun;
        private void FrmImportCOA_Load(object sender, EventArgs e)
        {
            int x = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));
            int y = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 6, 4));

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            //setahun.Properties.MaxValue=y;
            setahun.Value = y;

            pbulan = x;
            ptahun = y;
        }

        private void sbbrowse_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {              
                gridControl1.DataSource = null;
                cboSheet.Text = "";
                using OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx| Excel 97-2003 Workbook|*.xls" };
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void SBImport_Click(object sender, EventArgs e)
        {
            var p_tahun = (int)setahun.Value;
            if (XtraMessageBox.Show("Lanjutkan Proses Import Daftar Perkiraan ? " +
                "\n\nTahun : " + p_tahun + " " +
                "\nLokasi Data :" +CompanyInfo.IDDATA
                , "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;


            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);

            try
            {
                Stopwatch watch = new ();
                watch.Start();
                //Kosongkan Data Table Oracle
                Hapus_Data_Table_Tmp();
                //Copy Data dari DataTable ke Oracle
                JurnalServices.ImportCOAOracleBulkCopy("ACC_COA_TMP", dt, CompanyInfo.JENIS_AKUNTING);
                //Update IDDATA dan UserID
                Update_IDData_Userid();

                if (xeparsial.Checked == true)
                {
                    AccountServices.ImportCOAbyMerge(CompanyInfo.IDDATA, p_tahun);
                }
                else
                {


                    //cek coa salah induk
                    var salahinduk = AccountServices.CekSalahInduk();
                    if (salahinduk.Rows.Count > 0)
                    {
                        List<string> list = salahinduk.AsEnumerable()
                               .Select(r => r.Field<string>("ACCOUNT"))
                               .ToList();
                        var CekSalahInduk = string.Join(Environment.NewLine, list);
                        XtraMessageBox.Show("Import Daftar Perkiraan di Batalkan \n" +
                            "\nInduk Perkiraan salah " + salahinduk.Rows.Count.ToString("##,###") + " Induk." +
                            "\n" + CekSalahInduk +
                            "\nInduk Akun tidak boleh sama dengan kode akun", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //cek apakah parentacc valid ?
                    var isvalidparent = AccountServices.CekParentNotExist();
                    if (isvalidparent.Rows.Count > 0)
                    {
                        List<string> list = isvalidparent.AsEnumerable()
                               .Select(r => r.Field<string>("INDUK"))
                               .ToList();
                        var parentnotexist = string.Join(Environment.NewLine, list);
                        XtraMessageBox.Show("Import Daftar Perkiraan di Batalkan \n" +
                            "\nInduk Perkiraan tidak terdaftar sebanyak " + isvalidparent.Rows.Count.ToString("##,###") + " Induk." +
                            "\n" + parentnotexist, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //cek apakah parentacc valid 2?
                    var isvalidparent2 = AccountServices.CekParentNotExist2();
                    if (isvalidparent2.Rows.Count > 0)
                    {
                        List<string> list = isvalidparent2.AsEnumerable()
                               .Select(r => r.Field<string>("ACCOUNT"))
                               .ToList();
                        var parentnotexist2 = string.Join(Environment.NewLine, list);
                        XtraMessageBox.Show("Import Daftar Perkiraan di Batalkan \n" +
                            "\nKode Perkiraan Detail tidak memiliki INDUK " + isvalidparent2.Rows.Count.ToString("##,###") +
                            "\n" + parentnotexist2, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    //Copy data dari table ACC_COA_TMP ke table ACCT_COA                
                    //proses import
                    //AccountServices.ImportCOA(CompanyInfo.IDDATA, Tahun);
                    AccountServices.ImportCOAbyMerge(CompanyInfo.IDDATA, p_tahun);


                    //create periode akuntansi jika belum ada
                    //jika periode belum ada, buat periode
                    string periodedipilih = "01/" + setahun.Value.ToString();
                    int pexist = JurnalServices.CekPeriodeExist(CompanyInfo.IDDATA, periodedipilih);
                    if (pexist == 0)
                    {
                        AccountServices.CreateNextPeriode(CompanyInfo.IDDATA, 0, p_tahun);
                    }
                   
                    Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
                    Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
                    Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
                    Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);
                }

                AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 1, p_tahun, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                SBImport.Enabled = false;
                XtraMessageBox.Show("Import Chart Of Account Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Update_IDData_Userid()
        {
            AccountServices.UpdateCOATmpIdData(CompanyInfo.IDDATA, (int)setahun.Value, LoginInfo.userID);
        }
        readonly string[] KolomWajibKEBUN = { "Account", "Nama Perkiraan", "Jenis", "Level", "Induk", "Gen", "Saldo Normal", "Awal Tahun", "Saldo Awal","Debet","Kredit","Mutasi","Saldo Akhir", "Divisi", "Blok",  "TahunTanam" };
        readonly string[] KolomWajib = { "Account", "Nama Perkiraan", "Jenis", "Level", "Induk", "Gen", "Saldo Normal", "Awal Tahun", "Saldo Awal", "Debet", "Kredit", "Mutasi", "Saldo Akhir" };
        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dt = tables[cboSheet.SelectedItem.ToString()];
                if (dt != null)
                {
                   
                    if (CompanyInfo.JENIS_AKUNTING == "KEBUN")
                    {
                        bool isValid = AccountServices.ValidateColumnNames(dt, KolomWajibKEBUN);
                        if (isValid)
                        {
                            var list=ConvertDataTableToListKEBUN(dt);    

                            gridControl1.DataSource = list;
                            SBImport.Enabled = true;
                            gridView1.Columns[7].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                            gridView1.Columns[7].DisplayFormat.FormatString = "n2";

                            gridView1.BestFitColumns();
                        }
                    }
                    else
                    {
                        bool isValid = AccountServices.ValidateColumnNames(dt, KolomWajib);
                        if (isValid)
                        {
                            var list = ConvertDataTableToList(dt);

                            gridControl1.DataSource = list;
                            SBImport.Enabled = true;
                            gridView1.Columns[7].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                            gridView1.Columns[7].DisplayFormat.FormatString = "n2";

                            gridView1.BestFitColumns();
                        }

                    }
                    lblrecord.Text = dt.Rows.Count.ToString("##,###") + " Record";
                }
            }
            catch (SystemException ex)
            {
                SBImport.Enabled = false;
                XtraMessageBox.Show("Format File Excel Daftar Perkiraan Salah\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Hapus_Data_Table_Tmp()
        {
            AccountServices.TruncateCOATmp();
        }

        private static List<COA_Import> ConvertDataTableToList(DataTable table)
        {
            // Create a new list to hold the objects
            List<COA_Import> dataRowObjects = new();
            // Loop through each row in the DataTable
            foreach (DataRow row in table.Rows)
            {
                // Create a new object to hold the data from the row
                COA_Import dataRowObject = new()
                {
                    // Set the values of the object from the data in the row
                    Account = row["Account"].ToString(),
                    NamaPerkiraan = row["Nama Perkiraan"].ToString(),
                    Jenis = row["Jenis"].ToString(),
                    Level = row["Level"].ToString(),
                    Induk = row["Induk"].ToString(),
                    Gen = row["Gen"].ToString(),
                    Posisi = row["Saldo Normal"].ToString(),
                    AwalTahun = Convert.ToDecimal(row["Awal Tahun"].ToString())
                };

                // Add the object to the list
                dataRowObjects.Add(dataRowObject);
            }

            // Return the list of objects
            return dataRowObjects;
        }

        private static List<COA_ImportKEBUN> ConvertDataTableToListKEBUN(DataTable table)
        {
            // Create a new list to hold the objects
            List<COA_ImportKEBUN> dataRowObjects = new List<COA_ImportKEBUN>();
            // Loop through each row in the DataTable
            foreach (DataRow row in table.Rows)
            {
                // Create a new object to hold the data from the row
                COA_ImportKEBUN dataRowObject = new()
                {
                    // Set the values of the object from the data in the row
                    Account = row["Account"].ToString(),
                    NamaPerkiraan = row["Nama Perkiraan"].ToString(),
                    Jenis = row["Jenis"].ToString(),
                    Level = row["Level"].ToString(),
                    Induk = row["Induk"].ToString(),
                    Gen = row["Gen"].ToString(),
                    Posisi = row["Saldo Normal"].ToString(),
                    AwalTahun = Convert.ToDecimal(row["Awal Tahun"].ToString()),
                    Blok = row["Blok"].ToString(),
                    Divisi = row["Divisi"].ToString(),
                    TahunTanam = row["TahunTanam"].ToString()
                };
               

                // Add the object to the list
                dataRowObjects.Add(dataRowObject);
            }

            // Return the list of objects
            return dataRowObjects;
        }
    }
}
