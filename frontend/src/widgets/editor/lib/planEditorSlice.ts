import { Building, isITInfrastructureMetaProperties, isRoom, ITInfrastructure, ITInfrastructureMetaProperties, Level, RoomMetaProperties } from "@entities/map";
import { Room, Wall } from "@entities/map";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { findPolygonContainingPoint } from "@shared/map";
import { guid } from "@shared/types/guid";
import compareObjectsByFields from "@shared/utils/compareObjectsByFields";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION } from "@app/config/constants";

export enum CreateNewPlanStep {
    BuildingBoundariesSetup = 0,
    RoomsBoundariesSetup = 1,
    InfrastructureSetup = 2,
}

interface CreateNewPlanState {
    building: Building | undefined;

    currentLevelIndex: number;
    currentStep: CreateNewPlanStep;
    selectedFeatureInfo: {
        feature: Room | ITInfrastructure;
        additions: {
            level: {
                index: number;
                name: string;
            },
            roomName: string;
        }
    } | undefined
};

const initialState: CreateNewPlanState = {
    building: undefined,

    currentLevelIndex: 0,
    currentStep: CreateNewPlanStep.RoomsBoundariesSetup,
    selectedFeatureInfo: undefined
}

interface IninitNewStatePayload {
    building: Building;
    levelIndex: number;
    step: CreateNewPlanStep;
}

