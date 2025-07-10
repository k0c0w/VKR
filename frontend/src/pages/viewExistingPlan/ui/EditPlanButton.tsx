import { Button, Typography } from "@mui/material";
import EditIcon from '@mui/icons-material/Edit';
import { useNavigate, useParams } from "react-router-dom";
import { Routes } from "@app/routing/routes";

export default function EditPlanButton() {
    const navigate = useNavigate();
    const {id} = useParams();

    return (
        <Button
            onClick={() => navigate(Routes.FormatEditSpecificPlanRouteTemplate(id ?? ""), {replace: true})}
            variant="contained"
            color="primary"
            startIcon={<EditIcon />}
            sx={{
                width: { mobile: 'fit-content', tablet: 'auto' },
                ml: { mobile: 'auto', tablet: 0 },
            }}
        >
            <Typography sx={{
                    display: { mobile: 'none', tablet: 'block' },
                }}
            >
                Редактировать
            </Typography>
        </Button>
    );

}