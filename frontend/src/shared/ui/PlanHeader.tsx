import { Grid, Stack, Typography } from "@mui/material";
import { ReactNode } from "react";

export default function PlanHeader({address, name, actionSlot}: {address: string; name: string; actionSlot?: ReactNode}) {
    return (
        <Grid container sx={{ p: 2, alignItems: "center", mb: 1 }}>
            <Grid size={{  mobile: 8, tablet: 9, lg:10, xl:10}}>
                <Stack direction="column" spacing={0.5}>
                    <Typography variant="h5" sx={{ fontWeight: "medium", textAlign: "left" }}>
                        {name}
                    </Typography>
                    <Typography
                        variant="body2"
                        sx={{ fontWeight: "light", color: "text.secondary", textAlign: "left" }}
                    >
                        {address}
                    </Typography>
                </Stack>
            </Grid>
            <Grid sx={{ display: "flex", justifyContent: "flex-end" }} size={{  mobile: 4, tablet: 3, lg:2, xl:2}}>
                {actionSlot}
            </Grid>
        </Grid>
    );
}