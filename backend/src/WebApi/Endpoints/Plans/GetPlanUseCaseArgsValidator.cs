using FluentValidation;
using UseCases.Plans;

namespace WebApi.Endpoints.Plans;

public class GetPlanUseCaseArgsValidator : AbstractValidator<GetPlanUseCaseArgs>
{
    public GetPlanUseCaseArgsValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Поле обязательно.");
    }
}