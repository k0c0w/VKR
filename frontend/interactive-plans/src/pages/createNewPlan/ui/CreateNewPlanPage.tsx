import {LoadBuildingBoundariesWidget, CreateNewPlanWidget, CreateNewPlanStepperWidget} from "@widgets/map";
import { Building } from "@entities/map/Building";
import { Container, Skeleton } from "@mui/material";
import { useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { resetToInitialState, setBuilding } from "@widgets/map/lib/createNewPlanSlice";

export default function CreateNewPlanPage() {
    const [loadedBuilding, setLoadedBuilding] = useState<Building | undefined>();
    const building = useAppSelector(state => state.createNewPlanReducer.building);
    const dispatch = useAppDispatch();

    const [createButtonDisabled, setCreateButtonDisabled] = useState(false);
    const [validating, setValidating] = useState(false);

    useEffect(() => {
        if (loadedBuilding) {
            dispatch(setBuilding(loadedBuilding))
        } else {
            dispatch(resetToInitialState())
        }
    }, [loadedBuilding]);

    useEffect(() => {
        if (createButtonDisabled) {
            setCreateButtonDisabled(false);
        }
    }, [building]);

    return (<Container component="main" style={{width: 800, height: 600}}>
        {!loadedBuilding && <LoadBuildingBoundariesWidget 
            setBuilding={setLoadedBuilding}
            loaderBackground={<Skeleton width="100%" height={800}/>}
        />}
        {building && <>
            <CreateNewPlanWidget style={{width: 600, height: 800}} />
            <CreateNewPlanStepperWidget backwardButtonDisabled={validating} completeButtonDisabled={createButtonDisabled || validating} onComplete={() => alert("done")}/>
        </>}
    </Container>);
}