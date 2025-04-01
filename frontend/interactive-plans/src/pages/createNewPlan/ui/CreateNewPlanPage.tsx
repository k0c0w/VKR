import {LoadBuildingBoundariesWidget, CreateNewPlanWidget} from "@widgets/map";
import { Building } from "@entities/map/Building";
import { Container, Skeleton } from "@mui/material";
import { useState } from "react";


export default function CreateNewPlanPage() {
    const [building, setBuilding] = useState<Building | undefined>();

    return (<Container component="main">
        {!building && <LoadBuildingBoundariesWidget 
            setBuilding={setBuilding}
            loaderBackground={<Skeleton width="100%" height={800}/>}
        />}
        {building && <CreateNewPlanWidget building={building} />}
    </Container>);
}