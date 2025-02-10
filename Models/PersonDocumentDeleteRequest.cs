using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class PersonDocumentDeleteRequest
    {
        public int DocumentId {get;set;}
        public string DocumentName {get;set;} = default!;

    }
}