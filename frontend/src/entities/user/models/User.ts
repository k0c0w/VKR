export type User = {
    email: string;
    roles: ['user'] & string[];
}