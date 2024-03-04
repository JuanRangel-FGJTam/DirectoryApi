using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class ContactRequest
    {
        [Required]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", ErrorMessage = "Person id is not valid")]
        public string PersonID {get;set;} = null!;
        
        [Required]
        [Range(1,99)]
        public int ContactTypeID {get;set;}
        
        [Required]
        public string Value {get;set;} = null!;
    }
}