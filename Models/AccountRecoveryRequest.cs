using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Models
{
    public class AccountRecoveryRequest
    {
        public string? Name {get;set;}
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        public string? BirthDate {get;set;}
        public int? GenderId {get;set;}
        public string? Curp {get;set;}
        public int? NationalityId {get;set;}
        public string? ContactEmail {get;set;}
        public string? ContactEmail2 {get;set;}
        public string? ContactPhone {get;set;}
        public string? ContactPhone2 {get;set;}
        public string? RequestComments {get;set;}
    }
}