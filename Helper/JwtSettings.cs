using System;

namespace AuthApi.Helper
{
    public class JwtSettings
    {
        public string Issuer {get;set;} = "";
        public string Audience {get;set;} = "";
        public string Key {get;set;} = "";
        public int LifeTimeDays {get;set;} = 30;
    }
}