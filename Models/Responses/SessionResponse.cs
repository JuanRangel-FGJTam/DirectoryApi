using System;
using AuthApi.Entities;

namespace AuthApi.Models.Responses {
    public class SessionResponse {
        public string SessionID {get;set;} = null!;
        public Person Person {get;set;} = null!;
        public string? IpAddress {get;set;}
        public string? UserAgent {get;set;}

        public DateTime BegginAt {get;set;}
        public DateTime? EndAt {get;set;}
        
        public DateTime? DeletedAt {get;set;}
        
        public bool IsDeleted {
            get => DeletedAt != null;
        }

        public static SessionResponse FromEntity(Session session)
        {
            var sessionResponse = new SessionResponse()
            {
                SessionID = session.SessionID,
                Person = session.Person,
                IpAddress = session.IpAddress,
                UserAgent = session.UserAgent,
                BegginAt = session.BegginAt,
                EndAt = session.EndAt,
                DeletedAt = session.DeletedAt,
            };
            return sessionResponse;
        }

    }
}