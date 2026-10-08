using Accounting.BusinessLayer;
using Accounting.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.Services
{
    public static class NeracaSaldoReportService
    {
        public static async Task<IReadOnlyList<NeracaSaldoRow>> LoadRowsAsync(
            string idData,
            int year,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(idData);
            if (year < 1 || year > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Tahun Neraca Saldo tidak valid.");
            }

            try
            {
                IReadOnlyList<NeracaSaldoRow> rows = await LaporanServices
                    .GetNeracaSaldoRowsAsync(idData, year, cancellationToken)
                    .ConfigureAwait(false);

                return IncludeNonZeroRowsAndAncestors(rows);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "NeracaSaldo load_rows_failed iddata={IdData} tahun={Year}",
                    idData,
                    year);
                throw;
            }
        }

        public static IReadOnlyList<NeracaSaldoRow> IncludeNonZeroRowsAndAncestors(
            IEnumerable<NeracaSaldoRow> rows)
        {
            ArgumentNullException.ThrowIfNull(rows);

            List<NeracaSaldoRow> sourceRows = rows.ToList();
            Dictionary<string, NeracaSaldoRow> rowsByCode = new(StringComparer.OrdinalIgnoreCase);
            foreach (NeracaSaldoRow row in sourceRows)
            {
                if (!string.IsNullOrWhiteSpace(row.KodeAkun))
                {
                    rowsByCode.TryAdd(row.KodeAkun, row);
                }
            }

            HashSet<string> includedCodes = new(StringComparer.OrdinalIgnoreCase);
            foreach (NeracaSaldoRow seed in sourceRows.Where(row => row.HasAnyBalance))
            {
                NeracaSaldoRow? current = seed;
                HashSet<string> visitedCodes = new(StringComparer.OrdinalIgnoreCase);

                while (current != null && !string.IsNullOrWhiteSpace(current.KodeAkun))
                {
                    if (!visitedCodes.Add(current.KodeAkun))
                    {
                        break;
                    }

                    includedCodes.Add(current.KodeAkun);
                    if (string.IsNullOrWhiteSpace(current.ParentAkun) ||
                        !rowsByCode.TryGetValue(current.ParentAkun, out current))
                    {
                        break;
                    }
                }
            }

            return sourceRows
                .Where(row => includedCodes.Contains(row.KodeAkun))
                .ToList();
        }
    }
}
