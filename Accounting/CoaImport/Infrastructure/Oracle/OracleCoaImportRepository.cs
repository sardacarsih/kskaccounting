using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Accounting.CoaImport.Application;
using Accounting.CoaImport.Domain;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.CoaImport.Infrastructure.Oracle;

public sealed class OracleCoaImportRepository : ICoaImportRepository
{
    private readonly string _connectionString;

    public OracleCoaImportRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<CoaImportValidationIssue> ValidateRows(
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows,
        IProgress<CoaImportProgress>? progress = null)
    {
        return ExecuteOracleFunc("memvalidasi data Excel import COA", connection =>
        {
            IReadOnlySet<string> existingAccounts = FetchExistingAccounts(connection, scope);
            progress?.Report(new CoaImportProgress(8, "Membandingkan induk akun dengan master COA...", 0, rows.Count));
            return CoaImportRowValidator.Validate(rows, existingAccounts);
        });
    }

    public int ImportRows(
        CoaImportScope scope,
        CoaImportMode mode,
        IReadOnlyList<CoaImportRow> rows,
        IProgress<CoaImportProgress>? progress = null)
    {
        return ExecuteOracleFunc("menyimpan data COA ke master akun", connection =>
        {
            IReadOnlyDictionary<string, ExistingCoaRow> existingRows = FetchExistingRows(connection, scope);
            ImportPlan importPlan = BuildImportPlan(mode, rows, existingRows);

            progress?.Report(new CoaImportProgress(
                10,
                BuildImportPlanStage(importPlan),
                importPlan.SkippedRows,
                rows.Count,
                UseRowCount: true));

            using OracleTransaction transaction = connection.BeginTransaction();
            try
            {
                int processedRows = importPlan.SkippedRows;
                ExecuteInsertBatches(connection, transaction, scope, importPlan.InsertRows, rows.Count, progress, ref processedRows);
                ExecuteUpdateBatches(connection, transaction, scope, importPlan.UpdateRows, rows.Count, progress, ref processedRows);

                transaction.Commit();
                ReportImportProgress(progress, rows.Count, rows.Count, "Import COA selesai disimpan.", force: true);
                return 1;
            }
            catch
            {
                TryRollback(transaction);
                throw;
            }
        });
    }

    private static void ReportImportProgress(
        IProgress<CoaImportProgress>? progress,
        int processedRows,
        int totalRows,
        string stage,
        bool force = false)
    {
        if (progress is null ||
            (!force && processedRows % BatchSize != 0 && processedRows != totalRows))
        {
            return;
        }

        int percent = totalRows == 0
            ? 85
            : 10 + (int)Math.Round(processedRows * 75d / totalRows);
        progress.Report(new CoaImportProgress(
            Math.Clamp(percent, 10, 85),
            stage,
            processedRows,
            totalRows,
            UseRowCount: true));
    }

