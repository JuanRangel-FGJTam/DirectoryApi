using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Services
{
    public class SessionService(ILogger<SessionService> logger, DirectoryDBContext directoryDBContext, ICryptographyService cryptographyService)
    {

        private readonly ILogger<SessionService> logger = logger;
        private readonly ICryptographyService cryptographyService = cryptographyService;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;

        private readonly TimeSpan sessionLifeTime = TimeSpan.FromHours(6);


        /// <summary>
        /// Create a new session record and retrive the token 
        /// </summary>
        /// <param name="person"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <returns>String token calculated by hash the data</returns>
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

        /// <summary>
        /// Validate the session and retrive the person assigned
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Person</returns>
        /// <exception cref="SessionNotValid">The session token is not valid or expired</exception>
        public Person? GetPersonSession(string token){
            var session = ValidateSession(token, out string message) ?? throw new SessionNotValid( message );
            return session.Person;
        }

        public Session? ValidateSession( string sessionToken, out string message){

            var session = this.directoryDBContext.Sessions
                .Include( s => s.Person)
                .Where( s => s.Token == sessionToken)
                .FirstOrDefault();

            if(session == null){
                message = "El token no es valido";
                return null;
            }

            if( session.EndAt > DateTime.Now){
                message = "La sesion a expirado";
                return null;
            }

            message = "";
            return session;
        }

    }

    public class SessionNotValid : Exception {
        public SessionNotValid() : base() { }

        public SessionNotValid(string message) : base(message) { }

        public SessionNotValid(string message, Exception innerException) : base(message, innerException) { }

    }
}