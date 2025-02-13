using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("Proceedings")]
public class Proceeding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    public Guid PersonId {get;set;}
    public string? Name {get;set;}
    public string? Folio {get;set;}
    public ProceedingStatus? Status {get;set;}
    public Area? Area {get;set;}
    public string? DenunciaId {get;set;}
    public string? Observations {get;set;}
    public string? OfficeLocation {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
    
    // Navigation property for related files
    public virtual ICollection<ProceedingFile>? Files { get; set; }
}
