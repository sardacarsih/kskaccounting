using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace Accounting.DataLayer
{
    public class LaporanEstate : ILaporanEstate
    {

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
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            using OracleCommand _command = new("ACCT_LAP_ESTATE.TBM_BYDIVISI_Quarter", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Quarter", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_quarter", OracleDbType.Varchar2, 20).Value = p_quarter;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Quarter");
            return _ds;
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
            using OracleConnection conn = new(LoginInfo.OracleConnString);
            using OracleCommand _command = new("ACCT_LAP_ESTATE.TM_BYDIVISI_Quarter", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Quarter", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_quarter", OracleDbType.Varchar2, 20).Value = p_quarter;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Quarter");
            return _ds;
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
