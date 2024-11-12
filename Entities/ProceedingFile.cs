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
    public Proceeding Proceeding {get;set;} = default!;
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}

}
