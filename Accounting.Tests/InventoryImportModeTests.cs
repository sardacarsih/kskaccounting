using Accounting.Utilities;

namespace Accounting.Tests;

public class InventoryImportModeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeInventoryImportMode_DefaultsToLamaWhenMissing(string? value)
    {
        string result = ConnectionManager.NormalizeInventoryImportMode(value);

        Assert.Equal(ConnectionManager.InventoryImportModeLama, result);
    }

    [Theory]
    [InlineData("invlama")]
    [InlineData("INVLAMA")]
    [InlineData(" invlama ")]
    public void NormalizeInventoryImportMode_ReturnsLamaForLamaValues(string value)
    {
        string result = ConnectionManager.NormalizeInventoryImportMode(value);

        Assert.Equal(ConnectionManager.InventoryImportModeLama, result);
    }

    [Theory]
    [InlineData("invbaru")]
    [InlineData("INVBARU")]
    [InlineData(" invbaru ")]
    public void NormalizeInventoryImportMode_ReturnsBaruForBaruValues(string value)
    {
        string result = ConnectionManager.NormalizeInventoryImportMode(value);

        Assert.Equal(ConnectionManager.InventoryImportModeBaru, result);
    }

    [Theory]
    [InlineData("inventorybaru")]
    [InlineData("lama")]
    [InlineData("unknown")]
    public void NormalizeInventoryImportMode_DefaultsToLamaWhenInvalid(string value)
    {
        string result = ConnectionManager.NormalizeInventoryImportMode(value);

        Assert.Equal(ConnectionManager.InventoryImportModeLama, result);
    }
}
