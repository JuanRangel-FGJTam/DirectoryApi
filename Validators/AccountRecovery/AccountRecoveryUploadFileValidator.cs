using FluentValidation;
using AuthApi.Models;

namespace AuthApi.Validators.AccountRecovery;

public class AccountRecoveryUploadFileValidator : AbstractValidator<AccountRecoveryFileRequest>
{
    public AccountRecoveryUploadFileValidator()
    {

        RuleFor(m => m.DocumentTypeId)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("El tipo de documento no es valido");

        RuleFor(m => m.File)
            .NotNull()
            .WithMessage("El archivo es requerido")
            .WithName("File");

        RuleFor(m => m.File!.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10 MB
            .WithMessage("El archivo debe ser menor o igual a 10 MB.")
            .When(x => x.File != null)
            .WithName("File");

        RuleFor(x => x.File!.ContentType)
            .Must(contentType => new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" }.Contains(contentType))
            .WithMessage("Se acepta unicamente archivo PDF e imagenes (JPEG, PNG).")
            .When(x => x.File != null)
            .WithName("File");

    }
}