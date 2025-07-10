using System.Transactions;
using Domain;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using ResultMonad;
using Services.Authorization;

namespace UseCases.UserManagement;

public sealed class AddUserUseCase : UserManagementUseCaseBase, IUseCase<AddUserUseCase.AddUserArgs, ResultWithError<ErrorMessage>>

{
    private IUnitOfWork UnitOfWork { get; }
    
    private IUserManagementService UserManagementService { get; }
    
    public AddUserUseCase(
        IUserManagementService userManagementService,
        IUnitOfWork unitOfWork, 
        IUserRepository userRepository, 
        IAuthorizationService authorizationService)
    : base(userRepository, authorizationService)
    {
        UnitOfWork = unitOfWork;
        UserManagementService = userManagementService;
    }
    
    public record AddUserArgs : UserManagementArgsBase
    {
        [System.Text.Json.Serialization.JsonPropertyName("roles")]
        [Newtonsoft.Json.JsonProperty("roles")]
        public UserRole[] Roles { get; init; }
    }

    public async Task<ResultWithError<ErrorMessage>> RunAsync(AddUserArgs args, CancellationToken ct)
    {
        var pipelineResult = await AuthorizeCurrentRequestAndFindUserByEmailFromArgsAsync(args.Email, ct);
        if (pipelineResult.IsFailure)
        {
            return ResultWithError.Fail(pipelineResult.Error);
        }

        var (issuer, userFoundByEmail) = pipelineResult.Value;
        if (userFoundByEmail is not null)
        {
            return ResultWithError.Fail(ErrorMessage.EntityAlreadyExists);
        }

        try
        {
            await UnitOfWork.BeginAsync();
            var userCreationResult = await UserManagementService.CreateNewUserAtSystemAsync(issuer, args.Email, ct);
            if (userCreationResult.IsFailure)
            {
                await UnitOfWork.RollbackAsync();
                return ResultWithError.Fail(userCreationResult.Error);
            }
        
            var user = userCreationResult.Value;
            var grantResult = await issuer!.GrantRolesToUserAsync(user, args.Roles, UserRepository, ct);
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