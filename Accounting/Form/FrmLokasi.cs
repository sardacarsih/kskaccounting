using System;
using System.Data;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System.Media;
using Accounting.BusinessLayer;
using Accounting.Utilities;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmLokasi : DevExpress.XtraEditors.XtraForm
    {
       readonly OracleConnection conn = new(ConnectionManager.GetOracleConnection());
        public FrmLokasi()
        {
            InitializeComponent();
        }
        private SoundPlayer Player = new();
        private void FrmLokasi_Load(object sender, EventArgs e)
        {
            
            try
            {
                this.CenterToScreen();
                conn.Open();
                string SQL = @"SELECT PM.NAMAPT,PM.IDPT,PD.IDDATA,PD.WILAYAH,PD.JENIS_AKUNTANSI
                            FROM MASTER_LOGIN U
                            JOIN MASTER_USER_ROLES_LOC LOC ON LOC.USER_ID=U.USERID
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = LOC.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=LOC.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE AND U.AKTIF='Y'";
                OracleCommand cmd = new OracleCommand(SQL, conn);
                cmd.Parameters.Add(":USERID", OracleDbType.Varchar2, 25).Value = LoginInfo.userID;
                cmd.Parameters.Add(":APPNAME", OracleDbType.Varchar2, 25).Value = LoginInfo.MODULE;
                OracleDataReader dr = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dr);
                gridControl1.DataSource = dt;
                dr.Close();
                conn.Close();
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\iddata.wav";
                this.Player.Play();
            }            
                 catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                       
        }
        

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                INIT_PT();
            }          


        }

        private void INIT_PT()
        {
            try
            {
                
                var rowhandle = gridView1.FocusedRowHandle;
               CompanyInfo.IDDATA = gridView1.GetRowCellValue(rowhandle, "IDDATA").ToString();
                CompanyInfo.JENIS_AKUNTING = gridView1.GetRowCellValue(rowhandle, "JENIS_AKUNTANSI").ToString();
                CompanyInfo.IDPT = gridView1.GetRowCellValue(rowhandle, "IDPT").ToString();
                CompanyInfo.NAMAPT = gridView1.GetRowCellValue(rowhandle, "NAMAPT").ToString();
                CompanyInfo.WILAYAH = gridView1.GetRowCellValue(rowhandle, "WILAYAH").ToString();

                Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
                Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
                Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);               

                this.Hide();
                new MainView().Show();
                //_ = RunAsync();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //create daftar perkiraan jurnal dan laporan
                //LoadDataAsync();
               
            }

        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            INIT_PT();
        }

    }
}