import { createBrowserRouter } from "react-router";
import CreateNewPlanPage from "@pages/createNewPlan"
import MapObjectDescriptoPopup from "@widgets/map/ui/MapObjectDescriptionPopup";
import { RoomType } from "@entities/map";

const router = createBrowserRouter([
    {
        path: "/",
        element: <CreateNewPlanPage />
    },
]);

export default router;