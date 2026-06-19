using System.Collections.Generic;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public sealed class PreviewJurnalImportUseCase
{
    private readonly IJurnalImportWorkbookReader _reader;

    public PreviewJurnalImportUseCase(IJurnalImportWorkbookReader reader)
    {
        _reader = reader;
    }

    public IReadOnlyList<string> GetSheets(string path)
    {
        return _reader.GetSheets(path);
    }

    public IReadOnlyList<JurnalImportRow> Preview(string path, string sheetName)
    {
        IReadOnlyList<JurnalImportRow> rows = _reader.ReadSheet(path, sheetName);
        JurnalImportValidationException.ThrowIfAny(JurnalImportTemplateValidator.ValidateRows(rows));
        return rows;
    }
}
