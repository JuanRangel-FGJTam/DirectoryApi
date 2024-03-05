using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Data
{
    public interface ICryptographyService
    {
        public string EncryptData( string data );
        public string DecryptData(string encryptedData);
    }
}