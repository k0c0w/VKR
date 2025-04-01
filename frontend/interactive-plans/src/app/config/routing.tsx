import { createBrowserRouter } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan"

const router = createBrowserRouter([
    {
        path: "/",
        element: <CreateNewPlanPage />
    },
]);

export default router;