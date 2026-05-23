using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Accounting
{
    internal enum PasswordFormat
    {
        Pbkdf2Versioned,
        Pbkdf2LegacyBinary,
        Md5Hex,
        Sha1Hex,
        Sha256Hex,
        Plaintext
    }

    internal static class PasswordHashMigrator
    {
        public static bool NeedsMigration(string storedHash)
        {
            return DetectFormat(storedHash) != PasswordFormat.Pbkdf2Versioned;
        }

        public static PasswordFormat DetectFormat(string storedHash)
        {
            string normalizedHash = NormalizeStoredHash(storedHash);
            if (string.IsNullOrEmpty(normalizedHash))
            {
                return PasswordFormat.Plaintext;
            }

            if (normalizedHash.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase))
            {
                return PasswordFormat.Pbkdf2Versioned;
            }

            if (TryDecodeLegacyBase64(normalizedHash, out byte[] decodedBytes) && decodedBytes.Length == 40)
            {
                return PasswordFormat.Pbkdf2LegacyBinary;
            }

            if (IsHexString(normalizedHash))
            {
                return normalizedHash.Length switch
                {
                    32 => PasswordFormat.Md5Hex,
                    40 => PasswordFormat.Sha1Hex,
                    64 => PasswordFormat.Sha256Hex,
                    _ => PasswordFormat.Plaintext
                };
            }

            return PasswordFormat.Plaintext;
        }

        public static bool VerifyLegacyPassword(string password, string storedHash, PasswordFormat format)
        {
            string normalizedHash = NormalizeStoredHash(storedHash);
            switch (format)
            {
                case PasswordFormat.Plaintext:
                    return string.Equals(password, normalizedHash, StringComparison.Ordinal);

                case PasswordFormat.Md5Hex:
                    if (CompareHex(MD5.Create(), password, normalizedHash))
                    {
                        return true;
                    }

                    return string.Equals(password, normalizedHash, StringComparison.Ordinal);

                case PasswordFormat.Sha1Hex:
                    if (CompareHex(SHA1.Create(), password, normalizedHash))
                    {
                        return true;
                    }

                    return string.Equals(password, normalizedHash, StringComparison.Ordinal);

                case PasswordFormat.Sha256Hex:
                    if (CompareHex(SHA256.Create(), password, normalizedHash))
                    {
                        return true;
                    }

                    return string.Equals(password, normalizedHash, StringComparison.Ordinal);

                case PasswordFormat.Pbkdf2LegacyBinary:
                    return new PasswordCryptographyPbkdf2().VerifyPassword(password, normalizedHash).IsValid;

                default:
                    return false;
            }
        }

        public static string NormalizeStoredHash(string? storedHash)
        {
            return storedHash?.Trim() ?? string.Empty;
        }

        private static bool CompareHex(HashAlgorithm algorithm, string password, string storedHash)
        {
            using (algorithm)
            {
                byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                string hex = BitConverter.ToString(hash).Replace("-", "");
                return string.Equals(hex, storedHash, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static bool TryDecodeLegacyBase64(string value, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();
            try
            {
                string normalizedValue = NormalizeBase64Padding(value);
                bytes = Convert.FromBase64String(normalizedValue);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string NormalizeBase64Padding(string value)
        {
            string normalizedValue = NormalizeStoredHash(value);
            int remainder = normalizedValue.Length % 4;
            if (remainder == 0)
            {
                return normalizedValue;
            }

            return normalizedValue.PadRight(normalizedValue.Length + (4 - remainder), '=');
        }

        private static bool IsHexString(string value)
        {
            return value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }
    }
}
