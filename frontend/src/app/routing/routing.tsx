import { createBrowserRouter } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan";
import { Routes } from "./routes";
import { PlanPage } from "@pages/plan";

const router = createBrowserRouter([
    {
        path: Routes.CreateNewPlanRouteTemplate,
        element: <CreateNewPlanPage />
    },
    {
        path: Routes.SpecificPlanRouteTemplate,
        element: <PlanPage/>
    }
]);

export default router;