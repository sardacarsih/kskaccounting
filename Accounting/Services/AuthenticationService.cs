using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Accounting.Interface;
using Accounting.Models.Login;

namespace Accounting.Services
{
    internal static class AuthenticationService
    {
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public static LoginAuthResult AuthenticateForModule(IUsersManager repository, string userId, string password, string moduleName)
        {
            string normalizedUserId = userId?.Trim() ?? string.Empty;
            string clientMachine = Environment.MachineName;
            USERID_DTO? credential = repository.GetUserCredential(normalizedUserId);
            if (credential == null || string.IsNullOrWhiteSpace(credential.USERID))
            {
                RecordAudit(
                    repository,
                    normalizedUserId,
                    moduleName,
                    "LOGIN_REJECTED_INVALID_CREDENTIAL",
                    false,
                    "Login gagal dengan kredensial tidak valid.",
                    clientMachine);

                return new LoginAuthResult
                {
                    Status = LoginAuthStatus.UserNotFound
                };
            }

            DateTimeOffset? lockoutUntil = GetLockoutUntilUtc(credential);
            if (lockoutUntil.HasValue && lockoutUntil.Value > DateTimeOffset.UtcNow)
            {
                RecordAudit(
                    repository,
                    credential.USERID,
                    moduleName,
                    "LOGIN_REJECTED_LOCKED_OUT",
                    false,
                    "Login ditolak karena akun masih dalam masa lockout.",
                    clientMachine,
                    lockoutUntil);

                return new LoginAuthResult
                {
                    Status = LoginAuthStatus.LockedOut,
                    LockoutUntilUtc = lockoutUntil
                };
            }

            if (!string.Equals(credential.AKTIF, "Y", StringComparison.OrdinalIgnoreCase))
            {
                RecordAudit(
                    repository,
                    credential.USERID,
                    moduleName,
                    "LOGIN_REJECTED_INACTIVE_USER",
                    false,
                    "Login ditolak karena akun tidak aktif.",
                    clientMachine);

                return new LoginAuthResult { Status = LoginAuthStatus.InactiveUser };
            }

            PasswordVerificationResult verification = VerifyStoredPassword(password, credential.PASSWORD);
            if (!verification.IsValid)
            {
                repository.RegisterFailedLoginAttempt(credential.USERID, MaxFailedAttempts, (int)LockoutDuration.TotalMinutes);
                USERID_DTO? updatedCredential = repository.GetUserCredential(credential.USERID);
                DateTimeOffset? updatedLockoutUntil = GetLockoutUntilUtc(updatedCredential);
                string eventType = updatedLockoutUntil.HasValue && updatedLockoutUntil.Value > DateTimeOffset.UtcNow
                    ? "LOGIN_REJECTED_LOCKED_OUT"
                    : "LOGIN_REJECTED_INVALID_CREDENTIAL";
                string detailMessage = updatedLockoutUntil.HasValue && updatedLockoutUntil.Value > DateTimeOffset.UtcNow
                    ? "Login gagal dan akun masuk masa lockout."
                    : "Login gagal dengan kredensial tidak valid.";

                RecordAudit(
                    repository,
                    credential.USERID,
                    moduleName,
                    eventType,
                    false,
                    detailMessage,
                    clientMachine,
                    updatedLockoutUntil);

                return new LoginAuthResult
                {
                    Status = updatedLockoutUntil.HasValue && updatedLockoutUntil.Value > DateTimeOffset.UtcNow
                        ? LoginAuthStatus.LockedOut
                        : LoginAuthStatus.InvalidPassword,
                    LockoutUntilUtc = updatedLockoutUntil
                };
            }

            if (verification.NeedsRehash)
            {
                string newHash = new PasswordCryptographyPbkdf2().GetHashPassword(password);
                repository.ResetPassword(credential.USERID, newHash);
            }

            bool hasRbacModuleAccess = repository.HasRbacModuleAccess(credential.USERID, moduleName);
            bool hasLegacyModuleAccess = false;
            if (!hasRbacModuleAccess)
            {
                hasLegacyModuleAccess = repository.HasLegacyModuleAccess(credential.USERID, moduleName);
            }

            if (!hasRbacModuleAccess && !hasLegacyModuleAccess)
            {
                RecordAudit(
                    repository,
                    credential.USERID,
                    moduleName,
                    "LOGIN_REJECTED_NO_MODULE_ACCESS",
                    false,
                    "Kredensial valid tetapi tidak memiliki akses modul.",
                    clientMachine);

                return new LoginAuthResult { Status = LoginAuthStatus.NoModuleAccess };
            }

            List<LOGIN_USERS_DTO> users = repository.UserLoginByIDDATA(credential.USERID, moduleName);
            bool usedLegacyAccessPath = users.Any(user => user.IsLegacyAccessPath);
            if (users.Count == 0)
            {
                RecordAudit(
                    repository,
                    credential.USERID,
                    moduleName,
                    "LOGIN_REJECTED_NO_LOCATION_ACCESS",
                    false,
                    "Kredensial valid tetapi tidak memiliki akses lokasi.",
                    clientMachine);

                return new LoginAuthResult { Status = LoginAuthStatus.NoLocationAccess };
            }

            repository.UpdateSuccessfulLogin(credential.USERID);
            RecordAudit(
                repository,
                credential.USERID,
                moduleName,
                "LOGIN_AUTHENTICATED",
                true,
                users.Count == 1
                    ? $"Autentikasi berhasil. Lokasi aktif: {users[0].IDDATA}.{(usedLegacyAccessPath ? " Akses memakai jalur legacy." : string.Empty)}"
                    : $"Autentikasi berhasil. Menunggu pemilihan lokasi aktif.{(usedLegacyAccessPath ? " Akses memakai jalur legacy." : string.Empty)}",
                clientMachine,
                idData: users.Count == 1 ? users[0].IDDATA : string.Empty);

            return new LoginAuthResult
            {
                Status = LoginAuthStatus.Success,
                Users = users,
                RequiresPasswordChange = string.Equals(credential.PASSWORD_RESET_REQUIRED, "Y", StringComparison.OrdinalIgnoreCase)
            };
        }

