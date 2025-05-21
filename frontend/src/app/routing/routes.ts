import { guid } from "@shared/types/guid";


export class Routes {

    public static readonly CreateNewPlanRouteTemplate = "/plans/create";

    public static readonly SpecificPlanRouteTemplate = "/plans/:id";

    public static FormatSpecificPlanRouteTemplate(id: guid): string {
        return Routes.SpecificPlanRouteTemplate.replace(":id", id);
    }

    public static readonly AvailablePlansRouteTemplate = "/plans";

    public static readonly SignInRouteTemplate = "/login";

    public static readonly SignOutRouteTemplate = "/logout"
}