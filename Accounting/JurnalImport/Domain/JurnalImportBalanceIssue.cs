using System;

namespace Accounting.JurnalImport.Domain;

public sealed record JurnalImportBalanceIssue(
    string NoJurnal,
    DateTime Tanggal,
    decimal Debet,
    decimal Kredit)
{
    public decimal Selisih => Debet - Kredit;
}
