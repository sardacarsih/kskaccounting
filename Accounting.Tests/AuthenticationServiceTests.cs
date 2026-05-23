using Accounting.Interface;
using Accounting.Models.Login;
using Accounting.Services;

namespace Accounting.Tests;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public void AuthenticateForModule_WhenLegacyPlaintextPasswordIsValid_RehashesPasswordAndLogsIn()
    {
        FakeUsersManager repository = new()
        {
            Credential = CreateCredential("plain-secret"),
            HasRbacAccess = true,
            RbacUsers = [CreateLoginUser()]
        };

        LoginAuthResult result = AuthenticationService.AuthenticateForModule(
            repository,
            "legacy.user",
            "plain-secret",
            "GL");

        Assert.Equal(LoginAuthStatus.Success, result.Status);
        Assert.Single(result.Users);
        Assert.StartsWith("PBKDF2$", repository.Credential.PASSWORD, StringComparison.OrdinalIgnoreCase);
        Assert.True(repository.ResetPasswordCalled);
        Assert.True(repository.UpdateSuccessfulLoginCalled);
        Assert.Contains(repository.AuditRecords, record => record.EVENT_TYPE == "LOGIN_AUTHENTICATED" && record.SUCCESS_FLAG == "Y");
    }

    [Fact]
    public void AuthenticateForModule_WhenRbacAccessMissingButLegacyAccessExists_UsesLegacyAccessPath()
    {
        FakeUsersManager repository = new()
        {
            Credential = CreateCredential(new PasswordCryptographyPbkdf2().GetHashPassword("ValidPassword1!")),
            HasRbacAccess = false,
            HasLegacyAccess = true,
            LegacyUsers = [CreateLoginUser(isLegacy: true)]
        };

        LoginAuthResult result = AuthenticationService.AuthenticateForModule(
            repository,
            "legacy.user",
            "ValidPassword1!",
            "GL");

        Assert.Equal(LoginAuthStatus.Success, result.Status);
        Assert.Single(result.Users);
        Assert.True(result.Users[0].IsLegacyAccessPath);
        Assert.Contains(repository.AuditRecords, record =>
            record.EVENT_TYPE == "LOGIN_AUTHENTICATED" &&
            record.SUCCESS_FLAG == "Y" &&
            record.DETAIL_MESSAGE.Contains("jalur legacy", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AuthenticateForModule_WhenPasswordIsInvalid_RegistersFailedAttempt()
    {
        FakeUsersManager repository = new()
        {
            Credential = CreateCredential(new PasswordCryptographyPbkdf2().GetHashPassword("ValidPassword1!")),
            HasRbacAccess = true,
            RbacUsers = [CreateLoginUser()]
        };

        LoginAuthResult result = AuthenticationService.AuthenticateForModule(
            repository,
            "legacy.user",
            "wrong-password",
            "GL");

        Assert.Equal(LoginAuthStatus.InvalidPassword, result.Status);
        Assert.Equal(1, repository.FailedLoginAttempts);
        Assert.False(repository.ResetPasswordCalled);
        Assert.Contains(repository.AuditRecords, record => record.EVENT_TYPE == "LOGIN_REJECTED_INVALID_CREDENTIAL" && record.SUCCESS_FLAG == "N");
    }

    [Fact]
    public void AuthenticateForModule_WhenNoRbacOrLegacyAccess_ReturnsNoModuleAccess()
    {
        FakeUsersManager repository = new()
        {
            Credential = CreateCredential(new PasswordCryptographyPbkdf2().GetHashPassword("ValidPassword1!")),
            HasRbacAccess = false,
            HasLegacyAccess = false
        };

        LoginAuthResult result = AuthenticationService.AuthenticateForModule(
            repository,
            "legacy.user",
            "ValidPassword1!",
            "GL");

        Assert.Equal(LoginAuthStatus.NoModuleAccess, result.Status);
        Assert.Contains(repository.AuditRecords, record => record.EVENT_TYPE == "LOGIN_REJECTED_NO_MODULE_ACCESS" && record.SUCCESS_FLAG == "N");
    }

    private static USERID_DTO CreateCredential(string password)
    {
        return new USERID_DTO
        {
            USERID = "legacy.user",
            PASSWORD = password,
            AKTIF = "Y",
            NAMA = "Legacy User",
            DEPT = "IT",
            JABATAN = "Staff",
            PASSWORD_RESET_REQUIRED = "N"
        };
    }

    private static LOGIN_USERS_DTO CreateLoginUser(bool isLegacy = false)
    {
        return new LOGIN_USERS_DTO
        {
            USERID = "legacy.user",
            NAMA = "Legacy User",
            LEVEL_USER = "Staff",
            IDDATA = "ID01",
            NAMAPT = "PT Test",
            WILAYAH = "W01",
            JENIS_AKUNTANSI = "GL",
            IsLegacyAccessPath = isLegacy
        };
    }

    private sealed class FakeUsersManager : IUsersManager
    {
        public USERID_DTO Credential { get; set; } = CreateCredential("ValidPassword1!");
        public bool HasRbacAccess { get; set; }
        public bool HasLegacyAccess { get; set; }
        public List<LOGIN_USERS_DTO> RbacUsers { get; set; } = [];
        public List<LOGIN_USERS_DTO> LegacyUsers { get; set; } = [];
        public List<LoginAuditRecord> AuditRecords { get; } = [];
        public bool ResetPasswordCalled { get; private set; }
        public bool UpdateSuccessfulLoginCalled { get; private set; }
        public int FailedLoginAttempts { get; private set; }

        public void AddUser(USERID_DTO user) => throw new NotSupportedException();
        public void UpdateUser(USERID_DTO updatedUser) => throw new NotSupportedException();
        public void DeleteUser(string userId) => throw new NotSupportedException();
        public List<USERID_DTO> GetAllUsers() => throw new NotSupportedException();
        public List<WILAYAH_IDDATA_DTO> GetListIDDATA() => throw new NotSupportedException();
        public List<WILAYAH_ESTATE_DTO> GetWilayahEstate() => throw new NotSupportedException();
        public Dictionary<int, string> GetUserLevel() => throw new NotSupportedException();
        public void AddUserRole(string userid, int role_id, int moduleid) => throw new NotSupportedException();
        public void AddUserRole_Estate(string userid, int estateid) => throw new NotSupportedException();
        public void AddUserRole_IDDATA(string userid, string iddata) => throw new NotSupportedException();
        public void DeleteIDData(string userid, string iddata) => throw new NotSupportedException();
        public void DeleteAksesEstate(string userid, int estateid) => throw new NotSupportedException();
        public List<appsdetail_dto> GetAppDetailsByUserId(string userId, string modulename) => throw new NotSupportedException();
        public List<LOGIN_USERS_DTO> UserLogin(string userId, string appName) => throw new NotSupportedException();
        public List<LOGIN_USERS_DTO> UserLoginByEstate(string userId, string appName) => throw new NotSupportedException();

        public List<LOGIN_USERS_DTO> UserLoginByIDDATA(string userId, string appName)
        {
            return RbacUsers.Count > 0 ? RbacUsers : LegacyUsers;
        }

        public USERID_DTO? GetUserCredential(string userId) => Credential;
        public bool HasModuleAccess(string userId, string modulename) => HasRbacAccess || HasLegacyAccess;
        public bool HasRbacModuleAccess(string userId, string modulename) => HasRbacAccess;
        public bool HasLegacyModuleAccess(string userId, string modulename) => HasLegacyAccess;
        public string GetHashPassword(string userId) => Credential.PASSWORD;

        public void ResetPassword(string userId, string newHashedPassword)
        {
            ResetPasswordCalled = true;
            Credential.PASSWORD = newHashedPassword;
        }

        public void SetPasswordResetRequired(string userId, bool required)
        {
            Credential.PASSWORD_RESET_REQUIRED = required ? "Y" : "N";
        }

        public void RegisterFailedLoginAttempt(string userId, int maxFailedAttempts, int lockoutMinutes)
        {
            FailedLoginAttempts++;
            Credential.FAILED_LOGIN_COUNT++;
        }

        public void ResetLoginFailures(string userId)
        {
            Credential.FAILED_LOGIN_COUNT = 0;
            Credential.LOCKOUT_UNTIL_UTC = null;
        }

        public void UpdateSuccessfulLogin(string userId)
        {
            UpdateSuccessfulLoginCalled = true;
            Credential.LAST_LOGIN_AT_UTC = DateTime.UtcNow;
        }

        public void RecordLoginAudit(LoginAuditRecord record)
        {
            AuditRecords.Add(record);
        }

        public List<AKSES_ESTATE_DTO> GetAksesEstatebyUserID(string userId, string modulename) => throw new NotSupportedException();
        public List<AKSES_IDDATA_DTO> GetAksesIDDATAbyUserID(string userId, string modulename) => throw new NotSupportedException();
    }
}
