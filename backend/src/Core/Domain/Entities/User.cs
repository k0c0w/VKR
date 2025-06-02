using System.Diagnostics.CodeAnalysis;
using Domain.ValueObjects;

namespace Domain.Entities;

public sealed class User
{
    private readonly HashSet<UserRole> _roles;

    public Guid Id { get; }
    
    public string Email { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public User(string email)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        _roles = [UserRole.User];
    }

    private User(Guid id, string email, IEnumerable<UserRole> roles)
    {
        id.ThrowIfEmpty(nameof(id));
        Id = id;
        
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        Email = email;

        _roles = roles.Distinct()
            .ToHashSet();
    }

    public static User CreateExisting(Guid id, string email, IEnumerable<UserRole> userRoles)
        => new (id, email, userRoles);
}