        public static bool TryChangePassword(IUsersManager repository, string userId, string currentPassword, string newPassword, out string validationMessage)
        {
            if (!TryValidatePasswordPolicy(newPassword, out validationMessage))
            {
                return false;
            }

            USERID_DTO? credential = repository.GetUserCredential(userId);
            if (credential == null || string.IsNullOrWhiteSpace(credential.USERID))
            {
                validationMessage = "User tidak ditemukan.";
                return false;
            }

            PasswordVerificationResult verification = VerifyStoredPassword(currentPassword, credential.PASSWORD);
            if (!verification.IsValid)
            {
                validationMessage = "Password lama tidak valid.";
                return false;
            }

            UpdatePassword(repository, credential.USERID, newPassword, false, "PASSWORD_CHANGED_SELF", LoginInfo.MODULE);
            validationMessage = string.Empty;
            return true;
        }

        public static bool TryChangePasswordAfterVerifiedLogin(IUsersManager repository, string userId, string newPassword, out string validationMessage)
        {
            if (!TryValidatePasswordPolicy(newPassword, out validationMessage))
            {
                return false;
            }

            UpdatePassword(repository, userId, newPassword, false, "PASSWORD_CHANGED_REQUIRED_AFTER_RESET", LoginInfo.MODULE);
            validationMessage = string.Empty;
            return true;
        }

        public static string AdminResetPassword(IUsersManager repository, string userId, string moduleName)
        {
            USERID_DTO? credential = repository.GetUserCredential(userId);
            if (credential == null || string.IsNullOrWhiteSpace(credential.USERID))
            {
                throw new InvalidOperationException($"User '{userId}' tidak ditemukan.");
            }

            string temporaryPassword = GenerateTemporaryPassword();
            UpdatePassword(repository, credential.USERID, temporaryPassword, true, "PASSWORD_RESET_ADMIN", moduleName);
            repository.ResetLoginFailures(credential.USERID);
            return temporaryPassword;
        }

