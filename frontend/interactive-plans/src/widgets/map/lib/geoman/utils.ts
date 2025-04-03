import { PM } from "leaflet";

interface ControlsSettings {
    controlsVisable: boolean;
    drawMode: false | {
        shape: PM.SUPPORTED_SHAPES;
        options?: PM.DrawModeOptions;
    };
    editMode: boolean;
    dragMode: boolean;
    removalMode: boolean;
    cutMode: boolean;
    rotationMode: boolean;
}

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

export { disableAllModes, setControlsVisible };