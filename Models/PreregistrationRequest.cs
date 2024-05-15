using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class PreregistrationRequest
    {
        [Required(ErrorMessage = "El correo electronico es requerido")]
        [EmailAddress]
        [StringLength(200, ErrorMessage = "Maximo 200 caracteres")]
        public string Mail {get;set;} = null!;
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password {get;set;} = null!;

        [Required(ErrorMessage = "La confirmacion de la contraseña es requerida")]
        [Compare( nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword {get;set;} = null!;

        public string? Url {get;set;} = null;
    }
}