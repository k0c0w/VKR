import ReduxProvider from "./ReduxProvider";
import { ReactChildrenProps } from "../../shared/types/reactChildrenProps";
import RouteProvider from "./RouteProvider";


export default function UseProviders({children}: ReactChildrenProps) {
    return ( 
    <ReduxProvider>
        <RouteProvider>
            {children}
        </RouteProvider>
    </ReduxProvider>
    )
}