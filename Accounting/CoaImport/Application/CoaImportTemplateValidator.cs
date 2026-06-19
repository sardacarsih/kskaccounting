using System;
using System.Collections.Generic;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public static class CoaImportTemplateValidator
{
    public static readonly string[] PusatColumns =
    [
        "Account", "Nama Perkiraan", "Jenis", "Level", "Induk", "Gen", "Saldo Normal",
        "Awal Tahun", "Saldo Awal", "Debet", "Kredit", "Mutasi", "Saldo Akhir"
    ];

    public static readonly string[] KebunColumns =
    [
        "Account", "Nama Perkiraan", "Jenis", "Level", "Induk", "Gen", "Saldo Normal",
        "Awal Tahun", "Saldo Awal", "Debet", "Kredit", "Mutasi", "Saldo Akhir",
        "Divisi", "Blok", "TahunTanam"
    ];

    public static IReadOnlyList<CoaImportValidationIssue> Validate(IReadOnlyList<string> actualColumns, CoaImportKind kind)
    {
        string[] expectedColumns = kind == CoaImportKind.Kebun ? KebunColumns : PusatColumns;

        if (actualColumns.Count == KebunColumns.Length && expectedColumns.Length == PusatColumns.Length)
        {
            return [new CoaImportValidationIssue("WRONG_ACCOUNTING_KIND", "Akun Kebun tidak dapat digunakan di pusat.")];
        }

        if (actualColumns.Count == PusatColumns.Length && expectedColumns.Length == KebunColumns.Length)
        {
            return [new CoaImportValidationIssue("WRONG_ACCOUNTING_KIND", "Akun Pusat tidak dapat digunakan di kebun.")];
        }

        if (actualColumns.Count != expectedColumns.Length)
        {
            return
            [
                new CoaImportValidationIssue(
                    "COLUMN_COUNT_MISMATCH",
                    $"Jumlah kolom seharusnya {expectedColumns.Length}, jumlah kolom aktual {actualColumns.Count}.")
            ];
        }

        for (int i = 0; i < expectedColumns.Length; i++)
        {
            if (!string.Equals(actualColumns[i], expectedColumns[i], StringComparison.Ordinal))
            {
                return
                [
                    new CoaImportValidationIssue(
                        "COLUMN_ORDER_MISMATCH",
                        $"Urutan nama kolom seharusnya {expectedColumns[i]}, nama kolom aktual {actualColumns[i]}.",
                        expectedColumns[i],
                        actualColumns[i])
                ];
            }
        }

        return [];
    }
}
