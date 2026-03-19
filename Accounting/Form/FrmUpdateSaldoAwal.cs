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
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraEditors;
using ExcelDataReader;
using System.Text;
using DevExpress.XtraBars.Docking2010;
using System.Windows;
using Dapper;

namespace Accounting.Form
{
    public partial class FrmUpdateSaldoAwal : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public FrmUpdateSaldoAwal()
        {
            InitializeComponent();
        }
        DataTable dt;
        DataTableCollection tables;
        List<COA_SaldoAwal> DaftarAkunSaldoAwal = new();
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
                    using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
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
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void SBImport_Click(object sender, EventArgs e)
        {
            var p_tahun = (int)setahun.Value;
            if (XtraMessageBox.Show("Lanjutkan Proses Update Saldo Awal Daftar Perkiraan ? " +
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
            var p_iddata =CompanyInfo.IDDATA;
            var p_tahuncoa = Convert.ToInt16(setahun.Value);
           

                //bandingkan COA apakah sudah ada semua
                
            var accountList_toUpdate = RetrieveAccountData(p_iddata, p_tahuncoa);
            var accountList_Source = DaftarAkunSaldoAwal.Where(saldo => saldo.AwalTahun != 0);

                //cek apakah daftar akun source sudah ada pada daftar tujuan
            bool allExistInAccountList = accountList_Source.All(item1 => accountList_toUpdate.Any(item2 => item2.Account == item1.Account));
                //jika akun tujuan belum lengkap tampilkan aku apa saja
            if (!allExistInAccountList)
            {
                var recordsNotInAccountList = accountList_Source.Where(item1 => !accountList_toUpdate.Any(item2 => item2.Account == item1.Account)).ToList();

                if (recordsNotInAccountList.Any())
                {
                    string recordsCombined = string.Join(Environment.NewLine, recordsNotInAccountList.Select(item => $"{item.Account} {item.NamaPerkiraan}"));

                    // Show the records in the XtraMessageBox
                    XtraMessageBox.Show("Perkiraan Akun ini belum ada pada Daftar Perkiraan:\n" + recordsCombined, "Info", (MessageBoxButtons)MessageBoxButton.OK, MessageBoxIcon.Information);
         
                }
                return;
            }
                //set saldo awal tahun semua akun jadi 0
                UpdateSaldoAwalTahun_Nol(p_iddata, p_tahuncoa);

                //update coa saldo awal tahun

                UpdateSaldoAwalTahun_FromList(p_iddata, p_tahuncoa, accountList_Source);

            Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
                    Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
                    Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
                    Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);
              

                AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, 1, p_tahun, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                SBImport.Enabled = false;
                XtraMessageBox.Show("Update Saldo Awal Tahun Chart Of Account Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void UpdateSaldoAwalTahun_FromList(string p_iddata, short p_tahuncoa, IEnumerable<COA_SaldoAwal> accountList_Source)
        {
            using OracleConnection connection = new OracleConnection( LoginInfo.OracleConnString);
            connection.Open();

            foreach (var akun in accountList_Source)
            {
                string updateQuery = "UPDATE ACCT_COA SET SALDOAWAL = :saldoawal WHERE IDDATA = :p_iddata AND TAHUN = :p_tahun AND KODEACC = :p_kodeacc";

                connection.Execute(updateQuery, new
                {
                    saldoawal = akun.AwalTahun,
                    p_iddata = p_iddata,
                    p_tahun = p_tahuncoa,
                    p_kodeacc = akun.Account
                });
            }
        }

        private void UpdateSaldoAwalTahun_Nol(string p_iddata, short p_tahuncoa)
        {
            using (OracleConnection connection = new( LoginInfo.OracleConnString))
            {
                connection.Open();

                string query = "UPDATE ACCT_COA SET SALDOAWAL=0 WHERE IDDATA=:iddata AND TAHUN=:tahun ";
                // Replace YourTable, Tahun, and IdData with the actual table name and column names

                using OracleCommand command = new(query, connection);
                command.Parameters.Add("iddata", OracleDbType.Varchar2).Value = p_iddata;
                command.Parameters.Add("tahun", OracleDbType.Int32).Value = p_tahuncoa;
                command.ExecuteNonQuery();

            }
        }

        static List<COA_SaldoAwal> RetrieveAccountData(string iddata, int tahun)
        {
            List<COA_SaldoAwal> accountList = new();

            using (OracleConnection connection = new( LoginInfo.OracleConnString))
            {
                connection.Open();

                string query = "SELECT KODEACC FROM ACCT_COA WHERE IDDATA=:iddata AND TAHUN=:tahun ORDER BY KODEACC";
                // Replace YourTable, Tahun, and IdData with the actual table name and column names

                using OracleCommand command = new(query, connection);
                command.Parameters.Add("iddata", OracleDbType.Varchar2).Value = iddata;
                command.Parameters.Add("tahun", OracleDbType.Int32).Value = tahun;

                using OracleDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    COA_SaldoAwal daftarakuntoupdate = new()
                    {
                        Account = reader["KODEACC"].ToString()
                    };
                    accountList.Add(daftarakuntoupdate);
                }
                   
            }

            return accountList;
        }

        readonly string[] KolomWajib = { "Account", "Nama Perkiraan", "Awal Tahun" };
        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dt = tables[cboSheet.SelectedItem.ToString()];
                if (dt != null)
                {
                    bool isValid = AccountServices.ValidateColumnNames(dt, KolomWajib);
                    if (isValid)
                    {
                        DaftarAkunSaldoAwal = ConvertDataTableToList(dt);

                        gridControl1.DataSource = DaftarAkunSaldoAwal;
                        SBImport.Enabled = true;
                        gridView1.Columns[2].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[2].DisplayFormat.FormatString = "n2";

                        gridView1.BestFitColumns();
                    }
                }
                       
            }
            catch (SystemException ex)
            {
                SBImport.Enabled = false;
                XtraMessageBox.Show("Format File Excel Daftar Perkiraan Salah\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static List<COA_SaldoAwal> ConvertDataTableToList(DataTable table)
        {
            // Create a new list to hold the objects
            List<COA_SaldoAwal> dataRowObjects = new();
            // Loop through each row in the DataTable
            foreach (DataRow row in table.Rows)
            {
                // Create a new object to hold the data from the row
                COA_SaldoAwal dataRowObject = new()
                {
                    // Set the values of the object from the data in the row
                    Account = row["Account"].ToString(),
                    NamaPerkiraan = row["Nama Perkiraan"].ToString(),
                    AwalTahun = Convert.ToDecimal(row["Awal Tahun"].ToString())
                };

                // Add the object to the list
                dataRowObjects.Add(dataRowObject);
            }

            // Return the list of objects
            return dataRowObjects;
        }
    }
}
