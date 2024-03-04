using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class AddressRequest
    {

        [Required]
        public Guid PersonID {get;set;}
        
        [Required]
        [Range(1, 999)]
        public int CountryID {get;set;}
        
        [Required]
        [Range(1, 999)]
        public int StateID {get;set;}
        
        [Required]
        [Range(1, 999)]
        public int MunicipalityID {get;set;}
       
        [Required]
        [Range(1, 999)]
        public int? ColonyID {get;set;}
        
        [Required]
        public string? Street {get;set;}

        [Required]
        public string Number {get;set;} = null!;
        
        public string? NumberInside {get;set;} = null!;

        [Required]
        public int ZipCode {get;set;}

    }
}