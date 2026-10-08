using System;

namespace Accounting.Model
{
    public sealed record NeracaSaldoReportMetadata(
        string CompanyName,
        string Region,
        int Year,
        string UserId,
        DateTime GeneratedAt);
}