        public static void RecordLocationSelection(IUsersManager repository, string userId, string moduleName, string idData)
        {
            RecordAudit(
                repository,
                userId,
                moduleName,
                "LOGIN_LOCATION_SELECTED",
                true,
                $"Lokasi aktif dipilih: {idData}.",
                Environment.MachineName,
                idData: idData);
        }

        public static bool TryValidatePasswordPolicy(string password, out string validationMessage)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                validationMessage = "Password wajib diisi.";
                return false;
            }

            if (password.Length < 6)
            {
                validationMessage = "Password minimal 6 karakter.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }

        private static PasswordVerificationResult VerifyStoredPassword(string password, string storedHash)
        {
            PasswordCryptographyPbkdf2 passwordCryptography = new();
            PasswordVerificationResult pbkdf2Result = passwordCryptography.VerifyPassword(password, storedHash);
            if (pbkdf2Result.IsValid)
            {
                return pbkdf2Result;
            }

            if (!PasswordHashMigrator.NeedsMigration(storedHash))
            {
                return new PasswordVerificationResult(false, false);
            }

            PasswordFormat format = PasswordHashMigrator.DetectFormat(storedHash);
            bool isValidLegacy = PasswordHashMigrator.VerifyLegacyPassword(password, storedHash, format);
            return new PasswordVerificationResult(isValidLegacy, isValidLegacy);
        }

        private static DateTimeOffset? GetLockoutUntilUtc(USERID_DTO? credential)
        {
            if (credential?.LOCKOUT_UNTIL_UTC == null)
            {
                return null;
            }

            DateTime lockoutUtc = DateTime.SpecifyKind(credential.LOCKOUT_UNTIL_UTC.Value, DateTimeKind.Utc);
            return new DateTimeOffset(lockoutUtc);
        }

        private static void UpdatePassword(IUsersManager repository, string userId, string plainPassword, bool requirePasswordChange, string auditEventType, string moduleName = "")
        {
            PasswordCryptographyPbkdf2 crypto = new();
            string hashedPassword = crypto.GetHashPassword(plainPassword);
            repository.ResetPassword(userId, hashedPassword);
            repository.SetPasswordResetRequired(userId, requirePasswordChange);

            RecordAudit(
                repository,
                userId,
                moduleName,
                auditEventType,
                true,
                requirePasswordChange
                    ? "Password sementara baru diterbitkan dan wajib diganti saat login berikutnya."
                    : "Password berhasil diperbarui.",
                Environment.MachineName);
        }

        private static string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@$%*-_";
            string allChars = upper + lower + digits + symbols;

            char[] buffer =
            [
                upper[RandomNumberGenerator.GetInt32(upper.Length)],
                lower[RandomNumberGenerator.GetInt32(lower.Length)],
                digits[RandomNumberGenerator.GetInt32(digits.Length)],
                symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
            ];

            char[] password = new char[16];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            buffer.CopyTo(password, 0);
            for (int i = password.Length - 1; i > 0; i--)
            {
                int swapIndex = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
            }

            return new string(password);
        }

        private static void RecordAudit(
            IUsersManager repository,
            string userId,
            string moduleName,
            string eventType,
            bool success,
            string detailMessage,
            string clientMachine,
            DateTimeOffset? lockoutUntil = null,
            string idData = "")
        {
            repository.RecordLoginAudit(new LoginAuditRecord
            {
                USERID = userId ?? string.Empty,
                MODULE_NAME = moduleName ?? string.Empty,
                EVENT_TYPE = eventType,
                SUCCESS_FLAG = success ? "Y" : "N",
                CLIENT_MACHINE = clientMachine ?? string.Empty,
                DETAIL_MESSAGE = detailMessage ?? string.Empty,
                LOCKOUT_UNTIL_UTC = lockoutUntil?.UtcDateTime,
                IDDATA = idData ?? string.Empty
            });
        }
    }
}
