using Accounting.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting.DataLayer
{
    public class LaporanRepository : ILaporanRepository
    {

        // Each method opens and disposes its own connection (per-call), so the
        // repository is stateless and safe to reuse. Avoid a shared connection
        // field: it is not thread-safe and leaks/leaves connections in an
        // inconsistent open/closed state.
        // Accounting report V1 reads report metadata and ACCT_COA hierarchy in one
        // round-trip. Laba Rugi V2 remains a compatibility wrapper over this path.
        public DataSet ViewAccountingReport(string piddata, int pbulan, int ptahun, string userid, string reportCode, string jenisakunting)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_REPORT_ENGINE_V1.GET_REPORT", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            cmd.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            cmd.Parameters.Add("p_REPORT_CODE", OracleDbType.Varchar2, 30).Value = reportCode;
            cmd.Parameters.Add("p_JENISAKUNTING", OracleDbType.Varchar2, 20).Value = jenisakunting;
            cmd.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(cmd);
            DataSet ds = new();
            sqlAdapter.Fill(ds, reportCode);
            return ds;
        }

        public DataSet ViewAccountingReportDrillDown(string piddata, int pbulan, int ptahun, string reportCode, int sectionId, string kodeacc)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_REPORT_ENGINE_V1.GET_DRILLDOWN", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            cmd.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add("p_REPORT_CODE", OracleDbType.Varchar2, 30).Value = reportCode;
            cmd.Parameters.Add("p_SECTION_ID", OracleDbType.Int32).Value = sectionId;
            cmd.Parameters.Add("p_KODEACC", OracleDbType.Varchar2, 30).Value = kodeacc;
            cmd.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(cmd);
            DataSet ds = new();
            sqlAdapter.Fill(ds, "ReportDrillDown");
            return ds;
        }

        public DataSet ViewLap_LabaRugi_V2(string piddata, int pbulan, int ptahun, string userid, string jenisakunting)
        {
            DataSet ds = ViewAccountingReport(piddata, pbulan, ptahun, userid, "LABARUGI", jenisakunting);
            ds.Tables[0].TableName = "LabaRugi";
            return ds;
        }

        public List<LabaRugiRow> ViewLap_LabaRugiRows_V2(string piddata, int pbulan, int ptahun, string userid, string jenisakunting)
        {
            DataSet ds = ViewLap_LabaRugi_V2(piddata, pbulan, ptahun, userid, jenisakunting);
            DataTable table = GetRequiredTable(ds, "LabaRugi", LabaRugiRow.RequiredColumns);
            List<LabaRugiRow> rows = new();

            foreach (DataRow row in table.Rows)
            {
                rows.Add(LabaRugiRow.FromDataRow(row));
            }

            return rows;
        }

        private static DataTable GetRequiredTable(DataSet ds, string tableName, IReadOnlyCollection<string> requiredColumns)
        {
            if (ds == null)
            {
                throw new InvalidOperationException($"Data laporan tidak memiliki tabel {tableName}.");
            }

            DataTable table = ds.Tables[tableName];
            if (table == null)
            {
                throw new InvalidOperationException($"Data laporan tidak memiliki tabel {tableName}.");
            }

            List<string> missingColumns = new();
            foreach (string column in requiredColumns)
            {
                if (!table.Columns.Contains(column))
                {
                    missingColumns.Add(column);
                }
            }

            if (missingColumns.Count > 0)
            {
                throw new InvalidOperationException($"Data laporan {tableName} tidak lengkap. Kolom hilang: {string.Join(", ", missingColumns)}.");
            }

            return table;
        }
        public decimal Generate_Jurnal_Closing(string piddata, int pbulan, int ptahun, string userid, string jenisakunting)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_JURNAL_CLOSING_V2.JURNAL_CLOSING", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("LabaRugi", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            cmd.Parameters.Add(":jenisakunting", OracleDbType.Varchar2, 20).Value = jenisakunting;
            cmd.ExecuteNonQuery();
            return Convert.ToDecimal(cmd.Parameters["LabaRugi"].Value.ToString());
        }

        public int GenerateSub_LabaRugi(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string lap, string posisi)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_LAPORAN.ACC_GENREP_LRNR_SUB", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("SubLabaRugi", OracleDbType.Int32).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = p_IDDATA;
            cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            cmd.Parameters.Add(":p_kodeacc", OracleDbType.Varchar2, 30).Value = p_kodeacc;
            cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            cmd.Parameters.Add(":lap", OracleDbType.Varchar2, 20).Value = lap;
            cmd.Parameters.Add(":posisi", OracleDbType.Varchar2, 20).Value = posisi;
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(cmd.Parameters["SubLabaRugi"].Value.ToString());
        }

        public DataSet ViewLap_Neraca(string piddata, int p_bulan, int p_tahun, string userid)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_NERACA", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Neraca");
            return _ds;
        }

        public List<NeracaRow> ViewLap_NeracaRows_V2(string piddata, int p_bulan, int p_tahun, string userid)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN_V2.LAP_NERACA_V2", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            _command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            _command.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet ds = new();
            sqlAdapter.Fill(ds, "Neraca");

            DataTable table = GetRequiredTable(ds, "Neraca", NeracaRow.RequiredColumns);
            List<NeracaRow> rows = new();

            foreach (DataRow row in table.Rows)
            {
                rows.Add(NeracaRow.FromDataRow(row));
            }

            return rows;
        }

        public DataSet ViewSub_Neraca(string piddata, int p_bulan, int p_tahun, string p_kodeacc, string userid, string posisi)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN_V2.LAP_NERACA_SUB_V2", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            _command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add("p_KODEACC", OracleDbType.Varchar2, 30).Value = p_kodeacc;
            _command.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            _command.Parameters.Add("p_POSISI", OracleDbType.Varchar2, 20).Value = posisi;
            _command.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet ds = new();
            sqlAdapter.Fill(ds, "Neraca");
            return ds;
        }

        public DataSet ViewSub_LabaRugi(string piddata, string userid)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("select * from ACC_SUB_REPORT where iddata=:p_IDDATA and genuser=:p_userid ", connection)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "SubLabaRugi");
            return _ds;
        }

        public DataSet View_Jurnal(string piddata, string periode, string kode)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("select * from acct_jurnal_dtl where iddata=:iddata and periode=:periode and kode=:kode", connection)
            {
                CommandType = CommandType.Text
            };
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":periode", OracleDbType.Varchar2, 20).Value = periode;
            _command.Parameters.Add(":kode", OracleDbType.Varchar2, 20).Value = kode;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Jurnal");
            return _ds;
        }

        public DataSet ViewLap_NeracaHalfYear(string piddata, int p_tahun, string userid, int ishalf)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_NERACA_HALFYEAR", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            _command.Parameters.Add(":ishalf", OracleDbType.Int16).Value = ishalf;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Neraca");
            return _ds;
        }

        public DataTable ViewLap_NeracaKonsolidasi(int p_tahun, string p_pt, int p_bulan, string userid)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_NERACA_KONSOLIDASI", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_pt", OracleDbType.Varchar2, 20).Value = p_pt;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userid;
            using OracleDataReader dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            return _dt;
        }

        public DataSet ViewLap_BukuBesar(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_DYNAMIC_GL", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("BukuBesar", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":P_IDDATA", OracleDbType.Varchar2, 20).Value = P_IDDATA;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_sampaibulan", OracleDbType.Int16).Value = p_sampaibulan;
            _command.Parameters.Add(":DARIKODE", OracleDbType.Varchar2, 20).Value = DARIKODE;
            _command.Parameters.Add(":SAMPAIKODE", OracleDbType.Varchar2, 20).Value = SAMPAIKODE;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_Userid;
            _command.Parameters.Add(":DARILAPORAN", OracleDbType.Varchar2, 20).Value = DARILAPORAN;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "BukuBesar");
            return _ds;
        }

        // Hierarchy-aware general ledger for Laba Rugi drill-down: returns acct_jurnal_dtl
        // transactions for the clicked account AND all its descendant leaf accounts (the COA
        // tree is linked by PARENTACC, not code prefix, so a code range cannot capture children).
        // Same column shape as the LABARUGI branch of ACCT_LAPORAN.LAP_DYNAMIC_GL, so the
        // GeneralLedgerD2/K2 reports (DataMember "BukuBesar") render it unchanged.
        public DataSet ViewLap_BukuBesar_Tree(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string p_kode)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new(
                @"SELECT periode, kode, rekening, nojurnal, tanggal, keterangan, debet, kredit
                    FROM acct_jurnal_dtl
                   WHERE IDDATA = :p_iddata AND glyear = :p_tahun
                     AND glmonth BETWEEN :p_bulan AND :p_sampaibulan
                     AND KODE IN (
                          SELECT KODEACC FROM ACCT_COA
                           WHERE IDDATA = :p_iddata AND TAHUN = :p_tahun
                           START WITH KODEACC = :p_kode
                           CONNECT BY NOCYCLE PRIOR KODEACC = PARENTACC)
                   ORDER BY kode, tanggal, nojurnal ASC", connection)
            {
                CommandType = CommandType.Text,
                BindByName = true,
                CommandTimeout = 180
            };
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = P_IDDATA;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_sampaibulan", OracleDbType.Int16).Value = p_sampaibulan;
            _command.Parameters.Add(":p_kode", OracleDbType.Varchar2, 30).Value = p_kode;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "BukuBesar");
            return _ds;
        }

        public DataSet ViewLap_BukuBesarMultiTahun(string P_IDDATA, int p_tahundari, int p_tahunsampai, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_DYNAMIC_GL_Multi_YEAR", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("BukuBesar", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":P_IDDATA", OracleDbType.Varchar2, 20).Value = P_IDDATA;
            _command.Parameters.Add(":p_tahundari", OracleDbType.Int16).Value = p_tahundari;
            _command.Parameters.Add(":p_tahunsampai", OracleDbType.Int16).Value = p_tahunsampai;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_sampaibulan", OracleDbType.Int16).Value = p_sampaibulan;
            _command.Parameters.Add(":DARIKODE", OracleDbType.Varchar2, 20).Value = DARIKODE;
            _command.Parameters.Add(":SAMPAIKODE", OracleDbType.Varchar2, 20).Value = SAMPAIKODE;
            _command.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = p_Userid;
            _command.Parameters.Add(":DARILAPORAN", OracleDbType.Varchar2, 20).Value = DARILAPORAN;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "BukuBesar");
            return _ds;
        }

        public DataSet ViewLap_NeracaLajur(string piddata, int p_bulan, int p_tahun)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand _command = new("ACCT_LAPORAN.LAP_NERACA_LAJUR", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            _command.Parameters.Add("Neraca", OracleDbType.RefCursor).Direction = ParameterDirection.ReturnValue;
            _command.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            _command.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = p_bulan;
            _command.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = p_tahun;
            using OracleDataAdapter sqlAdapter = new(_command);
            DataSet _ds = new();
            sqlAdapter.Fill(_ds, "Neraca");
            return _ds;
        }

        public decimal Balanced_Check(string piddata, int pbulan, int ptahun)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_LAPORAN.BALANCED_CHECK", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("Selisih", OracleDbType.Decimal).Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = ptahun;
            cmd.ExecuteNonQuery();
            return Convert.ToDecimal(cmd.Parameters["Selisih"].Value.ToString());
        }

        public List<AccountSummary> NeracaSaldoTahun(string piddata, int p_tahun)
        {
            List<AccountSummary> accountDataList = new();

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            string sqlQuery = @"
                SELECT KODEACC, NAMAACC, LVL, SALDOAWAL,
                ""1S"" AS JAN, ""2S"" AS FEB, ""3S"" AS MAR, ""4S"" AS APR,
                ""5S"" AS MAY, ""6S"" AS JUN, ""7S"" AS JUL, ""8S"" AS AUG,
                ""9S"" AS SEP, ""10S"" AS OCT, ""11S"" AS NOV, ""12S"" AS DEC
                FROM ACCT_COA
                WHERE IDDATA = :piddata AND TAHUN = :p_tahun
                ORDER BY KODEACC";

            using OracleCommand command = new(sqlQuery, connection);
            command.Parameters.Add(new OracleParameter(":piddata", OracleDbType.Varchar2)).Value = piddata;
            command.Parameters.Add(new OracleParameter(":p_tahun", OracleDbType.Int32)).Value = p_tahun;

            using OracleDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                AccountSummary accountData = new()
                {
                    KODEACC = reader["KODEACC"].ToString(),
                    NAMAACC = reader["NAMAACC"].ToString(),
                    LVL = Convert.ToInt32(reader["LVL"]),
                    SALDOAWAL = Convert.ToDecimal(reader["SALDOAWAL"]),
                    JAN = Convert.ToDecimal(reader["JAN"]),
                    FEB = Convert.ToDecimal(reader["FEB"]),
                    MAR = Convert.ToDecimal(reader["MAR"]),
                    APR = Convert.ToDecimal(reader["APR"]),
                    MAY = Convert.ToDecimal(reader["MAY"]),
                    JUN = Convert.ToDecimal(reader["JUN"]),
                    JUL = Convert.ToDecimal(reader["JUL"]),
                    AUG = Convert.ToDecimal(reader["AUG"]),
                    SEP = Convert.ToDecimal(reader["SEP"]),
                    OCT = Convert.ToDecimal(reader["OCT"]),
                    NOV = Convert.ToDecimal(reader["NOV"]),
                    DEC = Convert.ToDecimal(reader["DEC"])
                };
                accountDataList.Add(accountData);
            }
            return accountDataList;
        }
    }

}
