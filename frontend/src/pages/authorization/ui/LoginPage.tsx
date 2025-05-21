import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Navigate, useLocation } from "react-router";
import { Routes } from "@app/routing/routes";

export default function LoginPage() {
    const currentUser = useAppSelector(state => state.authSlice.currentUser);
    const location = useLocation();
    const navigateTo = location.state?.from?.pathname ?? Routes.AvailablePlansRouteTemplate;

    if (currentUser) {
        return <Navigate to={navigateTo} replace />
    }

    //todo: login form
    return <form>

    </form>
}