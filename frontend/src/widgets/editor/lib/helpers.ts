import { isPolygonLayer, isPolylineLayer } from "@shared/map";
import { Layer, PM } from "leaflet";

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

export function isAnyGeomanEditModeEnabled(pm: PM.PMMap): boolean {
    return (
        pm.globalDrawModeEnabled() ||
        pm.globalEditModeEnabled() ||
        pm.globalRemovalModeEnabled() ||
        pm.globalRotateModeEnabled() ||
        pm.globalDragModeEnabled()
    );
}

export { disableAllModes, setControlsVisible };
