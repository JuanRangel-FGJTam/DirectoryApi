using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token {get;set;} = null!;

        [Required]
        public string Password {get;set;} = null!;

        [Required]
        [Compare( nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword {get;set;} = null!;

    }
}