/* Theme */
import { CssBaseline, ThemeProvider } from "@mui/material";
import Theme from "./config/theme";
/* State */
import { Provider as ReduxProvider } from "react-redux";
import RootStore from "./redux/appStore";
/* Routing */
import Routing from "./routing/routing";

export default function App() {

    return  <>
        <CssBaseline />
        <ReduxProvider store={RootStore}>
            <ThemeProvider theme={Theme}>
                <Routing />
            </ThemeProvider>
        </ReduxProvider>
    </>
}