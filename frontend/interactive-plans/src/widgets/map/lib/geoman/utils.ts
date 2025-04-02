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

function setUpControls(pm: PM.PMMap, {
    controlsVisable,
    drawMode,
    editMode,
    dragMode,
    removalMode,
    cutMode,
    rotationMode,
}: ControlsSettings) {
    if (controlsVisable !== pm.controlsVisible()) {
        pm.toggleControls();
    }

    if (drawMode === false) {
        pm.disableDraw();
    } else {
        const {shape, options} = drawMode as {
            shape: PM.SUPPORTED_SHAPES;
            options?: PM.DrawModeOptions;
        }
        pm.enableDraw(shape, options);
    }

    if (editMode !== pm.globalEditModeEnabled()) {
        editMode ? pm.enableGlobalEditMode() : pm.disableGlobalEditMode();
    }

    if (dragMode !== pm.globalDragModeEnabled()) {
        dragMode ? pm.enableGlobalDragMode() : pm.disableGlobalDragMode();
    }

    if (removalMode !== pm.globalRemovalModeEnabled()) {
        removalMode ? pm.enableGlobalRemovalMode() : pm.disableGlobalRemovalMode();
    }

    if (cutMode !== pm.globalCutModeEnabled()) {
        cutMode ? pm.enableGlobalCutMode() : pm.disableGlobalCutMode();
    }

    if (rotationMode !== pm.globalRotateModeEnabled()) {
        rotationMode ? pm.enableGlobalRotateMode() : pm.disableGlobalRotateMode();
    }
}

export {setUpControls};