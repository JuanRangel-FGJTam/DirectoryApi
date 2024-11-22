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
    [Table("DocumentTypes")]
    public class DocumentType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get;set;}

        public string Name {get;set;} = null!;
        
        [JsonIgnore]
        public DateTime CreatedAt {get;set;}
        
        public DateTime UpdatedAt {get;set;}
        
        [JsonIgnore]
        public DateTime? DeletedAt {get;set;}

    }
}