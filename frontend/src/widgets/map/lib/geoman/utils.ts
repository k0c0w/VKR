import { Layer, PathOptions, PM } from "leaflet";
import { CreateNewPlanStep } from "../createNewPlanSlice";
import { isPolygonLayer, isPolylineLayer } from "@shared/map";
import { audienceStyle, hallStyle, wallStyle } from "../styling/styling";
import { RoomType } from "@entities/map";

function disableAllModes(pm: PM.PMMap) {
    if (pm.globalDrawModeEnabled()) {
        pm.disableDraw();
    } 

    if (pm.globalEditModeEnabled()) {
        pm.disableGlobalEditMode();
    }

    if (pm.globalDragModeEnabled()) {
        pm.disableGlobalDragMode();
    }

    if (pm.globalRemovalModeEnabled()) {
        pm.disableGlobalRemovalMode();
    }

    if (pm.globalCutModeEnabled()) {
        pm.disableGlobalCutMode();
    }

    if (pm.globalRotateModeEnabled()) {
        pm.disableGlobalRotateMode();
    }
}

function setControlsVisible(pm: PM.PMMap, visible: boolean) {
    if (visible !== pm.controlsVisible()) {
        pm.toggleControls();
    }
}

export function setDefaultStyle(layer: Layer) {
    if (isPolygonLayer(layer)) {
        layer.setStyle(audienceStyle);
    } else if (isPolylineLayer(layer)) {
        layer.setStyle(wallStyle);
    }
}

export function getStyleByRoomType(type: RoomType): PathOptions {
    if (type === RoomType.Hall) {
        return hallStyle;
    }

    return audienceStyle;
}

export { disableAllModes, setControlsVisible };

