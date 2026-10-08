using Accounting.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

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
            string procedureName = reportCode == "NERACA"
                ? "ACCT_LAPORAN_V2.LAP_NERACA_V2"
                : "ACCT_LAPORAN_V2.LAP_LABARUGI_V2";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            cmd.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            if (reportCode != "NERACA")
            {
                cmd.Parameters.Add("p_JENISAKUNTING", OracleDbType.Varchar2, 20).Value = jenisakunting;
            }
            cmd.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(cmd);
            DataSet ds = new();
            sqlAdapter.Fill(ds, reportCode);
            return ds;
        }

        public DataSet ViewAccountingReportDrillDown(string piddata, int pbulan, int ptahun, string userid, string reportCode, string kodeacc)
        {
            const string procedureName = "ACCT_REPORT_DRILLDOWN_V1.GET_DRILLDOWN";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            cmd.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add("p_KODEACC", OracleDbType.Varchar2, 30).Value = kodeacc;
            cmd.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            cmd.Parameters.Add("p_REPORT_CODE", OracleDbType.Varchar2, 20).Value = reportCode;
            cmd.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(cmd);
            DataSet ds = new();
            sqlAdapter.Fill(ds, "ReportDrillDown");
            ReportDrillDownRow.EnsureRequiredColumns(ds.Tables["ReportDrillDown"]!);
            return ds;
        }

        public DataSet ViewCoaDrillDown(string piddata, int pbulan, int ptahun, string userid, string kodeacc)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand cmd = new("ACCT_COA_DRILLDOWN_V1.GET_CHILDREN", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            cmd.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = piddata;
            cmd.Parameters.Add("p_BULAN", OracleDbType.Int16).Value = pbulan;
            cmd.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = ptahun;
            cmd.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = userid;
            cmd.Parameters.Add("p_KODEACC", OracleDbType.Varchar2, 30).Value = kodeacc;
            cmd.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(cmd);
            DataSet dataSet = new();
            sqlAdapter.Fill(dataSet, "CoaDrillDown");
            CoaDrillDownRow.GetRequiredTable(dataSet);
            return dataSet;
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

        [Obsolete("Use ViewAccountingReportDrillDown or ViewSub_Neraca so report drill-downs do not depend on legacy temp generators.")]
        public int GenerateSub_LabaRugi(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string lap, string posisi)
        {
            return 0;
        }

        public DataSet ViewLap_Neraca(string piddata, int p_bulan, int p_tahun, string userid)
        {
            List<NeracaRow> rows = ViewLap_NeracaRows_V2(piddata, p_bulan, p_tahun, userid);
            return Accounting.Services.NeracaReportDataAdapter.CreateReportDataSet(rows);
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
            return ViewAccountingReportDrillDown(piddata, p_bulan, p_tahun, userid, "NERACA", p_kodeacc);
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
            int bulan = ishalf == 1 ? 6 : 12;
            return ViewLap_Neraca(piddata, bulan, p_tahun, userid);
        }
        public DataTable ViewLap_NeracaKonsolidasi(int p_tahun, string p_pt, int p_bulan, string userid)
        {
            DataTable locations = LoadConsolidationLocations(p_pt);
            DataTable result = CreateNeracaKonsolidasiTable(locations);
            Dictionary<string, DataRow> rowsByKey = new();

            foreach (DataRow location in locations.Rows)
            {
                string locationId = Convert.ToString(location["IDDATA"]);
                if (string.IsNullOrWhiteSpace(locationId))
                {
                    continue;
                }

                foreach (NeracaRow neracaRow in ViewLap_NeracaRows_V2(locationId, p_bulan, p_tahun, userid))
                {
                    string key = neracaRow.Kode + "|" + neracaRow.Kat + "|" + neracaRow.Cat2 + "|" + neracaRow.Akun;
                    if (!rowsByKey.TryGetValue(key, out DataRow row))
                    {
                        row = result.NewRow();
                        row["KODE"] = neracaRow.Akun;
                        row["KATEGORI"] = neracaRow.Kat;
                        row["KELOMPOK"] = neracaRow.Cat2;
                        row["REKENING"] = neracaRow.Tipe;
                        result.Rows.Add(row);
                        rowsByKey.Add(key, row);
                    }

                    row[locationId] = Convert.ToDouble(neracaRow.BulanIni);
                }
            }

            return result;
        }

        private static DataTable LoadConsolidationLocations(string idPt)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(
                @"SELECT IDDATA, WILAYAH
                    FROM MASTER_PT_DTL
                   WHERE IDPT = :p_idpt
                   ORDER BY IDDATA", connection)
            {
                CommandType = CommandType.Text,
                BindByName = true
            };
            command.Parameters.Add("p_idpt", OracleDbType.Varchar2, 20).Value = idPt;
            using OracleDataAdapter adapter = new(command);
            DataTable table = new();
            adapter.Fill(table);
            return table;
        }

        private static DataTable CreateNeracaKonsolidasiTable(DataTable locations)
        {
            DataTable table = new();
            table.Columns.Add("KODE", typeof(string));
            table.Columns.Add("KATEGORI", typeof(string));
            table.Columns.Add("KELOMPOK", typeof(string));
            table.Columns.Add("REKENING", typeof(string));

            foreach (DataRow location in locations.Rows)
            {
                string locationId = Convert.ToString(location["IDDATA"]);
                if (!string.IsNullOrWhiteSpace(locationId) && !table.Columns.Contains(locationId))
                {
                    table.Columns.Add(locationId, typeof(double));
                }
            }

            return table;
        }

        public DataSet ViewLap_BukuBesar(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            return ViewLap_BukuBesarDirect(P_IDDATA, p_tahun, p_tahun, p_bulan, p_sampaibulan, DARIKODE, SAMPAIKODE);
        }
        public DataSet ViewLap_BukuBesar_Tree(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string p_kode)
        {
            return ViewLap_BukuBesarDirect(P_IDDATA, p_tahun, p_tahun, p_bulan, p_sampaibulan, p_kode, p_kode);
        }

        public DataSet ViewLap_BukuBesarMultiTahun(string P_IDDATA, int p_tahundari, int p_tahunsampai, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            return ViewLap_BukuBesarDirect(P_IDDATA, p_tahundari, p_tahunsampai, p_bulan, p_sampaibulan, DARIKODE, SAMPAIKODE);
        }

        private static DataSet ViewLap_BukuBesarDirect(string iddata, int tahunDari, int tahunSampai, int bulanDari, int bulanSampai, string dariKode, string sampaiKode)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new("ACCT_LAPORAN_V2.LAP_BUKUBESAR_V2", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = iddata;
            command.Parameters.Add("p_TAHUNDARI", OracleDbType.Int16).Value = tahunDari;
            command.Parameters.Add("p_TAHUNSAMPAI", OracleDbType.Int16).Value = tahunSampai;
            command.Parameters.Add("p_BULANDARI", OracleDbType.Int16).Value = bulanDari;
            command.Parameters.Add("p_BULANSAMPAI", OracleDbType.Int16).Value = bulanSampai;
            command.Parameters.Add("p_DARIKODE", OracleDbType.Varchar2, 30).Value = dariKode;
            command.Parameters.Add("p_SAMPAIKODE", OracleDbType.Varchar2, 30).Value = sampaiKode;
            command.Parameters.Add("p_CURSOR", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

            using OracleDataAdapter sqlAdapter = new(command);
            DataSet ds = new();
            sqlAdapter.Fill(ds, "BukuBesar");
            GeneralLedgerRow.GetRequiredTable(ds);
            return ds;
        }
        public DataSet ViewLap_NeracaLajur(string piddata, int p_bulan, int p_tahun)
        {
            return ViewLap_Neraca(piddata, p_bulan, p_tahun, LoginInfo.userID);
        }

        public decimal Balanced_Check(string piddata, int pbulan, int ptahun)
        {
            const string sql = @"
                SELECT ROUND(
                    NVL(SUM(CASE :p_bulan
                        WHEN 1 THEN ""1D""
                        WHEN 2 THEN ""2D""
                        WHEN 3 THEN ""3D""
                        WHEN 4 THEN ""4D""
                        WHEN 5 THEN ""5D""
                        WHEN 6 THEN ""6D""
                        WHEN 7 THEN ""7D""
                        WHEN 8 THEN ""8D""
                        WHEN 9 THEN ""9D""
                        WHEN 10 THEN ""10D""
                        WHEN 11 THEN ""11D""
                        WHEN 12 THEN ""12D""
                        ELSE 0
                    END), 0) -
                    NVL(SUM(CASE :p_bulan
                        WHEN 1 THEN ""1K""
                        WHEN 2 THEN ""2K""
                        WHEN 3 THEN ""3K""
                        WHEN 4 THEN ""4K""
                        WHEN 5 THEN ""5K""
                        WHEN 6 THEN ""6K""
                        WHEN 7 THEN ""7K""
                        WHEN 8 THEN ""8K""
                        WHEN 9 THEN ""9K""
                        WHEN 10 THEN ""10K""
                        WHEN 11 THEN ""11K""
                        WHEN 12 THEN ""12K""
                        ELSE 0
                    END), 0),
                    2) SELISIH
                  FROM ACCT_COA
                 WHERE IDDATA = :p_iddata
                   AND TAHUN = :p_tahun
                   AND LVL = 1";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                CommandType = CommandType.Text,
                BindByName = true
            };
            command.Parameters.Add("p_bulan", OracleDbType.Int16).Value = pbulan;
            command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = piddata;
            command.Parameters.Add("p_tahun", OracleDbType.Int16).Value = ptahun;
            object result = command.ExecuteScalar();

            return result == DBNull.Value || result == null
                ? 0m
                : Convert.ToDecimal(result);
        }

        public async Task<IReadOnlyList<NeracaSaldoRow>> GetNeracaSaldoRowsAsync(
            string idData,
            int year,
            CancellationToken cancellationToken)
        {
            List<NeracaSaldoRow> rows = [];

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            const string sqlQuery = @"
                SELECT KODEACC,
                       NAMAACC,
                       NVL(PARENTACC, '') AS PARENTACC,
                       NVL(LVL, 0) AS LVL,
                       NVL(ISHEADER, 'D') AS ISHEADER,
                       NVL(ISAKTIF, 'Y') AS ISAKTIF,
                       NVL(SALDOAWAL, 0) AS SALDOAWAL,
                       NVL(""1S"", 0) AS JAN,
                       NVL(""2S"", 0) AS FEB,
                       NVL(""3S"", 0) AS MAR,
                       NVL(""4S"", 0) AS APR,
                       NVL(""5S"", 0) AS MEI,
                       NVL(""6S"", 0) AS JUN,
                       NVL(""7S"", 0) AS JUL,
                       NVL(""8S"", 0) AS AGU,
                       NVL(""9S"", 0) AS SEP,
                       NVL(""10S"", 0) AS OKT,
                       NVL(""11S"", 0) AS NOV,
                       NVL(""12S"", 0) AS DES
                FROM ACCT_COA
                WHERE IDDATA = :p_IDDATA
                  AND TAHUN = :p_TAHUN
                ORDER BY KODEACC";

            using OracleCommand command = new(sqlQuery, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text,
                CommandTimeout = 180
            };
            command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
            command.Parameters.Add("p_TAHUN", OracleDbType.Int32).Value = year;

            using DbDataReader reader = await command
                .ExecuteReaderAsync(cancellationToken)
                .ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                rows.Add(NeracaSaldoRow.FromDataRecord(reader));
            }

            return rows;
        }
    }

}