export const planEditorSlice = createSlice({
    name: "planEditor",
    initialState,
    reducers: {
        resetToInitialState(state) {
            state.currentLevelIndex = initialState.currentLevelIndex;
            state.building = initialState.building;
            state.currentStep = initialState.currentStep;
            state.selectedFeatureInfo = initialState.selectedFeatureInfo;
        },
        initNewState(state, {payload}: PayloadAction<IninitNewStatePayload>) {
            const {building, levelIndex, step} = payload;

            const buildingLevels = building.properties.levels;

            if (levelIndex >= buildingLevels.length || levelIndex < 0) {
                throw new Error("Invalid levelIndex set")
            }

            if (building.properties.levels.length === 0) {
                throw new Error("Building must have at least 1 level.")
            }

            state.currentLevelIndex = levelIndex;
            state.building = building;
            state.currentStep = step;
            state.selectedFeatureInfo = undefined;
        },
        setStep(state, {payload}: PayloadAction<CreateNewPlanStep>) {
            state.currentStep = payload
        },
        setCurrentLevelIndex(state, {payload}: PayloadAction<number>) {
            const levelIndexToSet = payload;
            const {building} = state;
            if (!building) {
                return;
            }

            if (0 <= levelIndexToSet && levelIndexToSet < building.properties.levels.length) {
                state.currentLevelIndex = levelIndexToSet;
            } else {
                console.warn("Attempt to set level out of bounds: %d of %d.", levelIndexToSet + 1, building.properties.levels);
            }
        },
        addLevelAndSwitchOnIt(state, {payload}: PayloadAction<{name: string;}>) {
            const { building } = state;
            if (!building) {
                return;
            }

            const levels  = building.properties.levels;
            const newLevel: Level = {
                buildingStructure: [],
                infrastructure: [],
                name: payload.name,
                number: levels[levels.length - 1].number + 1
            };

            building.properties.levels.push(newLevel);

            state.currentLevelIndex = levels.length - 1;
        },
        editCurrentLevel(state, {payload}: PayloadAction<{levelName: string}>) {
            const {levelName} = payload;

            const {currentLevelIndex, building} = state;
            if (!building) {
                return;
            }

            const level = building.properties.levels[currentLevelIndex];
            level.name = levelName;
        },
        removeCurrentLevel(state) {
            const { building, currentLevelIndex } = state;
            if (!building || building.properties.levels.length === 1) {
                return;
            }

            const levels = building.properties.levels;

            levels.splice(currentLevelIndex, 1);
            for (let i = currentLevelIndex; i < levels.length; i++) {
                levels[i].number--;
            }

            if (levels.length <= currentLevelIndex) {
                state.currentLevelIndex = levels.length - 1;
            }
        },
        editBuilding(state, {payload}: PayloadAction<Partial<Building>>) {
            if (state.building) {
                state.building = {
                    ...state.building,
                    ...payload
                };
            }
        },
        setBuildingStructureOnCurrentLevel(state, {payload}: PayloadAction<{featureId: guid, feature: Room | Wall | null}>) {
            const {featureId, feature} = payload;
            const level = state.building?.properties.levels[state.currentLevelIndex];

            const levelFeatures = level!.buildingStructure;
            const featureIndex = levelFeatures.findIndex(x => x.id === featureId);

            if (feature === null && featureIndex !== -1) {
                levelFeatures.splice(featureIndex, 1);
            } else if (feature !== null) {
                feature.geometry = roundCoordinates(feature.geometry, GEOJSON_PRECISION);
                if (featureIndex === -1) {
                    levelFeatures.push(feature);
                } else {
                    levelFeatures[featureIndex] = feature;
                }
            }
        },
        setRoomsOnCurrentLevel(state, {payload}: PayloadAction<Room[]>) {
            const rooms = payload;
            const {building, currentLevelIndex} = state;

            if (!building) {
                return;
            }

            const level = building.properties.levels[currentLevelIndex];
            level.buildingStructure = rooms; 
        },
        setItInfrastructureOnCurrentLevel(state, {payload}: PayloadAction<{featureId: guid, feature: ITInfrastructure | null}>) {
            const {featureId, feature} = payload;
            const level = state.building?.properties.levels[state.currentLevelIndex];

            const levelFeatures = level!.infrastructure;
            const featureIndex = levelFeatures.findIndex(x => x.id === featureId);

            if (feature === null && featureIndex !== -1) {
                levelFeatures.splice(featureIndex, 1);
            } else if (feature !== null) {
                feature.geometry = roundCoordinates(feature.geometry, GEOJSON_PRECISION);
                if (featureIndex === -1) {
                    levelFeatures.push(feature);
                } else {
                    levelFeatures[featureIndex] = feature;
                }
            }
        },
        updateMetaProperties(state, {payload}: PayloadAction<{levelIndex: number; featureId: guid; props: RoomMetaProperties | ITInfrastructureMetaProperties}>) {
            const {levelIndex, featureId, props} = payload;
            const {building} = state;
            if (!building) {
                return;
            }

            const level = building.properties.levels[levelIndex];
            let feature: Room | ITInfrastructure | undefined;
            if (isITInfrastructureMetaProperties(props)) {
                feature = level.infrastructure.find(x => x.id === featureId);
            } else {
                feature = level.buildingStructure.find(x => x.id === featureId && isRoom(x)) as Room | undefined;
            }

            if (feature === undefined) {
                return;
            }

            feature.properties = props;
        },
        focusOnFeature(state, {payload}: PayloadAction<{featureId: guid; levelIndex: number; isITInfrastructureFeature: boolean;} | undefined>) {
            if (payload && state.building) {
                const {featureId, levelIndex, isITInfrastructureFeature:searchInInfrastructure} = payload;
                const levels = state.building.properties.levels;
                if (levelIndex < 0 || levelIndex >= levels.length) {
                    return;
                }

                const level = levels[levelIndex];
                let feature: ITInfrastructure | Room | undefined;
                if (searchInInfrastructure) {
                    feature = level.infrastructure.find(x => x.id === featureId);
                } else {
                    feature = level.buildingStructure.find(x => x.id === featureId && isRoom(x)) as Room | undefined;
                }

                if (feature === undefined) {
                    return;
                }

                const levelAddition = {
                    index: levelIndex,
                    name: level.name,
                }

                const roomName = isRoom(feature) ? feature.properties?.name : findPolygonContainingPoint(level.buildingStructure.filter(x => isRoom(x)) as Room[], feature)?.properties.name;

                const newSelectedFeatureInfo = {
                    feature: feature,
                    additions: {
                        level: levelAddition,
                        roomName: roomName ?? ""
                    }
                }

                if (compareObjectsByFields(state.selectedFeatureInfo, newSelectedFeatureInfo)) {
                    return;
                } else {
                    state.selectedFeatureInfo = newSelectedFeatureInfo;
                }
            } else if (payload === undefined && state.selectedFeatureInfo !== undefined) {
                state.selectedFeatureInfo = undefined;
            }
        }
    }
})

export default planEditorSlice.reducer;

/* Map edit mode switching */ 
export const { setStep } = planEditorSlice.actions;

/* Building inside things */
export const { editBuilding, setBuildingStructureOnCurrentLevel, setItInfrastructureOnCurrentLevel, updateMetaProperties, setRoomsOnCurrentLevel } = planEditorSlice.actions;

/* Level Handling */
export const {addLevelAndSwitchOnIt, removeCurrentLevel, setCurrentLevelIndex, editCurrentLevel} = planEditorSlice.actions;

export const { focusOnFeature } = planEditorSlice.actions;

export const { resetToInitialState, initNewState } = planEditorSlice.actions;