namespace Accounting.Models.Login
{
    public readonly record struct PasswordVerificationResult(bool IsValid, bool NeedsRehash);
}
