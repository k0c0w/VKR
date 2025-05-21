import { Building } from "@entities/map/Building";
import { CircularProgress, Container, Skeleton, Stack, Typography } from "@mui/material";
import { ReactNode, useEffect, useState } from "react";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { CreateNewPlanStep, initNewState, resetToInitialState, } from "@widgets/editor/lib/planEditorSlice";
import CreateNewPlanSubPage from "./CreateNewPlanSubPage";
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query/react";
import { SerializedError } from "@reduxjs/toolkit";
import AlertDialog from "@shared/ui/AlertDialog";
import FullPageTint from "@shared/ui/FullPageTint";
import { useNavigate } from "react-router";
import { Routes } from "@app/routing/routes";
import LoadBuildingBoundariesSubPage from "./LoadBuildingBoundariesSubPage";
import { plansApi } from "@features/plans";
import { isRoom, RoomType } from "@entities/map";
import { LineString, Polygon } from "geojson";
import { isDomainErrorResponse, isFetchBaseQueryError, isServerErrorResponse } from "@shared/types/ProblemDetails";
import { isValidationErrorResponse } from "@features/plans/models/CreateNewPlan";

function parseError(error: FetchBaseQueryError | SerializedError): ReactNode {
    if (isFetchBaseQueryError(error)) {
        if (isValidationErrorResponse(error)) {
            return <Stack>
                    <Typography component="h5">Ошибки валидации</Typography>
                    <Typography component="h6">Постарайтесь исправить следующие ошибки:</Typography>
                    <Typography component="text" color="error">
                        {JSON.stringify(error.data.errors)}
                    </Typography>
                </Stack>
        }

        if (isDomainErrorResponse(error)) {
            return <Stack>
                    {
                        error.data.detail && <Typography component="h6">Постарайтесь исправить следующую ошибку: {
                        <Typography component="span" color="error">{error.data.detail}</Typography>}</Typography>
                    }
                </Stack>
        }

        if (isServerErrorResponse(error)) {
            return <Stack>
                    <Typography component="h5">Критическая ошибка на сервере: 500 статус код.</Typography>
            </Stack>
        }

    }
    return <Typography color="error">
        Произошла непредвиденная ошибка.
    </Typography>
}

export default function CreateNewPlanPage() {
    const [loadedBuilding, setLoadedBuilding] = useState<Building | undefined>();
    const [createNewPlan, {data, error, isLoading, isSuccess, reset}] = plansApi.useCreateNewPlanMutation();
    const building = useAppSelector(state => state.planEditorSlice.building);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    function onPlanCreate(building: Building) {
        const {geometry, properties} = building;
        const {address, levels} = properties;
        
        createNewPlan({
            address: {
                city: address.city,
                house: address.houseNumber,
                street: address.street
            },
            basementGeometry: geometry,
            levels: levels.map(l => ({
              name: l.name,
              number: l.number,
              structure: l.buildingStructure.map(x => {
                const geometry = x.geometry;
                if (isRoom(x)) {
                    return {
                        architectualId: x.properties.id,
                        name: x.properties.name ?? "",
                        geometry: geometry,
                        type: x.properties.type,
                        meaning: "Room",
                    } as {
                        architectualId:string;
                        name:string;
                        geometry: Polygon;
                        meaning: "Room",
                        type: RoomType
                    };
                }

                return {
                    geometry: geometry,
                    meaning: "Wall"
                } as {geometry: LineString; meaning:"Wall"};
              }),
              itEquipments: l.infrastructure.map(x => ({}))  
            })),
        });
    }

    useEffect(() => {
        if (isSuccess && data) {
            // todo: set actual plan state here from response;
            navigate(Routes.FormatSpecificPlanRouteTemplate(data.id));
            dispatch(resetToInitialState());
        }
    }, [isSuccess]);

    useEffect(() => {
        if (loadedBuilding) {
            dispatch(initNewState({
                building: loadedBuilding,
                step: CreateNewPlanStep.BuildingBoundariesSetup,
                levelIndex: 0,
            }))
        } else {
            dispatch(resetToInitialState())
        }
    }, [loadedBuilding]);

    return <>
        <title>Создать новый план</title>
        <Container component="main" style={{width: 800, height: 600}}>
            {!loadedBuilding && <LoadBuildingBoundariesSubPage 
                setBuilding={setLoadedBuilding}
                loaderBackground={<Skeleton width="100%" height={800}/>}
            />}
            {building && <CreateNewPlanSubPage createPlan={onPlanCreate} />}
            {isLoading && <FullPageTint><CircularProgress color="primary"/></FullPageTint> }
            {error && <AlertDialog
                title="Ошибка при создании плана"
                content={parseError(error)}
                handleClose={reset}
                open={error !== undefined}
            />}
        </Container>
    </>

}