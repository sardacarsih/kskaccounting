namespace Accounting.JurnalImport.Domain;

public sealed record JurnalImportProgress(
    int Percent,
    string Stage,
    int ProcessedRows,
    int TotalRows,
    bool UseRowCount = false);
