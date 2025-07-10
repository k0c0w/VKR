import guid, { Guid } from "@shared/types/guid";

export function generateRandomGuidWhichDoesNotExistsIn(others: guid[]): guid {
    for(const _ of Array.from({length: 10})) {
        const guid = Guid.New();

        const index = others.findIndex(otherGuid => guid === otherGuid);

        if (index === -1) {
            return guid;
        }
    }

    throw new Error("Number of guid generation attempts exceeded.")
}

function getRandomInt(min: number, max: number): number {
    min = Math.ceil(min);
    max = Math.floor(max); 
    return Math.floor(Math.random() * (max - min + 1)) + min; 
}

export function generateRandomNumberWhichDoesNotExistsIn(others: number[]): number {
    for(const _ of Array.from({length: 10})) {
        const rand = getRandomInt(-1_000_000, -1000);

        const index = others.findIndex(other => rand === other);

        if (index === -1) {
            return rand;
        }
    }

    throw new Error("Number of guid generation attempts exceeded.");
}