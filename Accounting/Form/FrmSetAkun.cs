using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Accounting.BusinessLayer; 
using Accounting.DataLayer;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.Form
{
    public partial class FrmSetAkun : DevExpress.XtraEditors.XtraForm
    { 

        OracleConnection con = new( LoginInfo.OracleConnString);
        public FrmSetAkun()
        {
            InitializeComponent();
            // Instantiate the controller with the repository implementation
            var jurnalRepository = new JurnalRepository(); 
        }
        private void SBUpdate_Click(object sender, EventArgs e)
        {
            
            
        }

       
        private void FrmSetAkun_Load(object sender, EventArgs e)
        {
            con.Open();
            NAMAPT.Text = CompanyInfo.NAMAPT;
            IDDATA.Text =CompanyInfo.IDDATA;
            WILAYAH.Text = CompanyInfo.WILAYAH;

            L0ad_COA();
            Akun_Default();
            InitializeHrisTab();
        }

        private void L0ad_COA()
        {
            
            var dt = JurnalServices.KodeUntukJurnal(CompanyInfo.IDDATA, Acct.TahunMax);
            lookUpEdit_labatahunberjalan.Properties.DataSource = dt;
            lookUpEdit_labatahunberjalan.Properties.DisplayMember = "KODE";
            lookUpEdit_labatahunberjalan.Properties.ValueMember = "KODE";

            lookUpEdit_labaditahan.Properties.DataSource = dt;
            lookUpEdit_labaditahan.Properties.DisplayMember = "KODE";
            lookUpEdit_labaditahan.Properties.ValueMember = "KODE";

            lookUpEdit_alokasiLaba.Properties.DataSource = dt;
            lookUpEdit_alokasiLaba.Properties.DisplayMember = "KODE";
            lookUpEdit_alokasiLaba.Properties.ValueMember = "KODE";

            lookUpEdit_ASTEK.Properties.DataSource = dt;
            lookUpEdit_ASTEK.Properties.DisplayMember = "KODE";
            lookUpEdit_ASTEK.Properties.ValueMember = "KODE";

            lookUpEdit_pph21.Properties.DataSource = dt;
            lookUpEdit_pph21.Properties.DisplayMember = "KODE";
            lookUpEdit_pph21.Properties.ValueMember = "KODE";

            lookUpEdit_ymh.Properties.DataSource = dt;
            lookUpEdit_ymh.Properties.DisplayMember = "KODE";
            lookUpEdit_ymh.Properties.ValueMember = "KODE";

            lookUpEdit_persediaan.Properties.DataSource = dt;
            lookUpEdit_persediaan.Properties.DisplayMember = "KODE";
            lookUpEdit_persediaan.Properties.ValueMember = "KODE";

            lookUpEdit_bebanPemakaian.Properties.DataSource = dt;
            lookUpEdit_bebanPemakaian.Properties.DisplayMember = "KODE";
            lookUpEdit_bebanPemakaian.Properties.ValueMember = "KODE";

            lookUpEdit_hutangPembelian.Properties.DataSource = dt;
            lookUpEdit_hutangPembelian.Properties.DisplayMember = "KODE";
            lookUpEdit_hutangPembelian.Properties.ValueMember = "KODE";

            lookUpEdit_selisih.Properties.DataSource = dt;
            lookUpEdit_selisih.Properties.DisplayMember = "KODE";
            lookUpEdit_selisih.Properties.ValueMember = "KODE";

            lookUpEdit_barangDalamPerjalanan.Properties.DataSource = dt;
            lookUpEdit_barangDalamPerjalanan.Properties.DisplayMember = "KODE";
            lookUpEdit_barangDalamPerjalanan.Properties.ValueMember = "KODE";
        }

        private void Akun_Default()
        {
            
            string query = "select nama,kodeacc from acct_default WHERE iddata=:p_iddata";

            OracleCommand _command = new (query, con)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value =CompanyInfo.IDDATA;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();

            if (_dt.Rows.Count == 0)
            {
                //create default perkiraan closing
                AccountServices.CreateClosingAcct(CompanyInfo.IDDATA);
                XtraMessageBox.Show("Silahkan buka kembali form ini", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            // Create a DataView from the DataTable
            DataView dv_RL_TAHUN_BERJALAN = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'RL_TAHUN_BERJALAN'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable RL_TAHUN_BERJALAN = dv_RL_TAHUN_BERJALAN.ToTable();
            if(RL_TAHUN_BERJALAN.Rows.Count>0)
            lookUpEdit_labatahunberjalan.EditValue = RL_TAHUN_BERJALAN.Rows[0]["KODEACC"].ToString();

            // Create a DataView from the DataTable
            DataView dv_LABA_DITAHAN = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'LABA_DITAHAN'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable LABA_DITAHAN = dv_LABA_DITAHAN.ToTable();
            if(LABA_DITAHAN.Rows.Count>0)
            lookUpEdit_labaditahan.EditValue = LABA_DITAHAN.Rows[0]["KODEACC"].ToString();

            // Create a DataView from the DataTable
            DataView dv_ALOKASI_LABA_DITAHAN = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'ALOKASI_LABA_DITAHAN'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable ALOKASI_LABA_DITAHAN = dv_ALOKASI_LABA_DITAHAN.ToTable();
            if(ALOKASI_LABA_DITAHAN.Rows.Count>0)
            lookUpEdit_alokasiLaba.EditValue = ALOKASI_LABA_DITAHAN.Rows[0]["KODEACC"].ToString();



            // Create a DataView from the DataTable
            DataView dv_TUNJANGAN_ASTEK = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'TUNJANGAN_ASTEK'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable TUNJANGAN_ASTEK = dv_TUNJANGAN_ASTEK.ToTable();
            if (TUNJANGAN_ASTEK.Rows.Count > 0)
                lookUpEdit_ASTEK.EditValue = TUNJANGAN_ASTEK.Rows[0]["KODEACC"].ToString();

            // Create a DataView from the DataTable
            DataView dv_HUTANG_PPH21 = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'HUTANG_PPH21'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable HUTANG_PPH21 = dv_HUTANG_PPH21.ToTable();
            if (HUTANG_PPH21.Rows.Count > 0)
                lookUpEdit_pph21.EditValue = HUTANG_PPH21.Rows[0]["KODEACC"].ToString();
            
            

            // Create a DataView from the DataTable
            DataView dv_GAJI_DAN_UPAH_YMH_DIBAYAR = new(_dt)
            {
                // Filter the DataView
                RowFilter = "nama = 'GAJI_DAN_UPAH_YMH_DIBAYAR'" // replace "Column1" and "value1" with the column name and value you want to filter on
            };

            // Get the filtered DataTable from the DataView
            DataTable GAJI_DAN_UPAH_YMH_DIBAYAR = dv_GAJI_DAN_UPAH_YMH_DIBAYAR.ToTable();
           
            if (GAJI_DAN_UPAH_YMH_DIBAYAR.Rows.Count > 0)
                    lookUpEdit_ymh.EditValue = GAJI_DAN_UPAH_YMH_DIBAYAR.Rows[0]["KODEACC"].ToString();

            LoadDefaultAccount(_dt, "PERSEDIAAN", lookUpEdit_persediaan);
            LoadDefaultAccount(_dt, "BEBAN_PEMAKAIAN_PERSEDIAAN", lookUpEdit_bebanPemakaian);
            LoadDefaultAccount(_dt, "HUTANG_PEMBELIAN_PERSEDIAAN", lookUpEdit_hutangPembelian);
            LoadDefaultAccount(_dt, "SELISIH_PERSEDIAAN", lookUpEdit_selisih);
            LoadDefaultAccount(_dt, "BARANG_DALAM_PERJALANAN", lookUpEdit_barangDalamPerjalanan);
        }

        private static void LoadDefaultAccount(DataTable source, string nama, LookUpEdit edit)
        {
            DataView dv = new(source)
            {
                RowFilter = "nama = '" + nama + "'"
            };
            DataTable filtered = dv.ToTable();
            if (filtered.Rows.Count > 0)
                edit.EditValue = filtered.Rows[0]["KODEACC"].ToString();
        }

        private void SaveDefaultAccount(string nama, LookUpEdit edit, LabelControl lbl)
        {
            // A cleared lookup reports its placeholder text ("[EditValue is null]") via edit.Text;
            // never persist that as a kodeacc.
            if (edit.EditValue == null)
            {
                return;
            }

            string keterangan = (edit.Properties.GetDataSourceRowByKeyValue(edit.EditValue)
                                    as DataRowView)?["PERKIRAAN"].ToString() ?? string.Empty;

            // ACCT_DEFAULT.ID is a manually-assigned NOT NULL key (no sequence/trigger), so a
            // MERGE insert fails with ORA-01400. Update existing rows; insert with MAX(ID)+1 otherwise.
            try
            {
                OracleCommand update = new()
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    BindByName = true,
                    CommandText = "UPDATE ACCT_DEFAULT SET kodeacc = :kode, keterangan = :keterangan " +
                                  "WHERE nama = :id AND iddata = :iddata"
                };
                update.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = edit.Text;
                update.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;
                update.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = nama;
                update.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = CompanyInfo.IDDATA;

                int rowsAffected = update.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    OracleCommand insert = new()
                    {
                        Connection = con,
                        CommandType = CommandType.Text,
                        BindByName = true,
                        CommandText = "INSERT INTO ACCT_DEFAULT (id, nama, kodeacc, iddata, keterangan) " +
                                      "VALUES ((SELECT NVL(MAX(id), 0) + 1 FROM ACCT_DEFAULT), :id, :kode, :iddata, :keterangan)"
                    };
                    insert.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = nama;
                    insert.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = edit.Text;
                    insert.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = CompanyInfo.IDDATA;
                    insert.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;
                    insert.ExecuteNonQuery();
                }

                lbl.Text = keterangan;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_persediaan_EditValueChanged(object sender, EventArgs e)
            => SaveDefaultAccount("PERSEDIAAN", lookUpEdit_persediaan, LBL_PERSEDIAAN);

        private void lookUpEdit_bebanPemakaian_EditValueChanged(object sender, EventArgs e)
            => SaveDefaultAccount("BEBAN_PEMAKAIAN_PERSEDIAAN", lookUpEdit_bebanPemakaian, LBL_BEBAN_PEMAKAIAN_PERSEDIAAN);

        private void lookUpEdit_hutangPembelian_EditValueChanged(object sender, EventArgs e)
            => SaveDefaultAccount("HUTANG_PEMBELIAN_PERSEDIAAN", lookUpEdit_hutangPembelian, LBL_HUTANG_PEMBELIAN_PERSEDIAAN);

        private void lookUpEdit_selisih_EditValueChanged(object sender, EventArgs e)
            => SaveDefaultAccount("SELISIH_PERSEDIAAN", lookUpEdit_selisih, LBL_SELISIH_PERSEDIAAN);

        private void lookUpEdit_barangDalamPerjalanan_EditValueChanged(object sender, EventArgs e)
            => SaveDefaultAccount("BARANG_DALAM_PERJALANAN", lookUpEdit_barangDalamPerjalanan, LBL_BARANG_DALAM_PERJALANAN);

        

        private void FrmSetAkun_FormClosed(object sender, FormClosedEventArgs e)
        {
            con.Close();
        }

        private void lookUpEdit_labatahunberjalan_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                 keterangan = row["PERKIRAAN"].ToString();
            }
                

            // Get the values from the text boxes
            string id = "RL_TAHUN_BERJALAN";
            string kode = lookUpEdit_labatahunberjalan.Text;
           
            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {
                
                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_RL_TAHUN_BERJALAN.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_pph21_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                keterangan = row["PERKIRAAN"].ToString();
            }


            // Get the values from the text boxes
            string id = "HUTANG_PPH21";
            string kode = lookUpEdit_pph21.Text;

            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {

                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_HUTANG_PPH21.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_ymh_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                keterangan = row["PERKIRAAN"].ToString();
            }


            // Get the values from the text boxes
            string id = "GAJI_DAN_UPAH_YMH_DIBAYAR";
            string kode = lookUpEdit_ymh.Text;

            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {
                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_GAJI_DAN_UPAH_YMH_DIBAYAR.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_labaditahan_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                keterangan = row["PERKIRAAN"].ToString();
            }


            // Get the values from the text boxes
            string id = "LABA_DITAHAN";
            string kode = lookUpEdit_labaditahan.Text;

            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {
                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_LABA_DITAHAN.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_alokasiLaba_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                keterangan = row["PERKIRAAN"].ToString();
            }


            // Get the values from the text boxes
            string id = "ALOKASI_LABA_DITAHAN";
            string kode = lookUpEdit_alokasiLaba.Text;

            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {
                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_ALOKASI_LABA_DITAHAN.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void lookUpEdit_ASTEK_EditValueChanged(object sender, EventArgs e)
        {
            string keterangan = string.Empty;
            var lookUpEdit = sender as LookUpEdit;
            DataRowView? row = lookUpEdit.Properties.GetDataSourceRowByKeyValue(lookUpEdit.EditValue) as DataRowView;
            if (row != null)
            {
                keterangan = row["PERKIRAAN"].ToString();
            }


            // Get the values from the text boxes
            string id = "TUNJANGAN_ASTEK";
            string kode = lookUpEdit_ASTEK.Text;

            string iddata =CompanyInfo.IDDATA;



            // Create an OracleCommand object
            OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            // Add parameters to the command
            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = id;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = iddata;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = kode;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = keterangan;

            try
            {

                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();

                LBL_TUNJANGAN_ASTEK.Text = keterangan;
            }
            catch (Exception ex)
            {
                // Display an error message
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private DataTable dtHrisSettings;
        private void InitializeHrisTab()
        {
            // 1. Create the Tab Page
            var tabHris = new DevExpress.XtraTab.XtraTabPage { Text = "HRIS & Payroll", Name = "tabHris" };
            xtraTabControl1.TabPages.Add(tabHris);

            // 2. Create the GridControl and GridView
            var gridControl = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill, Parent = tabHris };
            var gridView = new DevExpress.XtraGrid.Views.Grid.GridView(gridControl);
            gridControl.MainView = gridView;
            gridControl.ViewCollection.Add(gridView);

            // Configure GridView options
            gridView.OptionsView.ShowGroupPanel = false;
            gridView.OptionsView.ShowIndicator = false;
            gridView.OptionsBehavior.AllowAddRows = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsBehavior.AllowDeleteRows = DevExpress.Utils.DefaultBoolean.False;

            // 3. Define the Schema of our DataTable
            dtHrisSettings = new DataTable();
            dtHrisSettings.Columns.Add("Key", typeof(string));
            dtHrisSettings.Columns.Add("Label", typeof(string));
            dtHrisSettings.Columns.Add("AccountCode", typeof(string));
            dtHrisSettings.Columns.Add("AccountName", typeof(string));

            // Populate the DataTable with our default keys and display labels
            var hrisKeys = new Dictionary<string, string>
            {
                { "HRIS_GAJI_POKOK", "Gaji Pokok HRIS" },
                { "HRIS_UANG_MAKAN", "Uang Makan HRIS" },
                { "HRIS_PREMI_LEMBUR", "Premi Lembur & Insentif HRIS" },
                { "HRIS_TUNJ_JABATAN", "Tunjangan Jabatan HRIS" },
                { "HRIS_TUNJ_PERUMAHAN", "Tunjangan Perumahan HRIS" },
                { "HRIS_TUNJ_OPERASIONAL", "Tunjangan Operasional HRIS" },
                { "HRIS_TUNJ_TELPON", "Tunjangan Telepon HRIS" },
                { "HRIS_TUNJ_LUASAN", "Tunjangan Luasan HRIS" },
                { "HRIS_SEWA_KENDARAAN", "Sewa Kendaraan HRIS" },
                { "HRIS_POT_BPJS", "Potongan BPJS Kesehatan" },
                { "HRIS_POT_JHT", "Potongan JHT Karyawan" },
                { "HRIS_POT_PENSIUN", "Potongan Pensiun Karyawan" },
                { "HRIS_POT_TUNJANGAN", "Potongan Tunjangan Karyawan" },
                { "HRIS_POT_BPJS_TK", "General BPJS Ketenagakerjaan (Umum)" },
                { "HRIS_POT_BPJS_KES", "General BPJS Kesehatan (Umum)" },
                { "HRIS_POT_BPJS_PENSIUN", "General BPJS Pensiun (Umum)" }
            };

            // 4. Fetch the existing saved values from ACCT_DEFAULT
            string query = "SELECT nama, kodeacc, keterangan FROM acct_default WHERE iddata = :p_iddata";
            using var cmd = new OracleCommand(query, con);
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            using var dr = cmd.ExecuteReader();
            var existingDefaults = new DataTable();
            existingDefaults.Load(dr);

            foreach (var kvp in hrisKeys)
            {
                var row = dtHrisSettings.NewRow();
                row["Key"] = kvp.Key;
                row["Label"] = kvp.Value;

                var existing = existingDefaults.AsEnumerable()
                    .FirstOrDefault(r => string.Equals(r.Field<string>("NAMA"), kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    row["AccountCode"] = existing["KODEACC"];
                    row["AccountName"] = existing["KETERANGAN"];
                }
                else
                {
                    row["AccountCode"] = DBNull.Value;
                    row["AccountName"] = "";
                }
                dtHrisSettings.Rows.Add(row);
            }

            gridControl.DataSource = dtHrisSettings;

            // 5. Configure Columns in GridView
            gridView.Columns["Key"].Visible = false; // Hide the internal key

            var colLabel = gridView.Columns["Label"];
            colLabel.Caption = "Parameter Gaji / Potongan";
            colLabel.OptionsColumn.AllowEdit = false;
            colLabel.Width = 250;

            var colCode = gridView.Columns["AccountCode"];
            colCode.Caption = "Akun COA";
            colCode.Width = 150;

            var colName = gridView.Columns["AccountName"];
            colName.Caption = "Nama Perkiraan";
            colName.OptionsColumn.AllowEdit = false;
            colName.Width = 350;

            // 6. Bind the LookUpEdit Repository Item to the AccountCode column
            var repositoryLookUp = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            var dtCoa = JurnalServices.KodeUntukJurnal(CompanyInfo.IDDATA, Acct.TahunMax);
            repositoryLookUp.DataSource = dtCoa;
            repositoryLookUp.DisplayMember = "KODE";
            repositoryLookUp.ValueMember = "KODE";
            
            // Add columns to the repository Lookup for better UX
            repositoryLookUp.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("KODE", "Kode Akun", 100));
            repositoryLookUp.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("PERKIRAAN", "Nama Perkiraan", 250));
            repositoryLookUp.NullText = "[Pilih Akun COA]";

            gridControl.RepositoryItems.Add(repositoryLookUp);
            colCode.ColumnEdit = repositoryLookUp;

            // 7. Subscribe to CellValueChanged event to save settings dynamically
            gridView.CellValueChanged += GridView_CellValueChanged;
            
            gridView.BestFitColumns();
        }

        private void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (view == null || e.Column.FieldName != "AccountCode") return;

            string key = view.GetRowCellValue(e.RowHandle, "Key")?.ToString() ?? string.Empty;
            string code = e.Value?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code)) return;

            // Retrieve account description from COA datasource
            string description = string.Empty;
            var dtCoa = JurnalServices.KodeUntukJurnal(CompanyInfo.IDDATA, Acct.TahunMax);
            var coaRow = dtCoa.FirstOrDefault(r => string.Equals(r.KODE, code, StringComparison.OrdinalIgnoreCase));
            if (coaRow != null)
            {
                description = coaRow.PERKIRAAN ?? string.Empty;
            }

            // Update local DataTable Row
            view.SetRowCellValue(e.RowHandle, "AccountName", description);

            // Save to Database using MERGE
            using OracleCommand cmd = new()
            {
                Connection = con,
                CommandType = CommandType.Text,
                CommandText = "MERGE INTO ACCT_DEFAULT t " +
                              "USING (SELECT :id AS id, :iddata AS iddata, :kode AS kode, :keterangan AS keterangan FROM dual) s " +
                              "ON (t.NAMA = s.id and t.iddata = s.iddata  ) " +
                              "WHEN MATCHED THEN UPDATE SET t.kodeacc = s.kode,t.keterangan=s.keterangan " +
                              "WHEN NOT MATCHED THEN INSERT (nama, kodeacc, iddata,keterangan) VALUES (s.id, s.kode, s.iddata,s.keterangan)"
            };

            cmd.Parameters.Add("id", OracleDbType.Varchar2, 50).Value = key;
            cmd.Parameters.Add("iddata", OracleDbType.Varchar2, 50).Value = CompanyInfo.IDDATA;
            cmd.Parameters.Add("kode", OracleDbType.Varchar2, 50).Value = code;
            cmd.Parameters.Add("keterangan", OracleDbType.Varchar2, 100).Value = description;

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Gagal menyimpan data default akun: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}