using System;
using System.Collections.Generic;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public interface IJurnalImportDataStore
{
    string GetLockStatus(string idData, string period);
    int CountPeriod(string idData, string period);
    void CreateNextPeriod(string idData, int previousMonth, int year);
    void ClearStage(JurnalImportScope scope);
    void StageRows(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows);
    IReadOnlyList<JurnalImportValidationIssue> FindRowsWithNullKode(JurnalImportScope scope);
    IReadOnlyList<JurnalImportValidationIssue> FindExistingJournalNumbers(JurnalImportScope scope);
    IReadOnlyList<JurnalImportValidationIssue> FindMissingAccounts(JurnalImportScope scope);
    int ImportPartial(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows, IProgress<JurnalImportProgress>? progress);
    JurnalImportRecalcQueueResult QueueRecalculation(JurnalImportScope scope);
}
