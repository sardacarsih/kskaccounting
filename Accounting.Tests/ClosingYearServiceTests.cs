using Accounting.ClosingYear;

namespace Accounting.Tests;

public sealed class ClosingYearServiceTests
{
    [Fact]
    public void CloseYear_WhenRepositorySucceeds_ReturnsSuccess()
    {
        ClosingYearResult expected = ClosingYearResult.Success(2025, 2026, "12/2025", "CREATE", 125m);
        var service = new ClosingYearService(new FakeClosingYearRepository(expected));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.Success, result.Status);
        Assert.Equal(2026, result.NextYear);
        Assert.Equal("CREATE", result.CoaAction);
    }

    [Fact]
    public void CloseYear_WhenServerReportsLockedPeriod_MapsFailure()
    {
        var service = new ClosingYearService(new FakeClosingYearRepository(new InvalidOperationException("PERIOD_LOCKED: 12/2025")));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.LockedPeriod, result.Status);
        Assert.Equal("12/2025", result.Period);
        Assert.Contains("Dikunci", result.Message);
    }

    [Fact]
    public void CloseYear_WhenServerReportsUnbalanced_MapsFailureWithSelisih()
    {
        var service = new ClosingYearService(new FakeClosingYearRepository(new InvalidOperationException("NERACA_NOT_BALANCED: 250.75")));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.NotBalanced, result.Status);
        Assert.Equal(250.75m, result.Selisih);
    }

    [Fact]
    public void CloseYear_WhenOracleErrorIncludesCode_MapsUnbalancedSelisihAfterMarker()
    {
        var service = new ClosingYearService(new FakeClosingYearRepository(new InvalidOperationException("ORA-20311: NERACA_NOT_BALANCED: 250.75")));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.NotBalanced, result.Status);
        Assert.Equal(250.75m, result.Selisih);
    }

    [Fact]
    public void CloseYear_WhenOracleCodeIsLockedWithoutBusinessText_MapsLockedFromCode()
    {
        // No "PERIOD_LOCKED"/"Dikunci" text — mapping must rely on the ORA-20310 code.
        var service = new ClosingYearService(new FakeClosingYearRepository(new InvalidOperationException("ORA-20310: closing aborted by server")));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.LockedPeriod, result.Status);
        Assert.Equal("12/2025", result.Period);
    }

    [Fact]
    public void CloseYear_WhenOracleCodeIsUnbalancedWithoutMarker_DoesNotMistakeCodeForSelisih()
    {
        // Marker text absent: the ORA-20311 code must not be extracted as the selisih.
        var service = new ClosingYearService(new FakeClosingYearRepository(new InvalidOperationException("ORA-20311: 250.75")));

        ClosingYearResult result = service.CloseYear(CreateRequest());

        Assert.Equal(ClosingYearStatus.NotBalanced, result.Status);
        Assert.Equal(250.75m, result.Selisih);
    }

    private static ClosingYearRequest CreateRequest()
    {
        return new ClosingYearRequest("FSKPKS", 2025, "ADMIN", "KEBUN", true);
    }

    private sealed class FakeClosingYearRepository : IClosingYearRepository
    {
        private readonly ClosingYearResult? result;
        private readonly Exception? exception;

        public FakeClosingYearRepository(ClosingYearResult result)
        {
            this.result = result;
        }

        public FakeClosingYearRepository(Exception exception)
        {
            this.exception = exception;
        }

        public ClosingYearResult CloseYear(ClosingYearRequest request)
        {
            if (exception != null)
            {
                throw exception;
            }

            return result!;
        }
    }
}

