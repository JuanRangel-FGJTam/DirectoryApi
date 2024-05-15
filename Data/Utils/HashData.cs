using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Data.Utils
{
    public class HashData
    {
        public static readonly int hash_iterationCount = 100000;
        public static readonly int hash_bytesLength = 32;

        public enum ConvertMode : ushort
        {
            Base64 = 0,
            Hex = 1
        }

        public static string GetHash( string data, string saltString, ConvertMode mode = ConvertMode.Base64, int? iterationCount = null, int? length = null  )
        {
            byte[] salt = Encoding.UTF8.GetBytes( saltString );
            var hashedBytes = KeyDerivation.Pbkdf2(
                password: data,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: HashData.hash_iterationCount,
                numBytesRequested: HashData.hash_bytesLength
            );

            if(mode == ConvertMode.Base64){
                return Convert.ToBase64String(hashedBytes);
            }else{
                return Convert.ToHexString(hashedBytes);
            }
        }

        public static bool Validate( string data, string hashedData, string saltString, ConvertMode mode = ConvertMode.Base64, int? iterationCount = null, int? length = null )
        {
            string hashed =  GetHash( data, saltString, mode, iterationCount, length);
            return data == hashedData;
        }

    }
}