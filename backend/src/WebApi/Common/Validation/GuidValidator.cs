using FluentValidation;

namespace WebApi.Common.Validation;

public class GuidValidator : AbstractValidator<Guid>
{
    public GuidValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Поле обязательно");
    }
}