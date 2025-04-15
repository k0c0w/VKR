
export class ArrayExtensions {
    public static Range(start: number, end:number): number[] {
        return Array.from({length: (end - start)}, (v, k) => k + start);
    } 
}