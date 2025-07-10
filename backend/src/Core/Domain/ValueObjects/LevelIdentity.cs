namespace Domain.ValueObjects;

public sealed record LevelIdentity
{
    public Guid BuildingId { get; }
    
    public uint Number { get; }

    public LevelIdentity(Guid buildingId, uint number)
    {
        buildingId.ThrowIfEmpty(nameof(buildingId));
        ArgumentOutOfRangeException.ThrowIfZero(number, nameof(number));

        BuildingId = buildingId;
        Number = number;
    }
}