import { createBrowserRouter, Navigate } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan";
import { Routes } from "./routes";
import { PlanPage as ViewExistingPlanPage } from "@pages/viewExistingPlan";
import AvailablePlansPage from "@pages/viewAvailablePlans";
import { LoginPage, LogoutPage } from "@pages/authorization";

const router = createBrowserRouter([
    {
        path: Routes.AvailablePlansRouteTemplate,
        element: <AvailablePlansPage />,
        index: true,
    },
    {
        path: Routes.CreateNewPlanRouteTemplate,
        element: <CreateNewPlanPage />
    },
    {
        path: Routes.SpecificPlanRouteTemplate,
        element: <ViewExistingPlanPage/>
    },
    {
        path: Routes.SignInRouteTemplate,
        element: <LoginPage/>
    },
    {
        path: Routes.SignOutRouteTemplate,
        element: <LogoutPage/>
    },
    {
        path: "*",
        element: <Navigate to={Routes.AvailablePlansRouteTemplate} replace/>
    }
]);

export default router;