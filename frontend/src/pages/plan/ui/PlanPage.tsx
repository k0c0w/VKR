import { Routes } from "@app/routing/routes";
import { Building, Level, Room, Wall } from "@entities/map";
import { plansApi } from "@features/plans";
import { Button, CircularProgress, Container, Stack, Typography } from "@mui/material";
import { useAppDispatch, useAppSelector } from "@shared/hooks/reduxTypedHooks";
import { isNotFoundErrorResponse } from "@shared/types/ProblemDetails";
import { PlanEditorWidget } from "@widgets/editor";
import { initNewState } from "@widgets/editor";
import { useCallback, useEffect } from "react";
import { useNavigate, useParams } from "react-router"

export default function PlanPage() {
    const {id} = useParams();
    const building = useAppSelector(state => state.planEditorSlice.building);
    const [fetchPlan, {data, isSuccess, isError, isFetching, error}] = plansApi.useLazyGetSpecificPlanQuery();
    const dispatch = useAppDispatch();
    const navigate = useNavigate();

    const refetch = useCallback(() => {
        if (id) {
            fetchPlan({id:id});
        }
    }, [id, fetchPlan]);

    useEffect(() => {
        if (id && (building === undefined || building.id !== id)) {
            refetch();
        }
    }, [id]);

    useEffect(() => {
        if (isSuccess && data) {
            const building: Building = {
                id: data.id,
                geometry: data.basementGeometry,
                type: "Feature",
                properties: {
                    address: {
                        city: data.address.city,
                        street: data.address.street,
                        houseNumber: data.address.house
                    },
                    levels: data.levels.map(l => ({
                        name: l.name,
                        number: l.number,
                        infrastructure: l.itEquipments.map(ie => ({})),
                        buildingStructure: l.structure.map(s => {
                            if (s.meaning === "Wall") {
                                const wall: Wall = {
                                    id: s.id,
                                    type: "Feature",
                                    geometry: s.geometry,
                                    properties:{meaning: "Wall"}
                                }
                                return wall;
                            }
                            if (s.meaning === "Room") {
                                const room: Room = {
                                    id: s.id,
                                    type: "Feature",
                                    geometry: s.geometry,
                                    properties: {
                                        meaning: "Room",
                                        type: s.type,
                                        name: s.name,
                                        id: s.architectualId,
                                    }
                                }
                                return room;
                            }

                            throw new Error("Unknown structure type.");
                        })
                    })) as Level[]
                }
            }
            dispatch(initNewState({
                building: building,
                step: 1,
                levelIndex: 0
            }));
        }

        if (isError && error) {
            if (isNotFoundErrorResponse(error)) {
                navigate(Routes.AvailablePlansRouteTemplate);
            }
        }
    }, [data, isSuccess, isError, error, dispatch]);

    
    return <Container component="main" style={{width: 800, height: 600}}>
        {isSuccess && building && <PlanEditorWidget  style={{width: 600, height: 800}}/>}
        {!building && isFetching && <CircularProgress />}
        {isError && error && 
            <Stack>
                <Typography component="h3">Не удалось загрузить план здания.</Typography>
                <Button onClick={refetch}>Повторить</Button>
            </Stack>}
    </Container>
}
