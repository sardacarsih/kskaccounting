using Accounting;
using Accounting.Services;

namespace Accounting.Tests;

public sealed class AuthorizationServiceTests : IDisposable
{
    public AuthorizationServiceTests()
    {
        LoginInfo.userID = "dharyadi";
        LoginInfo.role = "ADMIN";
        LoginInfo.MODULE = "ACCOUNTING";
        AuthorizationService.InvalidateAllPermissions();
    }

    [Fact]
    public void EnsureCanManageUserLocationAccess_WhenTargetIsCurrentAdminUser_AllowsOperation()
    {
        Exception? exception = Record.Exception(() =>
            AuthorizationService.EnsureCanManageUserLocationAccess("dharyadi"));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanManageUsers_WhenTargetIsCurrentAdminUser_StillBlocksOperation()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageUsers("dharyadi"));

        Assert.Equal("Akun yang sedang aktif tidak dapat diubah melalui menu administrasi ini.", exception.Message);
    }

    [Fact]
    public void EnsureCanManageUserLocationAccess_WhenTargetIsProtectedUser_BlocksOperation()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageUserLocationAccess("Administrator"));

        Assert.Equal("Akses lokasi akun sistem inti tidak dapat diubah.", exception.Message);
    }

    [Fact]
    public void EnsureCanManageUserLocationAccess_WhenUserLacksPermission_BlocksOperation()
    {
        LoginInfo.role = "STAFF";
        AuthorizationService.InvalidateAllPermissions();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageUserLocationAccess("other.user"));

        Assert.Equal("Anda tidak memiliki izin mengelola akses lokasi user.", exception.Message);
    }

    [Fact]
    public void EnsureCanManageRoleAssignments_WhenTargetRoleIsProtectedAndUserIsDifferent_AllowsOperation()
    {
        Exception? exception = Record.Exception(() =>
            AuthorizationService.EnsureCanManageRoleAssignments("tia", 1));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanManageRoleAssignments_WhenTargetUserIsCurrentUser_StillBlocksOperation()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageRoleAssignments("dharyadi", 6));

        Assert.Equal("Role akun yang sedang aktif tidak dapat diubah dari sesi ini.", exception.Message);
    }

    [Fact]
    public void EnsureCanManageRolePermissions_WhenRoleIsProtected_StillBlocksOperation()
    {
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageRolePermissions(1));

        Assert.Equal("Permission untuk role sistem inti tidak dapat diubah dari layar ini.", exception.Message);
    }

    [Fact]
    public void EnsureCanManageRoleAssignments_WhenUserLacksPermission_BlocksOperation()
    {
        LoginInfo.role = "STAFF";
        AuthorizationService.InvalidateAllPermissions();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            AuthorizationService.EnsureCanManageRoleAssignments("tia", 1));

        Assert.Equal("Anda tidak memiliki izin mengubah assignment role user.", exception.Message);
    }

    public void Dispose()
    {
        LoginInfo.userID = string.Empty;
        LoginInfo.role = string.Empty;
        LoginInfo.MODULE = "ACCOUNTING";
        AuthorizationService.InvalidateAllPermissions();
    }
}
