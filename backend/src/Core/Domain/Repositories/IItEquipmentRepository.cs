using Domain.Aggregates;
using Domain.Errors;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Repositories;

public interface IItEquipmentRepository
{
    Task<Result<ItEquipment[], ErrorMessage>> GetItEquipmentByRoomIdsAsync(long[] roomIds, CancellationToken ct);
}