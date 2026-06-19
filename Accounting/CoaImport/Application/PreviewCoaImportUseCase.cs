using System.Collections.Generic;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public sealed class PreviewCoaImportUseCase
{
    private readonly ICoaImportWorkbookReader _reader;

    public PreviewCoaImportUseCase(ICoaImportWorkbookReader reader)
    {
        _reader = reader;
    }

    public IReadOnlyList<string> GetSheets(string path)
    {
        return _reader.GetSheets(path);
    }

    public IReadOnlyList<CoaImportRow> Preview(string path, string sheetName, CoaImportKind kind)
    {
        return _reader.ReadSheet(path, sheetName, kind);
    }
}
