import { useEffect } from "react";
import { useBuildingMap } from "./BuildingMapContext";

export const BuildingMapPanes = {
    buildingStructure: {
        zIndex: '450',
        pane: 'buildingStructurePane',
    },
    buildingBasement: {
        zIndex: '350',
        pane: 'buildingBasementPane'
    },
    labels: {
        pane: "labels",
        zIndex: '460',
    },
}

export function CreateMapPanesComponent() {
    const { map } = useBuildingMap();

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

        let labelsPane = map.getPane(BuildingMapPanes.labels.pane);
        if (!labelsPane) {
            labelsPane = map.createPane(BuildingMapPanes.labels.pane);
            labelsPane.style.zIndex = BuildingMapPanes.labels.zIndex;
        }

    }, [map]);

    return <></>
}