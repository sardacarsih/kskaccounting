using Accounting.Utilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Diagnostics;

namespace Accounting
{
    public partial class Partition : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new(ConnectionManager.GetOracleConnection());

        public Partition()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var data1 = Load_withPartition("KSKINTI", 2021);
            gridControl1.DataSource = data1;
            watch.Stop();
            memoEdit1.AppendText(watch.ElapsedMilliseconds.ToString());
            memoEdit1.AppendText(Environment.NewLine);
            //XtraMessageBox.Show(watch.ElapsedMilliseconds.ToString());
        }

        private DataTable Load_withPartition(string p_iddata, int p_tahun)
        {

            //string sql1 = "select kodeacc ,namaacc  from acct_coa_TAHUN where iddata=:piddata and tahun=:ptahun AND ISHEADER = 'D' AND isAKTIF <>'T'";
            var sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                       ",\"11S\"  SALDOAWAL ,\"12D\" DEBET ,\"12K\" KREDIT ,\"12S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                       "from acct_coa_tahun where iddata=:p_iddata and tahun=:p_tahun";
            using (OracleCommand _command = new OracleCommand(sql1, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":ptahun", OracleDbType.Int16).Value = p_tahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var data = Load_withoutPartition("KSKINTI", 2021);
            gridControl2.DataSource = data;
            watch.Stop();
            memoEdit2.AppendText(watch.ElapsedMilliseconds.ToString());
            memoEdit2.AppendText(Environment.NewLine);
            //XtraMessageBox.Show(watch.ElapsedMilliseconds.ToString());
        }

        private DataTable Load_withoutPartition(string p_iddata, int p_tahun)
        {
            //string sql1 = "select kodeacc KODE,namaacc PERKIRAAN from acct_coa where iddata=:p_iddata and tahun=:p_tahun AND ISHEADER = 'D' AND isAKTIF <>'T'";
            var sql1 = "SELECT ACCTCOAID ID,KODEACC KODEACC,PARENTACC INDUK,NAMAACC NAMAACC,POSISI ,LVL ,ISHEADER GD  ,GRP ,ISAKTIF ,SALDOAWAL AWALTAHUN  " +
                       ",\"11S\"  SALDOAWAL ,\"12D\" DEBET ,\"12K\" KREDIT ,\"12S\" SALDOAKHIR ,DIVISI ,BLOK ,TAHUNTANAM " +
                       "from acct_coa where iddata=:p_iddata and tahun=:p_tahun ";
            using (OracleCommand _command = new OracleCommand(sql1, conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":ptahun", OracleDbType.Int16).Value = p_tahun;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new DataTable();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }
    }
}
