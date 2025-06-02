using Domain.Aggregates;
using Domain.Entities;
using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Services;

public interface IItEquipmentCatalogue
{
    public Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentAtBuildingAsync(Address buildingAddress, CancellationToken ct);

    public Task<Result<ItEquipmentDescription, ErrorMessage>> GetItEquipmentAsync(string inventoryNumber, CancellationToken ct);
}