using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;

namespace AuthApi.Services
{
    public class SessionService(ILogger<SessionService> logger, DirectoryDBContext directoryDBContext, ICryptographyService cryptographyService)
    {

        private readonly ILogger<SessionService> logger = logger;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;

        private readonly TimeSpan sessionLifeTime = TimeSpan.FromHours(6);


        public string StartPersonSession(Person person, string ipAddress, string userAgent ){
            
            var _now = DateTime.Now;
            var _tokenPayload = $"{person.Id}{ipAddress}{userAgent}{_now.ToString("yyyyMMddHHmmss")}";
            
            // Create a session record
            var _record = new Session(){
                Person = person ,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                BegginAt = DateTime.Now,
                EndAt = _now.Add(sessionLifeTime),
                Token = cryptographyService.HashData(_tokenPayload)
            };
            directoryDBContext.Add(_record);
            directoryDBContext.SaveChanges();

            return _record.Token;
        }

    }
}