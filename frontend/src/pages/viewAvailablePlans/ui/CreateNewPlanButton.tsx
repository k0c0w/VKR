import { Routes } from "@app/routing/routes";
import { Button, Grid, Typography } from "@mui/material";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { useNavigate } from "react-router";
import AddIcon from '@mui/icons-material/Add';

export default function CreateNewPlanButton() {
    const hasModeratorRole = useAppSelector(s => s.authSlice.currentUser)?.roles.includes("editor") ?? false;
    const navigate = useNavigate();

    const goToCreateNewPlanPage = () => {
        navigate(Routes.CreateNewPlanRouteTemplate);
    }

    if (!hasModeratorRole) {
        return <></>
    }

    return (
        <Grid size={{ mobile: 12, tablet: 'auto' }}>
            <Button
                onClick={goToCreateNewPlanPage}
                variant="contained"
                color="primary"
                startIcon={<AddIcon />}
                sx={{
                width: { mobile: 'fit-content', tablet: 'auto' },
                ml: { mobile: 'auto', tablet: 0 },
                }}
            >
                <Typography sx={{
                        display: { mobile: 'none', tablet: 'block' },
                    }}
                >
                    Создать новый план
                </Typography>
            </Button>
        </Grid>
    );
}