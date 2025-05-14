using Common.Dto;

namespace UseCases.RetrieveBuildingByAddress;   

public record struct RetrieveBuildingByAddressArgs(AddressDto Address);
