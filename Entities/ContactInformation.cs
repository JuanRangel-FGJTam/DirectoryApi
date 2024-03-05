using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace AuthApi.Entities
{
    [Table("ContactInformations"    )]
    public class ContactInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID {get;set;}

        [JsonIgnore]       
        public Person Person {get;set;} = null!;
        
        public ContactType ContactType {get;set;} = null!;
        
        public string Value {get;set;} = null!;
        
        [JsonIgnore]
        public DateTime CreatedAt {get;set;}
        
        public DateTime UpdatedAt {get;set;}
        
        [JsonIgnore]
        public DateTime? DeletedAt {get;set;}

    }
}