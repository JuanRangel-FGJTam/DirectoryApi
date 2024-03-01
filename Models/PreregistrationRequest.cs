using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class PreregistrationRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Mail {get;set;} = null!;
        
        [Required]
        public string Password {get;set;} = null!;

        [Required]
        [Compare( nameof(Password), ErrorMessage = "Passwords do not match")]
        public string Confirm_Password {get;set;} = null!;
    }
}