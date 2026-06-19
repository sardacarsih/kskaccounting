using System;
using System.Collections.Generic;
using System.Linq;
using Accounting._3.Services;
using Accounting.BusinessLayer;
using Accounting.Models.Login;

namespace Accounting.Services;

internal enum AuthorizationCrudAction
{
    Create,
    Read,
    Update,
    Delete
}

internal static class AuthorizationService
{
    private const string UserAccessPermission = "USER_AKSES";
    private const string FixedAssetApprovalPermission = "FA_TRANSACTION_APPROVE";
    private const string FixedAssetModuleName = "FIXED_ASSET";
    private const string JurnalModuleName = "GL";
    private const string CoaViewPermission = "COA_VIEW";
    private const string CoaEntryPermission = "COA_ENTRY";
    private const string CoaDeletePermission = "COA_DELETE";
    private const string CoaImportPermission = "COA_IMPORT";
    private const string CoaExportPermission = "COA_EXPORT";
    private const string JurnalViewPermission = "JURNAL_VIEW";
    private const string JurnalEntryPermission = "JURNAL_ENTRY";
    private const string JurnalDeletePermission = "JURNAL_DELETE";
    private const string JurnalImportPermission = "JURNAL_IMPORT";
    private const string JurnalExportPermission = "JURNAL_EXPORT";
    private const string ReportViewPermission = "REPORT_VIEW";
    private const string ReportExportPermission = "REPORT_EXPORT";
    private const string ReportEstateViewPermission = "REPORT_ESTATE_VIEW";
    private const string SettingPeriodPermission = "SETTING_PERIOD";
    private const string SettingCompanyPermission = "SETTING_COMPANY";
    private const string SettingProfilePermission = "SETTING_PROFILE";
    private const string SettingRlPermission = "SETTING_RL";
    private const string SettingDeveloperPermission = "SETTING_DEVELOPER";
    private const string AuditTrailViewPermission = "AUDIT_TRAIL_VIEW";

