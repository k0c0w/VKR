import { useEffect, useState } from "react";
import "../lib/levelPicker/LevelPickerControl";
import L from "leaflet";
import { useBuildingMap } from "./BuildingMapContext";

export default function LevelPickControl({disableLevelRemoval}: {disableLevelRemoval: boolean;}) {
    const {map, building, currentLevelIndex} = useBuildingMap();
    const levelLabels = building.properties.levels.map(x => x.name);
    const [control] = useState(new L.Control.LevelPicker({showLevelButtons: false}));

    useEffect(() => {
        control.addTo(map);

        return () => {
            map.removeControl(control);
        }
    }, [map, control]);

    useEffect(() => {
        control.setLevels(levelLabels, currentLevelIndex);
    }, [control, levelLabels, currentLevelIndex]);

    useEffect(() => {
        control.setRubbishBinDisabled(disableLevelRemoval);
    }, [control, disableLevelRemoval]);

    return <></>
}