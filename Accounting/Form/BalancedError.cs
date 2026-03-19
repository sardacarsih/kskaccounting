using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraSplashScreen;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;

namespace Accounting.Form
{
    public partial class BalancedError : SplashScreen
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);
        public string Myperiode { get; set; }
        public int ibulan { get; set; }
        public int itahun { get; set; }
        public BalancedError()
        {
            InitializeComponent();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion

        public enum SplashScreenCommand
        {
        }

        private void BalancedError_Load(object sender, EventArgs e)
        {
            var cekj= Cekjurnal();
            if(cekj.Rows.Count >0)
            {
                gridControl1.DataSource = cekj;
                return;
            }
            var jumlah = CEKBiayaProduksi();
            if (jumlah != 0)
            {
                var bproduksi = BiayaProduksi();

                gridControl1.DataSource = bproduksi;
                gridView1.Columns[2].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridView1.Columns[2].DisplayFormat.FormatString = "n2";
                gridView1.BestFitColumns();
                XtraMessageBox.Show("Biaya Produksi Belum selesai direclass", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            else
            {
                using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.CekDoubleClosing", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                    _command.Parameters.Add("piddata", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
                    _command.Parameters.Add("pperiode", OracleDbType.Varchar2, 7).Value = this.Myperiode;
                    OracleDataReader dr;
                    dr = _command.ExecuteReader();
                    DataTable _dt = new DataTable();
                    _dt.Load(dr);
                    dr.Close();
                    conn.Close();
                    if (_dt.Rows.Count > 1)
                    {
                        gridControl1.DataSource = _dt;
                        gridView1.Columns[1].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        gridView1.Columns[1].DisplayFormat.FormatString = "dd-MMM-yyyy";

                        gridView1.Columns[2].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[2].DisplayFormat.FormatString = "n2";

                        gridView1.Columns[3].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[3].DisplayFormat.FormatString = "n2";

                        gridView1.Columns[4].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns[4].DisplayFormat.FormatString = "n2";
                        gridView1.Columns[4].Summary.Clear();
                        gridView1.Columns[4].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "selisih", "{0:N2}");
                        gridView1.BestFitColumns();
                        XtraMessageBox.Show("Jurnal Closing Double ?", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        XtraMessageBox.Show("Silahkan Cek Neraca Bulan sebelumnya, apakah sudah seimbang ?", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
         }
        private DataTable Cekjurnal()
        {
            using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.CekGlobalJurnalNotBalanced", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("piddata", OracleDbType.Varchar2,30).Value =CompanyInfo.IDDATA;
                _command.Parameters.Add("pperiode", OracleDbType.Varchar2,7).Value = this.Myperiode;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        private DataTable BiayaProduksi()
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.ViewBiayaProduksi", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("record", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
                _command.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = this.ibulan;
                _command.Parameters.Add("p_tahun", OracleDbType.Int16).Value = this.itahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        private Decimal CEKBiayaProduksi()
        {
            using (OracleCommand _command = new OracleCommand("ACCOUNTING.CekBiayaProduksi", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("JUMLAH", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 30).Value =CompanyInfo.IDDATA;
                _command.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = this.ibulan;
                _command.Parameters.Add("p_tahun", OracleDbType.Int16).Value = this.itahun;
                _command.ExecuteScalar();
                var result = Convert.ToDecimal(_command.Parameters["JUMLAH"].Value.ToString());              
                conn.Close();
                return result;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolTipController1_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.Info == null && e.SelectedControl == gridControl1)
            {
                GridView view = gridControl1.FocusedView as GridView;
                GridHitInfo info = view.CalcHitInfo(e.ControlMousePosition);
                if (info.InRowCell)
                {
                    string text = view.GetRowCellDisplayText(info.RowHandle, info.Column);
                    string cellKey = info.RowHandle.ToString() + " - " + info.Column.ToString();
                    e.Info = new ToolTipControlInfo(cellKey, text);
                }
            }
        }
    }
}