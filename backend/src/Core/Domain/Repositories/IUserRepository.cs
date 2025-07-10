using Domain.Entities;
using Domain.Errors;
using Domain.Repositories.Common;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Repositories;

public interface IUserRepository : IHaveAdd<User>, IHaveUpdate<User>, IHaveRemove<User>
{
    Task<Result<User, ErrorMessage>> GetAsync(GetUserFilter filter, CancellationToken ct);
    
    Task<Result<User[], ErrorMessage>> GetAllAsync(CancellationToken ct);

    Task<Result<bool, ErrorMessage>> AnyUserWithRole(UserRole role, CancellationToken ct);
    
    public sealed class GetUserFilter
    {
        public string? Email { get; init; }

        private GetUserFilter(string? email)
        {
            Email = email;
        }

        public static GetUserFilter WithEmail(string email) => new (email);
    }
}