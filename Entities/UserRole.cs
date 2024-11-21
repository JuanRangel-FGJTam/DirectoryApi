using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AuthApi.Entities
{
    
    [Table("UserRoles", Schema = "System")]
    public class UserRole
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();
        public required int UserId { get; set; }
        public required int RoleId { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Role? Role { get; set; }

    }
}