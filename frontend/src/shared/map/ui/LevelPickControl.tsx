import { useEffect, useState } from "react";
import "../lib/levelPicker/LevelPickerControl";
import { useMap } from "react-leaflet";
import L from "leaflet";

export default function LevelPickControl({disableLevelRemoval, levelLabels, initialSelectedLevelIndex}: {disableLevelRemoval: boolean; levelLabels: string[]; initialSelectedLevelIndex?: number}) {
    const map = useMap();
    const [control] = useState(new L.Control.LevelPicker());

    useEffect(() => {
        control.addTo(map);

        return () => {
            map.removeControl(control);
        }
    }, [map, control]);

    useEffect(() => {
        control.setLevels(levelLabels, initialSelectedLevelIndex);
    }, [control, levelLabels, initialSelectedLevelIndex]);

    useEffect(() => {
        control.setRubbishBinDisabled(disableLevelRemoval);
    }, [control, disableLevelRemoval]);

    return <></>
}