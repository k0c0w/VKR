using Domain;

namespace UseCases.Plans;

public record PlanShortDescriptionDto
{
    public string Id { get; init; }
    
    public string Address { get; init; }
}