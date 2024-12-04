using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class NewColonyRequest
    {
        [Required]
        public int? CountryId {get; set;}

        [Required]
        public int? StateId {get; set;}

        [Required]
        public int? MunicipalityId {get; set;}

        [Required]
        public string? Name {get; set;}

        [Required]
        public string? ZipCode {get; set;}
    }
}