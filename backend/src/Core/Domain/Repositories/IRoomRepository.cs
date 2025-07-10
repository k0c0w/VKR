using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories.Common;
using Domain.ValueObjects;
using ResultMonad;

namespace Domain.Repositories;

public interface IRoomRepository : IHaveAdd<Room>, IHaveRemove<Room>
{
    public Task<ResultWithError<ErrorMessage>> UpdateRoomDescriptionAsync(long roomId, RoomDescription roomDescription, CancellationToken ct);
}