using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using WatchWpfClient.Model.Dtos;
using System.Linq;

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

        internal async Task<NewUserDto> GenerateKeys(NewUserDto newUserDto)
        {
            // New RSA class with randomly generated keys
            RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 2048;
            // Save public key as XML to new user payload
            newUserDto.PublicKey = rsa.ToXmlString(false);
            // Pass new user payload to serverless function which stores server side user details, including public key.
            // Get salted password hash returned from function to use as PKCS file encyption key
            var privateKeyPassword = await _functionProxy!.NewUser(newUserDto);
            // Write the private/public keys locally to PKCS file
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            Directory.CreateDirectory(path);
            var privateKey = rsa.ExportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)privateKeyPassword, 
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 500000));
            var keyFileName = newUserDto.Handle + ".p8";
            var fullPath = Path.Join(path, keyFileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            using (BinaryWriter binaryWriter = new(File.Open(fullPath, FileMode.Create)))
            {
                binaryWriter.Write(privateKey);
            }
            return newUserDto;
        }

        internal async Task<RSACryptoServiceProvider> PublicRsa(string handle)
        {
            string keyXml;
            keyXml = await _functionProxy.PublicKey(handle);

            RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 2048;
            rsa.FromXmlString(keyXml);
            return rsa;
        }

        internal async Task<bool> UpdatePrivateKeyPassword(string handle, string channelNumber, string newLoginPassword)
        {
            // Get current private key password
            var currentPrivateKeyPassword = await _functionProxy.PrivateKeyPassword(channelNumber, handle);
            // Load key into rsa and encrypt signature
            byte[] signature;
            var rsa = PrivateRsa(handle, currentPrivateKeyPassword);
            var plainBytes = _byteConverter.GetBytes($"{channelNumber}{handle}");
            signature = rsa.SignData(plainBytes, SHA256.Create());
            // Pass dto to function and receive new private key password
            var dto = new PasswordResetDto()
            {
                ChannelNumber = channelNumber,
                Handle = handle,
                Password = newLoginPassword,
                Signature = signature
            };
            var pkcsEncryptionKey = await _functionProxy.PasswordReset(dto);
            // Delete old PKCS file and rewrite with new private key password
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            Directory.CreateDirectory(path);
            var privateKey = rsa.ExportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)pkcsEncryptionKey,
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 500000));
            var keyFileName = handle + ".p8";
            var fullPath = Path.Join(path, keyFileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            using (BinaryWriter binaryWriter = new(File.Open(fullPath, FileMode.Create)))
            {
                binaryWriter.Write(privateKey);
            }
            return true;
        }

        internal static RSACryptoServiceProvider PrivateRsa(string handle, string password)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            var keyPath = Path.Join(path, handle + ".p8");

            RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 2048;
            byte[] keyBytes;
            using (BinaryReader binaryReader = new(File.Open(keyPath, FileMode.Open)))
            {
                keyBytes = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
            }
            rsa.ImportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)password, keyBytes, out _);
            return rsa;
        }

        internal string Decrypt(byte[] encryptedBytes, string handle, string password)
        {
            byte[] decryptedBytes;
            using (var rsa = PrivateRsa(handle, password))
            {
                decryptedBytes = rsa.Decrypt(encryptedBytes, true);
            }
            return _byteConverter.GetString(decryptedBytes);            
        }

        internal async Task<byte[]> Encrypt(string plainString, string handle)
        {
            byte[] encryptedBytes;
            using (var rsa = await PublicRsa(handle))
            {
                var plainBytes = _byteConverter.GetBytes(plainString);
                encryptedBytes = rsa.Encrypt(plainBytes, true);
            }
            return encryptedBytes;
        }
    }
}
