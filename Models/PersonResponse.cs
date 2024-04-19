using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class PersonResponse
    {
        public string PersonId {get;set;}

        public string? Rfc {get;set;}
        
        public string? Curp {get;set;}
        
        public string? Name {get;set;}
        
        public string? FirstName {get;set;}
       
        public string? LastName {get;set;}
        
        public string? Email {get;set;}

        public DateTime? Birthdate {get;set;}

        public int? GenderId {get; set;}
        public string? GenderName {get; set;}
        
        public int? MaritalStatusId {get;set;}
        public string? MaritalStatusName {get;set;}
        
        public int? NationalityId {get;set;}
        public string? NationalityName {get;set;}
        
        public int? OccupationId {get;set;}
        public string? OccupationName{get;set;}

        public string? AppName {get;set;}

        public string FullName {
            get {
                return string.Join(' ', new string[]{Name??"", FirstName??"", LastName??""});
            }
        }

        public string? BirthdateFormated {
            get{
                try
                {
                    return Birthdate!.Value.ToString("dd MMM yyyy").ToUpper();
                }
                catch (System.Exception) {
                    return null;
                }
            }
        }


        public PersonResponse( Guid personId ){
            this.PersonId = personId.ToString();
        }

        public PersonResponse( string personId ){
            this.PersonId = personId;
        }


        public static PersonResponse FromEntity(Person person){
            var _person = new PersonResponse(person.Id)
            {
                Rfc = person.Rfc,
                Curp = person.Curp,
                Name = person.Name,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                Birthdate = person.Birthdate,
                AppName = person.AppOwner
            };

            if (person.Gender != null){
                _person.GenderId = person.Gender.Id;
                _person.GenderName = person.Gender.Name;
            }

            if(person.MaritalStatus != null){
                _person.MaritalStatusId = person.MaritalStatus.Id;
                _person.MaritalStatusName = person.MaritalStatus.Name;
            }

            if(person.Nationality != null){
                _person.NationalityId = person.Nationality.Id;
                _person.NationalityName = person.Nationality.Name;
            }

            if(person.Occupation != null){
                _person.OccupationId = person.Occupation.Id;
                _person.OccupationName = person.Occupation.Name;
            }

            return _person;
        }
    }
}