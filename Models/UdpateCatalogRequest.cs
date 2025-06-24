using System;
using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class UdpateCatalogRequest
    {
        public string? Name {get; set;}

        public string? ZipCode {get; set;}
    }
}