using System.Diagnostics.CodeAnalysis;
using Domain.ValueObjects;

namespace Domain.Entities;

public class ItEquipmentDescription : IHaveIdentity<string>
{
    /// <summary>
    /// Inventory Number.
    /// </summary>
    public required string Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string FinanciallyResponsiblePerson { get; init; }
    
    public required Person Assignee { get; init; }
    
    /// <summary>
    /// Equipment type. For example, Принтер/МФУ.
    /// </summary>
    public required string Type { get; set; } 
    
    /// <summary>
    /// For example, "в эксплуатации".
    /// </summary>
    public required string CurrentState { get; init; }
    
    /// <summary>
    /// Direct url link to raw card for this equipment.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Uri)]
    public required string ItEquipmentCardUrl { get; init; }
    
    public required InstallationPlace Location { get; init; }
}