using System;
using System.Collections.Generic;

namespace Accounting.CoaImport.Domain;

public sealed class CoaImportResult
{
    private CoaImportResult(
        bool isSuccess,
        int statusCode,
        string message,
        string batchId,
        TimeSpan elapsed,
        IReadOnlyList<CoaImportValidationIssue> issues)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
        BatchId = batchId;
        Elapsed = elapsed;
        Issues = issues;
    }

    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public string Message { get; }
    public string BatchId { get; }
    public TimeSpan Elapsed { get; }
    public IReadOnlyList<CoaImportValidationIssue> Issues { get; }

    public static CoaImportResult Success(int statusCode, string batchId, TimeSpan elapsed)
    {
        return new CoaImportResult(true, statusCode, "Import Chart Of Account selesai.", batchId, elapsed, []);
    }

    public static CoaImportResult Failed(int statusCode, string message, string batchId, TimeSpan elapsed)
    {
        return new CoaImportResult(false, statusCode, message, batchId, elapsed, []);
    }

    public static CoaImportResult ValidationFailed(IReadOnlyList<CoaImportValidationIssue> issues, string batchId, TimeSpan elapsed)
    {
        return new CoaImportResult(false, 0, "Import Chart Of Account dibatalkan karena validasi gagal.", batchId, elapsed, issues);
    }
}
