using System;

namespace AuthApi.Helper
{
    public class MinioSettings
    {
        public string Endpoint {get;set;}  = default!;
        public string AccessKey {get;set;} = default!;
        public string SecretKey {get;set;} = default!;
        public string BucketName {get;set;} = default!;
        public string BucketNameTmp {get;set;} = default!;
        public int ExpiryDuration {get; set;}

    }
}