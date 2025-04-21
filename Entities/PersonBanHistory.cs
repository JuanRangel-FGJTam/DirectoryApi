using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Entities;

public class PersonBanHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}

    public Guid PersonId {get;set;}

    [MaxLength(20)]
    public string Activo {get;set;} = default!; // 'BANNED' or 'UNBANNED'

    public string? Message {get;set;}

    public DateTime CreatedAt {get;set;}

    // Navigation property with ForeignKey attribute
    [JsonIgnore]
    [ForeignKey("PersonId")]
    public virtual Person? Person { get; set; }
}
