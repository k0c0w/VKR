import { Box, Paper, useTheme } from "@mui/material";
import { ReactNode } from "react";

export default function FormsLayout({children}: {children: ReactNode}) {
    const theme = useTheme();
    
    return <Box
               sx={{
                 display: "flex",
                 justifyContent: "center",
                 alignItems: "center",
                 minHeight: "100%",
                 width: "100%",
                 padding: theme.spacing(2),
                 [theme.breakpoints.down("mobile")]: {
                   padding: theme.spacing(1),
                 },
                 [theme.breakpoints.between("mobile", "tablet")]: {
                   padding: theme.spacing(1.5),
                 },
                 [theme.breakpoints.between("tablet", "lg")]: {
                   padding: theme.spacing(2),
                 },
                 [theme.breakpoints.up("lg")]: {
                   padding: theme.spacing(3),
                 },
               }}
           >
               <Paper
                 elevation={4}
                 sx={{
                   display: "flex",
                   flexDirection: "column",
                   justifyContent: "center",
                   width: "100%",
                   minHeight: 250,
                   maxWidth: {
                     mobile: "100%",
                     tablet: "90%",
                     lg: "80%",
                     xl: "70%",
                   },
                   padding: theme.spacing(3),
                   [theme.breakpoints.down("mobile")]: {
                     padding: theme.spacing(2),
                   },
                   [theme.breakpoints.between("mobile", "tablet")]: {
                     padding: theme.spacing(2.5),
                   },
                 }}
               >
                   {children}   
               </Paper>
           </Box>
}