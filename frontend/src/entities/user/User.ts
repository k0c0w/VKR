import { guid } from "@shared/types/guid";

export type User = {
    id: guid;
    name: string;
    roles: string[];
}