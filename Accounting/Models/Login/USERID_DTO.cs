namespace Accounting.Models.Login
{
    // The USERID_DTO class serves as the master class
    // The USERID_DTO class serves as the master class
    public class USERID_DTO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string USERID { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string NAMA { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string DEPT { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string JABATAN { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string PASSWORD { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string AKTIF { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public int FAILED_LOGIN_COUNT { get; set; }
        public System.DateTime? LOCKOUT_UNTIL_UTC { get; set; }
        public System.DateTime? LAST_LOGIN_AT_UTC { get; set; }
        public System.DateTime? LAST_FAILED_LOGIN_AT_UTC { get; set; }
        public System.DateTime? PASSWORD_CHANGED_AT_UTC { get; set; }
        public string? PASSWORD_RESET_REQUIRED { get; set; }
    }

    // The USERID_DETAIL_DTO class represents the detail with a master-detail relationship
    public class USERID_ESTATE_DTO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string USER_ID { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public int ESTATE_ID { get; set; }
    }
}

