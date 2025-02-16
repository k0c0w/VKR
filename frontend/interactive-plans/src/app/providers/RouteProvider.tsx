import {ReactChildrenProps} from "../../shared/types/reactChildrenProps";
import { BrowserRouter } from "react-router-dom";

export default function RouteProvider({children}: ReactChildrenProps) {
    return <BrowserRouter>
        {children}
    </BrowserRouter>
}