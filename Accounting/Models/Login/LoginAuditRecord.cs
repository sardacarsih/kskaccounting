using System;

namespace Accounting.Models.Login
{
    public sealed class LoginAuditRecord
    {
        public string USERID { get; init; } = string.Empty;
        public string MODULE_NAME { get; init; } = string.Empty;
        public string EVENT_TYPE { get; init; } = string.Empty;
        public string SUCCESS_FLAG { get; init; } = "N";
        public string CLIENT_MACHINE { get; init; } = string.Empty;
        public string DETAIL_MESSAGE { get; init; } = string.Empty;
        public DateTime? LOCKOUT_UNTIL_UTC { get; init; }
        public string IDDATA { get; init; } = string.Empty;
    }
}
