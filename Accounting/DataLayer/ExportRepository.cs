using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public  class ExportRepository:IExportRepository
    {
    private readonly OracleConnection conn = new( LoginInfo.OracleConnString);

        public DataTable ExportJurnalMonthly(string piddata, string periode)
        {
            string query = "select nojurnal,tanggal,baris,kode,rekening,debet,kredit,keterangan,Posted,periode from accT_jurnal_dtl "+
                            "where iddata =:iddata and periode =:periode order by nojurnal, TANGGAL, baris asc ";
                using (OracleCommand _command = new OracleCommand(query, conn)
                {
                    CommandType = CommandType.Text
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = piddata;
                     _command.Parameters.Add(":periode", OracleDbType.Varchar2, 10).Value = periode;
                    OracleDataReader dr;
                    dr = _command.ExecuteReader();
                    DataTable _dt = new DataTable();
                    _dt.Load(dr);
                    dr.Close();
                    conn.Close();
                    return _dt;
                }
         }

        public DataTable ExportJurnalRange(string piddata, DateTime fromDate, DateTime toDate)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_JURNAL.ExportJurnal", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add("CUR", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":iddata", OracleDbType.Varchar2, 20).Value = piddata;
                _command.Parameters.Add(":fromDate", OracleDbType.Date).Value = fromDate;
                _command.Parameters.Add(":toDate", OracleDbType.Date).Value = toDate;
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

