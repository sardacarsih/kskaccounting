using System;
using Accounting.BusinessLayer;
using Accounting.Models.Login;

namespace Accounting.Services
{
    public sealed class UserSession
    {
        public string UserId { get; init; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Module { get; init; } = string.Empty;
        public bool RequiresPasswordChange { get; set; }
        public string IdData { get; set; } = string.Empty;
        public string Estate { get; set; } = string.Empty;
        public string EstateName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AccountingType { get; set; } = string.Empty;
    }

    public static class AppSession
    {
        public static UserSession? Current { get; private set; }

        public static void StartAuthenticatedUser(string userId, string role, string module, bool requiresPasswordChange)
        {
            AuthorizationService.InvalidateAllPermissions();

            Current = new UserSession
            {
                UserId = userId,
                Role = role,
                Module = module,
                RequiresPasswordChange = requiresPasswordChange
            };

            LoginInfo.userID = userId;
            LoginInfo.role = role;
            LoginInfo.MODULE = module;
        }

        public static void ApplyLocationContext(LOGIN_USERS_DTO user)
        {
            AuthorizationService.InvalidateCurrentUserPermissions();

            if (Current == null)
            {
                StartAuthenticatedUser(user.USERID, user.LEVEL_USER, LoginInfo.MODULE, false);
            }

            Current!.Role = user.LEVEL_USER;
            Current.IdData = user.IDDATA ?? string.Empty;
            Current.Estate = user.ESTATE ?? string.Empty;
            Current.EstateName = user.ESTATENAMA ?? string.Empty;
            Current.CompanyName = user.NAMAPT ?? string.Empty;
            Current.Region = user.WILAYAH ?? string.Empty;
            Current.AccountingType = user.JENIS_AKUNTANSI ?? string.Empty;

            LoginInfo.role = user.LEVEL_USER;
            LoginInfo.userID = user.USERID;
            CompanyInfo.ESTATE = user.ESTATE;
            CompanyInfo.IDDATA = user.IDDATA;
            CompanyInfo.NAMAPT = user.NAMAPT;
            CompanyInfo.WILAYAH = user.WILAYAH;
            CompanyInfo.JENIS_AKUNTING = user.JENIS_AKUNTANSI;

            if (!string.IsNullOrWhiteSpace(CompanyInfo.IDDATA))
            {
                Acct.TahunMin = AccountServices.MinTahunCOA(CompanyInfo.IDDATA);
                Acct.TahunMax = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
                Acct.PeriodeMin = AccountServices.GetMinPeriode(CompanyInfo.IDDATA);
                Acct.PeriodeMax = AccountServices.GetMaxPeriode(CompanyInfo.IDDATA);
            }
        }

        public static void MarkPasswordChangeCompleted()
        {
            if (Current != null)
            {
                Current.RequiresPasswordChange = false;
            }
        }

        public static void Clear()
        {
            AuthorizationService.InvalidateAllPermissions();
            Current = null;
        }
    }
}
