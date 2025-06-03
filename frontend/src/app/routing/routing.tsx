import { createBrowserRouter, Navigate } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan";
import { Routes } from "./routes";
import { PlanPage as ViewExistingPlanPage } from "@pages/viewExistingPlan";
import AvailablePlansPage from "@pages/viewAvailablePlans";
import { LoginPage, LogoutPage } from "@pages/authorization";
import AuthorizationRequired from "@features/authorization/ui/AuthorizationRequired";

const router = createBrowserRouter([
    {
        path: Routes.AvailablePlansRouteTemplate,
        element: <AuthorizationRequired children={<AvailablePlansPage />}/>,
        index: true,
    },
    {
        path: Routes.CreateNewPlanRouteTemplate,
        element: <AuthorizationRequired children={<CreateNewPlanPage />} requiredRoles={['Moderator']}/>
    },
    {
        path: Routes.SpecificPlanRouteTemplate,
        element: <AuthorizationRequired children={<ViewExistingPlanPage/>}/>
    },
    {
        path: Routes.SignInRouteTemplate,
        element: <LoginPage/>
    },
    {
        path: Routes.SignOutRouteTemplate,
        element: <AuthorizationRequired children={<LogoutPage/>}/>
    },
    {
        path: "*",
        element: <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
    }
]);

export default router;