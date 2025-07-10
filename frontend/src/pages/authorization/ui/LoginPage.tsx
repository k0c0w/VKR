import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Navigate, useLocation } from "react-router";
import { Routes } from "@app/routing/routes";
import { LoginWidget } from "@widgets/authorization";
import { useEffect, useState } from "react";
import { Typography } from "@mui/material";
import AlertDialog from "@shared/ui/AlertDialog";
import PageContainer from "@shared/ui/PageContainer";

export default function LoginPage() {
    const {currentUser} = useAppSelector(state => state.authSlice);
    const location = useLocation();

    const navigateTo = location.state?.from?.pathname && !((location.state.from.pathname as string)?.startsWith(Routes.SignOutRouteTemplate))
        ? location.state.from.pathname : Routes.AvailablePlansRouteTemplate;

    const [openAlert, setOpenAlert] = useState(false);
    const [loginAttemptsCount, setLoginAttemptsCount] = useState(0);

    useEffect(() => {
        if (loginAttemptsCount !== 0 && loginAttemptsCount % 5 == 0) {
            setOpenAlert(true);
        }
    }, [loginAttemptsCount, setOpenAlert]);

    if (currentUser) {
        return <Navigate to={navigateTo} replace state={undefined}/>
    }

    return <>
        <title>Вход в систему</title>
        <PageContainer component="main" sx={{display: "flex", flexDirection:"column", justifyContent: "center"}}>
            <LoginWidget afterLoginSubmit={() => setLoginAttemptsCount(old => old + 1)}/> 
        </PageContainer>
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