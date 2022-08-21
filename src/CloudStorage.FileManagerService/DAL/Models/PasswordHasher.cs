using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CloudStorage.FileManagerService.DAL.Models
{
    public class PasswordHasher : IPasswordHasher<UserAuth>
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public string HashPassword(UserAuth user, string password)
        {
            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(SaltSize);
            var key = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            var outputBytes = new byte[1 + SaltSize + KeySize];
            outputBytes[0] = 1;
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(key, 0, outputBytes, 1 + SaltSize, KeySize);
            return Convert.ToBase64String(outputBytes);
        }

        public PasswordVerificationResult VerifyHashedPassword(UserAuth user, string hashedPassword, string providedPassword)
        {
            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(decodedHashedPassword, 1, salt, 0, SaltSize);

            var key = KeyDerivation.Pbkdf2(providedPassword, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            for (int i = 0; i < KeySize; ++i)
            {
                if (decodedHashedPassword[i + 1 + SaltSize] != key[i])
                {
                    return PasswordVerificationResult.Failed;
                }
            }

            return PasswordVerificationResult.Success;
        }
    }
}
