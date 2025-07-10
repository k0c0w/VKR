using FluentValidation;
using UseCases.Authorization;

namespace WebApi.Controllers.SignIn.Validation;

public class SignInArgsValidator : AbstractValidator<SignInUseCase.SignInArgs>
{
    public SignInArgsValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Поле обязательно")
            .EmailAddress()
            .WithMessage("Не валидное значение.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Поле обязательно");
    }
}