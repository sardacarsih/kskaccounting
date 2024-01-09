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
    public class LaporanEstate : ILaporanEstate
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);

        public DataTable Divisi(string p_iddata, int p_tahun)
        {
            throw new NotImplementedException();
        }

        public DataSet TBM_BYDIVISI(string p_iddata, int p_tahun, int p_bulan)
        {
            throw new NotImplementedException();
        }

        public DataSet TBM_BYDIVISI_Quarter(string p_iddata, int p_tahun, string p_quarter)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.TBM_BYDIVISI_Quarter", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("Quarter", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_quarter", OracleDbType.Varchar2, 20).Value = p_quarter;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Quarter");
                // return dataset result
                return _ds;
            }
        
        }

        public DataSet TBM_BYDIVISI_Semester(string p_iddata, int p_tahun, string p_semester)
        {
            throw new NotImplementedException();
        }

        public DataSet TBM_BYDIVISI_Tahun(string p_iddata, int p_tahun)
        {
            throw new NotImplementedException();
        }

        public DataSet TM_BYDIVISI(string p_iddata, int p_tahun, int p_bulan)
        {
            throw new NotImplementedException();
        }

        public DataSet TM_BYDIVISI_Quarter(string p_iddata, int p_tahun, string p_quarter)
        {
            using (OracleCommand _command = new OracleCommand("ACCT_LAP_ESTATE.TM_BYDIVISI_Quarter", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                //conn.Open();                
                _command.Parameters.Add("Quarter", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
                _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
                _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
                _command.Parameters.Add(":p_quarter", OracleDbType.Varchar2, 20).Value = p_quarter;
                OracleDataAdapter sqlAdapter = new OracleDataAdapter(_command);
                DataSet _ds = new DataSet();
                //Get the data in disconnected mode
                sqlAdapter.Fill(_ds, "Quarter");
                // return dataset result
                return _ds;
            }
        }

        public DataSet TM_BYDIVISI_Semester(string p_iddata, int p_tahun, string p_semester)
        {
            throw new NotImplementedException();
        }

        public DataSet TM_BYDIVISI_Tahun(string p_iddata, int p_tahun)
        {
            throw new NotImplementedException();
        }
    }
}