    public void EnsurePeriodExists(string idData, int year)
    {
        ExecuteOracleAction("memastikan periode awal tahun COA tersedia", connection =>
        {
            using OracleCommand checkCommand = new("ACCOUNTING.CekPeriodeExist", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true
            };
            checkCommand.Parameters.Add("ExistPeriode", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
            checkCommand.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
            checkCommand.Parameters.Add("p_bulan", OracleDbType.Int16).Value = 1;
            checkCommand.Parameters.Add("p_tahun", OracleDbType.Int16).Value = year;
            checkCommand.ExecuteScalar();

            int exists = Convert.ToInt32(checkCommand.Parameters["ExistPeriode"].Value.ToString());
            if (exists != 0)
            {
                return;
            }

            using OracleCommand createCommand = new("ACCOUNTING.CreateNextPeriode", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true
            };
            createCommand.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
            createCommand.Parameters.Add("p_bulan", OracleDbType.Int16).Value = 0;
            createCommand.Parameters.Add("p_tahun", OracleDbType.Int16).Value = year;
            createCommand.ExecuteNonQuery();
        });
    }

    public void RecalculateSaldo(string idData, int year, string userId)
    {
        ExecuteOracleAction("menghitung ulang saldo detail setelah import COA", connection =>
        {
            using OracleCommand command = new("ACCT_RECALLCULATIONS.RecalkulasiSaldoDetail", connection)
            {
                CommandType = CommandType.StoredProcedure,
                BindByName = true,
                CommandTimeout = 180
            };
            command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
            command.Parameters.Add("p_bulan", OracleDbType.Int16).Value = 1;
            command.Parameters.Add("p_tahun", OracleDbType.Int16).Value = year;
            command.Parameters.Add("p_Userid", OracleDbType.Varchar2, 20).Value = userId;
            command.ExecuteNonQuery();
        });
    }

    private void ExecuteOracleAction(string operation, Action<OracleConnection> action)
    {
        ExecuteOracleFunc(operation, connection =>
        {
            action(connection);
            return true;
        });
    }

    private T ExecuteOracleFunc<T>(string operation, Func<OracleConnection, T> action)
    {
        try
        {
            using OracleConnection connection = new(_connectionString);
            connection.Open();
            return action(connection);
        }
        catch (OracleException ex)
        {
            throw new InvalidOperationException(BuildOracleErrorMessage(operation, ex), ex);
        }
    }

    private const int BatchSize = 250;

    private static IReadOnlySet<string> FetchExistingAccounts(OracleConnection connection, CoaImportScope scope)
    {
        return FetchExistingRows(connection, scope).Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, ExistingCoaRow> FetchExistingRows(OracleConnection connection, CoaImportScope scope)
    {
        using OracleCommand command = new(
            """
SELECT KODEACC, GRP, PARENTACC, ISHEADER, LVL, POSISI, NAMAACC, SALDOAWAL, DIVISI, BLOK, TAHUNTANAM
  FROM ACCT_COA
 WHERE IDDATA = :p_iddata
   AND TAHUN = :p_tahun
""",
            connection)
        {
            CommandType = CommandType.Text,
            BindByName = true
        };
        command.Parameters.Add("p_iddata", OracleDbType.Varchar2, 20).Value = scope.IdData;
        command.Parameters.Add("p_tahun", OracleDbType.Int16).Value = scope.Year;

        Dictionary<string, ExistingCoaRow> rows = new(StringComparer.OrdinalIgnoreCase);
        using OracleDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string account = reader["KODEACC"]?.ToString()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(account))
            {
                rows[account] = new ExistingCoaRow(
                    account,
                    ReadString(reader, "GRP"),
                    ReadString(reader, "PARENTACC"),
                    ReadString(reader, "ISHEADER"),
                    ReadString(reader, "LVL"),
                    ReadString(reader, "POSISI"),
                    ReadString(reader, "NAMAACC"),
                    ReadDecimal(reader, "SALDOAWAL"),
                    ReadString(reader, "DIVISI"),
                    ReadString(reader, "BLOK"),
                    ReadString(reader, "TAHUNTANAM"));
            }
        }

        return rows;
    }

    private static ImportPlan BuildImportPlan(
        CoaImportMode mode,
        IReadOnlyList<CoaImportRow> rows,
        IReadOnlyDictionary<string, ExistingCoaRow> existingRows)
    {
        List<CoaImportRow> insertRows = [];
        List<CoaImportRow> updateRows = [];
        int skippedRows = 0;

        foreach (CoaImportRow row in rows)
        {
            string account = Normalize(row.Account);
            if (!existingRows.TryGetValue(account, out ExistingCoaRow? existingRow))
            {
                insertRows.Add(row);
                continue;
            }

            if (mode == CoaImportMode.Partial)
            {
                skippedRows++;
                continue;
            }

            if (existingRow.HasSameValue(row))
            {
                skippedRows++;
                continue;
            }

            updateRows.Add(row);
        }

        return new ImportPlan(insertRows, updateRows, skippedRows);
    }

    private static string BuildImportPlanStage(ImportPlan plan)
    {
        return $"Rencana import COA: insert {plan.InsertRows.Count:N0}, update {plan.UpdateRows.Count:N0}, skip {plan.SkippedRows:N0}.";
    }

