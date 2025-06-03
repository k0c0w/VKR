import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Navigate, useLocation, useNavigate } from "react-router";
import { Routes } from "@app/routing/routes";
import { LoginWidget } from "@widgets/authorization";
import { useEffect, useState } from "react";
import { Stack, Typography, useTheme } from "@mui/material";
import AlertDialog from "@shared/ui/AlertDialog";

export default function LoginPage() {
    const theme = useTheme();
    const currentUser = useAppSelector(state => state.authSlice.currentUser);
    const location = useLocation();
    const navigate = useNavigate();
    const navigateTo = location.state?.from?.pathname ?? Routes.AvailablePlansRouteTemplate;

    const [openAlert, setOpenAlert] = useState(false);
    const [loginAttemptsCount, setLoginAttemptsCount] = useState(0);

    useEffect(() => {
        if (currentUser !== undefined) {
            navigate(navigateTo, {
                replace: true,
                state: undefined,
            });
        }
    }, [currentUser, navigate, navigateTo]);

    useEffect(() => {
        if (loginAttemptsCount !== 0 && loginAttemptsCount % 5 == 0) {
            setOpenAlert(true);
        }
    }, [loginAttemptsCount, setOpenAlert]);

    if (currentUser) {
        return <Navigate to={navigateTo} replace />
    }

    return <>
        <title>Вход в систему</title>
        <Stack sx={{
            height: 'calc((1 - var(--template-frame-height, 0)) * 100dvh)',
            minHeight: '100%',
            padding: theme.spacing(2),
            [theme.breakpoints.up('mobile')]: {
                padding: theme.spacing(4),
            },
            '&::before': {
                content: '""',
                display: 'block',
                position: 'absolute',
                zIndex: -1,
                inset: 0,
                backgroundImage:'radial-gradient(ellipse at 50% 50%, hsl(210, 100%, 97%), hsl(0, 0%, 100%))',
                backgroundRepeat: 'no-repeat',
            },
            }}
        >
            <LoginWidget afterLoginSubmit={() => setLoginAttemptsCount(old => old + 1)}/>
        </Stack>
        <AlertDialog 
            open={openAlert} 
            handleClose={() => setOpenAlert(false)} 
            title="Слишком много неудачных попыток входа."
        >
            <Typography component="h6">
                Возможно, авторизующий сервер недоступен. Пожалуйста, попробуйте повторить авторизацию позже.
            </Typography>
        </AlertDialog>
    </>
}