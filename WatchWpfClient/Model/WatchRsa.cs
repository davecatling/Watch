using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace WatchWpfClient.Model
{
    internal class WatchRsa
    {
        private readonly UnicodeEncoding _byteConverter = new();
        private readonly FunctionProxy _functionProxy;

        public WatchRsa(FunctionProxy proxy)
        {
            _functionProxy = proxy;
        }

        internal async Task<RSACryptoServiceProvider> PublicRsa(string handle)
        {
            string keyXml;
            keyXml = await _functionProxy.PublicKey(handle);

            RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(keyXml);
            return rsa;
        }

        internal static RSACryptoServiceProvider PrivateRsa(string handle)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            var keyXml = File.ReadAllText(Path.Join(path, handle + ".key"));

            RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(keyXml);
            return rsa;
        }

        internal string Decrypt(string encryptedString, string handle)
        {
            byte[] decryptedBytes;
            using (var rsa = PrivateRsa(handle))
            {
                var encyptedBytes = _byteConverter.GetBytes(encryptedString);
                decryptedBytes = rsa.Decrypt(encyptedBytes, false);
            }
            return _byteConverter.GetString(decryptedBytes);            
        }

        internal async Task<string> Encrypt(string plainString, string handle)
        {
            byte[] encryptedBytes;
            using (var rsa = await PublicRsa(handle))
            {
                var plainBytes = _byteConverter.GetBytes(plainString);
                encryptedBytes = rsa.Encrypt(plainBytes, false);
            }
            return _byteConverter.GetString(encryptedBytes);
        }
    }
}
