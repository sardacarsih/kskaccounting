using Accounting.Form;

namespace Accounting.Tests;

public sealed class CoaPeriodNavigatorTests
{
    [Fact]
    public void Parse_ValidValueReturnsYearMonthAndDisplayName()
    {
        CoaPeriod period = CoaPeriod.Parse(202607, "period");

        Assert.Equal(2026, period.Year);
        Assert.Equal(7, period.Month);
        Assert.Equal(202607, period.Value);
        Assert.Equal("Juli", period.MonthName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(202600)]
    [InlineData(202613)]
    public void Parse_InvalidValueThrows(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CoaPeriod.Parse(value, "period"));
    }

    [Fact]
    public void Constructor_MinimumAfterMaximumThrows()
    {
        Assert.Throws<ArgumentException>(() => new CoaPeriodNavigator(202607, 202606));
    }

    [Fact]
    public void Constructor_StartsAtLatestAvailablePeriod()
    {
        CoaPeriodNavigator navigator = new(202501, 202607);

        Assert.Equal(new CoaPeriod(2026, 7), navigator.Current);
        Assert.True(navigator.CanMovePrevious);
        Assert.False(navigator.CanMoveNext);
    }

    [Fact]
    public void MovePrevious_CrossesJanuaryToPreviousDecember()
    {
        CoaPeriodNavigator navigator = new(202512, 202601);

        Assert.True(navigator.TryMoveByMonths(-1));
        Assert.Equal(new CoaPeriod(2025, 12), navigator.Current);
        Assert.False(navigator.CanMovePrevious);
        Assert.True(navigator.CanMoveNext);
    }

    [Fact]
    public void MoveNext_CrossesDecemberToNextJanuary()
    {
        CoaPeriodNavigator navigator = new(202512, 202601);
        Assert.True(navigator.TryMoveByMonths(-1));

        Assert.True(navigator.TryMoveByMonths(1));
        Assert.Equal(new CoaPeriod(2026, 1), navigator.Current);
    }

    [Fact]
    public void MoveAtBounds_DoesNotChangeCurrentPeriod()
    {
        CoaPeriodNavigator navigator = new(202601, 202601);

        Assert.False(navigator.TryMoveByMonths(-1));
        Assert.False(navigator.TryMoveByMonths(1));
        Assert.Equal(new CoaPeriod(2026, 1), navigator.Current);
        Assert.False(navigator.CanMovePrevious);
        Assert.False(navigator.CanMoveNext);
    }

    [Fact]
    public void TrySetCurrent_RejectsPeriodOutsideAvailableRange()
    {
        CoaPeriodNavigator navigator = new(202503, 202607);

        Assert.False(navigator.TrySetCurrent(new CoaPeriod(2025, 2), out bool changed));
        Assert.False(changed);
        Assert.Equal(new CoaPeriod(2026, 7), navigator.Current);
    }

    [Fact]
    public void TrySetCurrent_ReportsWhetherValidPeriodChanged()
    {
        CoaPeriodNavigator navigator = new(202503, 202607);

        Assert.True(navigator.TrySetCurrent(new CoaPeriod(2026, 6), out bool changed));
        Assert.True(changed);
        Assert.True(navigator.TrySetCurrent(new CoaPeriod(2026, 6), out changed));
        Assert.False(changed);
    }
}
