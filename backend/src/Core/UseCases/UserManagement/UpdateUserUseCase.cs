using System.Transactions;
using Domain;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases.UserManagement;

public class UpdateUserUseCase : UserManagementUseCaseBase, IUseCase<UpdateUserUseCase.UpdateUserArgs, ResultWithError<ErrorMessage>>

{
    private IUnitOfWork UnitOfWork { get; }
    
    public UpdateUserUseCase(IUnitOfWork unitOfWork, IUserRepository userRepository, IAuthorizationService authorizationService)
        : base(userRepository, authorizationService)
    {
        UnitOfWork = unitOfWork;
    }
    
    public record UpdateUserArgs : UserManagementArgsBase
    {
        [System.Text.Json.Serialization.JsonPropertyName("roles")]
        [Newtonsoft.Json.JsonProperty("roles")]
        public UserRole[] Roles { get; init; } = [UserRole.User];
    }

    public async Task<ResultWithError<ErrorMessage>> RunAsync(UpdateUserArgs args, CancellationToken ct)
    {
        var pipelineResult = await AuthorizeCurrentRequestAndFindUserByEmailFromArgsAsync(args.Email, ct);
        if (pipelineResult.IsFailure)
        {
            return ResultWithError.Fail(pipelineResult.Error);
        }

        var (issuer, userFoundByEmail) = pipelineResult.Value;
        if (userFoundByEmail is null)
        {
            return ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        
        try
        {
            await UnitOfWork.BeginAsync();
            var grantResult = await issuer!.GrantRolesToUserAsync(userFoundByEmail, args.Roles, UserRepository, ct);
            if (grantResult.IsFailure)
            {
                await UnitOfWork.RollbackAsync();
                return ResultWithError.Fail(grantResult.Error);
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