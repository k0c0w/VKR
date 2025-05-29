import { Guid, guid } from "@shared/types/guid";

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