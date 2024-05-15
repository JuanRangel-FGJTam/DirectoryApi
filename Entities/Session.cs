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
        [Required]
        public string IpAddress {get;set;} = null!;
        [Required]
        public string UserAgent {get;set;} = null!;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime BegginAt {get;set;}
        public DateTime? EndAt {get;set;}
        
        [Required]
        public string Token {get;set;} = null!;
    }
}