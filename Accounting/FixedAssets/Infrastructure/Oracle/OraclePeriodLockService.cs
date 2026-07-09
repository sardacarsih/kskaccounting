using System;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.FixedAssets.Infrastructure.Oracle;

public sealed class OraclePeriodLockService : IPeriodLockService
{
    private readonly string _connectionString;

    public OraclePeriodLockService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<bool> IsPeriodLockedAsync(string idData, string period, CancellationToken cancellationToken)
    {
        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        const string sql = """
            SELECT NVL(MAX(ISLOCKED), 'N')
            FROM ACCT_PERIODE
            WHERE IDDATA = :p_IDDATA
              AND PERIODE = :p_periode
            """;

        await using OracleCommand cmd = new(sql, conn)
        {
            CommandType = System.Data.CommandType.Text,
            BindByName = true
        };
        cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = period;
        object? scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        string result = Convert.ToString(scalar) ?? "N";
        return result.Trim().ToUpperInvariant() == "Y";
    }
}
