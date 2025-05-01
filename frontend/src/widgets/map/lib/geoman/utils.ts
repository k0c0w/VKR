import { Layer, PathOptions, PM } from "leaflet";
import { CreateNewPlanStep } from "../createNewPlanSlice";
import { isPolygonLayer, isPolylineLayer } from "@shared/map";
import { audienceStyle, hallStyle, wallStyle } from "./styling";
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

function setButtons(toolbar: PM.PMMapToolbar, buttons: string[], value: boolean) {
    for (const button of buttons) {
        toolbar.setButtonDisabled(button, !value);
    }
}

function setButtonsForStep(pm: PM.PMMap, step: CreateNewPlanStep) {
    const toolbar = pm.Toolbar;
    switch(step) {
        case CreateNewPlanStep.BuildingBoundariesSetup:
            setButtons(toolbar, ['drawMarker', 'drawCircleMarker', 'drawPolyline', 'drawPolygon', 'dragMode', 'removalMode', 'rotateMode'], false);
            setButtons(toolbar, ['editMode', 'cutPolygon'], true);
            break;
        case CreateNewPlanStep.RoomsBoundariesSetup:
            setButtons(toolbar, ['drawPolyline', 'drawPolygon', 'dragMode', 'removalMode', 'rotateMode', 'editMode', 'cutPolygon'], true);
            setButtons(toolbar, ['drawCircleMarker', 'drawMarker'], false);
            break;
        case CreateNewPlanStep.InfrastructureSetup:
            setButtons(toolbar, ['drawMarker', 'dragMode', 'removalMode'], true);
            setButtons(toolbar, ['drawCircleMarker', 'drawPolyline', 'drawPolygon', 'rotateMode', 'editMode', 'cutPolygon'], false);
            break;
        default:
            console.warn("Not all states of enum covered.", step);
            break;
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

export { disableAllModes, setControlsVisible, setButtonsForStep };

