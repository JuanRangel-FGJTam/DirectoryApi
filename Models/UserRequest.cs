using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class UserRequest
    {   
        [Required]
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [Required]
        [JsonPropertyName("password")]
        public string? Password { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("confirmPassword")]
        public string? ConfirmPassword { get; set; } = null!;
    }
}