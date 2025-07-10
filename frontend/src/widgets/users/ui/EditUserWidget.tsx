import { Button, Dialog, DialogActions, DialogTitle, Grid, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { userManagementApi } from "@features/users";
import { User } from "@entities/user/models/User";
import UserFormDialogContent from "@entities/user/ui/UserFromDialogContent";
import { isFetchBaseQueryError, isServerErrorResponse, isValidationProblemDetails, IValidationProblemDetails } from "@shared/types/ProblemDetails";
import { useForm } from "react-hook-form";
import ConfirmationDialog from "@shared/ui/ConfirmationDialog";
import { Action } from "@shared/types/Action";

interface EditUserRoleWidgetProps {
    onClose: Action;
    user: User;
}

export default function EditUserWidget({ user, onClose }: EditUserRoleWidgetProps) {
    const [openConfirmation, setOpenConfirmation] = useState(false);
    const [open, setOpen] = useState(false);
    const [apiError, setApiError] = useState("");

    const [removeUser, { isLoading: isDeleting, isError: isDeleteError, error: deleteError, isSuccess: isDeleteSuccess }] =
        userManagementApi.useRemoveUserFromSystemMutation();

    const [changeUserRoles, { data, isLoading, reset: resetMutation, isSuccess }] =
        userManagementApi.useChangeUserRolesMutation();

    const { control, handleSubmit, reset } = useForm<User>({ defaultValues: user });

    const confirmDelete = async () => {
        setApiError("");
        await removeUser({ email: user.email });
    };

    const closeForm = () => {
        resetMutation();
        reset();
        setApiError("");
        setOpen(false);
        onClose();
    };

    useEffect(() => {
        if (isSuccess && data) {
            closeForm();
        }
    }, [isSuccess, data, reset, closeForm]);

    useEffect(() => {
        if (isDeleteSuccess) {
            setOpen(false);
            setOpenConfirmation(false);
        } else if (isDeleteError && deleteError) {
            if (isFetchBaseQueryError(deleteError)) {
                if (isValidationProblemDetails(deleteError.data)) {
                    const validationErrors = (deleteError.data as IValidationProblemDetails).errors;
                    setApiError(Object.values(validationErrors).flat().join(", "));
                } else if (isServerErrorResponse(deleteError)) {
                    setApiError(deleteError.data.detail || "Ошибка сервера.");
                } else {
                    setApiError("Возникла непредвиденная ошибка.");
                }
            } else {
                setApiError("Сетевая ошибка.");
            }
        }
    }, [isDeleteSuccess, isDeleteError, deleteError]);

    const onSubmit = (user: User) => {
        if (!user.roles.includes("user")) {
            user.roles.push("user");
            return user;
        }
        setApiError("");
        changeUserRoles(user);
    };

    const onDialogClose = () => {
        setOpen(false);
        setApiError("");
        reset();
    };

    useEffect(() => {

        setOpen(true);
    }, [user]);

    return (
        <>
            <Dialog open={open} onClose={onDialogClose} fullWidth maxWidth="lg">
                <DialogTitle>Редактировать пользователя</DialogTitle>
                <UserFormDialogContent error={apiError ?? undefined} formControl={control} disableEmail={true} />
                <DialogActions>
                    <Button variant="text" color="error" disabled={isDeleting} onClick={() => setOpenConfirmation(true)}>
                        Удалить пользователя
                    </Button>
                    <Button onClick={handleSubmit(onSubmit)} disabled={isLoading}>
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>
            <ConfirmationDialog
                open={openConfirmation}
                question={`Удалить пользователя ${user.email}?`}
                onConfirm={confirmDelete}
                onDecline={() => setOpenConfirmation(false)}
                onClose={() => setOpenConfirmation(false)}
            />
        </>
    );
}
