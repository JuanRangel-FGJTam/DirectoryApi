using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class UpdatePasswordRequest
    {
        [Required(ErrorMessage ="El valor es requerido")]
        public string? OldPassword {get;set;}
        
        [Required(ErrorMessage ="El valor es requerido")]
        [Length(minimumLength:8, maximumLength:16, ErrorMessage = "La contraseña debe constar entre 8 y 16 caracteres")]
        public string? NewPassword {get;set;}

        [Required(ErrorMessage ="El valor es requerido")]
         [Length(minimumLength:8, maximumLength:16, ErrorMessage = "La contraseña debe constar entre 8 y 16 caracteres")]
        [Compare("NewPassword", ErrorMessage = "Passwords are no equals")]
        public string? ConfirmPassword {get;set;}

    }
}