using Accounting.Model;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using System;
using System.Data;

namespace Accounting.BusinessLayer
{
    internal sealed class RekalkulasiJobRecord
    {
        public long JobId { get; init; }
        public string IdData { get; init; } = string.Empty;
        public string Periode { get; init; } = string.Empty;
        public double JurnalId { get; init; }
        public int GlMonth { get; init; }
        public int GlYear { get; init; }
        public string UserId { get; init; } = string.Empty;
        public int AttemptCount { get; init; }
        public DateTimeOffset CreatedDateUtc { get; init; }
    }

    internal sealed class RekalkulasiJobStatusSummary
    {
        public int PendingCount { get; init; }
        public int RunningCount { get; init; }
        public int RetryCount { get; init; }
        public int FailedCount { get; init; }
        public DateTimeOffset? OldestPendingUtc { get; init; }
    }

    internal sealed class RekalkulasiJobStatusRecord
    {
        public long JobId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string LastError { get; init; } = string.Empty;
        public DateTimeOffset? UpdatedDateUtc { get; init; }
    }

    internal sealed class RekalkulasiJobStore
    {
        private const int MaxErrorLength = 1800;
        private const int ClaimCandidateBatchSize = 8;

        public long Enqueue(
            string idData,
            string periode,
            double jurnalId,
            int glMonth,
            int glYear,
            string userId,
            string recalcScope,
            int impactedAccountCount,
            int impactedRowCount)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            const string existingSql = @"SELECT JOB_ID
                FROM ACCT_RECALC_JOB
                WHERE IDDATA = :idData
                  AND PERIODE = :periode
                  AND JURNALID = :jurnalId
                  AND STATUS IN ('PENDING', 'RETRY', 'RUNNING')
                ORDER BY JOB_ID DESC
                FETCH FIRST 1 ROWS ONLY";

            long? existingJobId = connection.ExecuteScalar<long?>(existingSql, new { idData, periode, jurnalId });
            if (existingJobId.HasValue)
            {
                Log.Information(
                    "PERF RekalkulasiJobStore enqueue_dedup iddata={IdData} periode={Periode} jurnal_id={JurnalId} job_id={JobId}",
                    idData,
                    periode,
                    jurnalId,
                    existingJobId.Value);
                return existingJobId.Value;
            }

            const string insertSql = @"INSERT INTO ACCT_RECALC_JOB
                (IDDATA, PERIODE, JURNALID, GLMONTH, GLYEAR, USERID, STATUS, ATTEMPT_COUNT, CREATED_DATE, UPDATED_DATE)
                VALUES
                (:idData, :periode, :jurnalId, :glMonth, :glYear, :userId, 'PENDING', 0, SYSTIMESTAMP, SYSTIMESTAMP)
                RETURNING JOB_ID INTO :jobId";
            const string insertSqlWithTelemetry = @"INSERT INTO ACCT_RECALC_JOB
                (IDDATA, PERIODE, JURNALID, GLMONTH, GLYEAR, USERID, STATUS, ATTEMPT_COUNT, RECALC_SCOPE, IMPACTED_ACCOUNT_COUNT, IMPACTED_ROW_COUNT, CREATED_DATE, UPDATED_DATE)
                VALUES
                (:idData, :periode, :jurnalId, :glMonth, :glYear, :userId, 'PENDING', 0, :recalcScope, :impactedAccountCount, :impactedRowCount, SYSTIMESTAMP, SYSTIMESTAMP)
                RETURNING JOB_ID INTO :jobId";

            DynamicParameters parameters = new();
            parameters.Add("idData", idData, DbType.String);
            parameters.Add("periode", periode, DbType.String);
            parameters.Add("jurnalId", jurnalId, DbType.Double);
            parameters.Add("glMonth", glMonth, DbType.Int32);
            parameters.Add("glYear", glYear, DbType.Int32);
            parameters.Add("userId", userId, DbType.String);
            parameters.Add("recalcScope", string.IsNullOrWhiteSpace(recalcScope) ? JurnalRecalcScopes.None : recalcScope, DbType.String);
            parameters.Add("impactedAccountCount", Math.Max(0, impactedAccountCount), DbType.Int32);
            parameters.Add("impactedRowCount", Math.Max(0, impactedRowCount), DbType.Int32);
            parameters.Add("jobId", dbType: DbType.Int64, direction: ParameterDirection.Output);

