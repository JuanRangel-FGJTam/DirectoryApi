using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Entities
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public string SessionID {get;set;} = null!;
        [Required]
        public Person Person {get;set;} = null!;
        public string? IpAddress {get;set;}
        public string? UserAgent {get;set;}

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime BegginAt {get;set;}
        public DateTime? EndAt {get;set;}
        
        [Required]
        public string Token {get;set;} = null!;

        public DateTime? DeletedAt {get;set;}
        
        public bool IsDeleted {
            get => DeletedAt != null;
        }
    }
}