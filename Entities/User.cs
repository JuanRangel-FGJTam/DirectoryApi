using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthApi.Entities
{
    
    [Table("Users", Schema = "System")]
    public class User
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public string LastName { get; set; } = null!;
        public required string Username { get; set; }

        [JsonIgnore]
        public string Password { get; set; } = null!;
        public bool isActive { get; set; }
    }
}