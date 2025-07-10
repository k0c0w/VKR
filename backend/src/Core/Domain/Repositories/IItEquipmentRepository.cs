using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories.Common;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Repositories;

public interface IItEquipmentRepository : IHaveUpdate<ItEquipment>
{
    Task<Result<ItEquipment[], ErrorMessage>> GetItEquipmentByRoomIdsAsync(long[] roomIds, CancellationToken ct);
}