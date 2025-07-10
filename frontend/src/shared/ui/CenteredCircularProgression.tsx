import { CircularProgress, Stack } from "@mui/material";

export default function CenteredCircularProgress() {
    return <Stack width="100%" height="100%">
        <Stack sx={{display: "flex", flexDirection:"column", justifyContent:"center", alignItems:"center"}}>
            <CircularProgress />
        </Stack>
    </Stack>
}