            try
            {
                connection.Execute(insertSqlWithTelemetry, parameters);
            }
            catch (OracleException ex) when (IsRecalcTelemetryColumnsUnavailable(ex))
            {
                connection.Execute(insertSql, parameters);
            }

            long jobId = parameters.Get<long>("jobId");
            Log.Information(
                "PERF RekalkulasiJobStore enqueue_created iddata={IdData} periode={Periode} jurnal_id={JurnalId} job_id={JobId} scope={Scope} impacted_accounts={ImpactedAccounts} impacted_rows={ImpactedRows}",
                idData,
                periode,
                jurnalId,
                jobId,
                recalcScope,
                impactedAccountCount,
                impactedRowCount);
            return jobId;
        }

        public RekalkulasiJobRecord? TryClaimNextDueJob()
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleTransaction transaction = connection.BeginTransaction();
            try
            {
                const string candidateSql = @"SELECT JOB_ID
                    FROM (
                        SELECT JOB_ID
                        FROM ACCT_RECALC_JOB
                        WHERE STATUS IN ('PENDING', 'RETRY')
                          AND (NEXT_RETRY_AT IS NULL OR NEXT_RETRY_AT <= SYSTIMESTAMP)
                        ORDER BY CREATED_DATE, JOB_ID
                    )
                    WHERE ROWNUM <= :candidateLimit";

                const string claimByIdSql = @"UPDATE ACCT_RECALC_JOB
                      SET STATUS = 'RUNNING',
                          ATTEMPT_COUNT = ATTEMPT_COUNT + 1,
                          LAST_ERROR = NULL,
                          STARTED_DATE = SYSTIMESTAMP,
                          UPDATED_DATE = SYSTIMESTAMP
                      WHERE JOB_ID = :jobId
                        AND STATUS IN ('PENDING', 'RETRY')
                        AND (NEXT_RETRY_AT IS NULL OR NEXT_RETRY_AT <= SYSTIMESTAMP)";

                const string loadClaimedSql = @"SELECT JOB_ID AS JobId,
                           IDDATA AS IdData,
                           PERIODE AS Periode,
                           JURNALID AS JurnalId,
                           GLMONTH AS GlMonth,
                           GLYEAR AS GlYear,
                           USERID AS UserId,
                           ATTEMPT_COUNT AS AttemptCount,
                           CREATED_DATE AS CreatedDateUtc
                    FROM ACCT_RECALC_JOB
                    WHERE JOB_ID = :jobId";

                long[] candidateIds = connection
                    .Query<long>(
                        candidateSql,
                        new { candidateLimit = ClaimCandidateBatchSize },
                        transaction)
                    .AsList()
                    .ToArray();

                if (candidateIds.Length == 0)
                {
                    transaction.Commit();
                    return null;
                }

                foreach (long candidateJobId in candidateIds)
                {
                    int affectedRows = connection.Execute(
                        claimByIdSql,
                        new { jobId = candidateJobId },
                        transaction);

                    if (affectedRows != 1)
                    {
                        continue;
                    }

                    RekalkulasiJobRecord? claimedJob = connection.QueryFirstOrDefault<RekalkulasiJobRecord>(
                        loadClaimedSql,
                        new { jobId = candidateJobId },
                        transaction);

                    transaction.Commit();
                    return claimedJob;
                }

                transaction.Commit();
                return null;
            }
            catch (OracleException ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // best effort rollback
                }

