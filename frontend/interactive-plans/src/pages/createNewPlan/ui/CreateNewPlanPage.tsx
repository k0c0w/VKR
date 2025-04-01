import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, setBuilding as setBuildingAction } from "../lib/createNewPlanSlice";
import Map from "@widgets/map";
import EditBuildingBoundaries from "./EditBuildingBoundaries";
import EditRoomsBoundaries from "./EditRoomsBoundaries";
import EditInfrastructure from "./EditInfrastructure";
import {LoadBuildingBoundariesWidget} from "@widgets/map";
import { Building } from "@entities/map/Building";

export default function CreateNewPlanPage() {
    const dispatch = useAppDispatch();
    const { currentStep, building } = useAppSelector(state => state.createNewPlanReducer);
    
    const setBuilding = (building: Building) => dispatch(setBuildingAction(building));

    return (<>
        {currentStep === CreateNewPlanStep.Initializing && <LoadBuildingBoundariesWidget setBuilding={setBuilding}/>}
        {currentStep !== CreateNewPlanStep.Initializing &&
            <Map 
                center={getCenterByBuilding(building)}
            >
                <EditBuildingBoundaries buildingAddress={{
                    city: "",
                    houseNumber: "",
                    street: ""
                }}/>
                <EditRoomsBoundaries />
                <EditInfrastructure />
            </Map>            
        }
    </>);
}