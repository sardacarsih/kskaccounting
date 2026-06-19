namespace Accounting.CompanyMaster.Domain;

public sealed class CompanyMasterRecord
{
    public string IDPT { get; set; } = string.Empty;
    public string NAMAPT { get; set; } = string.Empty;
    public string IDGROUP { get; set; } = string.Empty;
    public string LookupText => string.IsNullOrWhiteSpace(IDPT) ? NAMAPT : $"{IDPT} - {NAMAPT}";
}
