using Domain.Entities;
using Domain.Errors;
using ResultMonad;

namespace Domain.Services;

public interface IItEquipmentCatalogue
{
    public Task<Result<ItEquipmentDescription[], ErrorMessage>> GetAllItEquipmentByRoomIdsAsync(long[] roomIds, CancellationToken ct = default);
}