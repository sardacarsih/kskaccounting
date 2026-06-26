using System;
using System.Globalization;
using System.Security.Cryptography;
using Accounting.Models.Login;

namespace Accounting
{
    public class PasswordCryptographyPbkdf2
    {
        private const string FormatMarker = "PBKDF2";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int IterationCount = 600000;
        private const int LegacySaltSize = 20;
        private const int LegacyHashSize = 20;
        private const int LegacyIterationCount = 100000;
        private static readonly HashAlgorithmName Pbkdf2Algorithm = HashAlgorithmName.SHA256;
        private static readonly HashAlgorithmName LegacyAlgorithm = HashAlgorithmName.SHA1;

        public string GetHashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            byte[] hash = ComputeHash(password, salt, IterationCount, Pbkdf2Algorithm, HashSize);

            return string.Join(
                "$",
                FormatMarker,
                Pbkdf2Algorithm.Name,
                IterationCount.ToString(CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public bool IsValidPassword(string password, string hashPass)
        {
            return VerifyPassword(password, hashPass).IsValid;
        }

        public PasswordVerificationResult VerifyPassword(string password, string hashPass)
        {
            string normalizedHash = PasswordHashMigrator.NormalizeStoredHash(hashPass);
            if (string.IsNullOrWhiteSpace(normalizedHash))
            {
                return new PasswordVerificationResult(false, false);
            }

            if (TryParseVersionedHash(normalizedHash, out HashAlgorithmName algorithm, out int iterations, out byte[] salt, out byte[] storedHash))
            {
                byte[] computedHash = ComputeHash(password, salt, iterations, algorithm, storedHash.Length);
                bool isValid = CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
                bool needsRehash =
                    isValid &&
                    (!string.Equals(algorithm.Name, Pbkdf2Algorithm.Name, StringComparison.OrdinalIgnoreCase) ||
                     iterations < IterationCount ||
                     salt.Length < SaltSize ||
                     storedHash.Length < HashSize);

                return new PasswordVerificationResult(isValid, needsRehash);
            }

            if (TryVerifyLegacyPbkdf2(password, normalizedHash))
            {
                return new PasswordVerificationResult(true, true);
            }

            return new PasswordVerificationResult(false, false);
        }

        private static byte[] ComputeHash(string password, byte[] salt, int iterations, HashAlgorithmName algorithm, int hashSize)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, algorithm);
            return pbkdf2.GetBytes(hashSize);
        }

        private static bool TryParseVersionedHash(string hashPass, out HashAlgorithmName algorithm, out int iterations, out byte[] salt, out byte[] storedHash)
        {
            algorithm = default;
            iterations = 0;
            salt = Array.Empty<byte>();
            storedHash = Array.Empty<byte>();

            hashPass = PasswordHashMigrator.NormalizeStoredHash(hashPass);
            string[] parts = hashPass.Split('$');
            if (parts.Length != 5 || !string.Equals(parts[0], FormatMarker, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                algorithm = new HashAlgorithmName(parts[1]);
                salt = Convert.FromBase64String(parts[3]);
                storedHash = Convert.FromBase64String(parts[4]);
                return salt.Length > 0 && storedHash.Length > 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool TryVerifyLegacyPbkdf2(string password, string hashPass)
        {
            byte[] hashBytes;
            try
            {
                string normalizedHash = PasswordHashMigrator.NormalizeStoredHash(hashPass);
                int remainder = normalizedHash.Length % 4;
                if (remainder != 0)
                {
                    normalizedHash = normalizedHash.PadRight(normalizedHash.Length + (4 - remainder), '=');
                }

                hashBytes = Convert.FromBase64String(normalizedHash);
            }
            catch (FormatException)
            {
                return false;
            }

            if (hashBytes.Length != LegacySaltSize + LegacyHashSize)
            {
                return false;
            }

            byte[] salt = new byte[LegacySaltSize];
            Array.Copy(hashBytes, 0, salt, 0, LegacySaltSize);

            byte[] storedHash = new byte[LegacyHashSize];
            Array.Copy(hashBytes, LegacySaltSize, storedHash, 0, LegacyHashSize);

            byte[] computedHash = ComputeHash(password, salt, LegacyIterationCount, LegacyAlgorithm, LegacyHashSize);
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}
