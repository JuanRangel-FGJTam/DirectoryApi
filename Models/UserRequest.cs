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
        [JsonPropertyName("email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        [JsonPropertyName("password")]
        public string? Password { get; set; } = null!;
        
        [Required]
        [JsonPropertyName("confirmPassword")]
        public string? ConfirmPassword { get; set; } = null!;
    }

    public class UserUpdateRequest
    {   
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }
        
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; } = null!;
        
        [JsonPropertyName("email")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string? Email { get; set; }
        
        [JsonPropertyName("password")]
        public string? Password { get; set; } = null!;
        
        [JsonPropertyName("confirmPassword")]
        public string? ConfirmPassword { get; set; } = null!;
    }
}