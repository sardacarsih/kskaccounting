using Accounting.CoaImport.Application;
using Accounting.CoaImport.Domain;
using Accounting.CoaImport.Presentation;

namespace Accounting.Tests;

public sealed class CoaImportTests
{
    [Fact]
    public void Validate_PusatTemplateForPusat_ReturnsNoIssues()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportTemplateValidator.Validate(
            CoaImportTemplateValidator.PusatColumns,
            CoaImportKind.Pusat);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_KebunTemplateForKebun_ReturnsNoIssues()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportTemplateValidator.Validate(
            CoaImportTemplateValidator.KebunColumns,
            CoaImportKind.Kebun);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_KebunTemplateForPusat_ReturnsWrongKindIssue()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportTemplateValidator.Validate(
            CoaImportTemplateValidator.KebunColumns,
            CoaImportKind.Pusat);

        Assert.Single(issues);
        Assert.Equal("WRONG_ACCOUNTING_KIND", issues[0].Code);
    }

    [Fact]
    public void Validate_WrongColumnOrder_ReturnsOrderIssue()
    {
        string[] columns = CoaImportTemplateValidator.PusatColumns.ToArray();
        columns[1] = "Nama Salah";

        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportTemplateValidator.Validate(columns, CoaImportKind.Pusat);

        Assert.Single(issues);
        Assert.Equal("COLUMN_ORDER_MISMATCH", issues[0].Code);
    }

    [Fact]
    public void PreviewSheet_WhenInvalidTemplate_ClearsRowsAndDisablesImport()
    {
        FakeWorkbookReader reader = new()
        {
            IssuesToThrow =
            [
                new CoaImportValidationIssue("COLUMN_COUNT_MISMATCH", "Jumlah kolom salah.")
            ]
        };
        FrmImportCoaViewModel viewModel = CreateViewModel(reader, new FakeCoaImportRepository());
        viewModel.LoadWorkbook("coa.xlsx");

        viewModel.PreviewSheet("Sheet1");

        Assert.False(viewModel.CanImport);
        Assert.Empty(viewModel.Rows);
        Assert.Equal("COLUMN_COUNT_MISMATCH", viewModel.LastIssues[0].Code);
    }

    [Fact]
    public void Import_PartialMode_StillRunsClientValidation()
    {
        FakeCoaImportRepository repository = new()
        {
            ValidationIssues =
            [
                new CoaImportValidationIssue("PARENT_NOT_FOUND", "Induk tidak ditemukan.", "INDUK", "10.01")
            ]
        };
        FrmImportCoaViewModel viewModel = CreateViewModel(new FakeWorkbookReader(), repository);
        viewModel.LoadWorkbook("coa.xlsx");
        viewModel.PreviewSheet("Sheet1");
        viewModel.Mode = CoaImportMode.Partial;

        CoaImportResult result = viewModel.Import();

        Assert.False(result.IsSuccess);
        Assert.Contains("ValidateRows", repository.Calls);
        Assert.DoesNotContain("ImportRows:Partial", repository.Calls);
    }

    [Fact]
    public void Import_WhenImportRowsReturnsFailure_DoesNotReportSuccessOrRecalculate()
    {
        FakeCoaImportRepository repository = new() { ImportStatusCode = 0 };
        FrmImportCoaViewModel viewModel = CreateViewModel(new FakeWorkbookReader(), repository);
        viewModel.LoadWorkbook("coa.xlsx");
        viewModel.PreviewSheet("Sheet1");

        CoaImportResult result = viewModel.Import();

        Assert.False(result.IsSuccess);
        Assert.Equal(0, result.StatusCode);
        Assert.DoesNotContain("RecalculateSaldo", repository.Calls);
    }

    [Fact]
    public void Execute_FullMode_ValidatesImportsAndRecalculatesAfterSuccess()
    {
        FakeCoaImportRepository repository = new();
        ImportCoaAppService service = new(repository);
        CoaImportScope scope = new("EST1", 2026, "USER1", "batch-001", CoaImportKind.Kebun);

        CoaImportResult result = service.Execute(scope, CoaImportMode.Full, [CreateRow()]);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ["ValidateRows", "ImportRows:Full", "EnsurePeriodExists", "RecalculateSaldo"],
            repository.Calls);
        Assert.Equal(scope, repository.LastScope);
    }

    [Fact]
    public void Execute_WhenProgressProvided_ReportsValidationImportAndCompletion()
    {
        FakeCoaImportRepository repository = new();
        ImportCoaAppService service = new(repository);
        CoaImportScope scope = new("EST1", 2026, "USER1", "batch-001", CoaImportKind.Kebun);
        List<CoaImportProgress> progressEvents = [];
        TestProgress progress = new(progressEvents);

        CoaImportResult result = service.Execute(scope, CoaImportMode.Full, [CreateRow()], progress);

        Assert.True(result.IsSuccess);
        Assert.Contains(progressEvents, item => item.Stage.Contains("Memvalidasi", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(progressEvents, item => item.Stage.Contains("Mengimport", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(100, progressEvents[^1].Percent);
    }

    [Fact]
    public void Execute_WhenValidationFails_ReportsValidationAndSkipsImportProgress()
    {
        FakeCoaImportRepository repository = new()
        {
            ValidationIssues =
            [
                new CoaImportValidationIssue("PARENT_NOT_FOUND", "Induk tidak ditemukan.", "INDUK", "10.01")
            ]
        };
        ImportCoaAppService service = new(repository);
        CoaImportScope scope = new("EST1", 2026, "USER1", "batch-001", CoaImportKind.Kebun);
        List<CoaImportProgress> progressEvents = [];
        TestProgress progress = new(progressEvents);

        CoaImportResult result = service.Execute(scope, CoaImportMode.Full, [CreateRow()], progress);

        Assert.False(result.IsSuccess);
        Assert.Contains(progressEvents, item => item.Stage.Contains("Memvalidasi", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(progressEvents, item => item.Stage.Contains("Mengimport", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("RecalculateSaldo", repository.Calls);
    }

    [Fact]
    public void RowValidator_WhenDuplicateAccount_ReturnsDuplicateIssue()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [CreateRow(account: "10.01"), CreateRow(account: "10.01")],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Contains(issues, issue => issue.Code == "DUPLICATE_ACCOUNT" && issue.Value == "10.01");
    }

    [Fact]
    public void RowValidator_WhenSelfParent_ReturnsSelfParentIssue()
    {
        CoaImportRow row = CreateRow(parent: "10.01");

        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [row],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Contains(issues, issue => issue.Code == "SELF_PARENT" && issue.Value == "10.01");
    }

    [Fact]
    public void RowValidator_WhenDetailWithoutParent_ReturnsDetailIssue()
    {
        CoaImportRow row = CreateRow(parent: string.Empty, gen: "D");

        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [row],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Contains(issues, issue => issue.Code == "DETAIL_WITHOUT_PARENT" && issue.Value == "10.01");
    }

    [Fact]
    public void RowValidator_WhenParentMissingFromFileAndDatabase_ReturnsParentIssue()
    {
        CoaImportRow row = CreateRow(parent: "10.00");

        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [row],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.Contains(issues, issue => issue.Code == "PARENT_NOT_FOUND" && issue.Value == "10.00");
    }

    [Fact]
    public void RowValidator_WhenParentExistsInFile_ReturnsNoParentIssue()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [CreateRow(account: "10.00", gen: "G"), CreateRow(account: "10.01", parent: "10.00")],
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        Assert.DoesNotContain(issues, issue => issue.Code == "PARENT_NOT_FOUND");
    }

    [Fact]
    public void RowValidator_WhenParentExistsInDatabase_ReturnsNoParentIssue()
    {
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportRowValidator.Validate(
            [CreateRow(parent: "10.00")],
            new HashSet<string>(["10.00"], StringComparer.OrdinalIgnoreCase));

        Assert.Empty(issues);
    }

    private static FrmImportCoaViewModel CreateViewModel(FakeWorkbookReader reader, FakeCoaImportRepository repository)
    {
        ImportCoaAppService appService = new(repository);
        return new FrmImportCoaViewModel(
            new PreviewCoaImportUseCase(reader),
            new ExecuteCoaImportUseCase(appService),
            "EST1",
            "USER1",
            CoaImportKind.Pusat);
    }

    private static CoaImportRow CreateRow(string account = "10.01", string parent = "", string gen = "D")
    {
        return new CoaImportRow
        {
            Account = account,
            NamaPerkiraan = "Kas",
            Jenis = "A",
            Level = "1",
            Induk = parent,
            Gen = gen,
            Posisi = "D",
            AwalTahun = 100m
        };
    }

    private sealed class FakeWorkbookReader : ICoaImportWorkbookReader
    {
        public IReadOnlyList<CoaImportValidationIssue> IssuesToThrow { get; init; } = [];

        public IReadOnlyList<string> GetSheets(string path)
        {
            return ["Sheet1"];
        }

        public IReadOnlyList<CoaImportRow> ReadSheet(string path, string sheetName, CoaImportKind kind)
        {
            if (IssuesToThrow.Count > 0)
            {
                throw new CoaImportValidationException(IssuesToThrow);
            }

            return [CreateRow()];
        }
    }

    private sealed class TestProgress : IProgress<CoaImportProgress>
    {
        private readonly List<CoaImportProgress> _events;

        public TestProgress(List<CoaImportProgress> events)
        {
            _events = events;
        }

        public void Report(CoaImportProgress value)
        {
            _events.Add(value);
        }
    }

    private sealed class FakeCoaImportRepository : ICoaImportRepository
    {
        public List<string> Calls { get; } = [];
        public CoaImportScope? LastScope { get; private set; }
        public IReadOnlyList<CoaImportValidationIssue> ValidationIssues { get; init; } = [];
        public int ImportStatusCode { get; init; } = 1;

        public IReadOnlyList<CoaImportValidationIssue> ValidateRows(
            CoaImportScope scope,
            IReadOnlyList<CoaImportRow> rows,
            IProgress<CoaImportProgress>? progress = null)
        {
            LastScope = scope;
            Calls.Add("ValidateRows");
            progress?.Report(new CoaImportProgress(8, "Validasi fake repository selesai.", 0, rows.Count));
            return ValidationIssues;
        }

        public int ImportRows(
            CoaImportScope scope,
            CoaImportMode mode,
            IReadOnlyList<CoaImportRow> rows,
            IProgress<CoaImportProgress>? progress = null)
        {
            LastScope = scope;
            Calls.Add($"ImportRows:{mode}");
            progress?.Report(new CoaImportProgress(85, $"Mengimport COA {rows.Count:N0} / {rows.Count:N0}...", rows.Count, rows.Count));
            return ImportStatusCode;
        }

        public void EnsurePeriodExists(string idData, int year)
        {
            Calls.Add("EnsurePeriodExists");
        }

        public void RecalculateSaldo(string idData, int year, string userId)
        {
            Calls.Add("RecalculateSaldo");
        }
    }
}
