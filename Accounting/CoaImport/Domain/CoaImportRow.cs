namespace Accounting.CoaImport.Domain;

public sealed class CoaImportRow
{
    public string Account { get; set; } = string.Empty;
    public string NamaPerkiraan { get; set; } = string.Empty;
    public string Jenis { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Induk { get; set; } = string.Empty;
    public string Gen { get; set; } = string.Empty;
    public string Posisi { get; set; } = string.Empty;
    public decimal AwalTahun { get; set; }
    public string Blok { get; set; } = string.Empty;
    public string Divisi { get; set; } = string.Empty;
    public string TahunTanam { get; set; } = string.Empty;
}
