using System;

namespace Accounting.JurnalImport.Domain;

public sealed class JurnalImportRow
{
    public string NoJurnal { get; init; } = string.Empty;
    public DateTime Tanggal { get; init; }
    public int Baris { get; init; }
    public string Kode { get; init; } = string.Empty;
    public string Rekening { get; init; } = string.Empty;
    public decimal Debet { get; init; }
    public decimal Kredit { get; init; }
    public string Keterangan { get; init; } = string.Empty;
    public string Posted { get; init; } = "True";
    public string Periode { get; init; } = string.Empty;
}
