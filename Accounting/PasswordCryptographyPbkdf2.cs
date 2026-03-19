using System;
using System.Security.Cryptography;
namespace Accounting
{

    public class PasswordCryptographyPbkdf2
    {
        /// <summary>
        /// Get the hash of the password
        /// </summary>
        /// <param name="password">string password</param>
        /// <returns>Hash secured password</returns>
        public string GetHashPassword(string password)
        {
            string hashPass = string.Empty;

            // 1.-Create the salt value with a cryptographic PRNG
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[20]);

            // 2.-Create the RFC2898DeriveBytes and get the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            // 3.-Combine the salt and password bytes for later use
            byte[] hashBytes = new byte[40];
            Array.Copy(salt, 0, hashBytes, 0, 20);
            Array.Copy(hash, 0, hashBytes, 20, 20);

            // 4.-Turn the combined salt+hash into a string for storage
            hashPass = Convert.ToBase64String(hashBytes);

            return hashPass;
        }

        /// <summary>
        /// Check if the password is valid
        /// </summary>
        /// <param name="password">Entered by user</param>
        /// <param name="hashPass">Stored password</param>
        /// <returns>True if is Valid.</returns>
        public bool IsValidPassword(string password, string hashPass)
        {
            bool result = true;

            // Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(hashPass);
            // Get the salt
            byte[] salt = new byte[20];
            Array.Copy(hashBytes, 0, salt, 0, 20);
            // Compute the hash on the password the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            // compare the results
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 20] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
