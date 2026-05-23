using Accounting.Services;

namespace Accounting.Form;

internal static class FixedAssetUiRoleHelper
{
    public static bool CanMasterEdit()
    {
        return AuthorizationService.CanEditFixedAssetMaster();
    }

    public static bool CanLifecycleCreate()
    {
        return AuthorizationService.CanCreateFixedAssetLifecycle();
    }

    public static bool CanApprovalAction()
    {
        return AuthorizationService.CanApproveFixedAssetTransaction();
    }

    public static bool CanPostReverse()
    {
        return AuthorizationService.CanPostOrReverseFixedAsset();
    }
}
