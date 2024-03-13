using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AuthenticateRequest
    {
        [JsonPropertyName("email")]
        [DefaultValue("System")]
        [Required]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        [DefaultValue("System")]
        [Required]
        public string? Password { get; set; }
    }
}