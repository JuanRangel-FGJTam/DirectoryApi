using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class NewMunicipalityRequest
    {
        [Required]
        public int? CountryId {get; set;}

        [Required]
        public int? StateId {get; set;}

        [Required]
        public string? Name {get; set;}
    }
}