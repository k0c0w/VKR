export default function compareObjectsByFields(draft1: any, draft2: any, visited: Set<any> = new Set()): boolean {
    if (draft1 === draft2) return true;

    if (typeof draft1 !== 'object' || typeof draft2 !== 'object' || draft1 === null || draft2 === null) {
        return false;
    }

    if (visited.has(draft1) || visited.has(draft2)) {
        return true;
    }

    visited.add(draft1);
    visited.add(draft2);

    const keys1 = Object.keys(draft1);
    const keys2 = Object.keys(draft2);

    if (keys1.length !== keys2.length) {
        return false;
    }

    for (let key of keys1) {
        if (!keys2.includes(key) || !compareObjectsByFields(draft1[key], draft2[key], visited)) {
            return false;
        }
    }

    return true;
}