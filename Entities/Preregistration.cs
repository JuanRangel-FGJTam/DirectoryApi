using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities
{
    
    public class Preregistration {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get;set;} = Guid.NewGuid();
        public string? Mail {get;set;} 
        public string? Phone {get;set;} 
        public string? Password {get;set;}
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime ValidTo {get;set;} = DateTime.Now.AddDays(2);
        
    }
}