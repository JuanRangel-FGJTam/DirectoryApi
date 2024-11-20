using FluentValidation;
using AuthApi.Models;

namespace AuthApi.Validators.Address;

public class NewDocumentTypeValidator: AbstractValidator<NewDocumentTypeRequest>
{
    public NewDocumentTypeValidator(){
        RuleFor(model => model.Name)
            .NotEmpty()
            .MaximumLength(250)
            .WithName("Nombre");
    }
}