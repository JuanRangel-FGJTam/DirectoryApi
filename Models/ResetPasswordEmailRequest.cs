using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ResetPasswordEmailRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email {get;set;} = null!;
    }
}