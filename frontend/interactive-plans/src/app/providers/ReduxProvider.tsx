import { Provider } from "react-redux";
import {ReactChildrenProps} from "../../shared/types/reactChildrenProps";
import store from "../appStore";

export default function ReduxProvider ({children}: ReactChildrenProps) {
    return <Provider store={store}>
        {children}
    </Provider>
}