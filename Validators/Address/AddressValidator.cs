using FluentValidation;
using AuthApi.Models;

namespace AuthApi.Validators.Address;

public class AddressValidator: AbstractValidator<AddressRequest>
{
    public AddressValidator(){
        RuleFor(model => model.PersonID)
            .NotNull()
            .WithName("Person ID");

        RuleFor(model => model.CountryID)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithName("Pais");

        RuleFor(model => model.StateID)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithName("Estado");

        RuleFor(model => model.MunicipalityID)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithName("Municipio");

        RuleFor(model => model.ColonyID)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithName("Colonia");

        RuleFor(model => model.Street)
            .NotEmpty()
            .MaximumLength(250)
            .WithName("Calle");

        RuleFor(model => model.Number)
            .NotEmpty()
            .MaximumLength(150)
            .WithName("Numero Exterior");

        RuleFor(model => model.NumberInside)
            .MaximumLength(150)
            .WithName("Numero Interior");

        RuleFor(model => model.ZipCode)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .WithName("Codigo Postal");
    }
}