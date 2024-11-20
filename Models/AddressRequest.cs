using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class AddressRequest
    {

        public Guid? PersonID {get;set;}
        
        public int CountryID {get;set;}
        
        public int StateID {get;set;}
        
        public int MunicipalityID {get;set;}

        public int? ColonyID {get;set;}
        
        public string? Street {get;set;}

        public string? Number {get;set;}
        
        public string? NumberInside {get;set;}

        public int ZipCode {get;set;}

    }
}