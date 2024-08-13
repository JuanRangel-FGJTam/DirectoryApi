using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class NewProceedingRequest
    {

        [Required]
        public string? Name {get;set;}
        public string? Folio {get;set;}

        public string? Status {get;set;}

        public string? Area {get;set;}

        public string? CreatedAt {get;set;}

    }
}