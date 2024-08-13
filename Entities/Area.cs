using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("Areas")]
public class Area
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id {get;set;}
    public string Name {get;set;} = default!;
    public bool? Disabled {get;set;}
}
