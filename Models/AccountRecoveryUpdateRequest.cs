using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AccountRecoveryUpdateRequest
    {
        public int TemplateId {get;set;}
        public int? NotifyEmail {get;set;}
        public string? ResponseComments {get;set;}
    }
}