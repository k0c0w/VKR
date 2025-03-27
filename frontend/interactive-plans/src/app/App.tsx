/* Theme */
import { CssBaseline, ThemeProvider } from "@mui/material";
import Theme from "./config/theme";
/* State */
import { Provider as ReduxProvider } from "react-redux";
import RootStore from "./redux/appStore";
/* Routing */
import { RouterProvider } from "react-router-dom";
import Router from "./config/routing";

export default function App() {

    return  <>
        <CssBaseline />
        <ReduxProvider store={RootStore}>
            <RouterProvider router={Router}>
                <ThemeProvider theme={Theme}>
                </ThemeProvider>
            </RouterProvider>
        </ReduxProvider>
    </>
}