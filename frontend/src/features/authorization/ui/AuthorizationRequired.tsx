import { Routes } from "@app/routing/routes";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { ReactElement, ReactNode } from "react";
import { Navigate, useLocation } from "react-router";

function userHasRoles(userRoles: string[], requiredRoles: string[]) {
    for(const requiredRole of requiredRoles) {
        if (!userRoles.find(x => x === requiredRole)) {
            return false;
        }
    }

    return true;
}

export default function AuthorizationRequired({children, requiredRoles}: {children:ReactElement<any, any>; requiredRoles?:string[]}) {
    const location = useLocation();
    const {currentUser} = useAppSelector(state => state.authSlice);

    if (!currentUser) {
        return <Navigate to="/login" state={{from: location}} />
    } else if (requiredRoles && !userHasRoles(currentUser.roles, requiredRoles)) {
        return <Navigate to={Routes.AvailablePlansRouteTemplate} replace />
    }

    return <>{children}</>;
}