using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;


namespace AuthApi.Helper
{
    public class CustomDateConverter : IsoDateTimeConverter{
        public CustomDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
