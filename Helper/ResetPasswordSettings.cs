using System;

namespace AuthApi.Helper
{
    public class ResetPasswordSettings
    {
        public long TokenLifeTimeSeconds {get;set;}
        public string DestinationUrl {get;set;} = "";
    }
}