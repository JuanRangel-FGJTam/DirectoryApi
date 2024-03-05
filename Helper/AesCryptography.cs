using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthApi.Data;

namespace AuthApi.Helper
{
    public class AesCryptographyService( string key ) : ICryptographyService
    {

        private byte[] aesKey = System.Text.Encoding.UTF8.GetBytes(key);

        public string EncryptData( string data )
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Key =  this.aesKey;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, null);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(
                    System.Text.Encoding.UTF8.GetBytes(data), 0, data.Length);
                return Convert.ToHexString(encryptedBytes);
            }
        }

        public string DecryptData(string encryptedData)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Key = this.aesKey;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, null);
                byte[] encryptedBytes = Convert.FromHexString(encryptedData);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }
        }

    }
}