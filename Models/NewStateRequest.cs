using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class NewStateRequest
    {
        [Required]
        public int? CountryId {get; set;}

        [Required]
        public string? Name {get; set;}
    }
}