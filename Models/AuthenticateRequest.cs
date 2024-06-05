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
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        [Required]
        public string? Password { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}