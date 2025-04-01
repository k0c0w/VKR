import { mapApi, parseFetchBuildingBoundariesError } from "@features/map";
import { Backdrop, Button, CircularProgress, Paper, Stack, Typography } from "@mui/material"
import { Address } from "@shared/types/ValueObjectsTypes"
import { Polygon } from "geojson";
import { useEffect } from "react";

interface AddressLoaderProps {
    address: Address;
    setBuildingInfo: (data: {bounds: Polygon; levels: number;}) => void;
};

export default function AddressLoader({address, setBuildingInfo}: AddressLoaderProps) {
    const {data, isSuccess, isFetching, isError, error, refetch} = mapApi.useFetchBuildingBoundariesQuery(address);

    function setDefaultBounds() {
        const defaultBounds: {
            bounds: Polygon;
            levels: number;
        } = {
            bounds: {
                type: "Polygon",
                coordinates: [[[-1, -1], [1, -1], [1, 1], [-1, 1], [-1, -1]]]
            },
            levels: 1
        };

        setBuildingInfo(defaultBounds);
    }

    useEffect(() => {
        if (isSuccess) {
            const { boundaries, levels } = data;

            setBuildingInfo({
                bounds: {
                    type: "Polygon", 
                    coordinates: boundaries
                },
                levels: levels
            });
        }
    }, [isSuccess, setBuildingInfo]);

    return <Backdrop
        sx={(theme) => ({color: "#fff", zIndex: theme.zIndex.drawer - 1})}
        open={true}
    >
        {isFetching && <CircularProgress color="inherit"/>}
        {isError && <Paper elevation={3}>
                <Stack>
                    <Typography color="error">{parseFetchBuildingBoundariesError(error)}</Typography>
                    <Button variant="outlined" onClick={refetch}>Повторить загрузку</Button>
                    <Button variant="contained" onClick={setDefaultBounds}>Создать самостоятельно</Button>
                </Stack>
            </Paper>}
    </Backdrop>
}