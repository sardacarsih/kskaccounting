namespace Accounting.Models.Login
{
    public class RoleSummary
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public int UserCount { get; set; }

        public bool IsProtected { get; set; }

        public bool IsSystemRole { get; set; }

        public string Status
        {
            get
            {
                if (IsProtected)
                {
                    return "Protected";
                }

                return IsSystemRole ? "System" : "Custom";
            }
        }
    }
}
