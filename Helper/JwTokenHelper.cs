using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthApi.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Helper
{
    public class JwTokenHelper
    {
        public static async Task<string> GenerateJwtToken(User user, string secret)
        {
            //Generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = await Task.Run(() =>
            {

                var key = Encoding.ASCII.GetBytes( secret );

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity( new[] {
                        new Claim("id", user.Id.ToString()),
                    }),
                    Expires = DateTime.UtcNow.AddMonths(3),
                    SigningCredentials = new SigningCredentials( new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                return tokenHandler.CreateToken(tokenDescriptor);
            });

            return tokenHandler.WriteToken(token);
        }
    }
}