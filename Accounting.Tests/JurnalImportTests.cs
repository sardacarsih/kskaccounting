using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;
using Accounting.DataLayer;

namespace Accounting.Tests;

public sealed class JurnalImportTests
{
    [Fact]
    public void Validate_WrongColumnOrder_ReturnsOrderIssue()
    {
        string[] columns = JurnalImportTemplateValidator.RequiredColumns.ToArray();
        columns[1] = "Tanggal Salah";

        IReadOnlyList<JurnalImportValidationIssue> issues = JurnalImportTemplateValidator.Validate(columns);

        Assert.Single(issues);
        Assert.Equal("COLUMN_ORDER_MISMATCH", issues[0].Code);
    }

    [Fact]
    public void Preview_WhenNegativeAmount_DisablesImportThroughValidation()
    {
        FakeWorkbookReader reader = new(
            [
                new JurnalImportRow
                {
                    NoJurnal = "JRN-001",
                    Tanggal = new DateTime(2026, 5, 10),
                    Baris = 1,
                    Kode = "10.01",
                    Rekening = "Kas",
                    Debet = -1m,
                    Kredit = 0m,
                    Keterangan = "Test",
                    Posted = "True",
                    Periode = "05/2026"
                }
            ]);
        PreviewJurnalImportUseCase preview = new(reader);

        JurnalImportValidationException ex = Assert.Throws<JurnalImportValidationException>(
            () => preview.Preview("jurnal.xlsx", "Sheet1"));

        Assert.Equal("NEGATIVE_AMOUNT", ex.Issues[0].Code);
    }

