import { Building, Level } from "@entities/map";
import { mapApi, parseFetchBuildingBoundariesError } from "@features/map";
import { Backdrop, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material"
import { Address } from "@shared/types/ValueObjectsTypes"
import { Polygon } from "geojson";
import { useEffect } from "react";

interface AddressLoaderProps {
    address: Address;
    setBuilding: (data: Building) => void;
};

const defaultLevels: Level[] =  [{
    buildingStructure: [],
    infrastructure: [],
    name: "1 этаж",
    number: 1,
}];

export default function AddressLoader({address, setBuilding}: AddressLoaderProps) {
    const {data, isSuccess, isFetching, isError, error, refetch} = mapApi.useFetchBuildingBoundariesQuery(address);

    function setDefault() {
        const defaultBounds: Polygon = {
            type: "Polygon",
            coordinates: [[[-50, -50], [50, -50], [50, 50], [-50, 50], [-50, -50]]]
        };

        setBuilding({
            type: "Feature",
            geometry: defaultBounds,
            properties: {
                levels: [...defaultLevels],
                address: address
            }
        });
    }

    useEffect(() => {
        if (isSuccess) {
            const { boundaries, levels } = data;

            const building: Building = {
                type: "Feature",
                geometry: {
                    type: "Polygon",
                    coordinates: boundaries,
                },
                properties: {
                    levels: [...defaultLevels],
                    address: address
                }
            };

            setBuilding(building);
        }
    }, [data, isSuccess, setBuilding]);

    return <Backdrop
        sx={(theme) => ({color: "#fff", zIndex: theme.zIndex.drawer - 1})}
        open={true}
    >
        {isFetching && <CircularProgress color="inherit"/>}
        {isError && <Paper elevation={3}>
                <Stack>
                    <Typography color="error">{parseFetchBuildingBoundariesError(error)}</Typography>
                    <Button variant="outlined" onClick={refetch}>Повторить загрузку</Button>
                    <Button variant="contained" onClick={setDefault}>Создать самостоятельно</Button>
                </Stack>
            </Paper>}
    </Backdrop>
}