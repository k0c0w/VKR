namespace Common.Dto;

public readonly record struct AddressDto
{
    [System.Text.Json.Serialization.JsonPropertyName("city")]
    [Newtonsoft.Json.JsonProperty("city")]
    public string City { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("street")]
    [Newtonsoft.Json.JsonProperty("street")]
    public string Street { get; init; }
    
    [System.Text.Json.Serialization.JsonPropertyName("house")]
    [Newtonsoft.Json.JsonProperty("house")]
    public string House { get; init; }
    
    public void Deconstruct(out string city, out string street, out string house)
    {
        city = City;
        street = Street;
        house = House;
    }
}
