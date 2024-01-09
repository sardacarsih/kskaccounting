using System;
using System.Data;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.Controllers;
using Accounting.Repositories;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.Form
{
    public partial class FrmSetAkun : DevExpress.XtraEditors.XtraForm
    {
        private readonly JurnalController _jurnalController;

        OracleConnection con = new(Acct.OracleConnString);
        public FrmSetAkun()
        {
            InitializeComponent();
            // Instantiate the controller with the repository implementation
            var jurnalRepository = new JurnalRepository();
            _jurnalController = new JurnalController(jurnalRepository);
        }
        private void SBUpdate_Click(object sender, EventArgs e)
        {
            
            
        }

       
        private void FrmSetAkun_Load(object sender, EventArgs e)
        {
            con.Open();
            NAMAPT.Text = CompanyInfo.NAMAPT;
            IDDATA.Text = CompanyInfo.INIT;
            WILAYAH.Text = CompanyInfo.WILAYAH;

            L0ad_COA();
            Akun_Default();

        }

        private void L0ad_COA()
        {
            
            var dt = _jurnalController.KodeUntukJurnal(CompanyInfo.INIT, Acct.TahunMax);
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
        }

        private void Akun_Default()
        {
            
            string query = "select nama,kodeacc from acct_default WHERE iddata=:p_iddata";

            OracleCommand _command = new (query, con)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.INIT;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();

            if (_dt.Rows.Count == 0)
            {
                //create default perkiraan closing
                AccountServices.CreateClosingAcct(CompanyInfo.INIT);
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


        }

        

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
           
            string iddata = CompanyInfo.INIT;



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

            string iddata = CompanyInfo.INIT;



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

            string iddata = CompanyInfo.INIT;



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

            string iddata = CompanyInfo.INIT;



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

            string iddata = CompanyInfo.INIT;



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

            string iddata = CompanyInfo.INIT;



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
    }
}