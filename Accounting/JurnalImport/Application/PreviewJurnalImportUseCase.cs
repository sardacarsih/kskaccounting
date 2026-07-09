using System.Collections.Generic;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public sealed class PreviewJurnalImportUseCase
{
    private const string MissingAccountMarker = "*** Kode Tidak Terdaftar ***";

    private readonly IJurnalImportWorkbookReader _reader;
    private readonly IJurnalImportDataStore _dataStore;

    public PreviewJurnalImportUseCase(IJurnalImportWorkbookReader reader, IJurnalImportDataStore dataStore)
    {
        _reader = reader;
        _dataStore = dataStore;
    }

    public IReadOnlyList<string> GetSheets(string path)
    {
        return _reader.GetSheets(path);
    }

    public IReadOnlyList<JurnalImportRow> Preview(string path, string sheetName, string idData, int coaYear)
    {
        IReadOnlyList<JurnalImportRow> rows = _reader.ReadSheet(path, sheetName);
        JurnalImportValidationException.ThrowIfAny(JurnalImportTemplateValidator.ValidateRows(rows));
        ApplyAccountNames(rows, idData, coaYear);
        return rows;
    }

    private void ApplyAccountNames(IReadOnlyList<JurnalImportRow> rows, string idData, int coaYear)
    {
        IReadOnlyDictionary<string, string> accountNames = _dataStore.GetAccountNames(idData, coaYear);
        foreach (JurnalImportRow row in rows)
        {
            string kode = row.Kode.Trim();
            row.Rekening = kode.Length > 0 && accountNames.TryGetValue(kode, out string? name)
                ? name
                : MissingAccountMarker;
        }
    }
}
