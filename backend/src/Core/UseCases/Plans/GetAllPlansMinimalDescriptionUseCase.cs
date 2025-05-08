using ResultMonad;

namespace UseCases.Plans;

public sealed class GetAllPlansMinimalDescriptionUseCase : IUseCase<Result<PlanShortDescriptionDto[]>>
{
    public Task<Result<PlanShortDescriptionDto[]>> RunAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Ok(new[]
        {
            new PlanShortDescriptionDto
            {
                Id = "1",
                Address = "г. Казань, улица Академика Парина, 32"
            },
            new PlanShortDescriptionDto
            {
                Id = "2",
                Address = "г. Москва, улица Пана, 25"
            }
        }));
    }
}