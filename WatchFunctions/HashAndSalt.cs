using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WatchFunctions
{
    public static class HashAndSalt
    {
        public static byte[] GenerateSalt()
        {
            var buffer = new byte[20];
            RandomNumberGenerator.Create().GetBytes(buffer);
            return buffer;
        }

        public static byte[] GetHash(string password, byte[] salt)
        {
            var passWord = Encoding.UTF8.GetBytes(password);
            var saltedValue = passWord.Concat(salt).ToArray();
            return SHA256.Create().ComputeHash(saltedValue);
        }
    }
}
