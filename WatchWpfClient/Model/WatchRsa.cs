﻿using System;
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

        internal NewUserDto GenerateKeys(NewUserDto newUserDto)
        {
            RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 2048;
            var publicKey = rsa.ToXmlString(false);
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            Directory.CreateDirectory(path);
            var privateKey = rsa.ExportEncryptedPkcs8PrivateKey((ReadOnlySpan<char>)newUserDto.Password, 
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 256));
            var keyFileName = newUserDto.Handle + ".p8";
            var fullPath = Path.Join(path, keyFileName);
            if (File.Exists(fullPath)) File.Delete(fullPath);
            using (BinaryWriter binaryWriter = new(File.Open(fullPath, FileMode.Create)))
            {
                binaryWriter.Write(privateKey);
            }
            newUserDto.PublicKey = publicKey;
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

        internal static RSACryptoServiceProvider PrivateRsa(string handle, string password)
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"\Watch");
            var keyPath = Path.Join(path, handle + ".p8");
            var passwordBytes = password.Select(c => (byte)c).ToArray();

            RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 2048;
            byte[] keyBytes;
            using (BinaryReader binaryReader = new(File.Open(keyPath, FileMode.Open)))
            {
                keyBytes = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
            }
            rsa.ImportEncryptedPkcs8PrivateKey((ReadOnlySpan<byte>)passwordBytes, keyBytes, out _);
            return rsa;
        }

        internal string Decrypt(byte[] encryptedBytes, string handle, string password)
        {
            byte[] decryptedBytes;
            using (var rsa = PrivateRsa(handle, password))
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
            return encryptedBytes;
        }
    }
}
