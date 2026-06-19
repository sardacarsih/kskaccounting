using System.Collections.Generic;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public interface IJurnalImportWorkbookReader
{
    IReadOnlyList<string> GetSheets(string path);
    IReadOnlyList<JurnalImportRow> ReadSheet(string path, string sheetName);
}
