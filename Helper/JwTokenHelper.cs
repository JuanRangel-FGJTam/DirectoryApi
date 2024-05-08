using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Helper
{
    public class JwTokenHelper
    {

        public static async Task<string> GenerateJwtToken(User user, JwtSettings jwtSettings)
        {
            var TokenLifetime = TimeSpan.FromDays( jwtSettings.LifeTimeDays);

            //Generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = await Task.Run(() =>
            {

                var tokenKey = Encoding.ASCII.GetBytes( jwtSettings.Key );

                var claims = new List<Claim>{
                    new("userId", user.Id.ToString()),
                    new( JwtRegisteredClaimNames.Email, user.Email),
                    new( JwtRegisteredClaimNames.Name, string.Join(" ", new []{user.FirstName, user.LastName} ) )
                };

                var tokenDescriptor = new SecurityTokenDescriptor{
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.Add(TokenLifetime),
                    Issuer = jwtSettings.Issuer,
                    Audience = jwtSettings.Audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                return tokenHandler.CreateToken( tokenDescriptor);
            });

            return tokenHandler.WriteToken(token);
        }

        public static async Task<string> GenerateJwtToken(IDictionary<string,string> claims, JwtSettings jwtSettings, TimeSpan? customLifeTime = null)
        {
            var TokenLifetime = TimeSpan.FromDays( jwtSettings.LifeTimeDays);

            //Generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = await Task.Run(() =>
            {

                var tokenKey = Encoding.ASCII.GetBytes( jwtSettings.Key );

                IEnumerable<Claim> _claims = claims.Select(item => new Claim(item.Key, item.Value)).ToList<Claim>();

                var tokenDescriptor = new SecurityTokenDescriptor{
                    Subject = new ClaimsIdentity(_claims),
                    Expires = DateTime.UtcNow.Add( customLifeTime ?? TokenLifetime),
                    Issuer = jwtSettings.Issuer,
                    Audience = jwtSettings.Audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                return tokenHandler.CreateToken( tokenDescriptor);
            });

            return tokenHandler.WriteToken(token);
        }
    }
}