using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AuthApi.Entities;
using AuthApi.Helper;
using Newtonsoft.Json;


namespace AuthApi.Models
{
    public class PersonSearchResponse
    {
        public string Id {get;set;}

        public string? Rfc {get;set;}
        
        public string? Curp {get;set;}
        
        public string? FullName {get;set;}
        
        public string? Email {get;set;}
        
        public DateTime? Birthdate {get;set;}

        public string? Gender {get; set;}

        public PersonSearchResponse(Guid id){
            this.Id = id.ToString();
        }

        public PersonSearchResponse(string id){
            this.Id = id;
        }

        public static PersonSearchResponse FromEntity(Person person){
            var _person = new PersonSearchResponse(person.Id) {
                Id = person.Id.ToString(),
                FullName = $"{person.Name} {person.FirstName} {person.LastName}",
                Birthdate = person.Birthdate,
                Email = person.Email,
                Curp = person.Curp
            };

            if (person.Gender != null){
                _person.Gender = person.Gender.Name;
            }

            return _person;
        }
    }
}