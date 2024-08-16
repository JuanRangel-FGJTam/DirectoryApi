using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class NewEmailRequest
    {
        [Required(ErrorMessage = "El correo electronico es requerido")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Debe ser un correo electronico valido")]
        public string Email {get;set;} = null!;


        [Required(ErrorMessage = "El codigo de validacion es requerido")]
        [Length(6,6, ErrorMessage = "El codigo debe ser de 6 caracteres")]
        public string ValidationCode {get;set;} = null!;
    }
}