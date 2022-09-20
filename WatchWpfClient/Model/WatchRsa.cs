using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using WatchWpfClient.Model.Dtos;

namespace WatchWpfClient.Model
{
    internal class WatchRsa
    {
        private readonly UnicodeEncoding _byteConverter = new();
        private readonly FunctionProxy _functionProxy;

        internal WatchRsa(FunctionProxy proxy)
        {
            _functionProxy = proxy;
        }

        internal NewUserDto GenerateKeys(NewUserDto newUserDto)
        {
            RSACryptoServiceProvider rsa = new();
            var publicKey = rsa.ToXmlString(false);
            var privateKey = rsa.ToXmlString(true);
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            Directory.CreateDirectory(path);
            var keyFileName = newUserDto.Handle + ".key";
            var fullPath = Path.Join(path, keyFileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            using (var textWriter = new StreamWriter(fullPath))
            {
                textWriter.Write(privateKey);
            }
            newUserDto.PublicKey = publicKey;
            return newUserDto;
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

        internal string Decrypt(byte[] encryptedBytes, string handle)
        {
            byte[] decryptedBytes;
            using (var rsa = PrivateRsa(handle))
            {
                decryptedBytes = rsa.Decrypt(encryptedBytes, false);
            }
            return _byteConverter.GetString(decryptedBytes);            
        }

        internal async Task<byte[]> Encrypt(string plainString, string handle)
        {
            byte[] encryptedBytes;
            using (var rsa = await PublicRsa(handle))
            {
                var plainBytes = _byteConverter.GetBytes(plainString);
                encryptedBytes = rsa.Encrypt(plainBytes, false);
            }
            using (var rsa = PrivateRsa(handle))
            {
                var plainBytes = rsa.Decrypt(encryptedBytes, false);
                var plainText = _byteConverter.GetString(plainBytes);
            }
            return encryptedBytes;
        }
    }
}
