import CreateNewPlanPage from "@pages/createNewPlan";
import { PlanPage as ViewExistingPlanPage } from "@pages/viewExistingPlan";
import AvailablePlansPage from "@pages/viewAvailablePlans";
import { LoginPage, LogoutPage } from "@pages/authorization";
import EditExistingPlanPage from "@pages/editExistingPlan";
import AuthorizationRequired from "@features/authorization/ui/AuthorizationRequired";
import WithRoles from "@features/authorization/ui/WithRoles";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import {Routes as RouteTemplates} from "./routes";
import NavBar from "./NavBar";
import UserManagementPage from "@pages/users";

export default function Routing() {

   return (
    <BrowserRouter>
        <NavBar />
        <Routes>
            <Route element={<AuthorizationRequired/>}>
                <Route path="/plans">
                    <Route path="" index element={<AvailablePlansPage />}/>
                    <Route path="create" element={<WithRoles requiredRoles={['editor']}/>}>
                        <Route path="" element={<CreateNewPlanPage />} />
                    </Route>
                    <Route path=":id">
                        <Route path="edit" element={<WithRoles requiredRoles={['editor']}/>}>
                            <Route path="" element={<EditExistingPlanPage />} />
                        </Route>
                        <Route path="" element={<ViewExistingPlanPage />}/>
                    </Route>
                </Route>
                <Route path={RouteTemplates.UserManagmentRouteTemplate} element={<WithRoles requiredRoles={['moderator']} byPassingRoles={['root']}/>}>
                    <Route path="" element={<UserManagementPage/>} />
                </Route>
                <Route path={RouteTemplates.SignOutRouteTemplate} element={<LogoutPage/>}/>
            </Route>
            <Route path={RouteTemplates.SignInRouteTemplate} element={<LoginPage/>}>
                
            </Route>
            <Route path="*" element={<Navigate to={RouteTemplates.AvailablePlansRouteTemplate} replace/>}/>
        </Routes>
    </BrowserRouter>);
}