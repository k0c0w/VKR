import { useEffect } from "react";
import { useMap } from "react-leaflet";

export const BuildingMapPanes = {
    buildingStructure: {
        zIndex: '450',
        pane: 'buildingStructurePane',
    },
    buildingBasement: {
        zIndex: '350',
        pane: 'buildingBasementPane'
    }
}

export function CreateMapPanesComponent() {
    const map = useMap();

    useEffect(() => {
        let buildingStructurePane = map.getPane(BuildingMapPanes.buildingStructure.pane);
        if (!buildingStructurePane) {

            buildingStructurePane = map.createPane(BuildingMapPanes.buildingStructure.pane);
            buildingStructurePane.style.zIndex = BuildingMapPanes.buildingStructure.zIndex;
        }

        let buildingBasementPane = map.getPane(BuildingMapPanes.buildingBasement.pane);
        if (!buildingBasementPane) {
            buildingBasementPane = map.createPane(BuildingMapPanes.buildingBasement.pane);
            buildingBasementPane.style.zIndex = BuildingMapPanes.buildingBasement.zIndex;
        }
    }, [map]);

    return <></>
}