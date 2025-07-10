import { Routes } from "@app/routing/routes";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Outlet, useLocation, Navigate } from "react-router";

export default function AuthorizationRequired() {
    const location = useLocation();
    const { currentUser } = useAppSelector(state => state.authSlice);

    if (!currentUser) {
        return <Navigate to={Routes.SignInRouteTemplate} state={{from: location}} replace/>;
    }

    return <Outlet />;
}
