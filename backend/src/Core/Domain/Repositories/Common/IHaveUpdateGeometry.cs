using Domain.Errors;
using ResultMonad;

namespace Domain.Repositories.Common;

public interface IHaveUpdateGeometry<in TEntityIdentityType, in TGeometry> 
    where TGeometry : GeoJSON.Net.Geometry.IGeometryObject
    where TEntityIdentityType : IEquatable<TEntityIdentityType>

{
    Task<ResultWithError<ErrorMessage>> UpdateGeometryAsync(TEntityIdentityType id, TGeometry geometry,
        CancellationToken ct);
}