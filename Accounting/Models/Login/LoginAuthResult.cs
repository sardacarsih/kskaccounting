using System.Collections.Generic;

namespace Accounting.Models.Login
{
    public enum LoginAuthStatus
    {
        Success = 0,
        UserNotFound = 1,
        InvalidPassword = 2,
        InactiveUser = 3,
        NoModuleAccess = 4,
        NoLocationAccess = 5
    }

    public sealed class LoginAuthResult
    {
        public LoginAuthStatus Status { get; init; }
        public List<LOGIN_USERS_DTO> Users { get; init; } = new();
    }
}
