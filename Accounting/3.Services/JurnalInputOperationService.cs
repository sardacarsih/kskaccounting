using Accounting._1.Interface;
using Accounting.Model;
using Accounting.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public enum JurnalInputFocusTarget
    {
        None = 0,
        Nomor = 1,
        Tanggal = 2,
        Periode = 3,
        GridDetail = 4
    }

    public static class JurnalSaveErrorCodes
    {
        public const string None = "";
        public const string Validation = "VALIDATION";
        public const string DuplicateNomor = "DUPLICATE_NOMOR";
        public const string DuplicateData = "DUPLICATE_DATA";
        public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
        public const string Unknown = "UNKNOWN";
    }

    public sealed class JurnalSaveRequest
    {
        public string IdData { get; init; } = string.Empty;
        public string Nomor { get; init; } = string.Empty;
        public DateTime? Tanggal { get; init; }
        public string Periode { get; init; } = string.Empty;
        public string PeriodeLockCheck { get; init; } = string.Empty;
        public bool IsJurnalBalik { get; init; }
        public bool IsEdit { get; init; }
        public double OldJurnalId { get; init; }
        public DateTime? ExpectedHeaderVersionUtc { get; init; }
        public Guid? ClientRequestId { get; init; }
        public bool FromModuleAis { get; init; }
        public bool FromModuleKasir { get; init; }
        public bool FromModuleInv { get; init; }
        public decimal DebetTotal { get; init; }
        public decimal KreditTotal { get; init; }
        public IReadOnlyList<JurnalDetailAdd> Details { get; init; } = [];
        public string UserId { get; init; } = string.Empty;
    }

    public sealed class JurnalSaveResult
    {
        public bool IsSuccess { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public JurnalInputFocusTarget FocusTarget { get; private init; } = JurnalInputFocusTarget.None;
        public string ErrorCode { get; private init; } = JurnalSaveErrorCodes.None;
        public double? JurnalId { get; private init; }
        public long? RecalcJobId { get; private init; }
        public IReadOnlyList<string> ImpactedAccountCodes { get; private init; } = [];

        public static JurnalSaveResult Success(double jurnalId, long? recalcJobId, IReadOnlyList<string>? impactedAccountCodes = null)
        {
            return new JurnalSaveResult
            {
                IsSuccess = true,
                JurnalId = jurnalId,
                RecalcJobId = recalcJobId,
                ImpactedAccountCodes = impactedAccountCodes ?? []
            };
        }

        public static JurnalSaveResult Fail(
            string message,
            JurnalInputFocusTarget focusTarget = JurnalInputFocusTarget.None,
            string errorCode = JurnalSaveErrorCodes.Validation)
        {
            return new JurnalSaveResult
            {
                IsSuccess = false,
                Message = message,
                FocusTarget = focusTarget,
                ErrorCode = errorCode
            };
        }
    }

    public sealed class JurnalDeleteRequest
    {
        public string IdData { get; init; } = string.Empty;
        public string Periode { get; init; } = string.Empty;
        public string PeriodeLockCheck { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        public IReadOnlyList<double> JurnalIds { get; init; } = [];
    }

    public sealed class JurnalDeleteResult
    {
        public bool IsSuccess { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public int DeletedCount { get; private init; }
        public IReadOnlyList<long> RecalcJobIds { get; private init; } = [];
        public IReadOnlyList<string> ImpactedAccountCodes { get; private init; } = [];
        public string ErrorCode { get; private init; } = JurnalSaveErrorCodes.None;

        public static JurnalDeleteResult Success(int deletedCount, IReadOnlyList<long> recalcJobIds, IReadOnlyList<string>? impactedAccountCodes = null)
        {
            return new JurnalDeleteResult
            {
                IsSuccess = true,
                DeletedCount = deletedCount,
                RecalcJobIds = recalcJobIds,
                ImpactedAccountCodes = impactedAccountCodes ?? []
            };
        }

        public static JurnalDeleteResult Fail(string message, string errorCode = JurnalSaveErrorCodes.Validation)
        {
            return new JurnalDeleteResult
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    public sealed class RekalkulasiJobStatusSnapshot
    {
        public long JobId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string LastError { get; init; } = string.Empty;
        public DateTimeOffset? UpdatedDateUtc { get; init; }
    }

    public sealed class JurnalInputOperationService
    {
        private static readonly TimeSpan[] RetryBackoffSchedule =
        [
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15)
        ];

        private static readonly object WorkerInitGate = new();
        private static readonly RekalkulasiJobStore RekalkulasiJobStore = new();
        private static bool workersStarted;
        private static bool healthMonitorStarted;
        private static int maxRekalkulasiAttempts = RetryBackoffSchedule.Length + 1;

        private readonly IJurnalEntryRepository jurnalEntryRepository;

        public JurnalInputOperationService(IJurnalEntryRepository repository)
        {
            jurnalEntryRepository = repository;
            EnsureRekalkulasiWorkersStarted();
        }

        public Task<JurnalSaveResult> SaveAsync(JurnalSaveRequest request)
        {
            return Task.Run(() => Save(request));
        }

        public Task<JurnalDeleteResult> DeleteAsync(JurnalDeleteRequest request)
        {
            return Task.Run(() => Delete(request));
        }

        public static RekalkulasiJobStatusSnapshot? GetRekalkulasiJobStatus(long jobId)
        {
            if (jobId <= 0)
            {
                return null;
            }

            RekalkulasiJobStatusRecord? status = RekalkulasiJobStore.TryGetJobStatus(jobId);
            if (status == null)
            {
                return null;
            }

            return new RekalkulasiJobStatusSnapshot
            {
                JobId = status.JobId,
                Status = status.Status,
                LastError = status.LastError,
                UpdatedDateUtc = status.UpdatedDateUtc
            };
        }

        public JurnalSaveResult Save(JurnalSaveRequest request)
        {
            try
            {
                if (request.IsEdit)
                {
                    AuthorizationService.EnsureCanUpdateJurnal();
                }
                else
                {
                    AuthorizationService.EnsureCanCreateJurnal();
                }
            }
            catch (InvalidOperationException ex)
            {
                return JurnalSaveResult.Fail(ex.Message);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            string lockStatus = jurnalEntryRepository.GetLockStatus(request.IdData, request.PeriodeLockCheck);
            if (lockStatus == "Y")
            {
                return JurnalSaveResult.Fail(
                    $"Transaksi tidak dapat disimpan diperiode ini...!!!\nPeriode Akuntansi : {request.Periode} Telah Dikunci.");
            }

            if (string.IsNullOrEmpty(request.Nomor))
            {
                return JurnalSaveResult.Fail("Nomor Jurnal tidak boleh kosong", JurnalInputFocusTarget.Nomor);
            }

            if (request.Nomor.Length > 30)
            {
                return JurnalSaveResult.Fail(
                    $"Nomor Jurnal MAX 30 Karakter,panjang karakter sekarang : {request.Nomor.Length}",
                    JurnalInputFocusTarget.Nomor);
            }

            if (!request.Tanggal.HasValue)
            {
                return JurnalSaveResult.Fail("Tanggal tidak boleh kosong", JurnalInputFocusTarget.Tanggal);
            }

            if (string.IsNullOrEmpty(request.Periode))
            {
                return JurnalSaveResult.Fail("Periode tidak boleh kosong", JurnalInputFocusTarget.Periode);
            }

            if (request.Details.Count > 1)
            {
                IEnumerable<JurnalDetailAdd> kodeKosong = request.Details.Where(detail =>
                    string.IsNullOrEmpty(detail.Kode) && ((detail.Debet ?? 0) > 0 || (detail.Kredit ?? 0) > 0));
                if (kodeKosong.Any())
                {
                    string barisText = string.Join(Environment.NewLine, kodeKosong.Select(detail => detail.BARIS));
                    return JurnalSaveResult.Fail($"Baris \n{barisText}\nBelum ada Kode", JurnalInputFocusTarget.GridDetail);
                }

                IEnumerable<object> ketmax200 = request.Details
                    .Where(detail => !string.IsNullOrEmpty(detail.Kode) && (detail.Keterangan?.Length ?? 0) > 200)
                    .Select(detail => new { detail.BARIS, Panjang = detail.Keterangan.Length });

                if (ketmax200.Any())
                {
                    string detailText = string.Join(Environment.NewLine, ketmax200);
                    return JurnalSaveResult.Fail($"{detailText}\nKeterangan Lebih dari 200 Karakter", JurnalInputFocusTarget.GridDetail);
                }
            }

            if (request.DebetTotal == 0 && request.KreditTotal == 0)
            {
                return JurnalSaveResult.Fail("Nilai Transaksi Jurnal Belum diisi");
            }

            if (request.DebetTotal != request.KreditTotal)
            {
                decimal selisih = Math.Abs(request.DebetTotal - request.KreditTotal);
                return JurnalSaveResult.Fail($"Jurnal tidak seimbang,\nselisih {selisih.ToString("N2", CultureInfo.InvariantCulture)}");
            }

            if (!request.Nomor.Contains("/ND", StringComparison.OrdinalIgnoreCase)
                && !request.Nomor.Contains("/NK", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.IsEdit)
                {
                    bool nomorExist = jurnalEntryRepository.CekNoJurnalExist_input(
                        request.IdData,
                        request.Nomor.ToUpperInvariant(),
                        request.Periode);
                    if (nomorExist)
                    {
                        return JurnalSaveResult.Fail(
                            $"Nomor Jurnal : {request.Nomor} Sudah ada...!!!",
                            JurnalInputFocusTarget.Nomor,
                            JurnalSaveErrorCodes.DuplicateNomor);
                    }
                }
                else
                {
                    bool nomorExist = jurnalEntryRepository.CekNoJurnalExistExceptJurnalId(
                        request.IdData,
                        request.Nomor.ToUpperInvariant(),
                        request.Periode,
                        request.OldJurnalId);
                    if (nomorExist)
                    {
                        return JurnalSaveResult.Fail(
                            $"Nomor Jurnal : {request.Nomor} Sudah ada...!!!",
                            JurnalInputFocusTarget.Nomor,
                            JurnalSaveErrorCodes.DuplicateNomor);
                    }
                }
            }

            try
            {
                long dbOperationStartMs = stopwatch.ElapsedMilliseconds;
                string sumber = ResolveSumber(request.FromModuleAis, request.FromModuleKasir, request.FromModuleInv);
                string isRe = request.IsJurnalBalik ? "Y" : "T";
                string nomorHid = request.IdData + request.Periode + request.Nomor + request.Tanggal.Value.ToString("yyMMdd");
                string hostName = System.Net.Dns.GetHostName();
                string ipAddress = ToolsServices.GetLocalIPAddress();

                JurnalHeaderAdd jurnalHeader = new()
                {
                    HID = nomorHid,
                    NOJURNAL = request.Nomor,
                    TANGGAL = request.Tanggal.Value,
                    PERIODE = request.Periode,
                    IDDATA = request.IdData,
                    USERID = request.UserId,
                    SUMBER = sumber,
                    ISRE = isRe,
                    PC = hostName,
                    IP_ADD = ipAddress
                };

                List<JurnalDetailAdd> jurnalDetails = request.Details.Where(detail => !string.IsNullOrEmpty(detail.Kode)).ToList();

                JurnalPersistResult persistResult = request.IsEdit
                    ? jurnalEntryRepository.UpdateJurnalMasterDetail(request.OldJurnalId, jurnalHeader, jurnalDetails, request.ExpectedHeaderVersionUtc)
                    : jurnalEntryRepository.InsertJurnalMasterDetail(jurnalHeader, jurnalDetails);

                if (persistResult.IsConflict)
                {
                    return JurnalSaveResult.Fail(
                        "Data jurnal sudah berubah oleh user lain. Silakan reload data terbaru sebelum update ulang.",
                        JurnalInputFocusTarget.None,
                        JurnalSaveErrorCodes.ConcurrencyConflict);
                }

                double jurnalId = persistResult.JurnalId;
                JurnalRecalcHint recalcHint = persistResult.RecalcHint;
                long dbElapsedMs = stopwatch.ElapsedMilliseconds - dbOperationStartMs;
                long? recalcJobId = QueueRekalkulasiSaldoAsync(request, jurnalId, recalcHint);
                string recalcMode = recalcJobId.HasValue ? "db_job" : "skip_no_impact";

                Log.Information(
                    "PERF JurnalInputOperationService.Save elapsed_ms={ElapsedMs} db_elapsed_ms={DbElapsedMs} is_edit={IsEdit} detail_count={DetailCount} jurnal_id={JurnalId} recalc_mode={RecalcMode} recalc_scope={RecalcScope} recalc_required={RecalcRequired} impacted_rows={ImpactedRows} impacted_accounts={ImpactedAccounts} recalc_job_id={RecalcJobId}",
                    stopwatch.ElapsedMilliseconds,
                    dbElapsedMs,
                    request.IsEdit,
                    jurnalDetails.Count,
                    jurnalId,
                    recalcMode,
                    recalcHint.Scope,
                    recalcHint.RequiresRecalc,
                    recalcHint.ImpactedRowCount,
                    recalcHint.ImpactedAccountCount,
                    recalcJobId);

                return JurnalSaveResult.Success(jurnalId, recalcJobId, recalcHint.ImpactedAccountCodes);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    return JurnalSaveResult.Fail("Duplikasi Data", JurnalInputFocusTarget.None, JurnalSaveErrorCodes.DuplicateData);
                }

                return JurnalSaveResult.Fail(ex.Message, JurnalInputFocusTarget.None, JurnalSaveErrorCodes.Unknown);
            }
        }

        public JurnalDeleteResult Delete(JurnalDeleteRequest request)
        {
            try
            {
                AuthorizationService.EnsureCanDeleteJurnal();
            }
            catch (InvalidOperationException ex)
            {
                return JurnalDeleteResult.Fail(ex.Message);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            if (request.JurnalIds == null || request.JurnalIds.Count == 0)
            {
                return JurnalDeleteResult.Fail("Tidak ada jurnal yang dipilih untuk dihapus");
            }

            string lockStatus = jurnalEntryRepository.GetLockStatus(request.IdData, request.PeriodeLockCheck);
            if (lockStatus == "Y")
            {
                return JurnalDeleteResult.Fail(
                    $"Transaksi tidak dapat dihapus diperiode ini...!!!\nPeriode Akuntansi : {request.Periode} Telah Dikunci.");
            }

            try
            {
                long dbOperationStartMs = stopwatch.ElapsedMilliseconds;
                List<long> recalcJobIds = new();
                HashSet<string> impactedAccountCodes = new(StringComparer.OrdinalIgnoreCase);

                foreach (double jurnalId in request.JurnalIds.Distinct())
                {
                    JurnalPersistResult persistResult = jurnalEntryRepository.HapusJurnal(jurnalId);
                    foreach (string kode in persistResult.RecalcHint.ImpactedAccountCodes)
                    {
                        impactedAccountCodes.Add(kode);
                    }
                    long? recalcJobId = QueueRekalkulasiSaldoAsync(
                        request.IdData,
                        request.Periode,
                        request.UserId,
                        persistResult.JurnalId,
                        persistResult.RecalcHint);
                    if (recalcJobId.HasValue)
                    {
                        recalcJobIds.Add(recalcJobId.Value);
                    }
                }

                long dbElapsedMs = stopwatch.ElapsedMilliseconds - dbOperationStartMs;
                Log.Information(
                    "PERF JurnalInputOperationService.Delete elapsed_ms={ElapsedMs} db_elapsed_ms={DbElapsedMs} delete_count={DeleteCount} periode={Periode} recalc_job_count={RecalcJobCount}",
                    stopwatch.ElapsedMilliseconds,
                    dbElapsedMs,
                    request.JurnalIds.Count,
                    request.Periode,
                    recalcJobIds.Count);
                string[] impactedCodes = impactedAccountCodes
                    .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                return JurnalDeleteResult.Success(request.JurnalIds.Count, recalcJobIds, impactedCodes);
            }
            catch (Exception ex)
            {
                return JurnalDeleteResult.Fail(ex.Message, JurnalSaveErrorCodes.Unknown);
            }
        }

        private static string ResolveSumber(bool fromModuleAis, bool fromModuleKasir, bool fromModuleInv)
        {
            string sumber = "JV";

            if (fromModuleAis)
            {
                sumber = "AIS";
            }

            if (fromModuleKasir)
            {
                sumber = "KASIR";
            }

            if (fromModuleInv)
            {
                sumber = "INV";
            }

            return sumber;
        }

        private static long? QueueRekalkulasiSaldoAsync(JurnalSaveRequest request, double jurnalId, JurnalRecalcHint recalcHint)
        {
            return QueueRekalkulasiSaldoAsync(
                request.IdData,
                request.Periode,
                request.UserId,
                jurnalId,
                recalcHint);
        }

        private static long? QueueRekalkulasiSaldoAsync(string idData, string periode, string userId, double jurnalId, JurnalRecalcHint recalcHint)
        {
            if (!recalcHint.RequiresRecalc)
            {
                Log.Information(
                    "PERF JurnalInputOperationService.Rekalkulasi skipped_no_impact jurnal_id={JurnalId} periode={Periode} scope={Scope}",
                    jurnalId,
                    periode,
                    recalcHint.Scope);
                return null;
            }

            if (!TryParsePeriode(periode, out int glMonth, out int glYear))
            {
                Log.Warning("PERF JurnalInputOperationService.Rekalkulasi skipped_invalid_periode periode={Periode} jurnal_id={JurnalId}", periode, jurnalId);
                return null;
            }

            long jobId = RekalkulasiJobStore.Enqueue(
                idData,
                periode,
                jurnalId,
                glMonth,
                glYear,
                userId,
                recalcHint.Scope,
                recalcHint.ImpactedAccountCount,
                recalcHint.ImpactedRowCount);
            Log.Information(
                "PERF JurnalInputOperationService.Rekalkulasi job_enqueued job_id={JobId} jurnal_id={JurnalId} periode={Periode} scope={Scope} impacted_rows={ImpactedRows} impacted_accounts={ImpactedAccounts}",
                jobId,
                jurnalId,
                periode,
                recalcHint.Scope,
                recalcHint.ImpactedRowCount,
                recalcHint.ImpactedAccountCount);
            return jobId;
        }

        private static void EnsureRekalkulasiWorkersStarted()
        {
            lock (WorkerInitGate)
            {
                if (workersStarted)
                {
                    return;
                }

                RecoverStaleRunningJobs();

                int workerCount = ResolveRekalkulasiWorkerCount();
                for (int i = 0; i < workerCount; i++)
                {
                    int workerId = i + 1;
                    _ = Task.Run(() => RekalkulasiWorkerLoop(workerId, CancellationToken.None));
                }

                if (!healthMonitorStarted)
                {
                    _ = Task.Run(() => RekalkulasiHealthMonitorLoop(CancellationToken.None));
                    healthMonitorStarted = true;
                }

                workersStarted = true;
                Log.Information("PERF JurnalInputOperationService.Rekalkulasi workers_started={WorkerCount}", workerCount);
            }
        }

        private static async Task RekalkulasiWorkerLoop(int workerId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                RekalkulasiJobRecord? job = null;
                try
                {
                    job = RekalkulasiJobStore.TryClaimNextDueJob();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PERF JurnalInputOperationService.Rekalkulasi worker_claim_exception worker={WorkerId}", workerId);
                    await Task.Delay(1500, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (job == null)
                {
                    await Task.Delay(750, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                DateTimeOffset createdAtUtc = job.CreatedDateUtc.ToUniversalTime();
                double queueDelayMs = Math.Max(0d, (DateTimeOffset.UtcNow - createdAtUtc).TotalMilliseconds);
                Log.Information(
                    "PERF JurnalInputOperationService.Rekalkulasi job_claimed job_id={JobId} jurnal_id={JurnalId} periode={Periode} worker={WorkerId} attempt={Attempt} queue_delay_ms={QueueDelayMs}",
                    job.JobId,
                    job.JurnalId,
                    job.Periode,
                    workerId,
                    job.AttemptCount,
                    queueDelayMs);

                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    AccountServices.RekalkulasiByJurnalID(
                        job.IdData,
                        job.GlMonth,
                        job.GlYear,
                        job.JurnalId,
                        job.Periode,
                        job.UserId);

                    RekalkulasiJobStore.MarkDone(job.JobId);
                    Log.Information(
                        "PERF JurnalInputOperationService.Rekalkulasi elapsed_ms={ElapsedMs} jurnal_id={JurnalId} periode={Periode} job_id={JobId} worker={WorkerId} queue_delay_ms={QueueDelayMs}",
                        stopwatch.ElapsedMilliseconds,
                        job.JurnalId,
                        job.Periode,
                        job.JobId,
                        workerId,
                        queueDelayMs);
                }
                catch (Exception ex)
                {
                    if (job.AttemptCount >= maxRekalkulasiAttempts)
                    {
                        RekalkulasiJobStore.MarkFailed(job.JobId, ex.Message);
                        Log.Error(
                            ex,
                            "PERF JurnalInputOperationService.Rekalkulasi failed_permanent job_id={JobId} jurnal_id={JurnalId} periode={Periode} attempt={Attempt}",
                            job.JobId,
                            job.JurnalId,
                            job.Periode,
                            job.AttemptCount);
                    }
                    else
                    {
                        TimeSpan retryDelay = ResolveRetryDelay(job.AttemptCount);
                        RekalkulasiJobStore.MarkRetry(job.JobId, ex.Message, retryDelay);
                        Log.Warning(
                            ex,
                            "PERF JurnalInputOperationService.Rekalkulasi failed_retry job_id={JobId} jurnal_id={JurnalId} periode={Periode} attempt={Attempt} retry_in_ms={RetryInMs}",
                            job.JobId,
                            job.JurnalId,
                            job.Periode,
                            job.AttemptCount,
                            retryDelay.TotalMilliseconds);
                    }
                }
            }
        }

        private static async Task RekalkulasiHealthMonitorLoop(CancellationToken cancellationToken)
        {
            int intervalSeconds = ResolveRekalkulasiHealthLogIntervalSeconds();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RecoverStaleRunningJobs();
                    RekalkulasiJobStatusSummary summary = RekalkulasiJobStore.GetStatusSummary();
                    double? oldestPendingAgeMs = summary.OldestPendingUtc.HasValue
                        ? Math.Max(0d, (DateTimeOffset.UtcNow - summary.OldestPendingUtc.Value.ToUniversalTime()).TotalMilliseconds)
                        : null;

                    Log.Information(
                        "PERF JurnalInputOperationService.Rekalkulasi health pending={PendingCount} running={RunningCount} retry={RetryCount} failed={FailedCount} oldest_pending_age_ms={OldestPendingAgeMs}",
                        summary.PendingCount,
                        summary.RunningCount,
                        summary.RetryCount,
                        summary.FailedCount,
                        oldestPendingAgeMs);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PERF JurnalInputOperationService.Rekalkulasi health_loop_exception");
                }

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken).ConfigureAwait(false);
            }
        }

        private static void RecoverStaleRunningJobs()
        {
            try
            {
                TimeSpan staleThreshold = ResolveRekalkulasiRunningStaleThreshold();
                int recoveredJobs = RekalkulasiJobStore.RecoverStaleRunningJobs(staleThreshold);
                if (recoveredJobs > 0)
                {
                    Log.Warning(
                        "PERF JurnalInputOperationService.Rekalkulasi recovered_stale_running jobs={RecoveredJobs} stale_threshold_seconds={StaleThresholdSeconds}",
                        recoveredJobs,
                        staleThreshold.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PERF JurnalInputOperationService.Rekalkulasi stale_recovery_exception");
            }
        }

        private static TimeSpan ResolveRetryDelay(int attemptCount)
        {
            int index = Math.Clamp(attemptCount - 1, 0, RetryBackoffSchedule.Length - 1);
            return RetryBackoffSchedule[index];
        }

        private static int ResolveRekalkulasiWorkerCount()
        {
            const int defaultWorkerCount = 2;
            string? rawValue = Environment.GetEnvironmentVariable("ACCOUNTING_RECALC_WORKERS");
            if (!int.TryParse(rawValue, out int parsed) || parsed < 1)
            {
                return defaultWorkerCount;
            }

            return Math.Min(parsed, 8);
        }

        private static int ResolveRekalkulasiHealthLogIntervalSeconds()
        {
            const int defaultIntervalSeconds = 30;
            string? rawValue = Environment.GetEnvironmentVariable("ACCOUNTING_RECALC_HEALTH_LOG_SECONDS");
            if (!int.TryParse(rawValue, out int parsed) || parsed < 5)
            {
                return defaultIntervalSeconds;
            }

            return Math.Min(parsed, 300);
        }

        private static TimeSpan ResolveRekalkulasiRunningStaleThreshold()
        {
            const int defaultStaleSeconds = 180;
            string? rawValue = Environment.GetEnvironmentVariable("ACCOUNTING_RECALC_STALE_SECONDS");
            if (!int.TryParse(rawValue, out int parsed) || parsed < 30)
            {
                return TimeSpan.FromSeconds(defaultStaleSeconds);
            }

            return TimeSpan.FromSeconds(Math.Min(parsed, 86400));
        }

        private static bool TryParsePeriode(string periode, out int month, out int year)
        {
            month = 0;
            year = 0;

            if (!DateTime.TryParseExact(periode, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return false;
            }

            month = parsed.Month;
            year = parsed.Year;
            return true;
        }
    }
}
