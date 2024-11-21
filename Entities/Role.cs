using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace AuthApi.Entities
{
    
    [Table("Roles", Schema = "System")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserRole> UserRoles {get;set;} = default!;
    }
}