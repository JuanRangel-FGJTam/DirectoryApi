using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ResetPasswordEmailRequest
    {
        [Required(ErrorMessage = "El correo electronico es requerido")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Debe ser un correo electronico valido")]
        public string Email {get;set;} = null!;
    }
}