using Domain.ValueObjects;
using FluentValidation;
using UseCases.UserManagement;
using WebApi.Common.Validation;

namespace WebApi.Controllers.Users.Validation;

public class AddUserArgsValidator : AbstractValidator<AddUserUseCase.AddUserArgs>
{
    public AddUserArgsValidator()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());

        RuleFor(x => x.Roles)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Поле обязательно.")
            .ForEach(role =>
            {
                role.IsInEnum()
                    .WithMessage("Не известная роль.");
            })
            .Must(x => x.Contains(UserRole.User))
            .WithMessage($"Роль {UserRole.User.ToString().ToLower()} обязательна.");
    }
}