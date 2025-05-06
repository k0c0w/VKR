import { LoadBuildingBoundariesWidget } from "@widgets/map";
import { Building } from "@entities/map/Building";
import { CircularProgress, Container, Skeleton } from "@mui/material";
import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, initNewState, resetToInitialState, } from "@widgets/map/lib/createNewPlanSlice";
import CreateNewPlanSubPage from "./CreateNewPlanSubPage";
import { mapApi } from "@features/map";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { SerializedError } from "@reduxjs/toolkit";
import AlertDialog from "@shared/ui/AlertDialog";
import FullPageTint from "@shared/ui/FullPageTint";

function parseError(error: FetchBaseQueryError | SerializedError): string {
    return "ОШИБКА"
}

export default function CreateNewPlanPage() {
    const [loadedBuilding, setLoadedBuilding] = useState<Building | undefined>();
    const [createNewPlan, {data, error, isLoading, isSuccess, reset}] = mapApi.useCreateNewPlanMutation();
    const building = useAppSelector(state => state.createNewPlanReducer.building);
    const dispatch = useAppDispatch();

    function onPlanCreate(building: Building) {
        createNewPlan({building});
    }

    useEffect(() => {
        if (isSuccess && data) {
            // todo: navigate to plan page
        }

        if (error) {
            //todo: show alert here
        }
    }, [error, isSuccess]);

    useEffect(() => {
        if (loadedBuilding) {
            dispatch(initNewState({
                building: loadedBuilding,
                step: CreateNewPlanStep.BuildingBoundariesSetup,
                levelIndex: 0,
            }))
        } else {
            dispatch(resetToInitialState())
        }
    }, [loadedBuilding]);

    return (<Container component="main" style={{width: 800, height: 600}}>
        {!loadedBuilding && <LoadBuildingBoundariesWidget 
            setBuilding={setLoadedBuilding}
            loaderBackground={<Skeleton width="100%" height={800}/>}
        />}
        {building && <CreateNewPlanSubPage createPlan={onPlanCreate} />}
        {isLoading && <FullPageTint><CircularProgress color="primary"/></FullPageTint> }
        <AlertDialog
            title="Ошибка при создании плана"
            content={error ? parseError(error) : ""}
            handleClose={reset}
            open={error !== undefined}
        />
    </Container>);
}