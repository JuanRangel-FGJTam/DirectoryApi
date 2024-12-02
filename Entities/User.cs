using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [JsonIgnore]
        public string Password { get; set; } = null!;
        public bool isActive { get; set; }

        public virtual ICollection<UserRole>? UserRoles {get;set;}
        public virtual ICollection<UserClaim>? UserClaims {get;set;}

        public override string ToString()
        {
            return string.Format("[{0}|{1}|{2}]", Id, $"{FirstName} {LastName}", Email);
        }

    }
}