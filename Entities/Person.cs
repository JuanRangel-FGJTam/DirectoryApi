using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Entities
{
    [Table("People")]
    public class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id {get;set;}
        
        public string? Rfc {get;set;}
        
        public string Curp {get;set;} = "";
        
        public string? Name {get;set;}
        
        public string? FirstName {get;set;}
       
        public string? LastName {get;set;}
        
        public string? Email {get;set;}

        public DateTime Birthdate {get;set;}
        
        public DateTime CreatedAt {get;set;}
        
        public Gender? Gender {get; set;}
        
        public MaritalStatus? MaritalStatus {get;set;}
        
        public Nationality? Nationality {get;set;}
        
        public Occupation? Occupation {get;set;}

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt {get;set;}

        public DateTime? DeletedAt {get;set;}
        public DateTime? ValidatedAt {get;set;}

        public virtual IEnumerable<Address>? Addresses {get;set;}
        public virtual IEnumerable<ContactInformation>? ContactInformations {get;set;}

        public string? AppOwner {get;set;}

        [JsonIgnore]
        public string? Password {get;set;}

        [NotMapped]
        public string FullName {
            get {
                return string.Join(' ', new string[]{Name??"", FirstName??"", LastName??""});
            }
        }
    }
}