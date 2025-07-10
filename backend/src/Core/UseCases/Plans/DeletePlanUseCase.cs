using System.Collections.Immutable;
using System.Transactions;
using Domain;
using Domain.Errors;
using Domain.Services;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases.Plans;

public sealed class DeletePlanUseCase :  WithAuthorizeUseCaseBase, IUseCase<DeletePlanUseCase.DeletePlanUseCaseArgs, ResultWithError<ErrorMessage>>
{
    public record DeletePlanUseCaseArgs(Guid PlanId);
    
    private IPlanService PlanService { get; }
    
    private IUnitOfWork UnitOfWork { get; }

    public DeletePlanUseCase(
        IUnitOfWork unitOfWork,
        IPlanService planService,
        IAuthorizationService authorizationService) 
        : base(authorizationService, [UserRole.Editor])
    {
        UnitOfWork = unitOfWork;
        PlanService = planService;
    }
    
    public async Task<ResultWithError<ErrorMessage>> RunAsync(DeletePlanUseCaseArgs args, CancellationToken ct)
    {
        if (Guid.Empty == args.PlanId)
        {
            return ResultWithError.Fail(ErrorMessage.ValidationError("Получен пустой индетефикатор плана."));
        }
        
        var authorization = await AuthorizeAsync();
        if (authorization.IsFailure)
        {
            return authorization;
        }

        var user = await GetCurrentUserAsync();
        try
        {
            await UnitOfWork.BeginAsync();

            var deletionResult = await PlanService.DeletePlanAsync(user, args.PlanId, ct);
            if (deletionResult.IsFailure)
            {
                await UnitOfWork.RollbackAsync();
                return deletionResult;
            }

            await UnitOfWork.CommitAsync();
            return ResultWithError.Ok<ErrorMessage>();
        }
        catch (TransactionAbortedException)
        {
            await UnitOfWork.RollbackAsync();
            return ResultWithError.Fail(ErrorMessage.DomainError($"Не удалось удалить план {args.PlanId}."));
        }
    }
}