import { Guid, guid } from "@shared/types/guid";
import { ArrayExtensions } from "./arrayExtensions";

export function generateRandomGuidWhichDoesNotExistsIn(others: guid[]): guid {
    for(const _ of ArrayExtensions.Range(0, 10)) {
        const guid = Guid.New();

        const index = others.findIndex(otherGuid => guid === otherGuid);

        if (index === -1) {
            return guid;
        }
    }

    throw new Error("Number of guid generation attempts exceeded.")
}