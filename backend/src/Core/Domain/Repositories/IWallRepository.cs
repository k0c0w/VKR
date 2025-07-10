using Domain.Entities;
using Domain.Errors;
using Domain.Repositories.Common;
using GeoJSON.Net.Geometry;
using ResultMonad;

namespace Domain.Repositories;

public interface IWallRepository : IHaveUpdateGeometry<Guid, LineString>, IHaveRemove<Wall>, IHaveAdd<Wall>
{ 
}