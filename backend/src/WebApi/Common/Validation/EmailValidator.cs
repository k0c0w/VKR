using FluentValidation;

namespace WebApi.Common.Validation;

internal class EmailValidator : AbstractValidator<string>
{
    public EmailValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Почта обязательна.")
            .EmailAddress()
            .WithMessage("Не верный формат почты.");
    }
}