    private static void ExecuteInsertBatches(
        OracleConnection connection,
        OracleTransaction transaction,
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows,
        int totalRows,
        IProgress<CoaImportProgress>? progress,
        ref int processedRows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        foreach (CoaImportRow[] batch in ChunkRows(rows))
        {
            using OracleCommand command = CreateBatchInsertCommand(connection, transaction, scope, batch);
            command.ExecuteNonQuery();
            processedRows += batch.Length;
            ReportImportProgress(progress, processedRows, totalRows, "Mengimport COA: menyimpan akun baru...", force: true);
        }
    }

    private static void ExecuteUpdateBatches(
        OracleConnection connection,
        OracleTransaction transaction,
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows,
        int totalRows,
        IProgress<CoaImportProgress>? progress,
        ref int processedRows)
    {
        if (rows.Count == 0)
        {
            return;
        }

        foreach (CoaImportRow[] batch in ChunkRows(rows))
        {
            using OracleCommand command = CreateBatchUpdateCommand(connection, transaction, scope, batch);
            command.ExecuteNonQuery();
            processedRows += batch.Length;
            ReportImportProgress(progress, processedRows, totalRows, "Mengimport COA: memperbarui akun existing...", force: true);
        }
    }

    private static OracleCommand CreateBatchInsertCommand(
        OracleConnection connection,
        OracleTransaction transaction,
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows)
    {
        const string sql = """
INSERT INTO ACCT_COA (
    ACCTCOAID, IDDATA, TAHUN, GRP, PARENTACC, ISHEADER, KODEACC,
    LVL, POSISI, NAMAACC, SALDOAWAL, ISAKTIF, DIVISI, BLOK, TAHUNTANAM
)
VALUES (
    RAWTOHEX(SYS_GUID()), :p_iddata, :p_tahun, :p_jenis, :p_induk, :p_gen, :p_account,
    :p_level, :p_posisi, :p_nama, :p_saldoawal, 'Y', :p_divisi, :p_blok, :p_tahuntanam
)
""";

        return CreateBatchCommand(connection, transaction, scope, rows, sql);
    }

    private static OracleCommand CreateBatchUpdateCommand(
        OracleConnection connection,
        OracleTransaction transaction,
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows)
    {
        const string sql = """
UPDATE ACCT_COA
   SET GRP = :p_jenis,
       PARENTACC = :p_induk,
       ISHEADER = :p_gen,
       LVL = :p_level,
       POSISI = :p_posisi,
       NAMAACC = :p_nama,
       SALDOAWAL = :p_saldoawal,
       DIVISI = :p_divisi,
       BLOK = :p_blok,
       TAHUNTANAM = :p_tahuntanam
 WHERE IDDATA = :p_iddata
   AND TAHUN = :p_tahun
   AND KODEACC = :p_account
""";

        return CreateBatchCommand(connection, transaction, scope, rows, sql);
    }

