namespace UseCases.RetrieveBuildingByAddress;

public record struct RetrieveBuildingByAddressDto
{
    public string City { get; init; }
    
    public string Street { get; init; }
    
    public string HouseNumber { get; init; }
}

public static class RetrieveBuildingByAddressDtoExtensions
{
    public static RetrieveBuildingByAddressDto WithTrimmedFields(this RetrieveBuildingByAddressDto args)
    {
        return  new RetrieveBuildingByAddressDto
        {
            City = args.City?.Trim() ?? "", 
            Street = args.Street?.Trim() ?? "", 
            HouseNumber = args.HouseNumber?.Trim() ?? ""
        };
    }
}