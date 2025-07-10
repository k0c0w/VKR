import AlertDialog from "@shared/ui/AlertDialog";
import { CreateNewPlanPageSteps } from "../lib/createNewPlanPageSteps";
import { SelectBuildingFromCatalogue } from "./SelectBuildingFromCatalogue";
import LoadBuildingStartInformation from "./LoadBuildingStartInformation";
import CreateNewPlanEditor from "./CreateNewPlanEditor";
import useCreateNewPlanReducer from "../lib/createNewPlanPageReducer";
import { useNavigate } from "react-router";
import { Routes } from "@app/routing/routes";
import guid from "@shared/types/guid";
import PageContainer from "@shared/ui/PageContainer";
import { Container } from "@mui/material";
import FormsLayout from "./FormsLayout";
import { useEffect, useState } from "react";

export default function CreateNewPlanPage() {
    const navigate = useNavigate();
    const { state, handleBuildingSelect, handleBuildingLoad, setError } = useCreateNewPlanReducer();
    const [showError, setShowError] = useState(false);
 
    const onPlanCreated = (createdPlanId: guid) => {
        // todo: set actual plan state here from response
        navigate(Routes.FormatSpecificPlanRouteTemplate(createdPlanId), {
            replace: true,
        });
    };

    useEffect(() => {
        if (state.error) {
            setShowError(true);
        }
    }, [state]);

    return <>
        <title>Создать новый план</title>
        <Container component="main" maxWidth={false} disableGutters>
            {state.step !== CreateNewPlanPageSteps.OpenPlanEditor && 
                <PageContainer sx={{display: "flex", flexDirection:"column", justifyContent:"center", minHeight:600}}>
                    <FormsLayout>
                        {state.step === CreateNewPlanPageSteps.SelectBuildingForPlan 
                            && <SelectBuildingFromCatalogue onSelectBuilding={handleBuildingSelect} onError={(args) => {setError(args); setShowError(true);}} />}
                        {state.step === CreateNewPlanPageSteps.LoadBuildingInformation 
                            && <LoadBuildingStartInformation buildingInfo={state.buildingData} onLoadComplete={handleBuildingLoad} />}
                    </FormsLayout>
                </PageContainer>
            }
            {state.step === CreateNewPlanPageSteps.OpenPlanEditor && 
                <Container maxWidth="xl" disableGutters fixed sx={{width: "100vw", height: "100vh"}}>
                    <CreateNewPlanEditor onError={setError} onPlanSucsessfulCreationCallback={onPlanCreated} />
                </Container>
            }
        </Container>
        <AlertDialog title={state.error?.title ?? ""}
            handleClose={() => setShowError(false)}
            open={showError}
        >
            {state.error?.payload}
        </AlertDialog>
    </>
}