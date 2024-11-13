using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class UpdateProceedingRequest
    {
        [Required]
        [StringLength(24, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? Status {get;set;}
        
        [StringLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? DenunciaId {get;set;}
        
        [StringLength(200, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? Observations {get;set;}

        public IFormFileCollection? File { get; set; }

    }
}