using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
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
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, null);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(
                    System.Text.Encoding.UTF8.GetBytes(data), 0, System.Text.Encoding.UTF8.GetBytes(data).Length);
                return Convert.ToHexString(encryptedBytes);
            }
        }

        public string DecryptData(string encryptedData)
        {
            using (Aes aesAlg = Aes.Create())
            {
                try{
                    aesAlg.Mode = CipherMode.ECB;
                    aesAlg.Key = this.aesKey;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, null);
                    byte[] encryptedBytes = Convert.FromHexString(encryptedData);
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return System.Text.Encoding.UTF8.GetString(decryptedBytes);
                }catch(Exception){
                    return "******";
                }
            }
        }
 
        public static string GetHash( string data, string saltString )
        {
            byte[] salt = Encoding.UTF8.GetBytes( saltString );
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: data,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: AuthApi.Data.Utils.HashData.hash_iterationCount,
                numBytesRequested: AuthApi.Data.Utils.HashData.hash_bytesLength )
            );
           return hashed;
        }

        public static bool Validate( string data, string hashedData, string saltString )
        {
            string hashed =  GetHash( data, saltString);
            return data == hashedData;
        }

        public string HashData(string data)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: data,
                salt: aesKey,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: AuthApi.Data.Utils.HashData.hash_iterationCount,
                numBytesRequested: AuthApi.Data.Utils.HashData.hash_bytesLength )
            );
           return hashed;
        }
        public bool Validate(string data, string hashedData)
        {
            string hashed = HashData( data );
            return data == hashedData;
        }
    }
}