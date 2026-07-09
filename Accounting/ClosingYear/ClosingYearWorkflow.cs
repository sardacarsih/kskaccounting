using System;

namespace Accounting.ClosingYear;

public sealed class ClosingYearWorkflow
{
    private readonly IClosingYearWorkflowDependencies dependencies;

    public ClosingYearWorkflow(IClosingYearWorkflowDependencies dependencies)
    {
        this.dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }

    public ClosingYearWorkflowResult Execute(ClosingYearRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string period = FormatPeriod(request.Year);
        string monthDisplay = FormatMonthDisplay(request.Year);

        if (dependencies.IsCoaMissing(request.IdData, request.Year))
        {
            return ClosingYearWorkflowResult.MissingCoa(request.Year, period, monthDisplay);
        }

        if (dependencies.HasCoaValidationErrors(request.IdData, request.Year))
        {
            return ClosingYearWorkflowResult.CoaValidationErrors(request.Year, period, monthDisplay);
        }

        ClosingYearResult result = dependencies.CloseYear(request);

        return result.Status switch
        {
            ClosingYearStatus.LockedPeriod => ClosingYearWorkflowResult.Locked(result, monthDisplay),
            ClosingYearStatus.NotBalanced => ClosingYearWorkflowResult.NotBalanced(result, monthDisplay),
            ClosingYearStatus.Success => ClosingYearWorkflowResult.Success(
                result,
                monthDisplay,
                dependencies.RefreshMaxYear(request.IdData)),
            _ => throw new InvalidOperationException($"Status tutup tahun tidak dikenal: {result.Status}")
        };
    }

    private static string FormatPeriod(int year)
    {
        return $"12/{year:0000}";
    }

    private static string FormatMonthDisplay(int year)
    {
        return $"Desember - {year:0000}";
    }
}