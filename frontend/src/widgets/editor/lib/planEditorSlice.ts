import { Building, isRoom, isWall, Level, RoomMetaProperties } from "@entities/map";
import { Room, Wall } from "@entities/map";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import guid from "@shared/types/guid";
import { roundCoordinates } from "@shared/map/lib/leafletUtilsAdditions";
import { GEOJSON_PRECISION } from "@app/config/constants";
import { CatalogueBuilding } from "@entities/catalogue";
import { Polygon } from "geojson";
import { isITEquipmentMetaProperties, ITEquipment, ITEquipmentMetaProperties } from "@entities/map/models/ItEquipment";

export enum CreateNewPlanStep {
    BuildingBoundariesSetup = 0,
    RoomsBoundariesSetup = 1,
    InfrastructureSetup = 2,
}

interface CreateNewPlanState {
    building: Building | undefined;
    buildingInfoFromCatalogue: CatalogueBuilding;

    currentStep: CreateNewPlanStep;

};

const initialState: CreateNewPlanState = {
    building: undefined,
    buildingInfoFromCatalogue: {
        address: "",
        levels: [],
        name: ""
    },

    currentStep: CreateNewPlanStep.RoomsBoundariesSetup,
}

interface IninitNewStatePayload {
    region: string;
    buildingInfoFromCatalogue: CatalogueBuilding;
    basementGeometry: Polygon;
    step: CreateNewPlanStep;
}

export const planEditorSlice = createSlice({
    name: "planEditor",
    initialState,
    reducers: {
        resetToInitialState(state) {
            state.building = initialState.building;
            state.currentStep = initialState.currentStep;
            state.buildingInfoFromCatalogue= initialState.buildingInfoFromCatalogue;
        },
        initStateWithBuilding(state, {payload}: PayloadAction<{building: Building; catalogue: CatalogueBuilding}>) {
            state.building = payload.building;
            state.buildingInfoFromCatalogue = payload.catalogue;
            state.currentStep = CreateNewPlanStep.RoomsBoundariesSetup;
        },
        initNewState(state, {payload}: PayloadAction<IninitNewStatePayload>) {
            const {step, buildingInfoFromCatalogue, basementGeometry, region} = payload;

            const levels = buildingInfoFromCatalogue.levels.length > 0
                ? buildingInfoFromCatalogue.levels.map((level, index) => ({
                        number: index + 1,
                        name: level.name,
                        buildingStructure: [],
                        infrastructure: [],
                      }))
                : [{number: 1, name: "1 этаж", buildingStructure:[], infrastructure: []}];

            state.building = {
                  type: "Feature",
                  geometry: basementGeometry,
                  properties: {
                    levels: levels,
                    region: region,
                    address: buildingInfoFromCatalogue.address,
                    name: buildingInfoFromCatalogue.name,
                  }
                };

            state.currentStep = step;
            state.buildingInfoFromCatalogue = buildingInfoFromCatalogue;
        },
        setStep(state, {payload}: PayloadAction<CreateNewPlanStep>) {
            state.currentStep = payload
        },
        addLevel(state, {payload}: PayloadAction<{name: string;}>) {
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
        },
        editLevel(state, {payload}: PayloadAction<{levelIndex: number, levelName: string}>) {
            const {levelIndex, levelName} = payload;

            const building = state.building;
            if (!building) {
                return;
            }
            const levels = building.properties.levels;
            if (levelIndex >= levels.length || levelIndex < 0) {
                return;
            }

            const level = building.properties.levels[levelIndex];
            level.name = levelName;
        },
        removeLevelAtIndex(state, {payload: index}: PayloadAction<number>) {
            const building = state.building;
            if (!building || building.properties.levels.length === 1) {
                return;
            }
            const levels = building.properties.levels;

            if (index >= levels.length || index < 0) {
                return;
            }

            levels.splice(index, 1);
            for (let i = index; i < levels.length; i++) {
                levels[i].number--;
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
        setBuildingStructureOnLevel(state,
            { payload }: PayloadAction<{ levelIndex: number; featureId: guid | number; feature: Partial<Room> | Wall | null }>
        ) {
            const { featureId, feature, levelIndex } = payload;
            const building = state.building;
            if (!building) {
              console.error('No building in state');
              return;
            }
            const levels = building.properties.levels;
            if (levelIndex >= levels.length || levelIndex < 0) {
              console.error('Invalid level index');
              return;
            }
        
            const level = levels[levelIndex];
            const levelFeatures = level.buildingStructure;
            const featureIndex = levelFeatures.findIndex((x) => x.id === featureId);
        
            if (feature === null && featureIndex !== -1) {
              // Delete feature
              levelFeatures.splice(featureIndex, 1);
            } else if (feature !== null) {
              if (feature.properties?.meaning === "Wall" && isWall(feature as Room | Wall)) {
                  if (featureIndex !== -1) {
                      levelFeatures[featureIndex] = feature as Wall;
                  } else {
                      levelFeatures.push(feature as Wall);
                  }
              } else {
                  if (featureIndex !== -1) {
                    const prevState = levelFeatures[featureIndex];
                    levelFeatures[featureIndex] = {...prevState, ...feature} as Room;
                  } 
                  else if (!feature.id || !feature.geometry || feature.properties?.meaning !== "Room" || !feature.properties?.type) {
                    return;
                  }
                  else {
                    levelFeatures.push({
                        ...feature,
                    } as Room);
                  }
              }
            }
        },
        setRoomsOnLevel(state, {payload}: PayloadAction<{levelIndex:number; rooms:Room[]}>) {
            const {levelIndex, rooms} = payload;
            const building = state.building;

            if (!building) {
                return;
            }
            const levels = building.properties.levels;
            if (levelIndex >= levels.length || levelIndex < 0) {
                return;
            }

            const level = building.properties.levels[levelIndex];
            level.buildingStructure = rooms; 
        },
        setItInfrastructureOnLevel(state, {payload}: PayloadAction<{levelIndex: number; featureId: string, feature: ITEquipment | null}>) {
            const {featureId, feature, levelIndex} = payload;
            const building = state.building;
            if (!building) {
                return;
            }
            const levels = building.properties.levels;
            if (levelIndex >= levels.length || levelIndex < 0) {
                return;
            }
            const level = state.building?.properties.levels[levelIndex];

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
        updateMetaProperties(state, {payload}: PayloadAction<{levelIndex: number; featureId: guid | number; props: RoomMetaProperties | ITEquipmentMetaProperties}>) {
            const {levelIndex, featureId, props} = payload;
            const {building} = state;
            if (!building) {
                return;
            }

            const level = building.properties.levels[levelIndex];
            let feature: Room | ITEquipment | undefined;
            if (isITEquipmentMetaProperties(props)) {
                feature = level.infrastructure.find(x => x.id === featureId);
            } else {
                feature = level.buildingStructure.find(x => x.id === featureId && isRoom(x)) as Room | undefined;
            }

            if (feature === undefined) {
                return;
            }

            feature.properties = props;
        }
    }
})

export default planEditorSlice.reducer;

/* Map edit mode switching */ 
export const { setStep } = planEditorSlice.actions;

/* Building inside things */
export const { editBuilding, setBuildingStructureOnLevel, setItInfrastructureOnLevel, updateMetaProperties, setRoomsOnLevel } = planEditorSlice.actions;

/* Level Handling */
export const { addLevel, removeLevelAtIndex, editLevel } = planEditorSlice.actions;

export const { resetToInitialState, initNewState, initStateWithBuilding } = planEditorSlice.actions;