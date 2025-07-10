import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { Outlet, Navigate } from "react-router";

function userHasRoles(userRoles: string[], requiredRoles: string[]) {
    for(const requiredRole of requiredRoles) {
        if (!userRoles.find(x => x === requiredRole)) {
            return false;
        }
    }

    return true;
}

export default function WithRoles({requiredRoles, byPassingRoles}:{requiredRoles:string[]; byPassingRoles?: string[]}) {
    const roles = useAppSelector(state => state.authSlice.currentUser)?.roles ?? [];

    if (byPassingRoles && roles.some(x => byPassingRoles.includes(x))) {
        return <Outlet/>
    }

    if (!userHasRoles(roles, requiredRoles)) {
        return <Navigate to="-1" replace/>
    }

    return <Outlet/>;
}