
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