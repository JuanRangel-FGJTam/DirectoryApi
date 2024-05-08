using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;

namespace AuthApi.Helper
{
    public class PreregisterToken
    {
        public static string GenerateToken(Preregistration register, ICryptographyService cryptographyService){
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append(register.Id.ToString().Trim());
            stringBuilder.Append(register.Mail!.ToString().Trim());
            stringBuilder.Append(DateTime.Now.ToString("yyyyMMddHHss"));
            
            return cryptographyService.EncryptData( stringBuilder.ToString());
        }
    }
}