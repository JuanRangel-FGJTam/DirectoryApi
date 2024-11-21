using FluentValidation;
using AuthApi.Models;

namespace AuthApi.Validators.Users;

public class UserRoleValidator: AbstractValidator<UserRoleRequest>
{
    public UserRoleValidator(){
        RuleFor(model => model.RoleId)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithName("Rol Id");
    }
}