    [Fact]
    public void Execute_WhenRowsValid_UsesScopedOrderAndRecalculatesAfterImport()
    {
        FakeDataStore dataStore = new();
        ExecuteJurnalImportUseCase useCase = new(dataStore);
        JurnalImportScope scope = CreateScope();
        IReadOnlyList<JurnalImportRow> rows = CreateBalancedRows();

        JurnalImportResult result = useCase.Execute(scope, rows);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ["GetLockStatus", "CountPeriod", "ClearStage", "StageRows", "FindRowsWithNullKode", "FindExistingJournalNumbers", "FindMissingAccounts", "ImportPartial", "RecalculateSaldo", "ClearStage"],
            dataStore.Calls);
        Assert.Equal(JurnalImportMode.AddOnly, dataStore.ImportScope?.Mode);
        Assert.Same(rows, dataStore.ImportRows);
    }

    [Fact]
    public void Execute_WhenReplacePeriod_SkipsExistingJournalValidationAndImports()
    {
        FakeDataStore dataStore = new();
        ExecuteJurnalImportUseCase useCase = new(dataStore);
        JurnalImportScope scope = CreateScope() with { Mode = JurnalImportMode.ReplacePeriod };

        JurnalImportResult result = useCase.Execute(scope, CreateBalancedRows());

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("FindExistingJournalNumbers", dataStore.Calls);
        Assert.Contains("ImportPartial", dataStore.Calls);
        Assert.Equal(JurnalImportMode.ReplacePeriod, dataStore.ImportScope?.Mode);
    }

    [Fact]
    public void Execute_WhenAddOnlyAndJournalExists_DoesNotImportOrRecalculateAndCleansUp()
    {
        FakeDataStore dataStore = new()
        {
            ExistingJournalIssues =
            [
                new JurnalImportValidationIssue("NOJURNAL_EXISTS", "Duplikasi NoJurnal.", "NOJURNAL", "JRN-001")
            ]
        };
        ExecuteJurnalImportUseCase useCase = new(dataStore);

        JurnalImportResult result = useCase.Execute(CreateScope(), CreateBalancedRows());

        Assert.False(result.IsSuccess);
        Assert.Equal("NOJURNAL_EXISTS", result.Issues[0].Code);
        Assert.DoesNotContain("ImportPartial", dataStore.Calls);
        Assert.DoesNotContain("RecalculateSaldo", dataStore.Calls);
        Assert.Equal("ClearStage", dataStore.Calls[^1]);
    }

    [Fact]
    public void Execute_WhenStageValidationFails_DoesNotImportOrRecalculateAndCleansUp()
    {
        FakeDataStore dataStore = new()
        {
            MissingAccountIssues =
            [
                new JurnalImportValidationIssue("ACCOUNT_NOT_FOUND", "Kode tidak terdaftar.", "ASAL", "10.01")
            ]
        };
        ExecuteJurnalImportUseCase useCase = new(dataStore);

        JurnalImportResult result = useCase.Execute(CreateScope(), CreateBalancedRows());

        Assert.False(result.IsSuccess);
        Assert.DoesNotContain("ImportPartial", dataStore.Calls);
        Assert.DoesNotContain("RecalculateSaldo", dataStore.Calls);
        Assert.Equal("ClearStage", dataStore.Calls[^1]);
    }

    [Fact]
    public void Execute_WhenPeriodMismatch_DoesNotStageRows()
    {
        FakeDataStore dataStore = new();
        ExecuteJurnalImportUseCase useCase = new(dataStore);
        JurnalImportScope scope = CreateScope() with { Period = "04/2026" };

        JurnalImportResult result = useCase.Execute(scope, [CreateRow()]);

        Assert.False(result.IsSuccess);
        Assert.Equal("PERIOD_MISMATCH", result.Issues[0].Code);
        Assert.Empty(dataStore.Calls);
    }

    [Fact]
    public void Execute_WhenJournalNotBalanced_ReturnsBalanceDetailsAndDoesNotStageRows()
    {
        FakeDataStore dataStore = new();
        ExecuteJurnalImportUseCase useCase = new(dataStore);

        JurnalImportResult result = useCase.Execute(CreateScope(), [CreateRow()]);

        Assert.False(result.IsSuccess);
        Assert.Equal(1, result.StatusCode);
        Assert.Equal("BALANCE_NOT_ZERO", result.Issues[0].Code);
        Assert.Single(result.BalanceIssues);
        Assert.Equal("JRN-001", result.BalanceIssues[0].NoJurnal);
        Assert.Equal(100m, result.BalanceIssues[0].Selisih);
        Assert.Empty(dataStore.Calls);
    }

    [Fact]
    public void Execute_WhenRecalculateFailsAfterImport_ReturnsSuccessWithWarning()
    {
        FakeDataStore dataStore = new()
        {
            RecalculateException = new InvalidOperationException("ORA-01008: not all variables bound")
        };
        ExecuteJurnalImportUseCase useCase = new(dataStore);

        JurnalImportResult result = useCase.Execute(CreateScope(), CreateBalancedRows());

        Assert.True(result.IsSuccess);
        Assert.True(result.HasRecalculationWarning);
        Assert.Contains("rekalkulasi saldo gagal", result.RecalculationWarning);
        Assert.Contains("ORA-01008", result.RecalculationWarning);
        Assert.Contains("ClearStage", dataStore.Calls);
    }

    [Fact]
    public void Execute_WhenImportPartialFails_ThrowsAndDoesNotReturnSuccessWarning()
    {
        FakeDataStore dataStore = new()
        {
            ImportException = new InvalidOperationException("insert failed")
        };
        ExecuteJurnalImportUseCase useCase = new(dataStore);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => useCase.Execute(CreateScope(), CreateBalancedRows()));

        Assert.Contains("insert failed", ex.Message);
        Assert.DoesNotContain("RecalculateSaldo", dataStore.Calls);
        Assert.Equal("ClearStage", dataStore.Calls[^1]);
    }

    [Fact]
    public void BuildJurnalDetailId_UsesDelimiterAndPadding_ToAvoidConcatenationCollision()
    {
        string first = JurnalRepository.BuildJurnalDetailId(123, 11);
        string second = JurnalRepository.BuildJurnalDetailId(1231, 1);

        Assert.Equal("123:000011", first);
        Assert.Equal("1231:000001", second);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Execute_WhenProgressProvided_ReportsValidationImportAndCompletion()
    {
        FakeDataStore dataStore = new();
        ExecuteJurnalImportUseCase useCase = new(dataStore);
        List<JurnalImportProgress> progressEvents = [];
        TestProgress progress = new(progressEvents);

        JurnalImportResult result = useCase.Execute(CreateScope(), CreateBalancedRows(), progress);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(progressEvents);
        Assert.Contains(progressEvents, p => p.Percent == 0 && p.Stage.Contains("Menyiapkan"));
        Assert.Contains(progressEvents, p => p.Percent == 5 && p.Stage.Contains("validasi"));
        Assert.Contains(progressEvents, p => p.Percent == 40 && p.Stage.Contains("Menyimpan"));
        Assert.Equal(100, progressEvents[^1].Percent);
    }

    [Fact]
    public void Execute_WhenRecalculateFails_ReportsProgressUpToRecalculationStage()
    {
        FakeDataStore dataStore = new()
        {
            RecalculateException = new InvalidOperationException("recalc error")
        };
        ExecuteJurnalImportUseCase useCase = new(dataStore);
        List<JurnalImportProgress> progressEvents = [];
        TestProgress progress = new(progressEvents);

        JurnalImportResult result = useCase.Execute(CreateScope(), CreateBalancedRows(), progress);

        Assert.True(result.IsSuccess);
        Assert.True(result.HasRecalculationWarning);
        Assert.Contains(progressEvents, p => p.Percent == 95 && p.Stage.Contains("ulang"));
        Assert.DoesNotContain(progressEvents, p => p.Percent == 100);
    }

    private sealed class TestProgress : IProgress<JurnalImportProgress>
    {
        private readonly List<JurnalImportProgress> _events;
        public TestProgress(List<JurnalImportProgress> events) { _events = events; }
        public void Report(JurnalImportProgress value) { _events.Add(value); }
    }

    private static JurnalImportScope CreateScope()
    {
        return new JurnalImportScope("EST1", "USER1", 5, 2026, 2026, "05/2026", JurnalImportSource.Excel, JurnalImportMode.AddOnly);
    }

    private static JurnalImportRow CreateRow()
    {
        return new JurnalImportRow
        {
            NoJurnal = "JRN-001",
            Tanggal = new DateTime(2026, 5, 10),
            Baris = 1,
            Kode = "10.01",
            Rekening = "Kas",
            Debet = 100m,
            Kredit = 0m,
            Keterangan = "Test",
            Posted = "True",
            Periode = "05/2026"
        };
    }

    private static IReadOnlyList<JurnalImportRow> CreateBalancedRows()
    {
        return
        [
            CreateRow(),
            new JurnalImportRow
            {
                NoJurnal = "JRN-001",
                Tanggal = new DateTime(2026, 5, 10),
                Baris = 2,
                Kode = "20.01",
                Rekening = "Hutang",
                Debet = 0m,
                Kredit = 100m,
                Keterangan = "Test",
                Posted = "True",
                Periode = "05/2026"
            }
        ];
    }

    private sealed class FakeWorkbookReader : IJurnalImportWorkbookReader
    {
        private readonly IReadOnlyList<JurnalImportRow> _rows;

        public FakeWorkbookReader(IReadOnlyList<JurnalImportRow> rows)
        {
            _rows = rows;
        }

        public IReadOnlyList<string> GetSheets(string path)
        {
            return ["Sheet1"];
        }

        public IReadOnlyList<JurnalImportRow> ReadSheet(string path, string sheetName)
        {
            return _rows;
        }
    }

    private sealed class FakeDataStore : IJurnalImportDataStore
    {
        public List<string> Calls { get; } = [];
        public IReadOnlyList<JurnalImportValidationIssue> MissingAccountIssues { get; init; } = [];
        public IReadOnlyList<JurnalImportValidationIssue> ExistingJournalIssues { get; init; } = [];
        public Exception? ImportException { get; init; }
        public Exception? RecalculateException { get; init; }
        public JurnalImportScope? ImportScope { get; private set; }
        public IReadOnlyList<JurnalImportRow>? ImportRows { get; private set; }

        public string GetLockStatus(string idData, string period)
        {
            Calls.Add("GetLockStatus");
            return "N";
        }

        public int CountPeriod(string idData, string period)
        {
            Calls.Add("CountPeriod");
            return 1;
        }

        public void CreateNextPeriod(string idData, int previousMonth, int year)
        {
            Calls.Add("CreateNextPeriod");
        }

        public void ClearStage(JurnalImportScope scope)
        {
            Calls.Add("ClearStage");
        }

        public void StageRows(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows)
        {
            Calls.Add("StageRows");
        }

        public IReadOnlyList<JurnalImportValidationIssue> FindRowsWithNullKode(JurnalImportScope scope)
        {
            Calls.Add("FindRowsWithNullKode");
            return [];
        }

        public IReadOnlyList<JurnalImportValidationIssue> FindExistingJournalNumbers(JurnalImportScope scope)
        {
            Calls.Add("FindExistingJournalNumbers");
            return ExistingJournalIssues;
        }

        public IReadOnlyList<JurnalImportValidationIssue> FindMissingAccounts(JurnalImportScope scope)
        {
            Calls.Add("FindMissingAccounts");
            return MissingAccountIssues;
        }

        public int ImportPartial(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows, IProgress<JurnalImportProgress>? progress)
        {
            Calls.Add("ImportPartial");
            if (ImportException != null)
            {
                throw ImportException;
            }

            ImportScope = scope;
            ImportRows = rows;
            return 99;
        }

        public void RecalculateSaldo(JurnalImportScope scope)
        {
            Calls.Add("RecalculateSaldo");
            if (RecalculateException != null)
            {
                throw RecalculateException;
            }
        }
    }
}
