using FluentValidation;
using AuthApi.Models;

namespace AuthApi.Validators.AccountRecovery;

public class NewAccountRecoberyValidator : AbstractValidator<AccountRecoveryRequest>
{
    public NewAccountRecoberyValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty()
            .MaximumLength(250)
            .WithName("Nombre");

        RuleFor(model => model.Curp)
            .MaximumLength(24)
            .WithName("CURP");

        RuleFor(model => model.FirstName)
            .MaximumLength(100)
            .WithName("Apellido Paterno");

        RuleFor(model => model.LastName)
            .MaximumLength(100)
            .WithName("Apellido Materno");

        RuleFor(model => model.BirthDate)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .WithMessage("La {PropertyName} debe estar en el formato 'yyyy-MM-dd'.")
            .WithName("Fecha de nacimiento");

        RuleFor(model => model.ContactEmail)
            .EmailAddress()
            .WithName("Correo contacto");

        RuleFor(model => model.ContactEmail2)
            .EmailAddress()
            .WithName("Correo contacto 2");

        RuleFor(model => model.ContactPhone)
            .MaximumLength(16)
            .WithName("Telefono contacto");

        RuleFor(model => model.ContactPhone2)
            .MaximumLength(16)
            .WithName("Telefono contacto 2");

        RuleFor(model => model.RequestComments)
            .MaximumLength(250)
            .WithName("Comentarios");

        // RuleFor(model => model.GenderId)
        //     .GreaterThan(0)
        //     .WithName("Genero");

        // RuleFor(model => model.NationalityId)
        //     .GreaterThan(0)
        //     .WithName("Nacionalidad");

    }
}