using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace AuthApi.Entities
{
    [Table("People")]
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get;set;}
        [Required]
        public string Rfc {get;set;} = "";
        [Required]
        public string Curp {get;set;} = "";
        [Required]
        public string? Name {get;set;}
        public string? FirstName {get;set;}
        public string? LastName {get;set;}
        [Required]
        public string? Email {get;set;}
        [Required]
        public string? Password {get;set;}
        public string? Birthdate {get;set;}
        public DateTime CreatedAt {get;set;}
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt {get;set;}
        public DateTime? DeletedAt {get;set;}

        public Gender? Gender {get; set;}
        public MaritalStatus? MaritalStatus {get;set;}
        public Notionality? Notionality {get;set;}
        public Occupation? Occupation {get;set;}
        
    }

    
}