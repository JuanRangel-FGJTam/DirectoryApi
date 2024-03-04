using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities
{
    [Table("Colonies")]
    public class Colony
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id {get;set;}
        public string Name {get;set;} = null!;
        public string ZipCode {get;set;} = null!;
        public Municipality? Municipality {get;set;}
    }
}