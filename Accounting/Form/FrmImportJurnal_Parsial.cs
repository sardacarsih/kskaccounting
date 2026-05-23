using Accounting.BusinessLayer;
using Accounting.Model;
using Accounting.Services;
using DevExpress.Mvvm.Native;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmImportJurnal_Parsial : DevExpress.XtraEditors.XtraForm
    {
        private readonly SoundPlayer Player = new SoundPlayer();
        public FrmImportJurnal_Parsial()
        {
            InitializeComponent();
        }
        
        DataTable dt;
        DataTableCollection tables;
        int pbulan, ptahun;
        string periodetujuan;
        readonly string[] KolomWajib = { "NoJurnal", "Tanggal", "RowNo", "Kode", "Rekening", "Debet", "Kredit", "Keterangan", "Posted", "Periode" };
        private void FrmImportJurnal_Parsial_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationService.EnsureCanImportJurnal();
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new MethodInvoker(Close));
                return;
            }

            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
                pbulan = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));
               
                
                cmbbulan.SelectedIndex = pbulan - 1;
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
                SBImport.Enabled = false;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void SBImport_Click(object sender, EventArgs e)
        {
            bool stagedRowsLoaded = false;
            try
            {
                AuthorizationService.EnsureCanImportJurnal();

                pbulan = cmbbulan.SelectedIndex + 1;
                ptahun = Convert.ToInt32(setahun.Value);
                periodetujuan = pbulan.ToString("0#") + "/" + ptahun;

                if (dt == null || dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Data excel belum dipilih atau kosong.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //jika periode telah dikunci,  batalkan proses import jurnal
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.IDDATA, periodetujuan);
                var Periode = cmbbulan.Text + " - " + setahun.Value.ToString();
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_dikunci.wav";
                    this.Player.Play();
                    sbbrowse.Enabled = false;
                    SBImport.Enabled = false;
                    XtraMessageBox.Show("Tidak dapat melakukan proses import Jurnal pada periode ini...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    sbbrowse.Enabled = true;
                }
                var Bulan = cmbbulan.Text + " - " + setahun.Value.ToString();
                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal ? " +
                    "\n\nPeriode : " + Bulan + " " +
                    "\nLokasi Data :" + CompanyInfo.IDDATA
                    , "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                Stopwatch watch = new();
                watch.Start();

                if (!TryGetSinglePeriodeFromData(dt, out string periodeAsal, out string periodeError))
                {
                    XtraMessageBox.Show(
                        "Import Jurnal dibatalkan.\n" + periodeError,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (periodetujuan != periodeAsal)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_beda.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nPilihan Periode tidak sama dengan sumber data ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                DataTable importScopeTable = BuildScopedImportDataTable(dt, CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID, ptahun, pbulan);

                //jika periode belum ada, buat periode
                int pexist = JurnalServices.CekPeriodeExist(CompanyInfo.IDDATA, periodetujuan);
                if (pexist == 0)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.IDDATA, pbulan - 1, ptahun);
                }

                //Kosongkan data tmp milik user+periode aktif saja (tanpa ganggu user lain).
                JurnalServices.DeleteJurnalTmpByScope(CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID);

                //Copy Data dari DataTable ke Oracle
                JurnalServices.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", importScopeTable);
                stagedRowsLoaded = true;

                //CEK KODE NULL
                DataTable KODENULL = JurnalServices.CekJurnal_KODENULL_Scoped(CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID);
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

                //cek duplikasi nojurnal pada periode dan lokasi data yang sama
                DataTable dup_nojurnal = JurnalServices.CekNoJurnalExistScoped(CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID);
                if (dup_nojurnal.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                    this.Player.Play();
                    List<string> list = dup_nojurnal.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarnojurnalexist = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nDuplikasi NoJurnal sebanyak " + dup_nojurnal.Rows.Count.ToString("##,###") + " Nomor." +
                        "\n" + daftarnojurnalexist, "Nomor Jurnal Telah digunakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            
                //    return;
                //}


                //cek aku master sudah ada ?
                int ptahun_asal = Convert.ToInt32(periodeAsal.Substring(3, 4));
                DataTable akun = JurnalServices.CekAkunMasterScoped(ptahun_asal, CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID);
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
                        "\nKode Akun dibawah ini belum ada di Daftar Perkiraan \n" + daftarakun, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                //Copy data dari table tmp ke table Acct_Jurnal_Dtl
                int sukses = JurnalServices.ImportJurnalParsialScoped(CompanyInfo.IDDATA, pbulan, ptahun, periodetujuan, LoginInfo.userID);
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

                AccountServices.RekalkulasiSaldo(CompanyInfo.IDDATA, pbulan, ptahun, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                SBImport.Enabled = false;
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
                this.Player.Play();
                XtraMessageBox.Show("Import Jurnal Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (stagedRowsLoaded)
                {
                    try
                    {
                        JurnalServices.DeleteJurnalTmpByScope(CompanyInfo.IDDATA, periodetujuan, LoginInfo.userID);
                    }
                    catch
                    {
                        // best effort cleanup only
                    }
                }
            }
        }

        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                dt = tables[cboSheet.SelectedItem.ToString()];
                if (dt != null)
                {
                    bool isValid = JurnalServices.ValidateColumnNames(dt, KolomWajib);
                    if (isValid)
                    {
                        List<ImportJurnalList> list = ConvertDataTableToList(dt);

                        var cekbaris = list.Where(x => x.RowNo < 0);
                        var duplicateEntries = list.GroupBy(j => new { j.NoJurnal,j.Tanggal,j.RowNo })
                                    .Where(g => g.Count() > 1)
                                    .SelectMany(g => g);

                        var transaksiMinus = list.Where(x => x.Debet < 0 || x.Kredit < 0);


                        if (transaksiMinus.Any())
                        {
                            gridControl1.DataSource = transaksiMinus;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Nilai Transaksi tidak boleh minust", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        bool hasValidationError = false;
                        if (cekbaris.Any())
                        {
                            gridControl1.DataSource = cekbaris;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Tentukan Nomor Baris dengan benar pada daftar nomor jurnal berikut", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            hasValidationError = true;
                        }
                        if (duplicateEntries.Any())
                        {
                            gridControl1.DataSource = duplicateEntries;
                            gridView1.Columns[0].Visible = false;
                            SBImport.Enabled = false;
                            XtraMessageBox.Show("Double Nomor urut pada nomor jurnal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            hasValidationError = true;
                        }

                        if (!hasValidationError)
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
                SBImport.Enabled = false;
                XtraMessageBox.Show("Format Jurnal File Excel Salah\n"+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static List<ImportJurnalList> ConvertDataTableToList(DataTable table)
        {
            List<ImportJurnalList> dataRowObjects = new();
            for (int index = 0; index < table.Rows.Count; index++)
            {
                DataRow row = table.Rows[index];
                int excelRow = index + 2; // +2 because header row is row 1 in excel

                ImportJurnalList dataRowObject = new()
                {
                    NoJurnal = ReadString(row, "NoJurnal"),
                    Tanggal = ReadDateTime(row, "Tanggal", excelRow),
                    RowNo = ReadInt32(row, "RowNo", excelRow),
                    Kode = ReadString(row, "Kode"),
                    Rekening = ReadString(row, "Rekening"),
                    Debet = ReadDecimal(row, "Debet", excelRow),
                    Kredit = ReadDecimal(row, "Kredit", excelRow),
                    Keterangan = ReadString(row, "Keterangan"),
                    Posted = ReadString(row, "Posted"),
                    Periode = ReadString(row, "Periode")
                };

                dataRowObjects.Add(dataRowObject);
            }

            return dataRowObjects;
        }

        private static bool TryGetSinglePeriodeFromData(DataTable source, out string periode, out string errorMessage)
        {
            periode = string.Empty;
            errorMessage = string.Empty;

            List<string> periodeValues = source.AsEnumerable()
                .Select(row => row["Periode"]?.ToString()?.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (periodeValues.Count == 0)
            {
                errorMessage = "Kolom Periode wajib diisi.";
                return false;
            }

            if (periodeValues.Count > 1)
            {
                errorMessage = "Ditemukan lebih dari satu nilai periode pada file import: " + string.Join(", ", periodeValues);
                return false;
            }

            periode = periodeValues[0];
            return true;
        }

        private static DataTable BuildScopedImportDataTable(
            DataTable source,
            string iddata,
            string periode,
            string userid,
            int glYear,
            int glMonth)
        {
            DataTable scoped = source.Copy();
            EnsureColumn(scoped, "IDDATA", typeof(string));
            EnsureColumn(scoped, "USERID", typeof(string));
            EnsureColumn(scoped, "GLYEAR", typeof(int));
            EnsureColumn(scoped, "GLMONTH", typeof(int));

            foreach (DataRow row in scoped.Rows)
            {
                row["Periode"] = periode;
                row["IDDATA"] = iddata;
                row["USERID"] = userid;
                row["GLYEAR"] = glYear;
                row["GLMONTH"] = glMonth;
            }

            return scoped;
        }

        private static void EnsureColumn(DataTable dataTable, string columnName, Type dataType)
        {
            if (!dataTable.Columns.Contains(columnName))
            {
                dataTable.Columns.Add(columnName, dataType);
                return;
            }

            if (dataTable.Columns[columnName].DataType != dataType)
            {
                throw new InvalidOperationException($"Kolom {columnName} memiliki tipe data tidak sesuai untuk proses import.");
            }
        }

        private static string ReadString(DataRow row, string columnName)
        {
            object value = row[columnName];
            return value == DBNull.Value ? string.Empty : value.ToString()?.Trim() ?? string.Empty;
        }

        private static int ReadInt32(DataRow row, string columnName, int excelRow)
        {
            object value = row[columnName];
            if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            {
                throw new FormatException($"Baris Excel {excelRow}: kolom {columnName} wajib diisi.");
            }

            if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
            {
                return parsed;
            }

            if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed))
            {
                return parsed;
            }

            throw new FormatException($"Baris Excel {excelRow}: kolom {columnName} tidak valid ({value}).");
        }

        private static decimal ReadDecimal(DataRow row, string columnName, int excelRow)
        {
            object value = row[columnName];
            if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return 0m;
            }

            if (value is decimal dec) return dec;
            if (value is double dbl) return Convert.ToDecimal(dbl);
            if (value is float flt) return Convert.ToDecimal(flt);

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
            {
                return parsed;
            }

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return parsed;
            }

            throw new FormatException($"Baris Excel {excelRow}: kolom {columnName} tidak valid ({value}).");
        }

        private static DateTime ReadDateTime(DataRow row, string columnName, int excelRow)
        {
            object value = row[columnName];
            if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            {
                throw new FormatException($"Baris Excel {excelRow}: kolom {columnName} wajib diisi.");
            }

            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            throw new FormatException($"Baris Excel {excelRow}: kolom {columnName} tidak valid ({value}).");
        }


    }
}
