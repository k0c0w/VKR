using Bogus;
using Common.Dto;
using Domain.ValueObjects;
using Moq;
using Services;

namespace UnitTests.UseCases.Fixtures;

public class AddressTestFixture
{
    public Faker<Address> AddressFaker { get; }

    public AddressTestFixture()
    {
        AddressFaker = new Faker<Address>()
            .CustomInstantiator(f => new Address(
                f.Address.City(),
                f.Address.StreetName(),
                f.PickRandom("улица", "проспект", "переулок"),
                f.Random.Number(1, 100).ToString(),
                f.Random.Bool() ? f.Random.Char(min:'а', max:'я').ToString() : ""));
    }

    public AddressDto CreateAddressDto(Address address)
    {
        return new AddressDto(
            address.City,
            $"{address.StreetType} {address.StreetName}",
            address.HouseNumber);
    }

    public void SetupAddressParsing(Mock<IAddressParser> addressParserMock, Address address, bool streetSuccess = true, bool houseSuccess = true)
    {
        addressParserMock
            .Setup(x => x.TryParseStreet($"{address.StreetType} {address.StreetName}", out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns((string s, out string type, out string name) =>
            {
                if (!streetSuccess)
                {
                    type = null;
                    name = null;
                    return false;
                }
                type = address.StreetType;
                name = address.StreetName;
                return true;
            });

        addressParserMock
            .Setup(x => x.TryParseHouse(address.HouseNumber, out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns((string h, out string number, out string unit) =>
            {
                if (!houseSuccess)
                {
                    number = null;
                    unit = null;
                    return false;
                }
                number = address.HouseNumber;
                unit = address.HouseUnit;
                return true;
            });
    }
}