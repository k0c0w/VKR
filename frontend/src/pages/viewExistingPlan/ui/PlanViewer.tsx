import { Building } from "@entities/map";
import { Container, Stack, Box } from "@mui/material";
import { useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { PlanViewerWidget } from "@widgets/editor";
import EditPlanButton from "./EditPlanButton";
import PlanHeader from "@shared/ui/PlanHeader";

export default function PlanViewer({ building }: { building: Building }) {

    return <>
        <title>{building.properties.name}</title>
        <Container maxWidth="xl" disableGutters fixed sx={{width: "100vw", height: "100vh"}}>
            <Stack height="100%">
                <PlanViewerHeader building={building}/>
                <Box sx={{ flex: 1, width: "100%", height: "100%", mb:2 }}>
                    <PlanViewerWidget building={building} />
                </Box>
                <Box minHeight={50} width="100%"/>
            </Stack>
        </Container>
    </>;
}

function PlanViewerHeader({building}: {building: Building;}) {
    const userHasEditorRole = useAppSelector(s => s.authSlice.currentUser)?.roles.includes("editor") ?? false;

    return <PlanHeader 
            address={building.properties.address} 
            name={building.properties.name} 
            actionSlot={userHasEditorRole && <EditPlanButton aria-label="Редактирование" />} 
        />
}