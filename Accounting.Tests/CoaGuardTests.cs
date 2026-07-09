using Accounting.DataLayer;

namespace Accounting.Tests;

public sealed class CoaGuardTests
{
    [Fact]
    public void IsCodeChanging_SameCode_ReturnsFalse()
    {
        Assert.False(CoaGuard.IsCodeChanging("01.00001.001", "01.00001.001"));
    }

    [Fact]
    public void IsCodeChanging_DifferentCode_ReturnsTrue()
    {
        Assert.True(CoaGuard.IsCodeChanging("01.00001.001", "01.00001.002"));
    }

    [Fact]
    public void IsCodeChanging_WhitespaceDifference_ReturnsFalse()
    {
        Assert.False(CoaGuard.IsCodeChanging(" 01.00001.001 ", "01.00001.001"));
    }

    [Fact]
    public void IsCodeChanging_NullValues_ReturnsFalse()
    {
        Assert.False(CoaGuard.IsCodeChanging(null, null));
    }
}
