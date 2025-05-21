import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { ReactNode } from "react";
import { Navigate, useLocation } from "react-router";

export default function AuthorizationRequired({children}: {children:ReactNode;}) {
    const location = useLocation();
    const {currentUser} = useAppSelector(state => state.authSlice);

    if (!currentUser) {
        return <Navigate to="/login" state={{from: location}} />
    }

    return children;
}