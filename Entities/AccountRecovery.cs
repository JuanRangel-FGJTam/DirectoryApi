using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApi.Entities;

[Table("AccountRecovery", Schema = "Recv")]
public class AccountRecovery
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id {get;set;}
    public Guid? PersonId {get;set;}
    public string Name {get;set;} = default!;
    public string? FirstName {get;set;}
    public string? LastName {get;set;}

    [DataType(DataType.Date)]
    public DateTime BirthDate {get;set;}
    public Gender? Gender {get;set;} = default!;
    public string? Curp {get;set;}
    public string? Rfc {get;set;}
    public Nationality? Nationality {get;set;} = default!;
    public Occupation? Occupation {get;set;} = default!;
    public MaritalStatus? MaritalStatus {get;set;} = default!;
    public string? ContactEmail {get;set;}
    public string? ContactEmail2 {get;set;}
    public string? ContactPhone {get;set;}
    public string? ContactPhone2 {get;set;}
    public string? RequestComments {get;set;}
    public string? ResponseComments {get;set;}

    public DateTime CreatedAt {get;set;} = DateTime.Now;
    public DateTime? AttendingAt {get;set;}
    public DateTime? DeletedAt {get;set;}
    public int? AttendingBy {get;set;}
    public int? DeletedBy {get;set;}

    public string? NotificationEmailResponse {get;set;}
    public string? NotificationEmailContent {get;set;}

    public virtual ICollection<AccountRecoveryFile>? Files { get; set; }
    public virtual User? UserAttended {get;set;}
    public virtual User? UserDeleted {get;set;}
}
