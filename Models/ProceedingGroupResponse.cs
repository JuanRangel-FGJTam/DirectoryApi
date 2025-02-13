using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;
using AuthApi.Services;

namespace AuthApi.Models
{
    public class ProceedingGroupResponse()
    {
        public string Folio {get;set;} = default!;
        public string Name {get;set;} = default!;
        public DateTime? FirtRegister {get;set;}
        public DateTime? LastModification {get;set;}
        public IEnumerable<ProceedingResponse> Proceedings {get;set;} = Array.Empty<ProceedingResponse>();

    }
}