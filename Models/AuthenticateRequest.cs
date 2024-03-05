using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AuthenticateRequest
    {
        [DefaultValue("System")]
        public required string Username { get; set; }

        [DefaultValue("System")]
        public required string Password { get; set; }
    }
}