import { Routes } from "@app/routing/routes";
import { authApi, setUser } from "@features/authorization"
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Navigate } from "react-router";

export default function LogoutPage() {
    const currentUser = useAppSelector(state => state.authSlice.currentUser);
    const [signOut] = authApi.useSignOutMutation();
    const dispatch = useAppDispatch();

    if (currentUser) {
        signOut({});
        dispatch(setUser(undefined));
    }

    return <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
} 