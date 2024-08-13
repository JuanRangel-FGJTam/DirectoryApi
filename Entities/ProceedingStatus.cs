using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("ProceedingStatuses")]
public class ProceedingStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    public string Name {get;set;} = default!;
    public bool? Disabled {get;set;}
    
}
