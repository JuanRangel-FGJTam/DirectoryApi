using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ValidateRegisterRequest
    {

        public string? Rfc {get;set;}
        
        // [Required(ErrorMessage = "El curp es requerido")]
        public string? Curp {get;set;}
        
        [Required(ErrorMessage = "El nombre es requerido")]
        public string? Name {get;set;}
        
        public string? FirstName {get;set;}

        public string? LastName {get;set;}
        
        [Required(ErrorMessage = "La fecha de nacimiento es requerido")]
        public DateTime? Birthdate {get;set;}

        [Range(1, 99)]
        public int? GenderId {get; set;}
        
        [Range(1, 999)]
        public int? MaritalStatusId {get;set;}
        
        [Range(1, 999)]
        public int? NationalityId {get;set;}
        
        [Range(1, 999)]
        public int? OccupationId {get;set;}

        public string? AppName {get;set;}
        
        [Required (ErrorMessage = "El codigo de pre-registro es requerido")]
        public string? Token {get;set;}

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }
    }
}