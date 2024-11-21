using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthApi.Entities
{
    
    [Table("UserClaims", Schema = "System")]
    public class UserClaim
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string ClaimType { get; set; } = default!;
        public required string ClaimValue { get; set; } = default!;
        
        // Navigation properties
        public User? User { get; set; }

    }
}