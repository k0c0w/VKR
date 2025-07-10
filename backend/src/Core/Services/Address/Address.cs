namespace Services.Address;

public record Address(
    string SettlementType, 
    string SettlementName, 
    string StreetType, 
    string StreetName, 
    string HouseNumber,
    string HouseUnit);
