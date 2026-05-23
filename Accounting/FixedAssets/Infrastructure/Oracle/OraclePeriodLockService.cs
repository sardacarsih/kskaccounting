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

        await using OracleCommand cmd = new("ACCT_JURNAL.GetStatusLock", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.Add("LockStatus", OracleDbType.Varchar2, 20).Direction = System.Data.ParameterDirection.ReturnValue;
        cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = period;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        string result = Convert.ToString(cmd.Parameters["LockStatus"].Value) ?? "N";
        return result.Trim().ToUpperInvariant() == "Y";
    }
}
