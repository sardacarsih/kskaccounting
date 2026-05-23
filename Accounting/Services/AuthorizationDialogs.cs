using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Accounting.Services;

internal static class AuthorizationDialogs
{
    public static bool TryEnsure(IWin32Window? owner, Action ensureAction, string title = "Akses Ditolak")
    {
        try
        {
            ensureAction();
            return true;
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(owner, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