    private static readonly object PermissionCacheSync = new();
    private static readonly Dictionary<string, IReadOnlyList<Permission>> PermissionCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] AdminRoles =
    [
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN"
    ];

    private static readonly string[] FixedAssetViewerRoles =
    [
        "VIEWER",
        "MAKER",
        "CHECKER",
        "APPROVER",
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN",
        "SUPERVISOR",
        "USER",
        "OPERATOR",
        "STAFF"
    ];

    private static readonly string[] FixedAssetEditorRoles =
    [
        "MAKER",
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN",
        "SUPERVISOR",
        "USER",
        "OPERATOR",
        "STAFF"
    ];

    private static readonly string[] FixedAssetDepreciationRoles =
    [
        "MAKER",
        "CHECKER",
        "APPROVER",
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN",
        "SUPERVISOR",
        "USER",
        "OPERATOR",
        "STAFF"
    ];

    private static readonly string[] FixedAssetApprovalRoles =
    [
        "CHECKER",
        "APPROVER",
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN",
        "SUPERVISOR"
    ];

    private static readonly string[] FixedAssetPostReverseRoles =
    [
        "APPROVER",
        "ADMIN",
        "ADMINISTRATOR",
        "SUPERADMIN",
        "SUPERVISOR"
    ];

    public static bool CanViewUserManagement()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Read)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageUsers()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Create)
            || HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanResetPasswords()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanViewRoleManagement()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Read)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageRoleAssignments()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageRolePermissions()
    {
        return HasAnyPermission([UserAccessPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanViewFixedAssetWorkspace()
    {
        return HasAnyPermission(["FA_MASTER_DATA", "FA_ASSET_MASTER"], AuthorizationCrudAction.Read, FixedAssetModuleName)
            || IsInAnyRole(FixedAssetViewerRoles);
    }

    public static bool CanViewJurnalWorkspace()
    {
        return HasAnyPermission([JurnalViewPermission, JurnalEntryPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || HasAnyPermission([JurnalEntryPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || HasAnyPermission([JurnalEntryPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || LegacyOpenFormAccess(16)
            || LegacyOpenFormAccess(4);
    }

    public static bool CanViewCoaWorkspace()
    {
        return HasAnyPermission([CoaViewPermission, CoaEntryPermission, CoaDeletePermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || HasAnyPermission([CoaEntryPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || HasAnyPermission([CoaEntryPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanCreateCoa()
    {
        return HasAnyPermission([CoaEntryPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanUpdateCoa()
    {
        return HasAnyPermission([CoaEntryPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanDeleteCoa()
    {
        return HasAnyPermission([CoaDeletePermission], AuthorizationCrudAction.Delete, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanImportCoa()
    {
        return HasAnyPermission([CoaImportPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanExportCoa()
    {
        return HasAnyPermission([CoaExportPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanViewReports()
    {
        return HasAnyPermission([ReportViewPermission, ReportExportPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanExportReports()
    {
        return HasAnyPermission([ReportExportPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanViewEstateReports()
    {
        return HasAnyPermission([ReportEstateViewPermission, ReportExportPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageAccountingPeriods()
    {
        return HasAnyPermission([SettingPeriodPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || HasAnyPermission([SettingPeriodPermission], AuthorizationCrudAction.Delete, JurnalModuleName)
            || HasAnyPermission([SettingPeriodPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageCompanyMaster()
    {
        return HasAnyPermission([SettingCompanyPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || HasAnyPermission([SettingCompanyPermission], AuthorizationCrudAction.Delete, JurnalModuleName)
            || HasAnyPermission([SettingCompanyPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageCompanyProfile()
    {
        return HasAnyPermission([SettingProfilePermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || HasAnyPermission([SettingProfilePermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanManageProfitLossSetup()
    {
        return HasAnyPermission([SettingRlPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || HasAnyPermission([SettingRlPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanAccessDeveloperTools()
    {
        return HasAnyPermission([SettingDeveloperPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || HasAnyPermission([SettingDeveloperPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanViewAuditTrail()
    {
        return HasAnyPermission([AuditTrailViewPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || IsInAnyRole(AdminRoles);
    }

    public static bool CanCreateJurnal()
    {
        return HasAnyPermission([JurnalEntryPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || LegacyOpenFormAccess(16);
    }

    public static bool CanUpdateJurnal()
    {
        return HasAnyPermission([JurnalEntryPermission], AuthorizationCrudAction.Update, JurnalModuleName)
            || LegacyUpdateAccess(4)
            || LegacyOpenFormAccess(16);
    }

    public static bool CanDeleteJurnal()
    {
        return HasAnyPermission([JurnalDeletePermission], AuthorizationCrudAction.Delete, JurnalModuleName)
            || LegacyDeleteAccess(4);
    }

    public static bool CanImportJurnal()
    {
        return HasAnyPermission([JurnalImportPermission], AuthorizationCrudAction.Create, JurnalModuleName)
            || LegacyOpenFormAccess(17)
            || LegacyOpenFormAccess(16);
    }

    public static bool CanExportJurnal()
    {
        return HasAnyPermission([JurnalExportPermission], AuthorizationCrudAction.Read, JurnalModuleName)
            || LegacyOpenFormAccess(17);
    }

    public static bool CanEditFixedAssetMaster()
    {
        return HasAnyPermission(["FA_MASTER_DATA", "FA_ASSET_MASTER"], AuthorizationCrudAction.Update, FixedAssetModuleName)
            || IsInAnyRole(FixedAssetEditorRoles);
    }

    public static bool CanCreateFixedAssetLifecycle()
    {
        return HasAnyPermission(["FA_TRANSACTION_CREATE", "FA_LIFECYCLE_CREATE"], AuthorizationCrudAction.Create, FixedAssetModuleName)
            || IsInAnyRole(FixedAssetEditorRoles);
    }

    public static bool CanRunFixedAssetDepreciation()
    {
        return HasAnyPermission(["FA_DEPRECIATION_RUN"], AuthorizationCrudAction.Update, FixedAssetModuleName)
            || IsInAnyRole(FixedAssetDepreciationRoles);
    }

    public static bool CanApproveFixedAssetTransaction()
    {
        return HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Update, FixedAssetModuleName)
            || HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(FixedAssetApprovalRoles);
    }

    public static bool CanPostOrReverseFixedAsset()
    {
        return HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Delete, FixedAssetModuleName)
            || HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Update, FixedAssetModuleName)
            || HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Delete)
            || HasAnyPermission([FixedAssetApprovalPermission], AuthorizationCrudAction.Update)
            || IsInAnyRole(FixedAssetPostReverseRoles);
    }

    public static void EnsureCanViewUserManagement()
    {
        Ensure(CanViewUserManagement(), "Anda tidak memiliki izin membuka manajemen user.");
    }

    public static void EnsureCanManageUsers(string? targetUserId = null)
    {
        Ensure(CanManageUsers(), "Anda tidak memiliki izin mengelola user.");

        if (!string.IsNullOrWhiteSpace(targetUserId) && IsCurrentUser(targetUserId))
        {
            throw new InvalidOperationException("Akun yang sedang aktif tidak dapat diubah melalui menu administrasi ini.");
        }

        if (IsProtectedUser(targetUserId))
        {
            throw new InvalidOperationException("Akun sistem inti tidak dapat diubah melalui menu administrasi ini.");
        }
    }

    public static void EnsureCanManageUserLocationAccess(string? targetUserId)
    {
        Ensure(CanManageUsers(), "Anda tidak memiliki izin mengelola akses lokasi user.");

        if (IsProtectedUser(targetUserId))
        {
            throw new InvalidOperationException("Akses lokasi akun sistem inti tidak dapat diubah.");
        }
    }

    public static void EnsureCanResetPassword(string targetUserId)
    {
        Ensure(CanResetPasswords(), "Anda tidak memiliki izin mereset password user.");

        if (IsCurrentUser(targetUserId))
        {
            throw new InvalidOperationException("Gunakan menu ganti password untuk akun yang sedang aktif.");
        }

        if (IsProtectedUser(targetUserId))
        {
            throw new InvalidOperationException("Password akun sistem inti tidak dapat direset dari layar ini.");
        }
    }

    public static void EnsureCanViewRoleManagement()
    {
        Ensure(CanViewRoleManagement(), "Anda tidak memiliki izin membuka pengaturan role dan permission.");
    }

    public static void EnsureCanViewJurnalWorkspace()
    {
        Ensure(CanViewJurnalWorkspace(), "Anda tidak memiliki izin membuka workspace jurnal.");
    }

    public static void EnsureCanViewCoaWorkspace()
    {
        Ensure(CanViewCoaWorkspace(), "Anda tidak memiliki izin membuka workspace chart of account.");
    }

    public static void EnsureCanCreateCoa()
    {
        Ensure(CanCreateCoa(), "Anda tidak memiliki izin menambah chart of account.");
    }

    public static void EnsureCanUpdateCoa()
    {
        Ensure(CanUpdateCoa(), "Anda tidak memiliki izin mengubah chart of account.");
    }

    public static void EnsureCanDeleteCoa()
    {
        Ensure(CanDeleteCoa(), "Anda tidak memiliki izin menghapus chart of account.");
    }

    public static void EnsureCanImportCoa()
    {
        Ensure(CanImportCoa(), "Anda tidak memiliki izin mengimpor chart of account.");
    }

    public static void EnsureCanExportCoa()
    {
        Ensure(CanExportCoa(), "Anda tidak memiliki izin mengekspor chart of account.");
    }

    public static void EnsureCanViewReports()
    {
        Ensure(CanViewReports(), "Anda tidak memiliki izin membuka laporan.");
    }

    public static void EnsureCanExportReports()
    {
        Ensure(CanExportReports(), "Anda tidak memiliki izin mengekspor laporan.");
    }

    public static void EnsureCanViewEstateReports()
    {
        Ensure(CanViewEstateReports(), "Anda tidak memiliki izin membuka laporan estate.");
    }

    public static void EnsureCanManageAccountingPeriods()
    {
        Ensure(CanManageAccountingPeriods(), "Anda tidak memiliki izin mengelola periode akuntansi.");
    }

    public static void EnsureCanManageCompanyMaster()
    {
        Ensure(CanManageCompanyMaster(), "Anda tidak memiliki izin mengelola master company dan lokasi.");
    }

    public static void EnsureCanManageCompanyProfile()
    {
        Ensure(CanManageCompanyProfile(), "Anda tidak memiliki izin mengubah profil company.");
    }

    public static void EnsureCanManageProfitLossSetup()
    {
        Ensure(CanManageProfitLossSetup(), "Anda tidak memiliki izin mengubah pengaturan laba rugi.");
    }

    public static void EnsureCanAccessDeveloperTools()
    {
        Ensure(CanAccessDeveloperTools(), "Anda tidak memiliki izin membuka developer tools.");
    }

    public static void EnsureCanViewAuditTrail()
    {
        Ensure(CanViewAuditTrail(), "Anda tidak memiliki izin membuka audit trail.");
    }

    public static void EnsureCanCreateJurnal()
    {
        Ensure(CanCreateJurnal(), "Anda tidak memiliki izin membuat jurnal baru.");
    }

    public static void EnsureCanUpdateJurnal()
    {
        Ensure(CanUpdateJurnal(), "Anda tidak memiliki izin mengubah jurnal.");
    }

    public static void EnsureCanDeleteJurnal()
    {
        Ensure(CanDeleteJurnal(), "Anda tidak memiliki izin menghapus jurnal.");
    }

    public static void EnsureCanImportJurnal()
    {
        Ensure(CanImportJurnal(), "Anda tidak memiliki izin mengimpor jurnal.");
    }

    public static void EnsureCanExportJurnal()
    {
        Ensure(CanExportJurnal(), "Anda tidak memiliki izin mengekspor jurnal.");
    }

    public static void EnsureCanManageRoleAssignments(string? targetUserId = null, int? targetRoleId = null)
    {
        Ensure(CanManageRoleAssignments(), "Anda tidak memiliki izin mengubah assignment role user.");

        if (!string.IsNullOrWhiteSpace(targetUserId) && IsCurrentUser(targetUserId))
        {
            throw new InvalidOperationException("Role akun yang sedang aktif tidak dapat diubah dari sesi ini.");
        }

        if (IsProtectedUser(targetUserId))
        {
            throw new InvalidOperationException("Assignment role untuk akun sistem inti tidak dapat diubah.");
        }

    }

    public static void EnsureCanManageRolePermissions(int? roleId = null)
    {
        Ensure(CanManageRolePermissions(), "Anda tidak memiliki izin mengubah permission role.");

        if (roleId.HasValue && IsProtectedRoleId(roleId.Value))
        {
            throw new InvalidOperationException("Permission untuk role sistem inti tidak dapat diubah dari layar ini.");
        }
    }

    public static bool IsProtectedRoleId(int roleId)
    {
        return roleId == 1;
    }

    public static bool IsProtectedUser(string? userId)
    {
        return string.Equals(userId?.Trim(), "Administrator", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsCurrentUser(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(LoginInfo.userID))
        {
            return false;
        }

        return string.Equals(userId.Trim(), LoginInfo.userID.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static void InvalidateCurrentUserPermissions()
    {
        string userId = LoginInfo.userID?.Trim() ?? string.Empty;
        if (userId.Length == 0)
        {
            return;
        }

        lock (PermissionCacheSync)
        {
            string[] keysToRemove = PermissionCache.Keys
                .Where(key => key.StartsWith(userId + "|", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string key in keysToRemove)
            {
                PermissionCache.Remove(key);
            }
        }
    }

    public static void InvalidateAllPermissions()
    {
        lock (PermissionCacheSync)
        {
            PermissionCache.Clear();
        }
    }

    private static bool HasAnyPermission(IEnumerable<string> permissionNames, AuthorizationCrudAction action, string? moduleName = null)
    {
        string[] requestedPermissions = permissionNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requestedPermissions.Length == 0)
        {
            return false;
        }

        foreach (string module in GetModulesToCheck(moduleName))
        {
            IReadOnlyList<Permission> permissions = GetPermissionsForModule(module);
            if (permissions.Count == 0)
            {
                continue;
            }

            foreach (string permissionName in requestedPermissions)
            {
                Permission? permission = permissions.FirstOrDefault(p =>
                    string.Equals(p.PermissionName, permissionName, StringComparison.OrdinalIgnoreCase));

                if (permission != null && EvaluatePermission(permission, action))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IReadOnlyList<Permission> GetPermissionsForModule(string moduleName)
    {
        string userId = LoginInfo.userID?.Trim() ?? string.Empty;
        string effectiveModule = moduleName?.Trim() ?? string.Empty;
        if (userId.Length == 0 || effectiveModule.Length == 0)
        {
            return Array.Empty<Permission>();
        }

        string cacheKey = $"{userId}|{effectiveModule}";
        lock (PermissionCacheSync)
        {
            if (PermissionCache.TryGetValue(cacheKey, out IReadOnlyList<Permission>? cachedPermissions))
            {
                return cachedPermissions;
            }
        }

        IReadOnlyList<Permission> loadedPermissions;
        try
        {
            loadedPermissions = Permission_Services.GetPermissionsForUser(userId, effectiveModule)
                .ToList();
        }
        catch
        {
            loadedPermissions = Array.Empty<Permission>();
        }

        lock (PermissionCacheSync)
        {
            PermissionCache[cacheKey] = loadedPermissions;
        }

        return loadedPermissions;
    }

    private static IEnumerable<string> GetModulesToCheck(string? explicitModuleName)
    {
        string currentModule = LoginInfo.MODULE?.Trim() ?? string.Empty;
        string requestedModule = explicitModuleName?.Trim() ?? string.Empty;

        if (requestedModule.Length > 0)
        {
            yield return requestedModule;
        }

        if (currentModule.Length > 0
            && !string.Equals(currentModule, requestedModule, StringComparison.OrdinalIgnoreCase))
        {
            yield return currentModule;
        }
    }

    private static bool EvaluatePermission(Permission permission, AuthorizationCrudAction action)
    {
        return action switch
        {
            AuthorizationCrudAction.Create => permission.CanCreate,
            AuthorizationCrudAction.Read => permission.CanRead,
            AuthorizationCrudAction.Update => permission.CanUpdate,
            AuthorizationCrudAction.Delete => permission.CanDelete,
            _ => false
        };
    }

    private static bool IsInAnyRole(IEnumerable<string> allowedRoles)
    {
        string[] currentRoles = NormalizeRoles(LoginInfo.role);
        string[] allowed = allowedRoles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .ToArray();

        if (currentRoles.Length == 0 || allowed.Length == 0)
        {
            return false;
        }

        return currentRoles.Any(role => allowed.Any(allowedRole =>
            string.Equals(role, allowedRole, StringComparison.OrdinalIgnoreCase)));
    }

    private static string[] NormalizeRoles(string? rawRole)
    {
        if (string.IsNullOrWhiteSpace(rawRole))
        {
            return Array.Empty<string>();
        }

        return rawRole
            .ToUpperInvariant()
            .Split([',', ';', '|', '/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void Ensure(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static bool LegacyOpenFormAccess(int actionId)
    {
        return TryLegacyAccess(() => LevelAksesServices.OpenForm(actionId, LoginInfo.userID));
    }

    private static bool LegacyUpdateAccess(int actionId)
    {
        return TryLegacyAccess(() => LevelAksesServices.Ubah(actionId, LoginInfo.userID));
    }

    private static bool LegacyDeleteAccess(int actionId)
    {
        return TryLegacyAccess(() => LevelAksesServices.Hapus(actionId, LoginInfo.userID));
    }

    private static bool TryLegacyAccess(Func<bool> check)
    {
        if (string.IsNullOrWhiteSpace(LoginInfo.userID))
        {
            return false;
        }

        try
        {
            return check();
        }
        catch
        {
            return false;
        }
    }
}
