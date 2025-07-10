import { Routes } from "@app/routing/routes";
import { authApi, unauthenticateUser } from "@features/authorization"
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Navigate } from "react-router";

export default function LogoutPage() {
    const currentUser = useAppSelector(state => state.authSlice.currentUser);
    const [signOut] = authApi.useSignOutMutation();
    const dispatch = useAppDispatch();

    if (currentUser) {
        signOut();
        dispatch(unauthenticateUser());
    }

    return <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
} 