    private static OracleCommand CreateBatchCommand(
        OracleConnection connection,
        OracleTransaction transaction,
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows,
        string sql)
    {
        OracleCommand command = new(sql, connection)
        {
            ArrayBindCount = rows.Count,
            Transaction = transaction,
            CommandType = CommandType.Text,
            BindByName = true,
            CommandTimeout = 600
        };

        AddArrayParameter(command, "p_iddata", OracleDbType.Varchar2, Repeat(scope.IdData, rows.Count));
        AddArrayParameter(command, "p_tahun", OracleDbType.Int32, Repeat(scope.Year, rows.Count));
        AddArrayParameter(command, "p_jenis", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Jenis)).ToArray());
        AddArrayParameter(command, "p_induk", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Induk)).ToArray());
        AddArrayParameter(command, "p_gen", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Gen)).ToArray());
        AddArrayParameter(command, "p_account", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Account)).ToArray());
        AddArrayParameter(command, "p_level", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Level)).ToArray());
        AddArrayParameter(command, "p_posisi", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Posisi)).ToArray());
        AddArrayParameter(command, "p_nama", OracleDbType.Varchar2, rows.Select(row => Normalize(row.NamaPerkiraan)).ToArray());
        AddArrayParameter(command, "p_saldoawal", OracleDbType.Decimal, rows.Select(row => row.AwalTahun).ToArray());
        AddArrayParameter(command, "p_divisi", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Divisi)).ToArray());
        AddArrayParameter(command, "p_blok", OracleDbType.Varchar2, rows.Select(row => Normalize(row.Blok)).ToArray());
        AddArrayParameter(command, "p_tahuntanam", OracleDbType.Varchar2, rows.Select(row => Normalize(row.TahunTanam)).ToArray());

        return command;
    }

    private static void AddArrayParameter(OracleCommand command, string name, OracleDbType dbType, Array values)
    {
        OracleParameter parameter = new(name, dbType)
        {
            Direction = ParameterDirection.Input,
            Value = values
        };
        command.Parameters.Add(parameter);
    }

    private static T[] Repeat<T>(T value, int count)
    {
        T[] values = new T[count];
        Array.Fill(values, value);
        return values;
    }

    private static IEnumerable<CoaImportRow[]> ChunkRows(IReadOnlyList<CoaImportRow> rows)
    {
        for (int index = 0; index < rows.Count; index += BatchSize)
        {
            int count = Math.Min(BatchSize, rows.Count - index);
            CoaImportRow[] batch = new CoaImportRow[count];
            for (int offset = 0; offset < count; offset++)
            {
                batch[offset] = rows[index + offset];
            }

            yield return batch;
        }
    }

    private static string ReadString(OracleDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value == DBNull.Value
            ? string.Empty
            : value.ToString()?.Trim() ?? string.Empty;
    }

    private static decimal ReadDecimal(OracleDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value == DBNull.Value
            ? 0m
            : Convert.ToDecimal(value);
    }

    private static string Normalize(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static void TryRollback(OracleTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
            // The original database error is more useful to the user.
        }
    }

    private static string BuildOracleErrorMessage(string operation, OracleException exception)
    {
        string detail = exception.Number switch
        {
            1 => "Data COA melanggar constraint unik di database. Periksa duplikasi kode akun untuk IDDATA dan tahun yang sama.",
            904 => "Kolom database ACCT_COA yang dipakai import COA belum sesuai. Periksa struktur tabel ACCT_COA.",
            942 => "Tabel ACCT_COA tidak ditemukan atau user database tidak memiliki hak akses.",
            1017 => "Login database gagal. Periksa user/password koneksi Oracle.",
            12154 or 12514 or 12541 => "Koneksi ke server Oracle gagal. Periksa host, service name, listener, dan jaringan.",
            _ => "Terjadi error Oracle saat proses import COA."
        };

        return $"Gagal {operation}.{Environment.NewLine}{detail}{Environment.NewLine}Kode Oracle: ORA-{exception.Number:00000}{Environment.NewLine}Detail teknis: {exception.Message}";
    }

    private sealed record ImportPlan(
        IReadOnlyList<CoaImportRow> InsertRows,
        IReadOnlyList<CoaImportRow> UpdateRows,
        int SkippedRows);

    private sealed record ExistingCoaRow(
        string Account,
        string Jenis,
        string Induk,
        string Gen,
        string Level,
        string Posisi,
        string Nama,
        decimal SaldoAwal,
        string Divisi,
        string Blok,
        string TahunTanam)
    {
        public bool HasSameValue(CoaImportRow row)
        {
            return string.Equals(Jenis, Normalize(row.Jenis), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Induk, Normalize(row.Induk), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Gen, Normalize(row.Gen), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Level, Normalize(row.Level), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Posisi, Normalize(row.Posisi), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Nama, Normalize(row.NamaPerkiraan), StringComparison.Ordinal) &&
                SaldoAwal == row.AwalTahun &&
                string.Equals(Divisi, Normalize(row.Divisi), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Blok, Normalize(row.Blok), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(TahunTanam, Normalize(row.TahunTanam), StringComparison.OrdinalIgnoreCase);
        }
    }
}
