using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuthApi.Entities;

[Table("PersonFiles")]
public class PersonFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    public Guid PersonId {get;set;}
    public int DocumentTypeId {get;set;}
    public string? FileName {get;set;}
    public string? FilePath {get;set;}
    public string? MimmeType {get;set;}
    public long FileSize {get;set;}

    [DataType(DataType.Date)]
    public DateTime Validation {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime? DeletedAt {get;set;}

    // Navigation property with ForeignKey attribute
    [JsonIgnore]
    [ForeignKey("PersonId")]
    public virtual Person? Person { get; set; }

    [ForeignKey("DocumentTypeId")]
    public virtual DocumentType? DocumentType { get; set; }

}
