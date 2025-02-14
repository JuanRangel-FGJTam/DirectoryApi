using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class NewPersonDocumentRequest
    {

        public string? Name {get;set;}

        public int? DocumentTypeId {get;set;}
        
        [DataType(DataType.Date)]
        public DateTime Validation {get;set;}

        public IFormFileCollection? File { get; set; }

    }
}