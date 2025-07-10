import { Button, Dialog, DialogActions, DialogTitle, Grid, Typography, useMediaQuery, useTheme } from "@mui/material";
import AddIcon from '@mui/icons-material/Add';
import { useEffect, useState } from "react";
import { userManagementApi } from "@features/users";
import { User } from "@entities/user/models/User";
import UserFormDialogContent from "@entities/user/ui/UserFromDialogContent";
import { isFetchBaseQueryError, isServerErrorResponse, isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { useForm } from "react-hook-form";

export default function AddUserWidget() {
    const [open, setOpen] = useState(false);

    const [apiError, setApiError] = useState("");
    const [triggerAddUser, { isLoading, isError, error, reset:resetMutation, isSuccess }] =
        userManagementApi.useAddUserToSystemMutation();
    const { control, handleSubmit, reset } = useForm<User>({
      defaultValues: { email: "", roles: ['user'] },
    });

    const closeForm = () => {
        resetMutation();
        reset();
        setApiError("");
        setOpen(false);
    }

    useEffect(() => {
        if (isSuccess) {
            closeForm();
        } 

    }, [isSuccess, reset, setApiError, closeForm]);

    useEffect(() => {
        if (isLoading) {
            setApiError("");
        } else if (isError && error) {
            if (isFetchBaseQueryError(error)) {
                if (isValidationProblemDetails(error.data)) {
                    const validationErrors = (error.data as IValidationProblemDetails).errors;
                    setApiError(Object.values(validationErrors).flat().join(", "));
                } else if (isServerErrorResponse(error)) {
                    setApiError(error.data.detail || "Ошибка сервера.");
                } else {
                    setApiError("Возникла непредвиденная ошибка.");
                }
            } else {
                setApiError("Сетевая ошибка.");
            }
        }
    }, [isError, isLoading, error, reset]);

    const onSubmit = (user: User) => {
        if (!user.roles.includes('user')) {
            user.roles.push('user');
            return user;
        }
        setApiError("");
        triggerAddUser(user);
    };

    return (
        <>
            <Grid size={{ mobile: 12, tablet: 'auto' }}>
                <Button
                    onClick={() => setOpen(true)}
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
                        Добавить пользователя
                    </Typography>
                </Button>
            </Grid>
            <Dialog open={open} onClose={closeForm} fullWidth maxWidth="lg">
                <DialogTitle>Добавить пользователя</DialogTitle>
                <UserFormDialogContent error={apiError ?? undefined}  formControl={control} />
                <DialogActions>
                    <Button onClick={handleSubmit(onSubmit)} disabled={isLoading}>
                      Сохранить
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
