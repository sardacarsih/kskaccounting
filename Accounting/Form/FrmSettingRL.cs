using Accounting.BusinessLayer;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmSettingRL : DevExpress.XtraEditors.XtraForm
    {
        public FrmSettingRL()
        {
            InitializeComponent();
        }
       private readonly OracleConnection conn = new(Acct.OracleConnString);
        DataSet DSSetup;
        OracleDataAdapter sqlAdapter;
        int MAXTAHUN;
        private void FrmSettingRL_Load(object sender, EventArgs e)
        {
            MAXTAHUN = AccountServices.MaxTahunCOA(CompanyInfo.INIT);
            string setlr = ConfigurationManager.AppSettings["setLR"];
            if (setlr == "Ya")
            {
                checkEdit1.EditValue = true;
            }
            else
            {
                checkEdit1.EditValue = false;
            }
            Load_SettingsRL();
            gridControl1.DataSource = DSSetup;
            gridControl1.DataMember = "Setup";
            gridView1.BestFitColumns();
        }

        private DataSet Load_SettingsRL()
        {
            string selectQuery = "select * from acc_setuplabarugi where JENIS=:JENIS order by urut asc"; ;
           
            OracleCommand _command = new OracleCommand(selectQuery, conn)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":JENIS", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();
            sqlAdapter = new OracleDataAdapter();
            sqlcmdbuilder.DataAdapter = sqlAdapter;
            sqlAdapter.SelectCommand = _command;
            DSSetup = new DataSet();
            //DSperiode.Clear();
            //Get the data in disconnected mode
            sqlAdapter.Fill(DSSetup, "Setup");
            // return dataset result
            return DSSetup;

        }

        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            ColumnView view = gridControl1.FocusedView as ColumnView;
            view.CloseEditor();
            if (view.UpdateCurrentRow())
            {

                // DSperiode.Tables("Periode").PrimaryKey = New DataColumn(){ DSperiode.Tables("Periode").Columns(0)};
                sqlAdapter.Update(DSSetup, "Setup");
            }
        }

       

        private void Load_ListAccountPKS(string IDDATA, int MAXTAHUN, string GRP, int LEVEL )
        {
            using (OracleCommand _command = new OracleCommand("select kodeacc,parentacc,namaacc,isheader,lvl from acct_coa where iddata=:IDDATA and  TAHUN=:PTAHUN and GRP=:GRP and lvl=:LVL  and isheader='D' order by kodeacc", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":IDDATA", OracleDbType.Varchar2, 20).Value = IDDATA;
                _command.Parameters.Add(":PTAHUN", OracleDbType.Int16).Value = MAXTAHUN;
                _command.Parameters.Add(":GRP", OracleDbType.Varchar2, 2).Value = GRP;
                _command.Parameters.Add(":LVL", OracleDbType.Int16).Value = LEVEL;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new ();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                gridControl2.DataSource = _dt;
            }
        }

        private void Load_ListAccount(string iddata,int MAXTAHUN, string GRP, int level)
        {
            using (OracleCommand _command = new OracleCommand("select kodeacc,parentacc,namaacc,isheader,lvl from acct_coa where iddata=:iddata and  TAHUN=:PTAHUN and GRP=:GRP and lvl=:lvl order by kodeacc", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
               _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = iddata;
                _command.Parameters.Add(":PTAHUN", OracleDbType.Int16).Value = MAXTAHUN;
                _command.Parameters.Add(":GRP", OracleDbType.Varchar2,2).Value = GRP;
                _command.Parameters.Add(":lvl", OracleDbType.Int16).Value = level;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new ();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                gridControl2.DataSource = _dt;
            }
        }


        private void checkEdit1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("setLR");
                if ((bool)checkEdit1.EditValue == true)
                {
                    config.AppSettings.Settings.Add("setLR", "Ya");
                }
                else
                {
                    config.AppSettings.Settings.Add("setLR", "Tidak");
                }
                
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

            }
            catch (Exception exc)
            {
                MessageBox.Show(@"Saving Error. " + exc.Message);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            using (OracleCommand _command = new OracleCommand("update acc_setuplabarugi set lvl=leveldefault", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.ExecuteReader();
                conn.Close();
            }
            Load_SettingsRL();
            gridControl1.DataSource = DSSetup;
            gridControl1.DataMember = "Setup";
            gridView1.BestFitColumns();
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {
            Load_AkunSetting();
        }

        private void Load_AkunSetting()
        {
           
            var rowhandle = gridView1.FocusedRowHandle;
            string IDDATA = CompanyInfo.INIT;
            var GRP = gridView1.GetRowCellValue(rowhandle, "GRP").ToString();
            var INDUK = gridView1.GetRowCellValue(rowhandle, "INDUK").ToString();
            int LEVEL = Convert.ToInt32(gridView1.GetRowCellValue(rowhandle, "LVL").ToString());

            if (CompanyInfo.JENIS_AKUNTING == "PKS")
            {
                Load_ListAccountPKS(IDDATA, MAXTAHUN, GRP, LEVEL);
            }
            else
            {
                Load_ListAccount(IDDATA, MAXTAHUN, GRP, LEVEL);
            }
            gridView2.BestFitColumns();
        }
    }
}