                Log.Error(
                    ex,
                    "PERF RekalkulasiJobStore claim_failed oracle_code={OracleCode}",
                    ex.Number);
                throw;
            }
            catch
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // best effort rollback
                }

                throw;
            }
        }

        public void MarkDone(long jobId)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            connection.Execute(
                @"UPDATE ACCT_RECALC_JOB
                  SET STATUS = 'DONE',
                      FINISHED_DATE = SYSTIMESTAMP,
                      UPDATED_DATE = SYSTIMESTAMP
                  WHERE JOB_ID = :jobId",
                new { jobId });
        }

        public void MarkRetry(long jobId, string errorMessage, TimeSpan retryDelay)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            connection.Execute(
                @"UPDATE ACCT_RECALC_JOB
                  SET STATUS = 'RETRY',
                      LAST_ERROR = :errorMessage,
                      NEXT_RETRY_AT = SYSTIMESTAMP + NUMTODSINTERVAL(:retryDelaySeconds, 'SECOND'),
                      UPDATED_DATE = SYSTIMESTAMP
                  WHERE JOB_ID = :jobId",
                new
                {
                    jobId,
                    errorMessage = NormalizeErrorMessage(errorMessage),
                    retryDelaySeconds = (int)Math.Max(1, retryDelay.TotalSeconds)
                });
        }

        public void MarkFailed(long jobId, string errorMessage)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            connection.Execute(
                @"UPDATE ACCT_RECALC_JOB
                  SET STATUS = 'FAILED',
                      LAST_ERROR = :errorMessage,
                      FINISHED_DATE = SYSTIMESTAMP,
                      UPDATED_DATE = SYSTIMESTAMP
                  WHERE JOB_ID = :jobId",
                new
                {
                    jobId,
                    errorMessage = NormalizeErrorMessage(errorMessage)
                });
        }

        public RekalkulasiJobStatusSummary GetStatusSummary()
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            const string sql = @"SELECT
                    NVL(SUM(CASE WHEN STATUS = 'PENDING' THEN 1 ELSE 0 END), 0) AS PendingCount,
                    NVL(SUM(CASE WHEN STATUS = 'RUNNING' THEN 1 ELSE 0 END), 0) AS RunningCount,
                    NVL(SUM(CASE WHEN STATUS = 'RETRY' THEN 1 ELSE 0 END), 0) AS RetryCount,
                    NVL(SUM(CASE WHEN STATUS = 'FAILED' THEN 1 ELSE 0 END), 0) AS FailedCount,
                    MIN(CASE WHEN STATUS IN ('PENDING', 'RETRY') THEN CREATED_DATE END) AS OldestPendingUtc
                FROM ACCT_RECALC_JOB";

            RekalkulasiJobStatusSummary? summary = connection.QueryFirstOrDefault<RekalkulasiJobStatusSummary>(sql);
            return summary ?? new RekalkulasiJobStatusSummary();
        }

        public RekalkulasiJobStatusRecord? TryGetJobStatus(long jobId)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            const string sql = @"SELECT
                    JOB_ID AS JobId,
                    STATUS AS Status,
                    NVL(LAST_ERROR, '') AS LastError,
                    UPDATED_DATE AS UpdatedDateUtc
                FROM ACCT_RECALC_JOB
                WHERE JOB_ID = :jobId";

            return connection.QueryFirstOrDefault<RekalkulasiJobStatusRecord>(sql, new { jobId });
        }

        public int RecoverStaleRunningJobs(TimeSpan staleThreshold)
        {
            int staleSeconds = (int)Math.Max(30, staleThreshold.TotalSeconds);
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            const string sql = @"UPDATE ACCT_RECALC_JOB
                SET STATUS = 'RETRY',
                    LAST_ERROR = CASE
                        WHEN LAST_ERROR IS NULL THEN 'Recovered stale RUNNING job after worker interruption'
                        ELSE LAST_ERROR
                    END,
                    NEXT_RETRY_AT = SYSTIMESTAMP,
                    UPDATED_DATE = SYSTIMESTAMP
                WHERE STATUS = 'RUNNING'
                  AND NVL(STARTED_DATE, UPDATED_DATE) < SYSTIMESTAMP - NUMTODSINTERVAL(:staleSeconds, 'SECOND')";

            return connection.Execute(sql, new { staleSeconds });
        }

        private static string NormalizeErrorMessage(string message)
        {
            string normalized = string.IsNullOrWhiteSpace(message) ? "Unknown error" : message.Trim();
            if (normalized.Length <= MaxErrorLength)
            {
                return normalized;
            }

            Log.Warning("PERF RekalkulasiJobStore error_message_truncated original_length={OriginalLength}", normalized.Length);
            return normalized[..MaxErrorLength];
        }

        private static bool IsRecalcTelemetryColumnsUnavailable(OracleException ex)
        {
            string message = ex.Message ?? string.Empty;
            return message.Contains("ORA-00904", StringComparison.OrdinalIgnoreCase);
        }
    }
}
