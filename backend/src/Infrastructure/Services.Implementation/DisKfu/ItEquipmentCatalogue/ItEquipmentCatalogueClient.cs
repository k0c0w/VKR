using Domain.Entities;
using Domain.Errors;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using ResultMonad;

namespace Services.ItEquipmentCatalogue;

public class ItEquipmentCatalogueClient(
    ILogger<IItEquipmentCatalogue> logger) : IItEquipmentCatalogue
{
    public Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentAtBuildingAsync(Address buildingAddress, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ItEquipmentDescription, ErrorMessage>> GetItEquipmentAsync(string inventoryNumber, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}