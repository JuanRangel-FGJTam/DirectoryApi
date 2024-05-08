using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Entities
{
    [Table("Preregistrations")]
    public class Preregistration {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get;set;} = Guid.NewGuid();
        public string? Mail {get;set;} 
        public string? Phone {get;set;}
        [JsonIgnore]
        public string? Password {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
        public DateTime ValidTo {get;set;} = DateTime.Now.AddDays(2);
        public string Token {get;set;} = null!;
        
    }
}