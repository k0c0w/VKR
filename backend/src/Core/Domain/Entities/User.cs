using System.Diagnostics.CodeAnalysis;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Entities;

public sealed class User : IEquatable<User>
{
    private HashSet<UserRole> _roles;

    public string Email { get; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public User(string email)
    {
        Email = email;
        _roles = [UserRole.User];
    }

    private User(string email, IEnumerable<UserRole> roles)
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        Email = email;

        _roles = roles.Distinct()
            .ToHashSet();
    }

    public async Task<ResultWithError<ErrorMessage>> GrantRolesToUserAsync(
        User anotherUser, 
        UserRole[] roles, 
        IUserRepository userRepository,
        CancellationToken ct = default)
    {
        if (Equals(anotherUser))
        {
            return ResultWithError.Fail(ErrorMessage.DomainError("Вы не можете изменить свои привелегии."));
        }
        
        var shouldForbidOperation = !_roles.Contains(UserRole.Moderator) || !_roles.Contains(UserRole.Moderator);
        if (shouldForbidOperation)
        {
            return ResultWithError.Fail((ErrorMessage.DomainError(ErrorMessage.AuthenticationErrors.AccessDenied)));
        }

        var newRoles = new HashSet<UserRole>(roles);
        if (!newRoles.Contains(UserRole.User))
        {
            newRoles.Add(UserRole.User);
        }

        var nonRootTriesToGiveRoot = !_roles.Contains(UserRole.Root) && newRoles.Contains(UserRole.Root);
        if (nonRootTriesToGiveRoot)
        {
            return ResultWithError.Fail((ErrorMessage.DomainError(ErrorMessage.AuthenticationErrors.AccessDenied)));
        }
        
        var oldRoles = anotherUser._roles;

        anotherUser._roles = newRoles;

        var updateResult = await userRepository.UpdateAsync(anotherUser, ct);
        if (updateResult.IsFailure)
        {
            anotherUser._roles = oldRoles;
        }

        return updateResult;
    }

    public static User CreateExisting(string email, IEnumerable<UserRole> userRoles)
        => new (email, userRoles);

    public bool Equals(User? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Email == other.Email;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is User other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Email.GetHashCode();
    }
}