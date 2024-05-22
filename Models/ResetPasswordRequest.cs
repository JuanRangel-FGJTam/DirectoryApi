using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage="El codigo de validacion es requerido")]
        [Length(minimumLength:6, maximumLength:6, ErrorMessage = "El codigo debe constar de 6 caracteres")]
        public string Code {get;set;} = null!;

        [Required(ErrorMessage="La nueva contraseña es requerida")]
        public string Password {get;set;} = null!;

        [Required(ErrorMessage="La confirmacion de la nueva contraseña es requerida")]
        [Compare( nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword {get;set;} = null!;

    }
}