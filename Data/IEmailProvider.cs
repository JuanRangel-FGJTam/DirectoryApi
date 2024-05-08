using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Data
{
    public interface IEmailProvider
    {

        public Task<string> SendEmail( string emailDestination, string subject, object data);
        
    }
}