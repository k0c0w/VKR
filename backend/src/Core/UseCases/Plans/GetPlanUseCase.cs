using Domain.Errors;
using ResultMonad;

namespace UseCases.Plans;

public class GetPlanUseCase : IUseCase<GetPlanUseCaseArgs, Result<PlanDto, ErrorMessage>>
{
    public Task<Result<PlanDto, ErrorMessage>> RunAsync(GetPlanUseCaseArgs args, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Ok<PlanDto, ErrorMessage>(new PlanDto()
        {
            Id = "1"
        }));
    }
}