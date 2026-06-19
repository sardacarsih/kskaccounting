using System;
using System.Collections.Generic;
using System.Linq;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public static class JurnalImportTemplateValidator
{
    public static readonly string[] RequiredColumns =
    [
        "NoJurnal",
        "Tanggal",
        "RowNo",
        "Kode",
        "Rekening",
        "Debet",
        "Kredit",
        "Keterangan",
        "Posted",
        "Periode"
    ];

    public static IReadOnlyList<JurnalImportValidationIssue> Validate(IReadOnlyList<string> columns)
    {
        if (columns.Count != RequiredColumns.Length)
        {
            return
            [
                new JurnalImportValidationIssue(
                    "COLUMN_COUNT_MISMATCH",
                    $"Jumlah Kolom Seharusnya: {RequiredColumns.Length}, Jumlah Kolom Aktual : {columns.Count}")
            ];
        }

        for (int i = 0; i < RequiredColumns.Length; i++)
        {
            if (!string.Equals(columns[i], RequiredColumns[i], StringComparison.Ordinal))
            {
                return
                [
                    new JurnalImportValidationIssue(
                        "COLUMN_ORDER_MISMATCH",
                        $"Urutan Nama Kolom Seharusnya: {RequiredColumns[i]}, Nama Kolom Aktual: {columns[i]}",
                        RequiredColumns[i],
                        columns[i])
                ];
            }
        }

        return [];
    }

    public static IReadOnlyList<JurnalImportValidationIssue> ValidateRows(IReadOnlyList<JurnalImportRow> rows)
    {
        List<JurnalImportValidationIssue> issues = [];

        issues.AddRange(rows
            .Where(row => row.Debet < 0 || row.Kredit < 0)
            .Select(row => new JurnalImportValidationIssue(
                "NEGATIVE_AMOUNT",
                "Nilai Transaksi tidak boleh minus",
                "NoJurnal",
                row.NoJurnal)));

        issues.AddRange(rows
            .Where(row => row.Baris < 0)
            .Select(row => new JurnalImportValidationIssue(
                "INVALID_ROW_NUMBER",
                "Tentukan Nomor Baris dengan benar pada daftar nomor jurnal berikut",
                "RowNo",
                row.Baris.ToString())));

        issues.AddRange(rows
            .GroupBy(row => new { row.NoJurnal, row.Tanggal, row.Baris })
            .Where(group => group.Count() > 1)
            .Select(group => new JurnalImportValidationIssue(
                "DUPLICATE_ROW_NUMBER",
                "Double Nomor urut pada nomor jurnal",
                "NoJurnal",
                group.Key.NoJurnal)));

        return issues;
    }

    public static bool TryGetSinglePeriod(IReadOnlyList<JurnalImportRow> rows, out string period, out JurnalImportValidationIssue? issue)
    {
        period = string.Empty;
        issue = null;

        List<string> periods = rows
            .Select(row => row.Periode?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()!;

        if (periods.Count == 0)
        {
            issue = new JurnalImportValidationIssue("PERIOD_REQUIRED", "Kolom Periode wajib diisi.", "Periode");
            return false;
        }

        if (periods.Count > 1)
        {
            issue = new JurnalImportValidationIssue(
                "PERIOD_MULTIPLE_VALUES",
                "Ditemukan lebih dari satu nilai periode pada file import: " + string.Join(", ", periods),
                "Periode");
            return false;
        }

        period = periods[0];
        return true;
    }
}
