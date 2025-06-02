namespace Domain.Entities;

public record Person(string Email, string Name, string MiddleName, string LastName, string? ContactNumber = default);