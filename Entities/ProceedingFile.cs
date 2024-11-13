using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("ProceedingFiles")]
public class ProceedingFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    public string? FileName {get;set;}
    public string? FilePath {get;set;}
    public string? FileType {get;set;}
    public long FileSize {get;set;}
    public int ProceedingId { get; set; }
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}

    // Navigation property with ForeignKey attribute
    [ForeignKey("ProceedingId")]
    public Proceeding? Proceeding { get; set; }

}
