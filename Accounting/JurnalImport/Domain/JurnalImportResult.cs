using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Accounting.JurnalImport.Domain;

public sealed class JurnalImportResult
{
    private JurnalImportResult(
        bool isSuccess,
        int statusCode,
        string message,
        IReadOnlyList<JurnalImportValidationIssue> issues,
        IReadOnlyList<JurnalImportBalanceIssue> balanceIssues,
        TimeSpan elapsed,
        IReadOnlyList<long>? recalcJobIds = null,
        IReadOnlyList<string>? impactedAccountCodes = null,
        bool hasRecalculationWarning = false,
        string recalculationWarning = "")
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        Issues = issues;
        BalanceIssues = balanceIssues;
        Elapsed = elapsed;
        RecalcJobIds = recalcJobIds ?? [];
        ImpactedAccountCodes = impactedAccountCodes ?? [];
        HasRecalculationWarning = hasRecalculationWarning;
        RecalculationWarning = recalculationWarning;
    }

    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string Message { get; }
    public IReadOnlyList<JurnalImportValidationIssue> Issues { get; }
    public IReadOnlyList<JurnalImportBalanceIssue> BalanceIssues { get; }
    public TimeSpan Elapsed { get; }
    public IReadOnlyList<long> RecalcJobIds { get; }
    public IReadOnlyList<string> ImpactedAccountCodes { get; }
    public bool HasRecalculationWarning { get; }
    public string RecalculationWarning { get; }

    public static JurnalImportResult Success(
        int statusCode,
        Stopwatch stopwatch,
        JurnalImportRecalcQueueResult? recalcQueueResult = null)
    {
        stopwatch.Stop();
        return new JurnalImportResult(
            true,
            statusCode,
            "Import Jurnal Selesai",
            [],
            [],
            stopwatch.Elapsed,
            recalcQueueResult?.JobIds,
            recalcQueueResult?.ImpactedAccountCodes);
    }

    public static JurnalImportResult SuccessWithRecalculationWarning(int statusCode, string warning, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new JurnalImportResult(
            true,
            statusCode,
            "Import Jurnal Selesai, tetapi rekalkulasi saldo gagal.",
            [],
            [],
            stopwatch.Elapsed,
            hasRecalculationWarning: true,
            recalculationWarning: warning);
    }

    public static JurnalImportResult ValidationFailed(IReadOnlyList<JurnalImportValidationIssue> issues, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        string message = issues.Count == 0 ? "Import Jurnal dibatalkan." : issues[0].Message;
        return new JurnalImportResult(false, -1, message, issues, [], stopwatch.Elapsed);
    }

    public static JurnalImportResult NotBalanced(IReadOnlyList<JurnalImportBalanceIssue> balanceIssues, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new JurnalImportResult(
            false,
            1,
            "Import Jurnal di Batalkan \nJurnal Tidak Seimbang ",
            [new JurnalImportValidationIssue("BALANCE_NOT_ZERO", "Import Jurnal di Batalkan \nJurnal Tidak Seimbang ")],
            balanceIssues,
            stopwatch.Elapsed);
    }

    public static JurnalImportResult Failed(int statusCode, string message, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return new JurnalImportResult(false, statusCode, message, [], [], stopwatch.Elapsed);
    }
}