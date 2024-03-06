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
    [Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get;set;}
        
        [JsonIgnore]
        public Person Person {get;set;} = null!;
        
        public Country Country {get;set;} = null!;
        
        public State State {get;set;} = null!;
        
        public Municipality Municipality {get;set;} = null!;
        
        public Colony? Colony {get;set;}
        
        public string Street {get;set;} = null!;
        
        public string Number {get;set;} = null!;
        
        public string? NumberInside {get;set;}
        
        public int ZipCode {get;set;}
        
        [JsonIgnore]
        public DateTime CreatedAt {get;set;}
        
        [JsonIgnore]
        public DateTime UpdatedAt {get;set;}
        
        [JsonIgnore]
        public DateTime? DeletedAt {get;set;}

    }
}