namespace Services.Map;

public interface IMapProviderService
{
    public Task<Dictionary<string, object>> GetBuildingInformationAsync(string city, string street, string houseNumber,
        CancellationToken cancellationToken);
}