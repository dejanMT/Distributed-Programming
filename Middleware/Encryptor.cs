using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    public class Encryptor : IEncryptor
    {
        private const int SALT_SIZE = 40;
        private const int ITERATIONS_COUNT = 10000;

        // Get salt of 40 bytes and return it as a Base64 string
        public string GetSalt()
        {
            var saltBytes = new byte[SALT_SIZE];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        // Get hash of the value using PBKDF2 with SHA256 and the provided salt
        public string GetHash(string value, string salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(value, GetBytes(salt), ITERATIONS_COUNT, HashAlgorithmName.SHA256);

            return Convert.ToBase64String(pbkdf2.GetBytes(SALT_SIZE));
        }

        // Convert a string to a byte array by converting each character to its byte representation
        private static byte[] GetBytes(string value)
        {
            var bytes = new byte[value.Length + sizeof(char)];
            Buffer.BlockCopy(value.ToCharArray(), 0, bytes, 0, bytes.Length);

            return bytes;
        }
    }
}
