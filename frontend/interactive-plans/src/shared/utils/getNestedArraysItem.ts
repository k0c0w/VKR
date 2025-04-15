export default function getNestedArraysItem<T>(arr: any[], indexPath: number[]): T {
    let current: any = arr;
    for (let i = 0; i < indexPath.length; i++) {
        const index = indexPath[i];
        if (!current || !current.length || !(index < current.length && index >= 0)) {
            throw new Error("Invalid index path or layer state.", {cause: indexPath});
        }

        current = current[index];
    }

    return current as T;
}