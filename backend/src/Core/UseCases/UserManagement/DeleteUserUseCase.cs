using System.Transactions;
using Domain;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using ResultMonad;
using Services.Authorization;

namespace UseCases.UserManagement;

public sealed class DeleteUserUseCase 
    : UserManagementUseCaseBase, IUseCase<DeleteUserUseCase.DeleteUserArgs, ResultWithError<ErrorMessage>>
{
    private IUserManagementService UserManagementService { get; }
    
    private IUnitOfWork UnitOfWork { get; }
    
    public DeleteUserUseCase(
        IUserManagementService userManagementService,
        IUnitOfWork unitOfWork, 
        IUserRepository userRepository, 
        IAuthorizationService authorizationService)
        : base(userRepository, authorizationService)
    {
        UnitOfWork = unitOfWork;
        UserManagementService = userManagementService;
    }

    public sealed record DeleteUserArgs : UserManagementArgsBase;

    public async Task<ResultWithError<ErrorMessage>> RunAsync(DeleteUserArgs args, CancellationToken ct)
    {
        var pipelineResult = await AuthorizeCurrentRequestAndFindUserByEmailFromArgsAsync(args.Email, ct);
        if (pipelineResult.IsFailure)
        {
            return ResultWithError.Fail(pipelineResult.Error);
        }

        var (issuer, userFoundByEmail) = pipelineResult.Value;
        if (userFoundByEmail is null)
        {
            return ResultWithError.Fail(ErrorMessage.EntityAlreadyExists);
        }
        
        try
        {
            await UnitOfWork.BeginAsync();
            var removeUserResult = await UserManagementService.RemoveUserFromSystemAsync(issuer, userFoundByEmail, ct);

            if (removeUserResult.IsFailure)
            {
                await UnitOfWork.RollbackAsync();

                return removeUserResult;
            }

            await UnitOfWork.CommitAsync();
            return ResultWithError.Ok<ErrorMessage>();
        }
        catch(TransactionException)
        {
            await UnitOfWork.RollbackAsync();
            return ResultWithError.Fail(ErrorMessage.DomainError("Транзакционная ошибка."));
        }
    }
}