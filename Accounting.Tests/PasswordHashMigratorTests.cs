using System.Security.Cryptography;
using System.Text;
using Accounting.Models.Login;

namespace Accounting.Tests;

public sealed class PasswordHashMigratorTests
{
    [Fact]
    public void DetectFormat_WhenVersionedPbkdf2_ReturnsVersioned()
    {
        string hash = new PasswordCryptographyPbkdf2().GetHashPassword("ValidPassword1!");

        Assert.Equal(PasswordFormat.Pbkdf2Versioned, PasswordHashMigrator.DetectFormat(hash));
        Assert.False(PasswordHashMigrator.NeedsMigration(hash));
    }

    [Fact]
    public void GetHashPassword_UsesCurrentPbkdf2Sha256Parameters()
    {
        const string password = "ValidPassword1!";
        PasswordCryptographyPbkdf2 cryptography = new();

        string firstHash = cryptography.GetHashPassword(password);
        string secondHash = cryptography.GetHashPassword(password);
        string[] parts = firstHash.Split('$');

        Assert.Equal(5, parts.Length);
        Assert.Equal("PBKDF2", parts[0]);
        Assert.Equal("SHA256", parts[1]);
        Assert.Equal("600000", parts[2]);
        Assert.Equal(16, Convert.FromBase64String(parts[3]).Length);
        Assert.Equal(32, Convert.FromBase64String(parts[4]).Length);
        Assert.NotEqual(firstHash, secondHash);
        Assert.True(cryptography.VerifyPassword(password, firstHash).IsValid);
        Assert.False(cryptography.VerifyPassword(password, firstHash).NeedsRehash);
    }

    [Fact]
    public void VerifyPassword_WhenPbkdf2Sha256UsesOldWorkFactor_RequiresRehash()
    {
        const string password = "ValidPassword1!";
        string oldHash = CreateVersionedPbkdf2Hash(password, 210000);

        PasswordVerificationResult result = new PasswordCryptographyPbkdf2()
            .VerifyPassword(password, oldHash);

        Assert.True(result.IsValid);
        Assert.True(result.NeedsRehash);
    }

    [Fact]
    public void VerifyLegacyPassword_WhenLegacyPbkdf2Base64_ReturnsTrue()
    {
        string password = "legacy-secret";
        string hash = CreateLegacyPbkdf2Hash(password);

        PasswordFormat format = PasswordHashMigrator.DetectFormat(hash);

        Assert.Equal(PasswordFormat.Pbkdf2LegacyBinary, format);
        Assert.True(PasswordHashMigrator.VerifyLegacyPassword(password, hash, format));
        Assert.False(PasswordHashMigrator.VerifyLegacyPassword("wrong", hash, format));
    }

    [Theory]
    [InlineData("md5")]
    [InlineData("sha1")]
    [InlineData("sha256")]
    public void VerifyLegacyPassword_WhenHexHash_ReturnsTrueForMatchingPassword(string algorithm)
    {
        string password = "legacy-secret";
        string hash = CreateHexHash(algorithm, password);
        PasswordFormat format = PasswordHashMigrator.DetectFormat(hash);

        Assert.True(PasswordHashMigrator.VerifyLegacyPassword(password, hash, format));
        Assert.False(PasswordHashMigrator.VerifyLegacyPassword("wrong", hash, format));
    }

    [Fact]
    public void VerifyLegacyPassword_WhenPlaintext_ReturnsTrueOnlyForExactMatch()
    {
        string storedPassword = "plain-secret";
        PasswordFormat format = PasswordHashMigrator.DetectFormat(storedPassword);

        Assert.Equal(PasswordFormat.Plaintext, format);
        Assert.True(PasswordHashMigrator.VerifyLegacyPassword(storedPassword, storedPassword, format));
        Assert.False(PasswordHashMigrator.VerifyLegacyPassword("Plain-Secret", storedPassword, format));
    }

    private static string CreateLegacyPbkdf2Hash(string password)
    {
        byte[] salt = Enumerable.Range(1, 20).Select(i => (byte)i).ToArray();
        using Rfc2898DeriveBytes pbkdf2 = new(password, salt, 100000, HashAlgorithmName.SHA1);
        byte[] hash = pbkdf2.GetBytes(20);

        byte[] combined = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
        return Convert.ToBase64String(combined);
    }

    private static string CreateVersionedPbkdf2Hash(string password, int iterations)
    {
        byte[] salt = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
        using Rfc2898DeriveBytes pbkdf2 = new(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        return string.Join(
            "$",
            "PBKDF2",
            "SHA256",
            iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    private static string CreateHexHash(string algorithm, string password)
    {
        byte[] input = Encoding.UTF8.GetBytes(password);
        byte[] hash = algorithm switch
        {
            "md5" => MD5.HashData(input),
            "sha1" => SHA1.HashData(input),
            "sha256" => SHA256.HashData(input),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
