using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AuthenticateRequest
    {
        [DefaultValue("System")]
        [Required]
        public string? Username { get; set; }

        [DefaultValue("System")]
        [Required]
        public string? Password { get; set; }
    }
}