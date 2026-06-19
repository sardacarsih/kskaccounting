namespace Accounting.CoaImport.Domain;

public sealed record CoaImportProgress(
    int Percent,
    string Stage,
    int ProcessedRows,
    int TotalRows,
    bool UseRowCount = false);
