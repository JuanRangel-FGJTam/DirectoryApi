using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class PersonRequest
    {

        public string? Rfc {get;set;}
        
        [Required]
        public string? Curp {get;set;}
        
        [Required]
        public string? Name {get;set;}
        
        [Required]
        public string? FirstName {get;set;}
       
        [Required]
        public string? LastName {get;set;}
        
        [Required]
        [EmailAddress]
        public string? Email {get;set;}

        [Required]
        public DateTime? Birthdate {get;set;}

        [Range(1, 99)]
        public int? GenderId {get; set;}
        
        [Range(1, 99)]
        public int? MaritalStatusId {get;set;}
        
        [Range(1, 99)]
        public int? NationalityId {get;set;}
        
        [Range(1, 99)]
        public int? OccupationId {get;set;}

        public string? AppName {get;set;}

        public string? Password {get;set;}

        public string? ConfirmedPassword {get;set;}

    }
}