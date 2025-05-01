import { createTheme } from "@mui/material";

declare module "@mui/material/styles" {
  interface BreakpointOverrides {
    xs: false;
    sm: false;
    mobile: true;
    tablet: true;
    md: false;
    lg: true;
    xl: true;
  }
}
const theme = createTheme({
  typography: {
    fontSize: 16,
  },
  breakpoints: {
    values: {
      mobile: 450,
      tablet: 850,
      lg: 1024,
      xl: 1440
    },
  },
});

if (theme.components) {
    /* perform global ovverides here */ 
}

export default theme;