using System.Collections.Generic;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public interface ICoaImportWorkbookReader
{
    IReadOnlyList<string> GetSheets(string path);
    IReadOnlyList<CoaImportRow> ReadSheet(string path, string sheetName, CoaImportKind kind);
}
