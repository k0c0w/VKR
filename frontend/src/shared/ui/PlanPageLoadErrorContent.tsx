import { Button, Stack, Typography } from "@mui/material";

export default function PlanPageLoadErrorContent({retry}: {retry: () => void}) {
    return <Stack>
                <Typography component="h3" textAlign="center" mb={2} color="error">Не удалось загрузить план здания.</Typography>
                <Button onClick={retry}>Повторить загрузку</Button>
        </Stack>
}