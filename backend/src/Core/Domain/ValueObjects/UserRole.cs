namespace Domain.ValueObjects;

public enum UserRole : short
{
    User = 1,
    Editor,
    Moderator,
    Root,
}