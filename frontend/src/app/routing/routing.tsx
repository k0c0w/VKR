import { createBrowserRouter } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan";
import { Routes } from "./routes";
import { PlanPage as ViewExistingPlanPage } from "@pages/viewExistingPlan";
import AvailablePlansPage from "@pages/viewAvailablePlans";

const router = createBrowserRouter([
    {
        path: Routes.CreateNewPlanRouteTemplate,
        element: <CreateNewPlanPage />
    },
    {
        path: Routes.SpecificPlanRouteTemplate,
        element: <ViewExistingPlanPage/>
    },
    {
        path: Routes.AvailablePlansRouteTemplate,
        element: <AvailablePlansPage />
    }
]);

export default router;