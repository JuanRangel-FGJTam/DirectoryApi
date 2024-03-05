using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class PersonRequest
    {

        [Required]
        public string Rfc {get;set;} = "";
        
        [Required]
        public string Curp {get;set;} = "";
        
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
        public DateTime? BirthDate {get;set;}

        [Required]
        [Range(1, 99)]
        public int GenderID {get; set;}
        
        [Range(1, 99)]
        public int MaritalStatusID {get;set;}
        
        [Range(1, 99)]
        public int NationalityID {get;set;}
        
        [Range(1, 99)]
        public int OccupationID {get;set;}

    }
}