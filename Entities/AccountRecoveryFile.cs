using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("AccountRecoveryFiles", Schema = "Recv")]
public class AccountRecoveryFile
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id {get;set;}
    public string? FileName {get;set;}
    public string? FilePath {get;set;}
    public string? FileType {get;set;}
    public long FileSize {get;set;}
    public AccountRecovery AccountRecovery {get;set;} = default!;

    public DateTime CreatedAt {get;set;}
    public DateTime? DeletedAt {get;set;}
}
