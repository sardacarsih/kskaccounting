using Accounting.BusinessLayer;
using Accounting.Utilities;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmPeriode : DevExpress.XtraEditors.XtraForm
    {
        private const int BaseDpi = 96;
        private const int LayoutPadding = 12;
        private const int TopRowHeight = 32;
        private const int TopSpacing = 10;
        readonly OracleConnection conn = new(ConnectionManager.GetOracleConnection());
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
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageAccountingPeriods))
            {
                Close();
                return;
            }
            ApplyInitialSizeByResolution();
            ApplyResponsiveLayout();
            if (Acct.PeriodeMax != 0)
            {
                setahun.Properties.MinValue = Acct.TahunMin;
                setahun.Properties.MaxValue = Acct.TahunMax;
                setahun.Value = Acct.TahunMax;
                LoadList_Periode();
            }
            
        }

        private void FrmPeriode_Resize(object sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void ApplyResponsiveLayout()
        {
            if (setahun == null || labelControl1 == null || simpleButton2 == null || gridControl1 == null)
            {
                return;
            }

            int padding = ScaleForDpi(LayoutPadding);
            int topRowHeight = ScaleForDpi(TopRowHeight);
            int topSpacing = ScaleForDpi(TopSpacing);
            int top = padding;
            labelControl1.Location = new Point(padding, top + ScaleForDpi(7));
            setahun.Location = new Point(labelControl1.Right + 8, top + 3);
            setahun.Size = new Size(Math.Max(setahun.Width, ScaleForDpi(92)), topRowHeight - ScaleForDpi(2));

            int buttonWidth = Math.Max(ScaleForDpi(88), simpleButton2.Width);
            simpleButton2.Size = new Size(buttonWidth, topRowHeight);
            simpleButton2.Location = new Point(ClientSize.Width - padding - buttonWidth, top);

            int gridTop = top + topRowHeight + topSpacing;
            int gridWidth = Math.Max(ScaleForDpi(280), ClientSize.Width - (padding * 2));
            int gridHeight = Math.Max(ScaleForDpi(220), ClientSize.Height - gridTop - padding);
            gridControl1.Location = new Point(padding, gridTop);
            gridControl1.Size = new Size(gridWidth, gridHeight);
        }

        private void ApplyInitialSizeByResolution()
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }

            Rectangle area = Screen.FromControl(this).WorkingArea;
            int targetWidth;
            int targetHeight;

            if (area.Width <= ScaleForDpi(1366))
            {
                targetWidth = ScaleForDpi(620);
                targetHeight = ScaleForDpi(560);
            }
            else if (area.Width <= ScaleForDpi(1920))
            {
                targetWidth = ScaleForDpi(720);
                targetHeight = ScaleForDpi(640);
            }
            else
            {
                targetWidth = ScaleForDpi(820);
                targetHeight = ScaleForDpi(700);
            }

            int finalWidth = Math.Min(targetWidth, (int)(area.Width * 0.82));
            int finalHeight = Math.Min(targetHeight, (int)(area.Height * 0.86));
            Size = new Size(finalWidth, finalHeight);
            StartPosition = FormStartPosition.CenterScreen;
        }

        private int ScaleForDpi(int value)
        {
            int dpi = DeviceDpi > 0 ? DeviceDpi : BaseDpi;
            return Math.Max(1, (value * dpi) / BaseDpi);
        }
        private void LoadList_Periode()
        {
            PeriodeAkuntansi(CompanyInfo.IDDATA, (int)setahun.Value);
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
            _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = piddata;
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
                if (!AuthorizationService.CanManageAccountingPeriods())
                {
                    return;
                }

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
            DXMenuItem checkItem = new("Hapus", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnHapusClick(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageAccountingPeriods))
            {
                return;
            }
           
            var rowhandle = gridView1.FocusedRowHandle;
            var PERIODE = gridView1.GetRowCellValue(rowhandle, "PERIODE").ToString();
            var LblPeriode = gridView1.GetRowCellValue(rowhandle, "BULAN").ToString()+'-'+setahun.Value.ToString();
            var adarecordjurnal = JurnalServices.CekRecordJurnalExist(CompanyInfo.IDDATA, PERIODE);
            if (adarecordjurnal >0)
            {
                XtraMessageBox.Show("Periode Akuntansi : " + LblPeriode + 
                    "\nPeriode ini tidak dapat diHapus...!!!" +
                    "\nKarena Telah Memiliki Transaksi Jurnal.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Hapus_Periode(CompanyInfo.IDDATA, PERIODE);
            
            string SetValueForPeriode = "Periode Akuntansi : " + AccountServices.GetNamaPeriode(CompanyInfo.IDDATA).ToString();
            WriteToParent(SetValueForPeriode);
            LoadList_Periode();
        }

        private void Hapus_Periode(string iNIT, string pERIODE)
        {
           
            string deletecmd = "delete from ACCT_PERIODE WHERE IDDATA=:p_IDDATA AND PERIODE=:p_periode";
            using (OracleCommand cmd = new(deletecmd, conn)
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
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageAccountingPeriods))
            {
                return;
            }
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
