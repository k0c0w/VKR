
export type Street = string;
export type HouseNumber = string;
export type City = string;

export type Address = {
    city: City;
    street: Street;
    houseNumber: HouseNumber;
} 

export function isCompletedAddress({city, street, houseNumber}: Address): boolean {
    return city.length > 0 && street.length > 0 && houseNumber.length > 0;
}

/**
 * Returns Address parsed from input.
 * @param fullAddress address string in format like 'г. Казань, улица Кремлёвская, 18к1'. Assumed that input has valid format.
 */
export function parseAddress(fullAddress: string): Address | undefined {
    const words = fullAddress.split(', ');

    if (words.length != 3) {
        return undefined;
    }

    const [city, street, house] = words;

    return {
        city: city.trim().replace("г. ", ""),
        street: street.trim(),
        houseNumber: house.trim()
    }
}