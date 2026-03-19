using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Accounting
{
    internal enum PasswordFormat
    {
        Pbkdf2,
        Md5Hex,
        Sha1Hex,
        Sha256Hex,
        Plaintext
    }

    internal static class PasswordHashMigrator
    {
        public static bool NeedsMigration(string storedHash)
        {
            return DetectFormat(storedHash) != PasswordFormat.Pbkdf2;
        }

        public static PasswordFormat DetectFormat(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return PasswordFormat.Plaintext;

            // PBKDF2: 56 Base64 chars that decode to exactly 40 bytes
            if (storedHash.Length == 56 && IsValidBase64(storedHash, out int byteLen) && byteLen == 40)
                return PasswordFormat.Pbkdf2;

            // Hex-based hashes
            if (IsHexString(storedHash))
            {
                return storedHash.Length switch
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
            switch (format)
            {
                case PasswordFormat.Plaintext:
                    return string.Equals(password, storedHash, StringComparison.Ordinal);

                case PasswordFormat.Md5Hex:
                    if (CompareHex(MD5.Create(), password, storedHash)) return true;
                    return string.Equals(password, storedHash, StringComparison.Ordinal);

                case PasswordFormat.Sha1Hex:
                    if (CompareHex(SHA1.Create(), password, storedHash)) return true;
                    return string.Equals(password, storedHash, StringComparison.Ordinal);

                case PasswordFormat.Sha256Hex:
                    if (CompareHex(SHA256.Create(), password, storedHash)) return true;
                    return string.Equals(password, storedHash, StringComparison.Ordinal);

                default:
                    return false;
            }
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

        private static bool IsValidBase64(string value, out int byteLength)
        {
            byteLength = 0;
            try
            {
                byte[] bytes = Convert.FromBase64String(value);
                byteLength = bytes.Length;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IsHexString(string value)
        {
            return value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
        }
    }
}
