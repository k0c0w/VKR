import { Theme } from "@mui/material";

export function getRoleColor(theme: Theme, role: string) {
    if (role === "editor") {
        return theme.palette.text.primary;
    }

    if (role === "moderator") {
        return theme.palette.info.main;
    }

    if (role === "root") {
        return theme.palette.error.main;
    }

    return theme.palette.text.secondary;
}