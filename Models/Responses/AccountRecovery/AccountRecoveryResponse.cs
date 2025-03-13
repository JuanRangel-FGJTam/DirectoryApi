using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;
using AuthApi.Services;

namespace AuthApi.Models
{
    public class AccountRecoveryResponse( Guid id)
    {
        public Guid Id {get;set;} = id;
        public Guid? PersonId {get;set;}
        public string Name {get;set;} = default!;
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public DateTime? BirthDate {get;set;}
        public string? GenderName {get;set;}
        public int? GenderId {get;set;}
        public string? Curp {get;set;}
        public string? Rfc {get;set;}
        public string? NationalityName {get;set;} = default!;
        public int? NationalityId {get;set;} = default!;
        public string? OccupationName {get;set;} = default!;
        public int? OccupationId {get;set;} = default!;
        public string? MaritalStatusName {get;set;} = default!;
        public int? MaritalStatusId {get;set;} = default!;
        public string? ContactEmail {get;set;}
        public string? ContactEmail2 {get;set;}
        public string? ContactPhone {get;set;}
        public string? ContactPhone2 {get;set;}
        public string? RequestComments {get;set;}
        public string? ResponseComments {get;set;}

        public DateTime CreatedAt {get;set;} = default!;
        public DateTime? AttendingAt {get;set;}
        public DateTime? DeletedAt {get;set;}
        public string? UserAttended {get;set;}
        public int? UserAttendedId {get;set;}
        public string? UserDeleted {get;set;}
        public int? UserDeletedId {get;set;}
        public int TotalDocuments {get;set;}

        public string? EmailNotificationResponse {get;set;}
        public string? EmailNotificationContent {get;set;}

        public IEnumerable<AccountRecoveryFileResponse> Files {get;set;} = Array.Empty<AccountRecoveryFileResponse>();

        public static AccountRecoveryResponse FromEntity(AccountRecovery p){
            var item = new AccountRecoveryResponse(p.Id)
            {
                PersonId  = p.PersonId,
                Name = p.Name,
                FirstName = p.FirstName,
                LastName = p.LastName,
                BirthDate = p.BirthDate,
                Curp = p.Curp,
                Rfc = p.Rfc,
                ContactEmail = p.ContactEmail,
                ContactEmail2 = p.ContactEmail2,
                ContactPhone = p.ContactPhone,
                ContactPhone2 = p.ContactPhone2,
                RequestComments = p.RequestComments,
                ResponseComments = p.ResponseComments,
                CreatedAt = p.CreatedAt,
                AttendingAt = p.AttendingAt,
                DeletedAt = p.DeletedAt
            };

            if( p.Files != null){
                item.TotalDocuments = p.Files.Count;
                if(p.Files?.Count > 0){
                    item.Files = p.Files.Select( f => AccountRecoveryFileResponse.FromEntity(f));
                }
            }
            
            if ( p.Gender != null){
                item.GenderName = p.Gender.Name;
                item.GenderId = p.Gender.Id;
            }
            
            if(p.Nationality != null){
                item.NationalityName = p.Nationality.Name;
                item.NationalityId = p.Nationality.Id;
            }

            if(p.Occupation != null){
                item.OccupationName = p.Occupation.Name;
                item.OccupationId = p.Occupation.Id;
            }

            if(p.MaritalStatus != null){
                item.MaritalStatusName = p.MaritalStatus.Name;
                item.MaritalStatusId = p.MaritalStatus.Id;
            }

            if(p.UserAttended != null)
            {
                item.UserAttended = p.UserAttended.FirstName + " " + p.UserAttended.LastName;
                item.UserAttendedId = p.UserAttended.Id;
            }

            if(p.UserDeleted != null)
            {
                item.UserDeleted = p.UserDeleted.FirstName + " " + p.UserDeleted.LastName;
                item.UserDeletedId = p.UserDeleted.Id;
            }

            item.EmailNotificationResponse = p.NotificationEmailResponse;
            item.EmailNotificationContent = p.NotificationEmailContent;
            return item;
        }
    }
}