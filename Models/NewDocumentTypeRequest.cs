using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class NewDocumentTypeRequest
    {
        [Required]
        public string? Name {get; set;}

    }
}