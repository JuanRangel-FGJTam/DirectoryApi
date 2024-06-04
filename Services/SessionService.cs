using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Data.Utils;
using AuthApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AuthApi.Services
{
    public class SessionService(ILogger<SessionService> logger, DirectoryDBContext directoryDBContext, IConfiguration configuration)
    {

        private readonly ILogger<SessionService> logger = logger;
        private readonly DirectoryDBContext directoryDBContext = directoryDBContext;
        private readonly IConfiguration configuration = configuration;
        private readonly TimeSpan sessionLifeTime = TimeSpan.FromHours(6);
        private readonly int tokenLength = 64;


        /// <summary>
        /// Create a new session record and retrive the token 
        /// </summary>
        /// <param name="person"></param>
        /// <param name="ipAddress"></param>
        /// <param name="userAgent"></param>
        /// <returns>String token calculated by hash the data</returns>
        public string StartPersonSession(Person person, string ipAddress, string userAgent ){
            
            var _now = DateTime.Now;

            // Verify if there is a session already stored with the same data
            var oldSession = this.directoryDBContext.Sessions
                .Where( s => s.Person.Id == person.Id )
                .Where( s => s.IpAddress == ipAddress )
                .Where( s => s.UserAgent == userAgent )
                .Where( s => s.DeletedAt == null)
                .Where( s => s.EndAt > DateTime.Now)
                .FirstOrDefault();

            if(oldSession != null){
                oldSession.EndAt = DateTime.Now.Add( this.sessionLifeTime);
                directoryDBContext.Sessions.Update( oldSession);
                directoryDBContext.SaveChanges();
                return oldSession.Token;
            }
            
            // Generate token
            var _tokenPayload = $"{person.Id}{ipAddress}{userAgent}{_now.ToString("yyyyMMddHHmmss")}";
            var token = HashData.GetHash(
                _tokenPayload,
                configuration["Secret"]!,
                mode: HashData.ConvertMode.Hex,
                length: tokenLength
            );
            
            // Create a session record
            var _record = new Session(){
                Person = person ,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                BegginAt = DateTime.Now,
                EndAt = _now.Add(sessionLifeTime),
                Token = token
            };
            directoryDBContext.Add(_record);
            directoryDBContext.SaveChanges();

            return _record.Token;
        }

        /// <summary>
        /// Validate the session and retrive the person assigned
        /// </summary>
        /// <param name="sessionToken"></param>
        /// <returns>Person</returns>
        /// <exception cref="SessionNotValid">The session token is not valid or expired</exception>
        public Person? GetPersonSession(string sessionToken){

            var session = this.directoryDBContext.Sessions
                .Where( s => s.DeletedAt == null)
                .Include( s => s.Person)
                .Where( s => s.Token == sessionToken)
                .FirstOrDefault();
            
            if( session != null){
                return session.Person;
            }
            return null;
        }

        public Session? ValidateSession( string sessionToken, string ipAddress, string? userAgent, out string message){

            var session = this.directoryDBContext.Sessions
                .Where( s => s.DeletedAt == null)
                .Where( s => s.Token == sessionToken)
                .FirstOrDefault();

            if(session == null){
                message = "El token no es valido";
                return null;
            }

            if( session.IpAddress != ipAddress){
                message = "Los datos de la sesión no coinciden";
                CloseTheSession(session.SessionID);
                return null;
            }

            if( session.EndAt < DateTime.Now){
                message = "La sesion a expirado";
                CloseTheSession(session.SessionID);
                return null;
            }

            message = "";
            return session;
        }

        public void CloseTheSession(string sessionId){
            var session = this.directoryDBContext.Sessions.Find(sessionId);
            if( session != null){
                session.DeletedAt = DateTime.Now;
                directoryDBContext.Sessions.Update( session);
                directoryDBContext.SaveChanges();
            }
        }

    }

    public class SessionNotValid : Exception {
        public SessionNotValid() : base() { }

        public SessionNotValid(string message) : base(message) { }

        public SessionNotValid(string message, Exception innerException) : base(message, innerException) { }

    }
}