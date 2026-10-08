using System;
using System.Collections.Generic;

namespace Accounting.Form
{
    internal readonly record struct CoaPeriod
    {
        private static readonly IReadOnlyList<string> MonthNames = Array.AsReadOnly(
            new[]
            {
                "Januari", "Februari", "Maret", "April", "Mei", "Juni",
                "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember"
            });

        internal CoaPeriod(int year, int month)
        {
            if (year is < 1 or > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Tahun periode harus berada di antara 1 dan 9999.");
            }

            if (month is < 1 or > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month), month, "Bulan periode harus berada di antara 1 dan 12.");
            }

            Year = year;
            Month = month;
        }

        internal int Year { get; }

        internal int Month { get; }

        internal int Value => checked((Year * 100) + Month);

        internal string MonthName => MonthNames[Month - 1];

        internal static IReadOnlyList<string> IndonesianMonthNames => MonthNames;

        internal static CoaPeriod Parse(int value, string parameterName)
        {
            int year = value / 100;
            int month = value % 100;

            try
            {
                return new CoaPeriod(year, month);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    $"Periode harus menggunakan format yyyyMM dengan bulan 01 sampai 12. {ex.Message}");
            }
        }

        internal CoaPeriod AddMonths(int months)
        {
            DateTime firstDay = new(Year, Month, 1);
            DateTime result = firstDay.AddMonths(months);
            return new CoaPeriod(result.Year, result.Month);
        }
    }

    internal sealed class CoaPeriodNavigator
    {
        internal CoaPeriodNavigator(int minimumValue, int maximumValue)
        {
            Minimum = CoaPeriod.Parse(minimumValue, nameof(minimumValue));
            Maximum = CoaPeriod.Parse(maximumValue, nameof(maximumValue));

            if (Minimum.Value > Maximum.Value)
            {
                throw new ArgumentException("Periode minimum tidak boleh melebihi periode maksimum.", nameof(minimumValue));
            }

            Current = Maximum;
        }

        internal CoaPeriod Minimum { get; }

        internal CoaPeriod Maximum { get; }

        internal CoaPeriod Current { get; private set; }

        internal bool CanMovePrevious => Current.Value > Minimum.Value;

        internal bool CanMoveNext => Current.Value < Maximum.Value;

        internal bool Contains(CoaPeriod period)
        {
            return period.Value >= Minimum.Value && period.Value <= Maximum.Value;
        }

        internal bool TrySetCurrent(CoaPeriod period, out bool changed)
        {
            changed = false;
            if (!Contains(period))
            {
                return false;
            }

            if (period == Current)
            {
                return true;
            }

            Current = period;
            changed = true;
            return true;
        }

        internal bool TryMoveByMonths(int months)
        {
            CoaPeriod candidate = Current.AddMonths(months);
            return TrySetCurrent(candidate, out bool changed) && changed;
        }
    }
}
