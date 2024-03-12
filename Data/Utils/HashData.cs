using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Data.Utils
{
    public class HashData
    {
        public static string GetHash( string data, string saltString )
        {
            byte[] salt = Encoding.UTF8.GetBytes( saltString );
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: data,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8)
            );
           return hashed;
        }

        public static bool Validate( string data, string hashedData, string saltString )
        {
            string hashed =  GetHash( data, saltString);
            return data == hashedData;
        }

    }
}