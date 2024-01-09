using Accounting.BusinessLayer;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmPeriode : DevExpress.XtraEditors.XtraForm
    {
        readonly OracleConnection conn = new(Acct.OracleConnString);
        DataSet DSperiode;
        OracleDataAdapter sqlAdapter;
        public FrmPeriode()
        {
            InitializeComponent();
        }

        private void WriteToParent(string SetValueForPeriode)
        {
            //retrieve parent and cast to parent type then send text to property
            ((MainView)this.MdiParent).SetValueForPeriode = SetValueForPeriode; 
          
        }
        

        private void FrmPeriode_Load(object sender, EventArgs e)
        {
            if (Acct.PeriodeMax != 0)
            {
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
                LoadList_Periode();
            }
            
        }
        private void LoadList_Periode()
        {
            PeriodeAkuntansi(CompanyInfo.INIT, (int)setahun.Value);
            gridControl1.DataSource = DSperiode;
            gridControl1.DataMember = "Periode";
            gridView1.Focus();
        }

        private DataSet PeriodeAkuntansi(string piddata, int ptahun)
        {

            String selectQuery = "select idperiode,periode,namabulan bulan,islocked kunci from acct_periode where iddata=:iddata and tahun=:tahun order by periode asc";
            OracleCommand _command = new OracleCommand(selectQuery, conn)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 10).Value = piddata;
                _command.Parameters.Add(":tahun", OracleDbType.Int16).Value = ptahun;
                OracleCommandBuilder sqlcmdbuilder = new OracleCommandBuilder();
                sqlAdapter = new OracleDataAdapter();
                sqlcmdbuilder.DataAdapter = sqlAdapter;
                sqlAdapter.SelectCommand = _command;
                DSperiode = new DataSet();
                //DSperiode.Clear();
                //Get the data in disconnected mode
                sqlAdapter.Fill(DSperiode, "Periode");
                // return dataset result
                return DSperiode;            
        }
       
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            LoadList_Periode();
        }

        private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                //hapus menu jika ada
                e.Menu.Items.Clear();

                DXMenuItem hapus = CreateMenuItemHapus(view, rowHandle);
              
                hapus.BeginGroup = true;

                e.Menu.Items.Add(hapus);

            }
        }

        private DXMenuItem CreateMenuItemHapus(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Hapus", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnHapusClick(object sender, EventArgs e)
        {
           
            var rowhandle = gridView1.FocusedRowHandle;
            var PERIODE = gridView1.GetRowCellValue(rowhandle, "PERIODE").ToString();
            var LblPeriode = gridView1.GetRowCellValue(rowhandle, "BULAN").ToString()+'-'+setahun.Value.ToString();
            var adarecordjurnal = JurnalServices.CekRecordJurnalExist(CompanyInfo.INIT, PERIODE);
            if (adarecordjurnal >0)
            {
                XtraMessageBox.Show("Periode Akuntansi : " + LblPeriode + 
                    "\nPeriode ini tidak dapat diHapus...!!!" +
                    "\nKarena Telah Memiliki Transaksi Jurnal.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Hapus_Periode(CompanyInfo.INIT, PERIODE);
            
            string SetValueForPeriode = "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.INIT).ToString();
            WriteToParent(SetValueForPeriode);
            LoadList_Periode();
        }

        private void Hapus_Periode(string iNIT, string pERIODE)
        {
           
            string deletecmd = "delete from ACCT_PERIODE WHERE IDDATA=:p_IDDATA AND PERIODE=:p_periode";
            using (OracleCommand cmd = new OracleCommand(deletecmd, conn)
            {
                CommandType = CommandType.Text
            })
            {
                conn.Open();
                cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = iNIT;
                cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = pERIODE;
                cmd.ExecuteReader();
                conn.Close();
            }
        }

        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            ColumnView view = gridControl1.FocusedView as ColumnView;
            view.CloseEditor();
            if (view.UpdateCurrentRow())
            {
               
               // DSperiode.Tables("Periode").PrimaryKey = New DataColumn(){ DSperiode.Tables("Periode").Columns(0)};
                sqlAdapter.Update(DSperiode, "Periode");
            }
        }
    }
}