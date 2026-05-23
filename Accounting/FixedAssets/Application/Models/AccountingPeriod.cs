using System;
using System.Globalization;

namespace Accounting.FixedAssets.Application.Models;

public readonly record struct AccountingPeriod(int Month, int Year)
{
    public string PeriodeString => $"{Month:00}/{Year:0000}";
    public DateTime StartDate => new(Year, Month, 1);
    public DateTime EndDate => StartDate.AddMonths(1).AddDays(-1);

    public static AccountingPeriod Parse(string period)
    {
        if (string.IsNullOrWhiteSpace(period))
        {
            throw new ArgumentException("Periode wajib diisi dalam format MM/YYYY.", nameof(period));
        }

        if (!DateTime.TryParseExact($"01/{period}", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            throw new ArgumentException($"Periode '{period}' tidak valid. Gunakan format MM/YYYY.", nameof(period));
        }

        return new AccountingPeriod(parsed.Month, parsed.Year);
    }
}
