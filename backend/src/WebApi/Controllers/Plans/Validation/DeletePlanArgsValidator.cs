using FluentValidation;
using UseCases.Plans;
using WebApi.Common.Validation;

namespace WebApi.Controllers.Plans.Validation;

public class DeletePlanArgsValidator : AbstractValidator<DeletePlanUseCase.DeletePlanUseCaseArgs>
{
    public DeletePlanArgsValidator()
    {
        RuleFor(x => x.PlanId).SetValidator(new GuidValidator());
    }
}