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
    
    /// <summary>
    /// Direct url link to raw card for this equipment.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Uri)]
    public required string ItEquipmentCardUrl { get; set; }
    
    /// <summary>
    /// Direct url link to raw history card for this equipment.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Uri)]
    public required string  ItEquipmentHistoryUrl { get; set; }
    
    public required long LocationAudienceCatalogueId { get; init; }
}