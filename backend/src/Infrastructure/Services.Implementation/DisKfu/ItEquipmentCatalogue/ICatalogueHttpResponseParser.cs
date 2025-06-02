using Domain.Entities;
using Domain.Errors;
using ResultMonad;

namespace Services.ItEquipmentCatalogue;

public interface ICatalogueHttpResponseParser
{
    ValueTask<Result<ItEquipmentDescription[], ErrorMessage>> ParseEquipmentListAsync(HttpResponseMessage responseMessage, CancellationToken ct);
}