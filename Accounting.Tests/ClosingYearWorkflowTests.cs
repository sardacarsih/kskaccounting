using Accounting.ClosingYear;

namespace Accounting.Tests;

public sealed class ClosingYearWorkflowTests
{
    [Fact]
    public void Constructor_WhenDependenciesNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ClosingYearWorkflow(null!));
    }

    [Fact]
    public void Execute_WhenCoaMissing_ReturnsMissingCoaAndDoesNotCloseYear()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            IsCoaMissingResult = true
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        ClosingYearWorkflowResult result = workflow.Execute(CreateRequest());

        Assert.Equal(ClosingYearWorkflowStatus.MissingCoa, result.Status);
        Assert.Equal("12/2025", result.Period);
        Assert.Equal("Desember - 2025", result.MonthDisplay);
        Assert.Equal(0, dependencies.CloseYearCallCount);
    }

    [Fact]
    public void Execute_WhenCoaValidationErrorsExist_ReturnsCoaValidationErrors()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            HasCoaValidationErrorsResult = true
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        ClosingYearWorkflowResult result = workflow.Execute(CreateRequest());

        Assert.Equal(ClosingYearWorkflowStatus.CoaValidationErrors, result.Status);
        Assert.Equal(12, result.Month);
        Assert.Equal(2025, result.Year);
        Assert.Equal(0, dependencies.CloseYearCallCount);
    }

    [Fact]
    public void Execute_WhenCloseYearReturnsLocked_ReturnsLockedPeriod()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            CloseYearResult = ClosingYearResult.LockedPeriod(2025, "12/2025")
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        ClosingYearWorkflowResult result = workflow.Execute(CreateRequest());

        Assert.Equal(ClosingYearWorkflowStatus.LockedPeriod, result.Status);
        Assert.Contains("Dikunci", result.Message);
        Assert.Equal(0, dependencies.RefreshMaxYearCallCount);
    }

    [Fact]
    public void Execute_WhenCloseYearReturnsNotBalanced_ReturnsNotBalanced()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            CloseYearResult = ClosingYearResult.NotBalanced(2025, "12/2025", 125.50m)
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        ClosingYearWorkflowResult result = workflow.Execute(CreateRequest());

        Assert.Equal(ClosingYearWorkflowStatus.NotBalanced, result.Status);
        Assert.Equal(125.50m, result.ClosingResult!.Selisih);
        Assert.Equal(0, dependencies.RefreshMaxYearCallCount);
    }

    [Fact]
    public void Execute_WhenCloseYearSucceeds_RefreshesMaxYearAndReturnsSuccess()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            CloseYearResult = ClosingYearResult.Success(2025, 2026, "12/2025", "CREATE", 1000m),
            RefreshedMaxYear = 2026
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        ClosingYearWorkflowResult result = workflow.Execute(CreateRequest());

        Assert.Equal(ClosingYearWorkflowStatus.Success, result.Status);
        Assert.Equal(2026, result.RefreshedMaxYear);
        Assert.Equal("CREATE", result.ClosingResult!.CoaAction);
        Assert.Equal(1, dependencies.RefreshMaxYearCallCount);
    }

    [Fact]
    public void Execute_WhenCloseYearReturnsUnknownStatus_ThrowsInvalidOperationException()
    {
        var dependencies = new FakeWorkflowDependencies
        {
            CloseYearResult = new ClosingYearResult((ClosingYearStatus)999, 2025, 2026, "12/2025", string.Empty, 0m, 0m, string.Empty)
        };
        var workflow = new ClosingYearWorkflow(dependencies);

        Assert.Throws<InvalidOperationException>(() => workflow.Execute(CreateRequest()));
        Assert.Equal(0, dependencies.RefreshMaxYearCallCount);
    }

    private static ClosingYearRequest CreateRequest()
    {
        return new ClosingYearRequest("FSKPKS", 2025, "ADMIN", "KEBUN", true);
    }

    private sealed class FakeWorkflowDependencies : IClosingYearWorkflowDependencies
    {
        public bool IsCoaMissingResult { get; init; }

        public bool HasCoaValidationErrorsResult { get; init; }

        public ClosingYearResult CloseYearResult { get; init; } = ClosingYearResult.Success(2025, 2026, "12/2025", "CREATE", 0m);

        public int RefreshedMaxYear { get; init; } = 2026;

        public int CloseYearCallCount { get; private set; }

        public int RefreshMaxYearCallCount { get; private set; }

        public bool IsCoaMissing(string idData, int year)
        {
            return IsCoaMissingResult;
        }

        public bool HasCoaValidationErrors(string idData, int year)
        {
            return HasCoaValidationErrorsResult;
        }

        public ClosingYearResult CloseYear(ClosingYearRequest request)
        {
            CloseYearCallCount++;

            return CloseYearResult;
        }

        public int RefreshMaxYear(string idData)
        {
            RefreshMaxYearCallCount++;

            return RefreshedMaxYear;
        }
    }
}